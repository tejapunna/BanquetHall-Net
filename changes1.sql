-- Changes for Lead Management Restructuring
USE banquethall;

-- New table: FunctionNames (dropdown values)
CREATE TABLE IF NOT EXISTS FunctionNames (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    SortOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE
);

-- Seed FunctionNames
INSERT INTO FunctionNames (Name, SortOrder, IsActive) VALUES
('Birthday', 1, TRUE),
('Reception', 2, TRUE),
('Half-Saree Function', 3, TRUE),
('Engagement', 4, TRUE),
('Get-Together', 5, TRUE),
('Doti Function', 6, TRUE),
('Marriage', 7, TRUE);

-- New table: FunctionHalls (dropdown values)
CREATE TABLE IF NOT EXISTS FunctionHalls (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    SortOrder INT DEFAULT 0,
    IsActive BOOLEAN DEFAULT TRUE
);

-- Seed FunctionHalls
INSERT INTO FunctionHalls (Name, SortOrder, IsActive) VALUES
('Main Hall', 1, TRUE),
('Mini Hall', 2, TRUE),
('Garden Area', 3, TRUE),
('Terrace', 4, TRUE);

-- Add new columns to Guests table
ALTER TABLE Guests ADD COLUMN FirstName VARCHAR(100) AFTER Name;
ALTER TABLE Guests ADD COLUMN LastName VARCHAR(100) AFTER FirstName;
ALTER TABLE Guests ADD COLUMN Village VARCHAR(150) AFTER Email;
ALTER TABLE Guests ADD COLUMN Mandal VARCHAR(150) AFTER Village;
ALTER TABLE Guests ADD COLUMN GuestAadhaar VARCHAR(20) AFTER Mandal;
ALTER TABLE Guests ADD COLUMN GuestPan VARCHAR(20) AFTER GuestAadhaar;
ALTER TABLE Guests ADD COLUMN Remarks TEXT AFTER GuestPan;

-- Add new columns to GuestFunctions table
ALTER TABLE GuestFunctions ADD COLUMN FunctionNameId INT AFTER FunctionType;
ALTER TABLE GuestFunctions ADD COLUMN MealType VARCHAR(20) AFTER MealPlan;
ALTER TABLE GuestFunctions ADD COLUMN NoOfPacks INT DEFAULT 0 AFTER MealType;
ALTER TABLE GuestFunctions ADD COLUMN GuaranteedPacks INT DEFAULT 0 AFTER NoOfPacks;
ALTER TABLE GuestFunctions ADD COLUMN SpecialInstruction TEXT AFTER GuaranteedPacks;
ALTER TABLE GuestFunctions ADD COLUMN AssignedManagerId INT AFTER SpecialInstruction;
ALTER TABLE GuestFunctions ADD COLUMN FunctionHallId INT AFTER AssignedManagerId;

-- Update existing Guest records: split Name into FirstName/LastName
UPDATE Guests SET FirstName = SUBSTRING_INDEX(Name, ' ', 1), LastName = SUBSTRING_INDEX(Name, ' ', -1) WHERE FirstName IS NULL;
