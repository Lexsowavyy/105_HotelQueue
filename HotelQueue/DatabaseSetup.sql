-- ============================================
-- Grand Azure Hotel - Database Setup Script
-- Run this script in SQL Server Management Studio (SSMS)
-- ============================================

-- Create Database
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HotelQueueDB')
BEGIN
    CREATE DATABASE HotelQueueDB;
END
GO

USE HotelQueueDB;
GO

-- Create Customers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE Customers (
        CustomerId INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(20) NOT NULL,
        Type NVARCHAR(20) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
    );
END
GO

-- Create Rooms Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms')
BEGIN
    CREATE TABLE Rooms (
        RoomId INT IDENTITY(1,1) PRIMARY KEY,
        RoomNumber NVARCHAR(10) NOT NULL,
        Type NVARCHAR(20) NOT NULL,
        PricePerNight DECIMAL(18,2) NOT NULL,
        IsAvailable BIT NOT NULL DEFAULT 1,
        Capacity INT NOT NULL
    );
END
GO

-- Create Reservations Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Reservations')
BEGIN
    CREATE TABLE Reservations (
        ReservationId NVARCHAR(50) PRIMARY KEY,
        CustomerId INT NOT NULL,
        RoomId INT NOT NULL,
        CheckInDate DATETIME NOT NULL,
        CheckOutDate DATETIME NOT NULL,
        Status NVARCHAR(20) NOT NULL,
        CustomerType NVARCHAR(20) NOT NULL,
        Priority INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        ConfirmedAt DATETIME NULL,
        CancelledAt DATETIME NULL,
        CONSTRAINT FK_Reservations_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_Reservations_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(RoomId)
    );
END
GO

-- Create Notifications Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE Notifications (
        NotificationId INT IDENTITY(1,1) PRIMARY KEY,
        ReservationId NVARCHAR(50) NULL,
        CustomerId INT NOT NULL,
        Message NVARCHAR(500) NOT NULL,
        Type NVARCHAR(30) NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        IsRead BIT NOT NULL DEFAULT 0
    );
END
GO

-- Create WaitlistEntries Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'WaitlistEntries')
BEGIN
    CREATE TABLE WaitlistEntries (
        WaitlistId INT IDENTITY(1,1) PRIMARY KEY,
        CustomerId INT NOT NULL,
        PreferredRoomType NVARCHAR(20) NOT NULL,
        PreferredCheckIn DATETIME NOT NULL,
        CustomerType NVARCHAR(20) NOT NULL,
        Priority INT NOT NULL,
        AddedAt DATETIME NOT NULL DEFAULT GETDATE(),
        IsPromoted BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_WaitlistEntries_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
    );
END
GO

-- Create Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reservations_CustomerId')
    CREATE INDEX IX_Reservations_CustomerId ON Reservations(CustomerId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reservations_RoomId')
    CREATE INDEX IX_Reservations_RoomId ON Reservations(RoomId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reservations_Status')
    CREATE INDEX IX_Reservations_Status ON Reservations(Status);
GO

-- Seed Data - Rooms (Prices in Philippine Peso)
IF NOT EXISTS (SELECT * FROM Rooms)
BEGIN
    INSERT INTO Rooms (RoomNumber, Type, PricePerNight, IsAvailable, Capacity) VALUES
    ('101', 'Standard', 5600.00, 1, 2),
    ('102', 'Standard', 5600.00, 1, 2),
    ('103', 'Standard', 5600.00, 1, 2),
    ('104', 'Standard', 5600.00, 1, 3),
    ('105', 'Standard', 5600.00, 1, 2),
    ('201', 'Deluxe', 11200.00, 1, 2),
    ('202', 'Deluxe', 11200.00, 1, 2),
    ('203', 'Deluxe', 11200.00, 1, 3),
    ('204', 'Deluxe', 11200.00, 1, 2),
    ('205', 'Deluxe', 11200.00, 1, 3),
    ('301', 'Suite', 19600.00, 1, 4),
    ('302', 'Suite', 19600.00, 1, 4),
    ('303', 'Suite', 19600.00, 1, 4),
    ('304', 'Suite', 22400.00, 1, 4),
    ('305', 'Suite', 22400.00, 1, 4);
END
GO

PRINT 'Database setup completed successfully!';
PRINT 'HotelQueueDB has been created with all tables and seed data.';
GO
