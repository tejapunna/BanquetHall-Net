-- Banquet Hall Management System - Database Setup
-- Run this script against your MySQL server to create all tables and seed initial data

CREATE DATABASE IF NOT EXISTS banquethall;
USE banquethall;

-- ============================================
-- TABLE CREATION
-- ============================================

CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL,
    PasswordHash TEXT NOT NULL,
    FullName VARCHAR(150) NOT NULL,
    Role VARCHAR(50) NOT NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Guests (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    Name VARCHAR(150) NOT NULL,
    Mobile VARCHAR(20) NOT NULL,
    Email VARCHAR(150),
    ReferredByName VARCHAR(150),
    ReferredByPhone VARCHAR(20),
    Status VARCHAR(50),
    InitiatedByManagerId INT,
    FOREIGN KEY (InitiatedByManagerId) REFERENCES Users(Id)
);

CREATE TABLE GuestFunctions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GuestId INT NOT NULL,
    FunctionDate DATETIME NOT NULL,
    FunctionType VARCHAR(100) NOT NULL,
    MealPlan VARCHAR(150) NOT NULL,
    InitiatedBy VARCHAR(150),
    GuestAddress TEXT,
    GuestAadhaar VARCHAR(20),
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GuestId) REFERENCES Guests(Id)
);

CREATE TABLE Payments (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GuestId INT NOT NULL,
    FunctionId INT NOT NULL,
    PaymentType VARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    NoOfPacks INT NOT NULL,
    GuaranteedPacks INT NOT NULL,
    PricePerPack DECIMAL(18,2) NOT NULL,
    AdvancePaid BOOLEAN DEFAULT FALSE,
    AdvanceAmount DECIMAL(18,2) DEFAULT 0,
    RemainingAmount DECIMAL(18,2) DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GuestId) REFERENCES Guests(Id),
    FOREIGN KEY (FunctionId) REFERENCES GuestFunctions(Id)
);

CREATE TABLE FollowUps (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GuestId INT NOT NULL,
    ManagerId INT NOT NULL,
    Status VARCHAR(50) NOT NULL,
    FollowupStatus VARCHAR(50) NOT NULL,
    FollowupDate DATETIME NOT NULL,
    Remarks TEXT,
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (GuestId) REFERENCES Guests(Id),
    FOREIGN KEY (ManagerId) REFERENCES Users(Id)
);

CREATE TABLE ActivityLogs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Username VARCHAR(100) NOT NULL,
    ActionType VARCHAR(50) NOT NULL,
    EntityType VARCHAR(50) NOT NULL,
    EntityId INT,
    Details TEXT,
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- ============================================
-- SEED DATA
-- ============================================

-- Default Users
-- Password for all users: "password123"
-- BCrypt hash generated using BCrypt.Net-Next 4.2.0

INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive, CreatedAt) VALUES
('admin', '$2a$11$yyqtbegMKa1gXT7guh5d5OBLlx3A/sd43m4wmJ9sZlewwJAtoF9xe', 'System Admin', 'Admin', TRUE, NOW()),
('manager1', '$2a$11$yyqtbegMKa1gXT7guh5d5OBLlx3A/sd43m4wmJ9sZlewwJAtoF9xe', 'Rajesh Kumar', 'Manager', TRUE, NOW()),
('manager2', '$2a$11$yyqtbegMKa1gXT7guh5d5OBLlx3A/sd43m4wmJ9sZlewwJAtoF9xe', 'Priya Sharma', 'Manager', TRUE, NOW()),
('reception1', '$2a$11$yyqtbegMKa1gXT7guh5d5OBLlx3A/sd43m4wmJ9sZlewwJAtoF9xe', 'Anita Reddy', 'Receptionist', TRUE, NOW());

-- Sample Guests (created by manager1, Id=2)
INSERT INTO Guests (Name, Mobile, Email, ReferredByName, ReferredByPhone, Status, InitiatedByManagerId, CreatedAt) VALUES
('Venkat Rao', '9876543210', 'venkat@email.com', 'Suresh', '9988776655', 'VIP', 2, NOW()),
('Lakshmi Devi', '8765432109', 'lakshmi@email.com', NULL, NULL, 'Normal', 2, NOW()),
('Ramesh Babu', '7654321098', 'ramesh@email.com', 'Mahesh', '9123456789', 'VVIP', 3, NOW()),
('Sita Kumari', '6543210987', NULL, 'Geetha', '9234567890', 'Normal', 2, NOW()),
('Anil Kumar', '9012345678', 'anil@email.com', NULL, NULL, 'VIP', 3, NOW());

-- Sample Guest Functions
INSERT INTO GuestFunctions (GuestId, FunctionDate, FunctionType, MealPlan, InitiatedBy, GuestAddress, GuestAadhaar, CreatedAt) VALUES
(1, '2026-06-15 10:00:00', 'Wedding', 'Veg Lunch', 'Rajesh Kumar', '123 MG Road, Hyderabad', '123456789012', NOW()),
(1, '2026-06-16 18:00:00', 'Reception', 'NonVeg Dinner', 'Rajesh Kumar', '123 MG Road, Hyderabad', '123456789012', NOW()),
(2, '2026-07-20 11:00:00', 'Wedding', 'Veg Breakfast', 'Rajesh Kumar', '456 Tank Bund, Hyderabad', '234567890123', NOW()),
(3, '2026-08-10 12:00:00', 'Reception', 'NonVeg Lunch', 'Priya Sharma', '789 Jubilee Hills, Hyderabad', '345678901234', NOW()),
(4, '2026-09-05 10:00:00', 'Wedding', 'Veg Dinner', 'Rajesh Kumar', '321 Banjara Hills, Hyderabad', '456789012345', NOW());

-- Sample Payments
INSERT INTO Payments (GuestId, FunctionId, PaymentType, Amount, NoOfPacks, GuaranteedPacks, PricePerPack, AdvancePaid, AdvanceAmount, RemainingAmount, CreatedAt) VALUES
(1, 1, 'UPI', 150000.00, 300, 300, 500.00, TRUE, 50000.00, 100000.00, NOW()),
(1, 2, 'Cash', 200000.00, 400, 400, 500.00, TRUE, 75000.00, 125000.00, NOW()),
(2, 3, 'Card', 100000.00, 200, 200, 500.00, TRUE, 30000.00, 70000.00, NOW()),
(3, 4, 'UPI', 180000.00, 300, 300, 600.00, FALSE, 0.00, 180000.00, NOW()),
(4, 5, 'Cash', 120000.00, 200, 200, 600.00, TRUE, 40000.00, 80000.00, NOW());

-- Sample Follow-Ups
INSERT INTO FollowUps (GuestId, ManagerId, Status, FollowupStatus, FollowupDate, Remarks, UpdatedAt) VALUES
(1, 2, 'In Progress', 'May Close', '2026-05-20 00:00:00', 'Guest confirmed venue visit', NOW()),
(2, 2, 'New', 'May Close', '2026-05-25 00:00:00', 'Initial inquiry, interested in July dates', NOW()),
(3, 3, 'Followup', 'May Not Close', '2026-05-18 00:00:00', 'Comparing with other venues', NOW()),
(4, 2, 'In Progress', 'May Close', '2026-05-22 00:00:00', 'Budget discussion pending', NOW()),
(5, 3, 'New', 'May Close', '2026-05-30 00:00:00', 'Referred by existing client', NOW());

-- Sample Activity Logs
INSERT INTO ActivityLogs (UserId, Username, ActionType, EntityType, EntityId, Details, Timestamp) VALUES
(2, 'manager1', 'Create', 'Guest', 1, 'Guest ''Venkat Rao'' created', NOW()),
(2, 'manager1', 'Create', 'Guest', 2, 'Guest ''Lakshmi Devi'' created', NOW()),
(3, 'manager2', 'Create', 'Guest', 3, 'Guest ''Ramesh Babu'' created', NOW()),
(2, 'manager1', 'Create', 'GuestFunction', 1, 'Function ''Wedding'' created for guest 1', NOW()),
(2, 'manager1', 'Create', 'Payment', 1, 'Payment of 150000.00 created for guest 1', NOW());
