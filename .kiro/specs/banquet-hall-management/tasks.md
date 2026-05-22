# Implementation Plan: Banquet Hall Management System

## Overview

This plan implements the Banquet Hall Management System as an ASP.NET Core 9 Razor Pages application with MySQL (Pomelo EF Core), Bootstrap 5, and jQuery AJAX. Tasks follow a bottom-up dependency order: NuGet packages → Models → DbContext → Repositories → Services → Authentication → Shared Layouts → Pages → AJAX endpoints → Reporting → Activity Logging → Aadhaar masking.

## Tasks

- [ ] 1. Set up NuGet packages and project foundation
  - [ ] 1.1 Add required NuGet packages to BanquetHall.csproj
    - Add Pomelo.EntityFrameworkCore.MySql (8.0.x for .NET 9 compatibility)
    - Add BCrypt.Net-Next for password hashing
    - Add Microsoft.AspNetCore.Authentication.Cookies
    - _Requirements: 14.1, 14.2, 1.3_

  - [ ] 1.2 Create entity models and DTOs
    - Create `Models/Guest.cs`, `Models/GuestFunction.cs`, `Models/Payment.cs`, `Models/FollowUp.cs`, `Models/User.cs`, `Models/ActivityLog.cs` with data annotations and navigation properties as defined in design
    - Create `ViewModels/LoginViewModel.cs`, `ViewModels/GuestCreateDto.cs`, `ViewModels/GuestUpdateDto.cs`, `ViewModels/FunctionCreateDto.cs`, `ViewModels/PaymentCreateDto.cs`, `ViewModels/FollowUpCreateDto.cs`, `ViewModels/FollowUpUpdateDto.cs`
    - Create `ViewModels/GuestSearchResultDto.cs`, `ViewModels/GuestDetailDto.cs`, `ViewModels/PaymentSummaryDto.cs`, `ViewModels/RevenueReportDto.cs`, `ViewModels/DailyRevenueDto.cs`, `ViewModels/ManagerReportDto.cs`, `ViewModels/FollowUpMonitorDto.cs`, `ViewModels/DateRangeFilter.cs`, `ViewModels/ActivityLogFilter.cs`
    - Create `Services/ServiceResult.cs` generic result wrapper
    - _Requirements: 14.3, 2.1, 4.1, 5.1, 6.1_

  - [ ] 1.3 Create BanquetHallDbContext with Fluent API configuration
    - Create `Data/BanquetHallDbContext.cs` with all DbSet properties
    - Configure entity relationships in OnModelCreating (Guest→Functions, Guest→Payments, Guest→FollowUps, GuestFunction→Payments, User→FollowUps, ActivityLog→User)
    - Use `[Table]` attributes on models to map to existing tables
    - _Requirements: 14.2, 14.3, 14.6_

  - [ ] 1.4 Configure connection string and register DbContext in Program.cs
    - Add MySQL connection string to `appsettings.json` under `ConnectionStrings:DefaultConnection`
    - Register `BanquetHallDbContext` with Pomelo MySQL provider in `Program.cs` with scoped lifetime and 30-second connection timeout
    - _Requirements: 14.1, 14.6_

- [ ] 2. Implement repository layer
  - [ ] 2.1 Create generic repository base and interfaces
    - Create `Repositories/IRepository.cs` generic interface with GetByIdAsync, GetAllAsync, Query, AddAsync, Update, SaveChangesAsync
    - Create `Repositories/Repository.cs` base implementation using BanquetHallDbContext
    - _Requirements: 14.4_

  - [ ] 2.2 Create specialized repository interfaces and implementations
    - Create `Repositories/IGuestRepository.cs` and `Repositories/GuestRepository.cs` with SearchAsync method (case-insensitive partial match on Name, Mobile, Email, max 10 results)
    - Create `Repositories/IGuestFunctionRepository.cs` and `Repositories/GuestFunctionRepository.cs` with GetByGuestIdAsync and FindByAadhaarAsync
    - Create `Repositories/IPaymentRepository.cs` and `Repositories/PaymentRepository.cs` with GetByGuestIdAsync and GetByDateRangeAsync
    - Create `Repositories/IFollowUpRepository.cs` and `Repositories/FollowUpRepository.cs` with GetByManagerIdAsync, GetActiveByManagerIdAsync, GetLatestByGuestIdAsync
    - Create `Repositories/IUserRepository.cs` and `Repositories/UserRepository.cs` with GetByUsernameAsync, UsernameExistsAsync, GetByRoleAsync
    - Create `Repositories/IActivityLogRepository.cs` and `Repositories/ActivityLogRepository.cs` with GetFilteredAsync
    - _Requirements: 14.4, 3.1, 11.1_

  - [ ] 2.3 Register all repositories in DI container
    - Register all repository interfaces and implementations as scoped services in Program.cs
    - _Requirements: 14.4, 14.6_

- [ ] 3. Implement service layer
  - [ ] 3.1 Implement AuthService
    - Create `Services/IAuthService.cs` and `Services/AuthService.cs`
    - Implement ValidateCredentialsAsync: verify username exists, password matches hash (BCrypt), and IsActive is true
    - Implement CreateClaimsPrincipalAsync: create ClaimsPrincipal with Id, Username, FullName, Role claims
    - Implement HashPassword and VerifyPassword using BCrypt
    - _Requirements: 1.1, 1.2, 1.3, 1.8_

  - [ ]* 3.2 Write property tests for AuthService
    - **Property 1: Authentication validates credentials and active status**
    - **Property 2: Password hash round-trip**
    - **Validates: Requirements 1.1, 1.2, 1.3**

  - [ ] 3.3 Implement GuestService
    - Create `Services/IGuestService.cs` and `Services/GuestService.cs`
    - Implement CreateGuestAsync: validate DTO, set CreatedAt and InitiatedByManagerId, save via repository, log activity
    - Implement UpdateGuestAsync: validate permissions (manager is initiator or assigned), update fields, log activity
    - Implement SearchGuestsAsync: delegate to repository SearchAsync, map to GuestSearchResultDto
    - Implement GetGuestDetailAsync: fetch guest with functions/followups, apply Aadhaar masking based on role
    - _Requirements: 2.1, 2.2, 2.3, 3.1, 3.2, 3.3, 3.4_

  - [ ]* 3.4 Write property tests for GuestService validation and search
    - **Property 3: Guest creation preserves input data**
    - **Property 4: Guest input validation**
    - **Property 5: Guest search returns correct partial matches**
    - **Validates: Requirements 2.1, 2.2, 2.4, 2.5, 2.6, 2.7, 3.1, 7.2**

  - [ ] 3.5 Implement FunctionService
    - Create `Services/IFunctionService.cs` and `Services/FunctionService.cs`
    - Implement CreateFunctionAsync: validate FunctionDate not in past, validate Aadhaar format (12 digits), validate FunctionType (Wedding/Reception), validate MealPlan, set CreatedAt and InitiatedBy, save, log activity
    - Implement GetFunctionsByGuestAsync: return functions for a guest
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8_

  - [ ]* 3.6 Write property tests for date and Aadhaar validation
    - **Property 6: Date validation rejects past dates**
    - **Property 7: Aadhaar format validation**
    - **Validates: Requirements 4.7, 4.8, 6.8, 12.4**

  - [ ] 3.7 Implement PaymentService
    - Create `Services/IPaymentService.cs` and `Services/PaymentService.cs`
    - Implement CreatePaymentAsync: validate GuestId/FunctionId exist, validate PaymentType (Cash/UPI/Card), calculate Amount = GuaranteedPacks × PricePerPack, handle AdvancePaid logic, validate AdvanceAmount ≤ Amount, save, log activity
    - Implement CalculatePaymentAsync: return PaymentSummaryDto with computed values
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8, 5.9_

  - [ ]* 3.8 Write property tests for payment calculation
    - **Property 8: Payment calculation consistency**
    - **Validates: Requirements 5.2, 5.3, 5.6, 5.7, 5.8**

  - [ ] 3.9 Implement FollowUpService
    - Create `Services/IFollowUpService.cs` and `Services/FollowUpService.cs`
    - Implement CreateFollowUpAsync: validate Status (New/In Progress/Followup), validate FollowupStatus (May Close/May Not Close/Success/Failed), validate FollowupDate ≥ today, validate Remarks ≤ 2000 chars, save, log activity
    - Implement UpdateFollowUpAsync: set UpdatedAt, validate fields, save, log activity
    - Implement TransferLeadsAsync: update ManagerId on all active FollowUps from source to target in a transaction, return count, log transfer activity
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.7, 6.8, 10.1, 10.2, 10.3, 10.4, 10.5_

  - [ ]* 3.10 Write property tests for lead transfer and active leads filtering
    - **Property 10: Active leads filtering for manager dashboard**
    - **Property 11: Manager edit permission**
    - **Property 12: Lead transfer correctness**
    - **Validates: Requirements 8.1, 8.3, 10.1, 10.4**

  - [ ] 3.11 Implement ActivityLogService
    - Create `Services/IActivityLogService.cs` and `Services/ActivityLogService.cs`
    - Implement LogAsync: create ActivityLog entry with UserId, Username, ActionType, EntityType, EntityId, Details, Timestamp
    - Implement LogTransferAsync: create transfer-specific log entry with source/target manager details and count
    - Implement GetLogsAsync: query with filters (userId, actionType, entityType, date range)
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 13.6, 13.7, 13.8_

  - [ ] 3.12 Implement ReportService
    - Create `Services/IReportService.cs` and `Services/ReportService.cs`
    - Implement GetRevenueReportAsync: sum Amount from Payments filtered by DateRangeFilter (Today, ThisWeek, ThisMonth, FinancialYear, ExactDate, Custom)
    - Implement GetManagerReportAsync: total revenue, follow-up count, daily activities, lead conversion rate for a manager
    - Implement GetFollowUpMonitorAsync: follow-ups updated per manager on date, guests contacted, upcoming follow-ups within 7 days
    - Implement CalculateConversionRateAsync: Success count / total count, return 0 when total is 0
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6, 11.7_

  - [ ]* 3.13 Write property tests for reporting
    - **Property 13: Revenue report date filtering**
    - **Property 14: Conversion rate calculation**
    - **Validates: Requirements 11.1, 11.5, 11.6**

  - [ ] 3.14 Implement Aadhaar masking utility
    - Create `Helpers/AadhaarHelper.cs` with static method `MaskAadhaar(string? aadhaar, string role)` that returns full value for Manager/Admin, masked (XXXXXXXX + last 4) for Receptionist, empty string for null/empty
    - Integrate masking into GuestService.GetGuestDetailAsync and search result DTOs
    - _Requirements: 12.1, 12.2, 12.3, 12.5_

  - [ ]* 3.15 Write property tests for Aadhaar masking
    - **Property 9: Aadhaar masking by role**
    - **Validates: Requirements 7.6, 12.1, 12.2, 12.5**

  - [ ] 3.16 Register all services in DI container
    - Register IAuthService, IGuestService, IFunctionService, IPaymentService, IFollowUpService, IReportService, IActivityLogService as scoped services in Program.cs
    - _Requirements: 14.6_

- [ ] 4. Checkpoint - Foundation verification
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 5. Implement authentication and authorization middleware
  - [ ] 5.1 Configure cookie authentication in Program.cs
    - Add cookie authentication scheme with 30-minute sliding expiration
    - Configure login path to `/Login`, access denied path redirects to role dashboard
    - Add `app.UseAuthentication()` before `app.UseAuthorization()`
    - _Requirements: 1.4, 1.5, 1.7_

  - [ ] 5.2 Create Login Razor Page
    - Create `Pages/Login.cshtml` with Bootstrap 5 form (Username, Password fields)
    - Create `Pages/Login.cshtml.cs` with OnPostAsync: validate credentials via AuthService, sign in with cookie, redirect to role-appropriate dashboard
    - Display generic error message on failure without revealing which field was incorrect
    - Handle `?expired=true` query param to show session expired message
    - _Requirements: 1.1, 1.2, 1.7_

  - [ ] 5.3 Create Logout handler
    - Create `Pages/Logout.cshtml.cs` with OnPostAsync: sign out cookie, redirect to Login
    - _Requirements: 1.4_

  - [ ] 5.4 Configure role-based authorization policies
    - Add authorization policies for Admin, Manager, Receptionist roles in Program.cs
    - Apply `[Authorize(Policy = "Admin")]` convention to `/Admin` folder
    - Apply `[Authorize(Policy = "Manager")]` convention to `/Manager` folder
    - Apply `[Authorize(Policy = "Receptionist")]` convention to `/Receptionist` folder
    - _Requirements: 1.6, 7.7, 7.8, 7.9, 8.6, 8.7, 8.8_

- [ ] 6. Create shared layouts and navigation
  - [ ] 6.1 Create role-specific layouts
    - Update `Pages/Shared/_Layout.cshtml` as base layout with Bootstrap 5 CDN/local references, jQuery, common footer
    - Create `Pages/Shared/_AdminLayout.cshtml` extending base with Admin navigation (Dashboard, Users, Reports, Leads, Activity Logs, Logout)
    - Create `Pages/Shared/_ManagerLayout.cshtml` extending base with Manager navigation (Dashboard, New Booking, Guests, Follow-ups, My Reports, Logout)
    - Create `Pages/Shared/_ReceptionistLayout.cshtml` extending base with Receptionist navigation (Dashboard, Guest Search, Logout)
    - _Requirements: 1.6, 8.8, 9.3_

  - [ ] 6.2 Create shared partial views
    - Create `Pages/Shared/_BookingWizardStepIndicator.cshtml` partial for wizard step progress display
    - Create `Pages/Shared/_GuestSearchPartial.cshtml` partial for reusable guest search/autofill component
    - Update `Pages/Shared/_ValidationScriptsPartial.cshtml` to include jQuery validation
    - _Requirements: 15.1, 3.1_

- [ ] 7. Implement Manager booking wizard
  - [ ] 7.1 Create Booking Wizard Razor Page structure
    - Create `Pages/Manager/Booking/Wizard.cshtml` with 4-step form layout (Guest, Function, Payment, Follow-up) and step indicator
    - Create `Pages/Manager/Booking/Wizard.cshtml.cs` with named handlers: OnPostStep1Async, OnPostStep2Async, OnPostStep3Async, OnPostStep4Async
    - Each handler validates input, calls service, returns JSON `{ success, data/errors }`
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.5_

  - [ ] 7.2 Implement wizard client-side JavaScript
    - Create `wwwroot/js/booking-wizard.js` with jQuery AJAX calls for each step
    - Implement step navigation (next/back), form validation display using Bootstrap `is-invalid` class
    - Handle Smart Autofill integration: when existing guest selected, skip Step 1 and go to Step 2
    - Display booking confirmation summary on Step 4 completion
    - _Requirements: 15.2, 15.3, 15.4, 15.5, 15.6, 15.7, 15.8_

- [ ] 8. Implement Manager guest and follow-up pages
  - [ ] 8.1 Create Manager Guest List and Edit pages
    - Create `Pages/Manager/Guests/Index.cshtml` and `.cshtml.cs` showing guests where manager is initiator or assigned via active follow-up
    - Create `Pages/Manager/Guests/Edit.cshtml` and `.cshtml.cs` with edit form, permission check (initiator or assigned manager)
    - _Requirements: 8.3, 2.1, 2.7_

  - [ ] 8.2 Create Manager Follow-Up pages
    - Create `Pages/Manager/FollowUps/Index.cshtml` and `.cshtml.cs` listing active follow-ups for the manager with guest name, status, followup status, date
    - Create `Pages/Manager/FollowUps/Edit.cshtml` and `.cshtml.cs` with form to update Status, FollowupStatus, FollowupDate, Remarks
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6_

  - [ ] 8.3 Create Manager Dashboard page
    - Create `Pages/Manager/Index.cshtml` and `.cshtml.cs` showing active leads (guests with active follow-ups assigned to this manager), current FollowupStatus per lead
    - Include quick-action links to New Booking wizard and Follow-up list
    - _Requirements: 8.1, 8.2_

- [ ] 9. Implement Smart Autofill AJAX endpoints
  - [ ] 9.1 Create Guest Search API endpoint
    - Create `Pages/Api/GuestSearch.cshtml.cs` with OnGetSearchAsync handler
    - Accept `term` query parameter, require minimum 3 characters
    - Return JSON array of GuestSearchResultDto (max 10 results) with partial match on Name, Mobile, Email
    - Also search GuestFunctions by exact Aadhaar match
    - Apply Aadhaar masking based on authenticated user's role
    - _Requirements: 3.1, 3.5, 3.6, 12.1, 12.2_

  - [ ] 9.2 Create Autofill Detail API endpoint
    - Create `Pages/Api/Autofill.cshtml.cs` with OnGetGuestDetailAsync handler
    - Accept `id` query parameter (guest ID)
    - Return full guest detail including function history (FunctionDate, FunctionType, MealPlan), current follow-up manager, initiating manager
    - Apply Aadhaar masking based on role
    - _Requirements: 3.2, 3.3, 3.4_

  - [ ] 9.3 Create autofill client-side JavaScript
    - Create `wwwroot/js/smart-autofill.js` with debounced search on Name/Mobile/Email/Aadhaar fields
    - Display dropdown suggestions, handle selection to populate form fields
    - Show function history panel when guest selected
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.6_

- [ ] 10. Implement Receptionist dashboard and pages
  - [ ] 10.1 Create Receptionist Dashboard and Guest Search
    - Create `Pages/Receptionist/Index.cshtml` and `.cshtml.cs` with search interface
    - Create `Pages/Receptionist/Guests/Index.cshtml` and `.cshtml.cs` with search form and results table (Name, Mobile, Email, Status)
    - Implement search by partial Name, Mobile, or Email (case-insensitive)
    - Display "No results found" message when search returns empty
    - _Requirements: 7.1, 7.2, 7.3_

  - [ ] 10.2 Create Receptionist Guest Details page
    - Create `Pages/Receptionist/Guests/Details.cshtml` and `.cshtml.cs`
    - Display guest info (Name, Mobile, Email, ReferredByName, ReferredByPhone, Status)
    - Display function history (FunctionDate, FunctionType, MealPlan, GuestAddress, InitiatedBy)
    - Display initiating manager and current follow-up manager
    - Mask Aadhaar numbers showing only last 4 digits (XXXXXXXX1234)
    - Ensure no edit/create actions are available
    - _Requirements: 7.1, 7.4, 7.5, 7.6, 7.7, 7.8, 7.9_

- [ ] 11. Checkpoint - Core features verification
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 12. Implement Admin dashboard and user management
  - [ ] 12.1 Create Admin Dashboard page
    - Create `Pages/Admin/Index.cshtml` and `.cshtml.cs` with overview cards (total guests, active leads, revenue summary)
    - Include navigation links to Users, Reports, Leads, Activity Logs
    - _Requirements: 9.3_

  - [ ] 12.2 Create User Management pages
    - Create `Pages/Admin/Users/Index.cshtml` and `.cshtml.cs` listing all users with Username, FullName, Role, IsActive status
    - Create `Pages/Admin/Users/Create.cshtml` and `.cshtml.cs` with form for Username (unique, max 100), FullName (max 150), Role (Manager/Receptionist), Password (min 6 chars)
    - Create `Pages/Admin/Users/Edit.cshtml` and `.cshtml.cs` allowing edit of FullName, Role, IsActive, password reset; Username is read-only
    - Validate unique username on create, prevent self-deactivation
    - _Requirements: 9.1, 9.2, 9.6, 9.7, 9.8, 9.9_

  - [ ] 12.3 Create Lead Transfer page
    - Create `Pages/Admin/Leads/Transfer.cshtml` and `.cshtml.cs`
    - Display source Manager dropdown and target Manager dropdown (populated from active managers)
    - Validate source ≠ target, show error if no transferable leads exist
    - On submit, call FollowUpService.TransferLeadsAsync, display confirmation with count
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5_

- [ ] 13. Implement Admin reporting pages
  - [ ] 13.1 Create Revenue Report page
    - Create `Pages/Admin/Reports/Revenue.cshtml` and `.cshtml.cs`
    - Implement date filter controls: Today, This Week, This Month, Financial Year, Specific Date, Custom Range
    - Display total revenue, total bookings, daily breakdown table
    - Show empty state message when no data exists for selected period
    - _Requirements: 11.1, 11.7_

  - [ ] 13.2 Create Manager-Wise Report page
    - Create `Pages/Admin/Reports/ManagerWise.cshtml` and `.cshtml.cs`
    - Display per-manager: total revenue, follow-up count, daily activities count, lead conversion rate
    - Show which manager initiated each guest and which currently follows
    - _Requirements: 11.2, 11.3, 11.5, 11.6_

  - [ ] 13.3 Create Follow-Up Monitor page
    - Create `Pages/Admin/Reports/FollowUpMonitor.cshtml` and `.cshtml.cs`
    - Show follow-ups updated per manager on selected date
    - List guests contacted with current follow-up statuses
    - Display upcoming follow-ups within next 7 days
    - _Requirements: 11.4_

- [ ] 14. Implement Activity Log viewer
  - [ ] 14.1 Create Activity Log page
    - Create `Pages/Admin/ActivityLogs/Index.cshtml` and `.cshtml.cs`
    - Display logs in table sortable by timestamp
    - Add filter controls for user, action type (Create/Update/Transfer), entity type (Guest/GuestFunction/Payment/FollowUp)
    - Ensure logs are read-only (no edit/delete actions)
    - _Requirements: 13.6, 13.7, 13.8_

- [ ] 15. Implement Manager performance report
  - [ ] 15.1 Create Manager My Performance page
    - Create `Pages/Manager/Reports/MyPerformance.cshtml` and `.cshtml.cs`
    - Display manager's own follow-up count, leads with Success status, total revenue from payments on assigned guests
    - Restrict to only the authenticated manager's data
    - _Requirements: 8.5, 8.7_

- [ ] 16. Final checkpoint - Full system verification
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- Unit tests validate specific examples and edge cases
- The existing MySQL database schema is used as-is; no EF Core migrations are applied
- All AJAX endpoints return consistent JSON format: `{ success: bool, data?: object, errors?: object }`
- Bootstrap 5 is already available in `wwwroot/lib/bootstrap` and jQuery in `wwwroot/lib/jquery`

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2"] },
    { "id": 2, "tasks": ["1.3"] },
    { "id": 3, "tasks": ["1.4", "2.1"] },
    { "id": 4, "tasks": ["2.2"] },
    { "id": 5, "tasks": ["2.3", "3.1"] },
    { "id": 6, "tasks": ["3.2", "3.3", "3.11", "3.14"] },
    { "id": 7, "tasks": ["3.4", "3.5", "3.15"] },
    { "id": 8, "tasks": ["3.6", "3.7"] },
    { "id": 9, "tasks": ["3.8", "3.9"] },
    { "id": 10, "tasks": ["3.10", "3.12"] },
    { "id": 11, "tasks": ["3.13", "3.16"] },
    { "id": 12, "tasks": ["5.1"] },
    { "id": 13, "tasks": ["5.2", "5.3", "5.4"] },
    { "id": 14, "tasks": ["6.1"] },
    { "id": 15, "tasks": ["6.2", "7.1", "8.3"] },
    { "id": 16, "tasks": ["7.2", "8.1", "8.2"] },
    { "id": 17, "tasks": ["9.1", "9.2"] },
    { "id": 18, "tasks": ["9.3", "10.1"] },
    { "id": 19, "tasks": ["10.2", "12.1"] },
    { "id": 20, "tasks": ["12.2", "12.3"] },
    { "id": 21, "tasks": ["13.1", "13.2", "13.3"] },
    { "id": 22, "tasks": ["14.1", "15.1"] }
  ]
}
```
