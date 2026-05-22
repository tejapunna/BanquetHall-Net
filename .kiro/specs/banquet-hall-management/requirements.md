# Requirements Document

## Introduction

The Banquet Hall Management System is a comprehensive lead management, booking, follow-up, payment tracking, and reporting application for banquet halls and catering businesses. Built on ASP.NET Core 9 Razor Pages with MySQL and Entity Framework Core, the system provides role-based dashboards for Admins, Managers, and Receptionists to manage guests, function bookings, payments, follow-ups, and business analytics.

## Glossary

- **System**: The Banquet Hall Management System application
- **Guest**: A customer or potential customer who inquires about or books a function at the banquet hall
- **Function**: An event or ceremony (Wedding, Reception) booked by a Guest at the banquet hall
- **Follow_Up**: A lead tracking record that captures the status and progress of a Guest inquiry
- **Manager**: A system user with the role to create guests, manage bookings, handle payments, and track follow-ups
- **Receptionist**: A system user with read-only access to guest information and function history
- **Admin**: A system user with full access to all features including user management, reporting, and lead reassignment
- **Booking_Wizard**: The step-by-step workflow (Guest → Function → Payment → Follow-up) used to create a complete booking
- **Lead**: A Guest record that is being actively tracked through the follow-up process
- **Pack**: A catering service unit used for pricing calculations (Amount = GuaranteedPacks × PricePerPack)
- **Lead_Handover**: The process of transferring active leads from one Manager to another
- **Aadhaar**: A 12-digit unique identity number issued by the Government of India
- **Smart_Autofill**: The feature that detects existing guest records and pre-populates form fields to avoid duplicate creation
- **Activity_Log**: A record of system actions performed by users for audit purposes

## Requirements

### Requirement 1: User Authentication

**User Story:** As a system user, I want to log in with my credentials, so that I can access the system features appropriate to my role.

#### Acceptance Criteria

1. WHEN a user submits a valid Username and Password combination matching an active user record (IsActive = true), THE System SHALL authenticate the user and redirect to the role-appropriate dashboard (Admin Dashboard, Manager Dashboard, or Receptionist Dashboard)
2. IF a user submits credentials that do not match any active user record (incorrect username, incorrect password, or IsActive = false), THEN THE System SHALL display a generic error message indicating invalid credentials and remain on the login page without revealing which field was incorrect
3. THE System SHALL store passwords using a secure one-way hash algorithm
4. WHILE a user session is active, THE System SHALL maintain the authenticated state using cookie-based authentication
5. WHEN a session exceeds 30 minutes of inactivity, THE System SHALL expire the session and redirect the user to the login page
6. THE System SHALL enforce role-based authorization using three roles: Admin, Manager, and Receptionist
7. IF an unauthenticated user attempts to access a protected page, THEN THE System SHALL redirect the user to the login page
8. WHEN a user is authenticated, THE System SHALL store the user's Id, Username, FullName, and Role in the authentication cookie claims

### Requirement 2: Guest Creation

**User Story:** As a Manager, I want to create guest records with contact and referral information, so that I can track potential and confirmed bookings.

#### Acceptance Criteria

1. WHEN a Manager submits the guest creation form with valid data, THE System SHALL create a Guest record with Name, Mobile, Email, ReferredByName, ReferredByPhone, and Status fields
2. THE System SHALL record the InitiatedByManagerId as the currently authenticated Manager
3. THE System SHALL automatically set CreatedAt to the current date and time
4. WHEN a Manager selects a Status, THE System SHALL accept one of the following values: VIP, VVIP, or Normal
5. IF a required field is missing or invalid, THEN THE System SHALL display a validation error indicating which field failed validation and prevent form submission
6. THE System SHALL require Name and Mobile as mandatory fields, and treat Email, ReferredByName, ReferredByPhone, and Status as optional fields
7. THE System SHALL validate that Name does not exceed 150 characters, Mobile does not exceed 20 characters, Email does not exceed 150 characters and conforms to a valid email format, ReferredByName does not exceed 150 characters, and ReferredByPhone does not exceed 20 characters
8. IF a non-Manager user attempts to access the guest creation form, THEN THE System SHALL deny access and redirect the user to their role-appropriate dashboard

### Requirement 3: Smart Autofill and Duplicate Detection

**User Story:** As a Manager or Receptionist, I want the system to detect existing guests when I enter identifying information, so that I can avoid creating duplicate records.

#### Acceptance Criteria

1. WHEN a user enters at least 3 characters in the Name, Mobile, or Email field, or enters a value in the Aadhaar field, THE System SHALL search existing Guest records using a case-insensitive partial match on Name, Mobile, and Email fields in the Guests table, and an exact match on GuestAadhaar in the GuestFunctions table, and display up to 10 matching guest suggestions for selection
2. WHEN a user selects a matched guest, THE System SHALL auto-fill all guest form fields (Name, Mobile, Email, ReferredByName, ReferredByPhone, and Status) with the existing record data
3. WHEN a matched guest is selected, THE System SHALL display the existing function history for that guest including FunctionDate, FunctionType, and MealPlan for each associated GuestFunction record
4. WHEN a matched guest is selected, THE System SHALL display the current follow-up Manager and the Manager who initiated the guest
5. THE System SHALL provide Smart_Autofill functionality to both Manager and Receptionist roles
6. IF no existing Guest record matches the entered value, THEN THE System SHALL allow the user to proceed with creating a new Guest record

### Requirement 4: Function Booking

**User Story:** As a Manager, I want to create function bookings for guests, so that I can record event details and meal plans.

#### Acceptance Criteria

1. WHEN a Manager submits the function form with valid data, THE System SHALL create a GuestFunction record with FunctionDate, FunctionType, MealPlan, GuestAddress, and GuestAadhaar, and set CreatedAt to the current date and time
2. THE System SHALL accept FunctionType values of Wedding or Reception
3. THE System SHALL accept MealPlan as a single selection consisting of one diet type (Veg or NonVeg) and one meal time (Breakfast, Lunch, Snacks, or Dinner), stored as a single string value (e.g., "Veg Breakfast", "NonVeg Dinner")
4. THE System SHALL associate the function with the selected Guest via GuestId, requiring that the Guest record already exists
5. THE System SHALL record the InitiatedBy field as the FullName of the authenticated Manager who created the function
6. WHEN an existing guest books another function, THE System SHALL create a new GuestFunction record linked to the same Guest without creating a duplicate Guest record
7. IF FunctionDate is set to a date in the past, THEN THE System SHALL display a validation error and prevent form submission
8. IF any required field (FunctionDate, FunctionType, MealPlan, GuestAddress, or GuestAadhaar) is missing or GuestAadhaar is not exactly 12 digits, THEN THE System SHALL display a validation error indicating the invalid field and prevent form submission

### Requirement 5: Payment Recording

**User Story:** As a Manager, I want to record payment details for function bookings, so that I can track financial transactions and outstanding balances.

#### Acceptance Criteria

1. WHEN a Manager submits the payment form, THE System SHALL create a Payment record with PaymentType, NoOfPacks, GuaranteedPacks, PricePerPack, AdvancePaid, and AdvanceAmount, where NoOfPacks and GuaranteedPacks are integers of at least 1, and PricePerPack is a decimal value of at least 0.01
2. THE System SHALL calculate Amount as GuaranteedPacks multiplied by PricePerPack
3. THE System SHALL calculate RemainingAmount as Amount minus AdvanceAmount
4. THE System SHALL accept PaymentType values of Cash, UPI, or Card
5. THE System SHALL associate the Payment with both the Guest (GuestId) and the Function (FunctionId)
6. WHEN AdvancePaid is set to true, THE System SHALL require an AdvanceAmount value greater than 0.00
7. IF AdvancePaid is set to false, THEN THE System SHALL set AdvanceAmount to 0.00 and RemainingAmount equal to Amount
8. IF AdvanceAmount exceeds the calculated Amount, THEN THE System SHALL display a validation error and prevent submission
9. IF the specified GuestId or FunctionId does not reference an existing record, THEN THE System SHALL display a validation error and prevent submission

### Requirement 6: Follow-Up Management

**User Story:** As a Manager, I want to track and update follow-up statuses for guest leads, so that I can manage the sales pipeline effectively.

#### Acceptance Criteria

1. WHEN a Manager creates a follow-up, THE System SHALL record the GuestId, ManagerId, Status, FollowupStatus, FollowupDate, and Remarks, where GuestId, ManagerId, Status, FollowupStatus, and FollowupDate are required and Remarks is optional with a maximum length of 2000 characters
2. THE System SHALL accept Status values of New, In Progress, or Followup
3. THE System SHALL accept FollowupStatus values of May Close, May Not Close, Success, or Failed
4. WHEN a Manager updates a follow-up, THE System SHALL set UpdatedAt to the current date and time
5. THE System SHALL allow any authenticated Manager to create or update follow-up records for any guest, regardless of which Manager is currently assigned to that lead
6. THE System SHALL display the assigned Manager's full name for each follow-up record
7. IF a Manager submits a follow-up with a missing required field or an invalid Status or FollowupStatus value, THEN THE System SHALL display a validation error indicating the invalid field and prevent the record from being saved
8. WHEN a Manager sets FollowupDate, THE System SHALL accept only a date that is equal to or later than the current date

### Requirement 7: Receptionist Dashboard

**User Story:** As a Receptionist, I want to view guest information and function history, so that I can assist with inquiries without modifying records.

#### Acceptance Criteria

1. THE System SHALL allow Receptionists to view guest details including Name, Mobile, Email, ReferredByName, ReferredByPhone, and Status
2. WHEN a Receptionist enters a search term, THE System SHALL return guests matching by partial Name, Mobile, or Email (case-insensitive)
3. IF a search returns no matching guests, THEN THE System SHALL display a message indicating no results were found
4. THE System SHALL allow Receptionists to view function history for any guest, displaying FunctionDate, FunctionType, MealPlan, GuestAddress, and InitiatedBy
5. WHILE a Receptionist is viewing a guest record, THE System SHALL display the Manager who created the guest (InitiatedByManagerId) and the Manager currently assigned to follow up
6. WHILE a Receptionist is viewing guest or function records, THE System SHALL display Aadhaar numbers in masked format showing only the last 4 digits (XXXXXXXX1234)
7. THE System SHALL prevent Receptionists from creating or editing guest records
8. THE System SHALL prevent Receptionists from managing follow-ups
9. THE System SHALL prevent Receptionists from accessing reports

### Requirement 8: Manager Dashboard

**User Story:** As a Manager, I want a dashboard showing my assigned leads and activities, so that I can manage my daily workload.

#### Acceptance Criteria

1. THE System SHALL display all guests where the Manager is the assigned ManagerId in active FollowUp records (Status of New, In Progress, or Followup), along with each guest's current FollowupStatus
2. THE System SHALL allow Managers to create guests, functions, payments, and follow-ups through the Booking_Wizard workflow
3. THE System SHALL allow Managers to edit guest details for guests where the Manager is either the InitiatedByManagerId on the Guest record or the assigned ManagerId on an active FollowUp for that guest
4. WHEN a Manager's assigned lead requires follow-up and the assigned Manager is unavailable, THE System SHALL allow another Manager to create or update follow-up records for that guest
5. THE System SHALL allow Managers to view their own business reports including their follow-up count, leads with Success status, and total revenue from payments on their assigned guests
6. THE System SHALL prevent Managers from creating, editing, or deleting user accounts of any role
7. THE System SHALL prevent Managers from viewing cross-manager reports, overall revenue analytics, or any report data belonging to other Managers
8. IF a Manager attempts to access a restricted feature, THEN THE System SHALL deny access and not display the restricted functionality in the Manager's navigation

### Requirement 9: Admin Dashboard and User Management

**User Story:** As an Admin, I want full access to manage users, view all data, and configure the system, so that I can oversee all business operations.

#### Acceptance Criteria

1. THE System SHALL allow Admins to create, edit, and deactivate Manager accounts, where deactivation sets the IsActive field to false and prevents the user from logging in
2. THE System SHALL allow Admins to create, edit, and deactivate Receptionist accounts, where deactivation sets the IsActive field to false and prevents the user from logging in
3. THE System SHALL allow Admins to view all guests, functions, payments, and follow-ups across all Managers
4. THE System SHALL allow Admins to reassign individual leads from one Manager to another by updating the ManagerId on the selected FollowUp record
5. THE System SHALL allow Admins to view Activity_Log records showing user actions including user identity, action performed, and timestamp
6. WHEN an Admin creates a new user, THE System SHALL require Username (maximum 100 characters, unique across all users), FullName (maximum 150 characters), Role (Manager or Receptionist), and a password (minimum 6 characters)
7. WHEN an Admin edits a user account, THE System SHALL allow modification of FullName, Role, IsActive status, and password reset, but SHALL NOT allow modification of the Username
8. IF an Admin attempts to deactivate their own account, THEN THE System SHALL prevent the action and display an error message indicating that self-deactivation is not permitted
9. IF an Admin attempts to create a user with a Username that already exists, THEN THE System SHALL display a validation error and prevent the account creation

### Requirement 10: Lead Handover

**User Story:** As an Admin, I want to transfer leads between Managers, so that business continuity is maintained when a Manager is unavailable.

#### Acceptance Criteria

1. WHEN an Admin selects a source Manager and a target Manager and initiates a lead transfer, THE System SHALL reassign all FollowUp records with Status values of New, In Progress, or Followup from the source Manager to the target Manager by updating the ManagerId on each transferred FollowUp record
2. IF the source Manager has no FollowUp records with Status values of New, In Progress, or Followup, THEN THE System SHALL inform the Admin that no transferable leads exist and perform no transfer
3. IF the Admin selects the same Manager as both source and target, THEN THE System SHALL display a validation error and prevent the transfer
4. WHEN a lead transfer completes successfully, THE System SHALL display a confirmation to the Admin indicating the number of leads transferred
5. WHEN a lead transfer completes successfully, THE System SHALL record the transfer action in the Activity_Log with the source Manager, target Manager, number of leads transferred, and timestamp

### Requirement 11: Admin Reporting

**User Story:** As an Admin, I want comprehensive business reports, so that I can analyze revenue, manager performance, and lead conversion.

#### Acceptance Criteria

1. THE System SHALL provide revenue reports that sum the Amount field from the Payments table, filterable by Today, This Week (Monday to current day), This Month, Financial Year (April 1 to March 31), specific date, and custom date range
2. THE System SHALL provide manager-wise reports showing total revenue from Payments associated with that Manager's guests, follow-up count, count of follow-ups created or updated on the selected date as daily activities, and lead conversion rate
3. THE System SHALL display which Manager initiated each guest (InitiatedByManagerId) and which Manager currently follows the guest (ManagerId from the most recent FollowUp record)
4. THE System SHALL provide follow-up monitoring showing the count of follow-ups updated per Manager on the selected date, the list of guests contacted, current follow-up statuses, and upcoming follow-ups with a FollowupDate within the next 7 days
5. THE System SHALL calculate lead conversion rate as the number of follow-ups with FollowupStatus "Success" divided by the total number of follow-ups (across all FollowupStatus values) for a Manager within the selected date range
6. IF a Manager has zero follow-ups in the selected date range, THEN THE System SHALL display the lead conversion rate as 0% instead of performing the division
7. IF no data exists for the selected filter period, THEN THE System SHALL display an empty state message indicating no records were found for the selected criteria

### Requirement 12: Aadhaar Data Security

**User Story:** As a system administrator, I want Aadhaar numbers masked for unauthorized roles, so that sensitive identity data is protected.

#### Acceptance Criteria

1. WHILE a Receptionist is viewing guest or function records in any context including search results, Smart_Autofill suggestions, and function history, THE System SHALL display Aadhaar numbers in masked format showing only the last 4 digits (XXXXXXXX1234)
2. WHILE a Manager or Admin is viewing guest or function records, THE System SHALL display the full 12-digit Aadhaar number
3. THE System SHALL store the complete Aadhaar number in the database regardless of display masking
4. IF a user submits an Aadhaar value that is not exactly 12 digits or contains non-numeric characters, THEN THE System SHALL display a validation error and prevent form submission
5. IF the Aadhaar field is empty or null for a guest record, THEN THE System SHALL display the field as blank without applying masking format

### Requirement 13: Activity Logging

**User Story:** As an Admin, I want all significant system actions logged, so that I can audit user activities and track changes.

#### Acceptance Criteria

1. WHEN a user creates a Guest record, THE System SHALL create an Activity_Log entry recording the user identity, timestamp, action type as "Create", entity type as "Guest", and the affected entity identifier
2. WHEN a user modifies a Guest or Function record, THE System SHALL create an Activity_Log entry recording the user identity, timestamp, action type as "Update", entity type, and the affected entity identifier
3. WHEN a payment is created or updated, THE System SHALL create an Activity_Log entry recording the user identity, timestamp, action type as "Create" or "Update" respectively, entity type as "Payment", and the affected entity identifier
4. WHEN a lead transfer occurs, THE System SHALL create an Activity_Log entry recording the user identity, timestamp, action type as "Transfer", source Manager identity, target Manager identity, and the number of leads transferred
5. WHEN a follow-up is created or updated, THE System SHALL create an Activity_Log entry recording the user identity, timestamp, action type as "Create" or "Update" respectively, entity type as "FollowUp", and the affected entity identifier
6. THE System SHALL display Activity_Log records to Admin users in a list view sortable by timestamp, filterable by user, action type, and entity type
7. THE System SHALL prevent Manager and Receptionist users from viewing, modifying, or deleting Activity_Log records
8. THE System SHALL prevent any user, including Admin, from modifying or deleting Activity_Log records once created

### Requirement 14: Database Integration

**User Story:** As a developer, I want the application connected to the existing MySQL database, so that all data operations use the pre-existing schema.

#### Acceptance Criteria

1. THE System SHALL connect to the MySQL database using the connection string stored in the "ConnectionStrings:DefaultConnection" key of appsettings.json with a connection timeout of 30 seconds
2. THE System SHALL use Entity Framework Core with the Pomelo.EntityFrameworkCore.MySql provider as the ORM for all database operations
3. THE System SHALL map entity models to the existing database tables (Guests, GuestFunctions, Payments, FollowUps, Users) without applying EF Core migrations that alter, create, or drop columns or tables in the database
4. THE System SHALL use the repository pattern to abstract data access from business logic, exposing one repository per database table
5. IF the database connection cannot be established within the configured timeout, THEN THE System SHALL return an error response indicating the database is unavailable and SHALL NOT terminate the application process
6. WHEN the application starts, THE System SHALL register the DbContext in the dependency injection container with a scoped lifetime

### Requirement 15: Booking Wizard Workflow

**User Story:** As a Manager, I want a step-by-step booking wizard, so that I can complete the full booking process in a guided sequence.

#### Acceptance Criteria

1. THE System SHALL present the booking process as a sequential wizard with four steps: Guest Creation, Function Details, Payment, and Follow-up, displaying a visual step indicator that highlights the current active step
2. WHEN a Manager submits Step 1 (Guest Creation) and the step passes validation, THE System SHALL save the Guest record via AJAX without a full page reload and proceed to Step 2 (Function Details)
3. WHEN a Manager submits Step 2 (Function Details) and the step passes validation, THE System SHALL save the GuestFunction record via AJAX without a full page reload and proceed to Step 3 (Payment)
4. WHEN a Manager submits Step 3 (Payment) and the step passes validation, THE System SHALL save the Payment record via AJAX without a full page reload and proceed to Step 4 (Follow-up)
5. IF a step fails validation, THEN THE System SHALL remain on the current step, display the validation errors adjacent to the relevant fields, and preserve all user-entered data in the form
6. WHEN an existing guest is selected via Smart_Autofill, THE System SHALL skip Guest Creation, set the step indicator to Step 2, and proceed to Step 2 (Function Details) with the selected guest's data associated to the booking
7. WHILE the wizard is on Step 2, Step 3, or Step 4, THE System SHALL allow the Manager to navigate back to any previously completed step to review the saved data
8. WHEN a Manager completes Step 4 (Follow-up) and the step passes validation, THE System SHALL save the FollowUp record and display a booking confirmation summary showing the Guest name, Function date, Payment amount, and Follow-up status
