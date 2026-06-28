-- =========================
-- SQLite Schema for SharedDemo
-- Uses SQLite-compatible types: INTEGER, TEXT, REAL, NUMERIC, BLOB
-- SQLite uses double-quoted identifiers like PostgreSQL
-- =========================

PRAGMA foreign_keys = ON;

-- =========================
-- USERS
-- =========================
CREATE TABLE "Users" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Guid" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Age" INTEGER NOT NULL,
    "Salary" NUMERIC NOT NULL DEFAULT 0,
    "Rating" REAL NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "DepartmentId" INTEGER NOT NULL,
    "ManagerId" INTEGER,
    "RoleId" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT
);

-- =========================
-- DEPARTMENTS
-- =========================
CREATE TABLE "Departments" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Guid" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Code" TEXT NOT NULL,
    "Location" TEXT NOT NULL,
    "Budget" NUMERIC NOT NULL DEFAULT 0,
    "HeadCount" INTEGER NOT NULL DEFAULT 0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT,
    "Description" TEXT NOT NULL,
    "ParentDepartmentId" INTEGER,
    "ManagerId" INTEGER NOT NULL
);

-- =========================
-- ROLES
-- =========================
CREATE TABLE "Roles" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Guid" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Level" INTEGER NOT NULL,
    "BaseSalary" NUMERIC NOT NULL DEFAULT 0,
    "BonusRate" REAL NOT NULL DEFAULT 0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "CanApproveBudget" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT,
    "Description" TEXT NOT NULL
);

-- =========================
-- PROJECTS
-- =========================
CREATE TABLE "Projects" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "Guid" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Code" TEXT NOT NULL,
    "OwnerId" INTEGER NOT NULL,
    "DepartmentId" INTEGER NOT NULL,
    "Budget" NUMERIC NOT NULL DEFAULT 0,
    "Progress" REAL NOT NULL DEFAULT 0,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "StartDate" TEXT NOT NULL,
    "EndDate" TEXT,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT
);

-- =========================
-- USER PROJECTS
-- =========================
CREATE TABLE "UserProjects" (
    "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
    "UserId" INTEGER NOT NULL,
    "ProjectId" INTEGER NOT NULL,
    "Role" TEXT NOT NULL,
    "Allocation" REAL NOT NULL,
    "HourlyRate" NUMERIC NOT NULL,
    "IsActive" INTEGER NOT NULL DEFAULT 1,
    "AssignedAt" TEXT NOT NULL,
    "RemovedAt" TEXT,
    "CreatedAt" TEXT NOT NULL DEFAULT (datetime('now')),
    "UpdatedAt" TEXT
);

-- =========================
-- INDEXES
-- =========================
CREATE INDEX "IX_Users_DepartmentId" ON "Users"("DepartmentId");
CREATE INDEX "IX_Users_RoleId" ON "Users"("RoleId");
CREATE INDEX "IX_Users_ManagerId" ON "Users"("ManagerId");

CREATE INDEX "IX_Projects_DepartmentId" ON "Projects"("DepartmentId");
CREATE INDEX "IX_Projects_OwnerId" ON "Projects"("OwnerId");

CREATE INDEX "IX_UserProjects_UserId" ON "UserProjects"("UserId");
CREATE INDEX "IX_UserProjects_ProjectId" ON "UserProjects"("ProjectId");
