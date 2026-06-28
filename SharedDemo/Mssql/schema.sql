-- ======================================================================
-- MSSQL Demo Schema
-- DDL script for creating demo tables
-- ======================================================================

-- Departments table (no dependencies)
CREATE TABLE [dbo].[Departments] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Guid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [Code] NVARCHAR(100) NOT NULL,
    [Location] NVARCHAR(255) NOT NULL,
    [Budget] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [HeadCount] BIGINT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [Description] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [ParentDepartmentId] BIGINT NULL,
    [ManagerId] BIGINT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    CONSTRAINT [FK_Departments_Parent] FOREIGN KEY ([ParentDepartmentId]) REFERENCES [Departments]([Id])
);

-- Roles table (no dependencies)
CREATE TABLE [dbo].[Roles] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Guid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name] NVARCHAR(100) NOT NULL,
    [Level] BIGINT NOT NULL,
    [BaseSalary] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [BonusRate] FLOAT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CanApproveBudget] BIT NOT NULL DEFAULT 0,
    [Description] NVARCHAR(MAX) NOT NULL DEFAULT '',
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL
);

-- Users table (depends on Departments, Roles)
CREATE TABLE [dbo].[Users] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Guid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [Age] BIGINT NOT NULL,
    [Salary] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Rating] FLOAT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DepartmentId] BIGINT NOT NULL,
    [ManagerId] BIGINT NULL,
    [RoleId] BIGINT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    CONSTRAINT [FK_Users_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments]([Id]),
    CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]),
    CONSTRAINT [FK_Users_Manager] FOREIGN KEY ([ManagerId]) REFERENCES [Users]([Id])
);

-- Projects table (depends on Departments, Users)
CREATE TABLE [dbo].[Projects] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [Guid] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Name] NVARCHAR(255) NOT NULL,
    [Code] NVARCHAR(100) NOT NULL,
    [OwnerId] BIGINT NOT NULL,
    [DepartmentId] BIGINT NOT NULL,
    [Budget] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Progress] FLOAT NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [StartDate] DATETIME2 NOT NULL,
    [EndDate] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    CONSTRAINT [FK_Projects_Departments] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments]([Id]),
    CONSTRAINT [FK_Projects_Owner] FOREIGN KEY ([OwnerId]) REFERENCES [Users]([Id])
);

-- UserProjects junction table (depends on Users, Projects)
CREATE TABLE [dbo].[UserProjects] (
    [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [UserId] BIGINT NOT NULL,
    [ProjectId] BIGINT NOT NULL,
    [Role] NVARCHAR(100) NOT NULL,
    [Allocation] FLOAT NOT NULL DEFAULT 0,
    [HourlyRate] DECIMAL(18,2) NOT NULL DEFAULT 0,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [AssignedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [RemovedAt] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [UpdatedAt] DATETIME2 NULL,
    
    CONSTRAINT [FK_UserProjects_User] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]),
    CONSTRAINT [FK_UserProjects_Project] FOREIGN KEY ([ProjectId]) REFERENCES [Projects]([Id])
);
