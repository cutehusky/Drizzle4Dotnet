-- ======================================================================
-- MSSQL Demo Data
-- Sample data for demo tables
-- ======================================================================

-- Departments
INSERT INTO [dbo].[Departments] ([Guid], [Name], [Code], [Location], [Budget], [HeadCount], [IsActive], [Description], [ManagerId])
VALUES 
    (NEWID(), 'Engineering', 'ENG', 'Building A, Floor 3', 500000.00, 50, 1, 'Software engineering and development', 1),
    (NEWID(), 'Marketing', 'MKT', 'Building B, Floor 2', 300000.00, 20, 1, 'Marketing and brand management', 2),
    (NEWID(), 'Sales', 'SAL', 'Building B, Floor 1', 400000.00, 30, 1, 'Sales and business development', 3),
    (NEWID(), 'Human Resources', 'HR', 'Building A, Floor 1', 150000.00, 10, 1, 'HR and personnel management', 4),
    (NEWID(), 'Finance', 'FIN', 'Building A, Floor 2', 250000.00, 15, 1, 'Financial management and accounting', 5);

-- Roles
INSERT INTO [dbo].[Roles] ([Guid], [Name], [Level], [BaseSalary], [BonusRate], [IsActive], [CanApproveBudget], [Description])
VALUES 
    (NEWID(), 'Software Engineer', 3, 80000.00, 0.10, 1, 0, 'Develops and maintains software'),
    (NEWID(), 'Senior Software Engineer', 4, 120000.00, 0.15, 1, 0, 'Leads technical projects'),
    (NEWID(), 'Marketing Specialist', 3, 65000.00, 0.08, 1, 0, 'Executes marketing campaigns'),
    (NEWID(), 'Sales Manager', 4, 100000.00, 0.20, 1, 0, 'Manages sales team'),
    (NEWID(), 'HR Coordinator', 3, 55000.00, 0.05, 1, 0, 'Coordinates HR activities'),
    (NEWID(), 'CFO', 6, 200000.00, 0.25, 1, 1, 'Chief Financial Officer'),
    (NEWID(), 'CTO', 6, 220000.00, 0.25, 1, 1, 'Chief Technology Officer'),
    (NEWID(), 'DevOps Engineer', 3, 90000.00, 0.10, 1, 0, 'Manages infrastructure and deployments');

-- Users (self-referencing ManagerId, need to insert without ManagerId first)
INSERT INTO [dbo].[Users] ([Guid], [Name], [Email], [Age], [Salary], [Rating], [IsActive], [DepartmentId], [ManagerId], [RoleId])
VALUES 
    -- Department managers
    (NEWID(), 'Alice Johnson', 'alice@company.com', 35, 150000.00, 4.8, 1, 1, NULL, 7),  -- CTO, Engineering
    (NEWID(), 'Bob Smith', 'bob@company.com', 40, 130000.00, 4.5, 1, 2, NULL, 4),     -- Sales Manager, Marketing
    (NEWID(), 'Charlie Brown', 'charlie@company.com', 38, 140000.00, 4.6, 1, 3, NULL, 4), -- Sales Manager, Sales
    (NEWID(), 'Diana Prince', 'diana@company.com', 45, 200000.00, 4.9, 1, 5, NULL, 6), -- CFO, Finance
    (NEWID(), 'Eve Davis', 'eve@company.com', 32, 65000.00, 4.2, 1, 4, NULL, 5);      -- HR Coordinator

-- Update ManagerId for departments
UPDATE [dbo].[Departments] SET [ManagerId] = 1 WHERE [Id] = 1;
UPDATE [dbo].[Departments] SET [ManagerId] = 2 WHERE [Id] = 2;
UPDATE [dbo].[Departments] SET [ManagerId] = 3 WHERE [Id] = 3;
UPDATE [dbo].[Departments] SET [ManagerId] = 5 WHERE [Id] = 4;
UPDATE [dbo].[Departments] SET [ManagerId] = 4 WHERE [Id] = 5;

-- Regular users with managers
INSERT INTO [dbo].[Users] ([Guid], [Name], [Email], [Age], [Salary], [Rating], [IsActive], [DepartmentId], [ManagerId], [RoleId])
VALUES 
    (NEWID(), 'Frank Miller', 'frank@company.com', 28, 95000.00, 4.3, 1, 1, 1, 2),    -- Senior Engineer
    (NEWID(), 'Grace Lee', 'grace@company.com', 25, 80000.00, 4.1, 1, 1, 1, 1),      -- Software Engineer
    (NEWID(), 'Henry Wilson', 'henry@company.com', 30, 90000.00, 4.4, 1, 1, 1, 8),   -- DevOps
    (NEWID(), 'Ivy Chen', 'ivy@company.com', 27, 85000.00, 4.0, 1, 1, 1, 1),         -- Software Engineer
    (NEWID(), 'Jack Taylor', 'jack@company.com', 29, 70000.00, 3.8, 1, 2, 2, 3),     -- Marketing Specialist
    (NEWID(), 'Karen White', 'karen@company.com', 31, 110000.00, 4.5, 1, 3, 3, 4),   -- Sales Manager
    (NEWID(), 'Leo Martinez', 'leo@company.com', 26, 60000.00, 3.9, 1, 4, 5, 5),     -- HR Coordinator
    (NEWID(), 'Mia Anderson', 'mia@company.com', 33, 95000.00, 4.2, 1, 5, 4, 6);     -- Finance

-- Projects
INSERT INTO [dbo].[Projects] ([Guid], [Name], [Code], [OwnerId], [DepartmentId], [Budget], [Progress], [IsActive], [StartDate], [EndDate])
VALUES 
    (NEWID(), 'Cloud Migration', 'CLD-2024', 1, 1, 200000.00, 0.75, 1, '2024-01-15', '2024-12-31'),
    (NEWID(), 'Mobile App v2', 'MOB-2024', 6, 1, 150000.00, 0.50, 1, '2024-03-01', '2025-06-30'),
    (NEWID(), 'Brand Refresh', 'BRD-2024', 10, 2, 80000.00, 0.30, 1, '2024-06-01', NULL),
    (NEWID(), 'CRM Integration', 'CRM-2024', 11, 3, 100000.00, 0.90, 1, '2024-02-01', '2024-10-31'),
    (NEWID(), 'HR Portal', 'HRP-2024', 12, 4, 50000.00, 0.60, 1, '2024-04-01', '2025-03-31');

-- UserProjects
INSERT INTO [dbo].[UserProjects] ([UserId], [ProjectId], [Role], [Allocation], [HourlyRate], [IsActive], [AssignedAt])
VALUES 
    (1, 1, 'Project Owner', 0.30, 0.00, 1, '2024-01-15'),
    (6, 1, 'Tech Lead', 0.50, 0.00, 1, '2024-01-15'),
    (7, 1, 'Developer', 0.80, 0.00, 1, '2024-01-15'),
    (8, 1, 'DevOps', 0.40, 0.00, 1, '2024-02-01'),
    (6, 2, 'Tech Lead', 0.50, 0.00, 1, '2024-03-01'),
    (9, 2, 'Developer', 0.80, 0.00, 1, '2024-03-01'),
    (10, 3, 'Marketing Lead', 0.60, 0.00, 1, '2024-06-01'),
    (11, 4, 'Sales Lead', 0.40, 0.00, 1, '2024-02-01'),
    (12, 5, 'HR Lead', 0.50, 0.00, 1, '2024-04-01'),
    (13, 5, 'Finance Advisor', 0.20, 0.00, 1, '2024-04-01');
