Banquet Hall Management System
Technical Requirement & Implementation Document
ASP.NET Core Razor Pages + MySQL
1. Project Overview

The Banquet Hall Management System is a lead management, booking, follow-up, payment tracking, and reporting application developed using:

ASP.NET Core Razor Pages
MySQL Database
Entity Framework Core
Bootstrap UI
Role-Based Authentication & Authorization

The application is designed for banquet halls, catering services, and function management businesses.

2. Database Configuration

The database already exists with the following credentials:

Property	Value
Host	localhost
Port	3306
Username	root
Password	alpine
Database Name	banquethall
appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=banquethall;Uid=root;Pwd=alpine;"
  }
}
3. Database Tables Structure
Table 1: Guests

Stores primary guest/customer information.

Column Name	Type	Description
Id	INT PK AI	S.No
CreatedAt	DATETIME	Date/Time Created
Name	VARCHAR(150)	Guest Name
Mobile	VARCHAR(20)	Mobile Number
Email	VARCHAR(150)	Email Address
ReferredByName	VARCHAR(150)	Reference Person Name
ReferredByPhone	VARCHAR(20)	Reference Person Phone
Status	VARCHAR(50)	VIP / VVIP / Normal
InitiatedByManagerId	INT FK	Manager Who Created User
SQL Structure
CREATE TABLE Guests (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    Name VARCHAR(150),
    Mobile VARCHAR(20),
    Email VARCHAR(150),
    ReferredByName VARCHAR(150),
    ReferredByPhone VARCHAR(20),
    Status VARCHAR(50),
    InitiatedByManagerId INT
);
Table 2: GuestFunctions

Stores function/event details.

Column Name	Type	Description
Id	INT PK AI	Primary Key
GuestId	INT FK	Guest Reference
FunctionDate	DATETIME	Function Date
FunctionType	VARCHAR(100)	Wedding / Reception
MealPlan	VARCHAR(150)	Veg/NonVeg + Breakfast/Lunch/Snacks/Dinner
InitiatedBy	VARCHAR(150)	Created By
GuestAddress	TEXT	Address
GuestAadhaar	VARCHAR(20)	Aadhaar
CreatedAt	DATETIME	Created Date
SQL Structure
CREATE TABLE GuestFunctions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GuestId INT,
    FunctionDate DATETIME,
    FunctionType VARCHAR(100),
    MealPlan VARCHAR(150),
    InitiatedBy VARCHAR(150),
    GuestAddress TEXT,
    GuestAadhaar VARCHAR(20),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
Table 3: Payments

Stores payment details.

Column Name	Type	Description
Id	INT PK AI	Primary Key
GuestId	INT FK	Guest ID
FunctionId	INT FK	Function ID
PaymentType	VARCHAR(50)	Cash/UPI/Card
Amount	DECIMAL(18,2)	Amount
NoOfPacks	INT	Number Of Packs
GuaranteedPacks	INT	Guaranteed Packs
PricePerPack	DECIMAL(18,2)	Price Per Pack
AdvancePaid	BOOLEAN	Advance Paid Yes/No
AdvanceAmount	DECIMAL(18,2)	Advance Amount
RemainingAmount	DECIMAL(18,2)	Balance Amount
CreatedAt	DATETIME	Created Date
SQL Structure
CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GuestId INT,
    FunctionId INT,
    PaymentType VARCHAR(50),
    Amount DECIMAL(18,2),
    NoOfPacks INT,
    GuaranteedPacks INT,
    PricePerPack DECIMAL(18,2),
    AdvancePaid BOOLEAN,
    AdvanceAmount DECIMAL(18,2),
    RemainingAmount DECIMAL(18,2),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
Table 4: FollowUps

Stores follow-up and lead tracking.

Column Name	Type	Description
Id	INT PK AI	Primary Key
GuestId	INT FK	Guest Reference
ManagerId	INT FK	Assigned Manager
Status	VARCHAR(50)	New / In Progress / Followup
FollowupStatus	VARCHAR(50)	May Close / May Not Close / Success / Failed
FollowupDate	DATETIME	Follow-up Date
Remarks	TEXT	Remarks
UpdatedAt	DATETIME	Updated Date
SQL Structure
CREATE TABLE FollowUps (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GuestId INT,
    ManagerId INT,
    Status VARCHAR(50),
    FollowupStatus VARCHAR(50),
    FollowupDate DATETIME,
    Remarks TEXT,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
Table 5: Users

Stores system login users.

Column Name	Type	Description
Id	INT PK AI	Primary Key
Username	VARCHAR(100)	Login Username
PasswordHash	TEXT	Encrypted Password
FullName	VARCHAR(150)	Full Name
Role	VARCHAR(50)	Admin / Manager / Receptionist
IsActive	BOOLEAN	Active Status
CreatedAt	DATETIME	Created Date
SQL Structure
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100),
    PasswordHash TEXT,
    FullName VARCHAR(150),
    Role VARCHAR(50),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);
4. Application Workflow

The application follows a step-by-step booking wizard flow.

Step 1: Guest Creation

Manager creates a guest with:

Name
Mobile
Email
Referred By
Status (VIP/VVIP)
Smart Autofill Feature

When entering:

Name
Mobile
Email
Aadhaar

The system checks existing records.

If available:

All guest details auto-fill.
Existing function history loads.
Current follow-up manager is displayed.
Initiated manager is displayed.

This feature works for:

Managers
Receptionists
Step 2: Guest Functions

Manager enters:

Function Date
Function Type
Meal Plan
Guest Address
Guest Aadhaar

Meal Plan Examples:

Veg Breakfast
Veg Lunch
Veg Dinner
NonVeg Lunch
NonVeg Dinner
Veg Snacks
Step 3: Payments

Manager enters:

Number Of Packs
Guaranteed Packs
Price Per Pack
Advance Paid (Yes/No)
Advance Amount
Payment Calculation Logic
Amount = GuaranteedPacks * PricePerPack;
RemainingAmount = Amount - AdvanceAmount;
Step 4: Follow-up Status

Manager updates lead status.

Main Status Types
Status
New
In Progress
Followup
Follow-up Sub Status
Sub Status
May Close
May Not Close
Success
Failed
Additional Fields
Follow-up Date
Remarks
Assigned Manager
5. Multiple Function Booking Logic

If an existing guest books another function:

When searching using:

Name
Phone
Email
Aadhaar

The system automatically:

Fetches guest details
Auto-fills forms
Shows previous functions
Displays follow-up history
Displays current manager

This avoids duplicate guest creation.

6. User Roles & Dashboards

The system has 3 user types with separate dashboards.

6.1 Receptionist Dashboard

Receptionist can:

✅ View guest details
✅ View who created the user
✅ View which manager is following up
✅ Search guests
✅ View function history

Receptionist cannot:

❌ Create users
❌ Edit users
❌ Manage follow-ups
❌ Access reports

6.2 Manager Dashboard

Manager can:

✅ Create guests
✅ Edit guest details
✅ Create functions
✅ Add payments
✅ Follow up leads
✅ Update statuses
✅ Follow up another manager’s users if unavailable
✅ View own business reports

Manager cannot:

❌ Create staff
❌ Delete staff
❌ Access full financial analytics

6.3 Admin Dashboard

Admin has full access.

Admin can:

✅ Create/Edit/Delete Managers
✅ Create/Edit/Delete Receptionists
✅ View all guests
✅ View all functions
✅ View all payments
✅ View all follow-ups
✅ Reassign manager leads
✅ Track business analytics
✅ View staff activities

7. Admin Reporting System

Admin dashboard includes advanced business intelligence reports.

Revenue Reports

Admin can view business closed:

Today
This Week
This Month
Financial Year
Exact Date
From Date → To Date
Manager Wise Reports

Admin can view:

Business closed by a particular manager
Follow-up count by manager
Today's manager activities
Lead conversion rate
Current User Management Tracking

Admin can view:

Which manager initiated the user
Which manager currently follows the user
User transfer history
Follow-up Monitoring

Admin can view:

How many users a manager followed today
Which users were contacted
Current statuses
Upcoming follow-ups
8. Lead Handover System

If a manager is unavailable or on leave:

Admin can transfer all leads from:

Manager A → Manager B
Transfer Conditions

Transfer:

In Progress Leads
Follow-up Leads
Active Leads
Example Logic
var leads = _context.FollowUps
    .Where(x => x.ManagerId == oldManagerId);

foreach (var lead in leads)
{
    lead.ManagerId = newManagerId;
}
9. Authentication & Authorization

The application uses:

ASP.NET Core Identity
OR
Cookie Authentication
Role Authorization Example
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Manager")]
[Authorize(Roles = "Receptionist")]
10. Security Requirements
Aadhaar Masking

Receptionists should not see full Aadhaar numbers.

Example:

XXXXXXXX1234
Password Security
Store hashed passwords only
Use secure authentication
Use session expiration
Use role-based access
Activity Logs

Track:

User Creation
User Modification
Payment Updates
Lead Transfers
Follow-up Updates
11. Recommended Technology Stack
Component	Technology
Backend	ASP.NET Core Razor Pages
Database	MySQL
ORM	Entity Framework Core
Frontend	Bootstrap 5
AJAX	jQuery AJAX
Charts	Chart.js
Alerts	SweetAlert2
Authentication	ASP.NET Identity
12. Recommended Project Structure
/Pages
/Models
/Data
/Services
/Repositories
/ViewModels
/wwwroot
13. Final Functional Flow
Guest Create
    ↓
Guest Functions
    ↓
Payments
    ↓
Follow-up
14. Final Summary

The Banquet Hall Management System provides:

Guest Management
Function Booking
Smart Duplicate Detection
Payment Tracking
Follow-up Management
Role-Based Dashboards
Financial Reporting
Manager Tracking
Lead Handover
Business Intelligence Analytics

The system is designed for scalability, security, and efficient banquet business operations using ASP.NET Core Razor Pages with MySQL.