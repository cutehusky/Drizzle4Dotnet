-- =========================
-- USERS
-- =========================
CREATE TABLE "Users" (
                         "Id" INTEGER PRIMARY KEY,
                         "Guid" UUID NOT NULL,
                         "Name" TEXT NOT NULL,
                         "Email" TEXT NOT NULL,
                         "Age" INTEGER NOT NULL,
                         "Salary" NUMERIC(18,2) NOT NULL,
                         "Rating" DOUBLE PRECISION NOT NULL,
                         "IsActive" BOOLEAN NOT NULL,
                         "DepartmentId" INTEGER NOT NULL,
                         "ManagerId" INTEGER NULL,
                         "RoleId" INTEGER NOT NULL,
                         "CreatedAt" TIMESTAMP NOT NULL,
                         "UpdatedAt" TIMESTAMP NULL
);

-- =========================
-- DEPARTMENTS
-- =========================
CREATE TABLE "Departments" (
                               "Id" INTEGER PRIMARY KEY,
                               "Guid" UUID NOT NULL,
                               "Name" TEXT NOT NULL,
                               "Code" TEXT NOT NULL,
                               "Location" TEXT NOT NULL,
                               "Budget" NUMERIC(18,2) NOT NULL,
                               "HeadCount" INTEGER NOT NULL,
                               "IsActive" BOOLEAN NOT NULL,
                               "CreatedAt" TIMESTAMP NOT NULL,
                               "UpdatedAt" TIMESTAMP NULL,
                               "Description" TEXT NOT NULL,
                               "ParentDepartmentId" INTEGER NULL,
                               "ManagerId" INTEGER
);

-- =========================
-- ROLES
-- =========================
CREATE TABLE "Roles" (
                         "Id" INTEGER PRIMARY KEY,
                         "Guid" UUID NOT NULL,
                         "Name" TEXT NOT NULL,
                         "Level" INTEGER NOT NULL,
                         "BaseSalary" NUMERIC(18,2) NOT NULL,
                         "BonusRate" DOUBLE PRECISION NOT NULL,
                         "IsActive" BOOLEAN NOT NULL,
                         "CanApproveBudget" BOOLEAN NOT NULL,
                         "CreatedAt" TIMESTAMP NOT NULL,
                         "UpdatedAt" TIMESTAMP NULL,
                         "Description" TEXT NOT NULL
);

-- =========================
-- PROJECTS
-- =========================
CREATE TABLE "Projects" (
                            "Id" INTEGER PRIMARY KEY,
                            "Guid" UUID NOT NULL,
                            "Name" TEXT NOT NULL,
                            "Code" TEXT NOT NULL,
                            "OwnerId" INTEGER NOT NULL,
                            "DepartmentId" INTEGER NOT NULL,
                            "Budget" NUMERIC(18,2) NOT NULL,
                            "Progress" DOUBLE PRECISION NOT NULL,
                            "IsActive" BOOLEAN NOT NULL,
                            "StartDate" TIMESTAMP NOT NULL,
                            "EndDate" TIMESTAMP NULL,
                            "CreatedAt" TIMESTAMP NOT NULL,
                            "UpdatedAt" TIMESTAMP NULL
);

-- =========================
-- USER PROJECTS
-- =========================
CREATE TABLE "UserProjects" (
                                "Id" INTEGER PRIMARY KEY,
                                "UserId" INTEGER NOT NULL,
                                "ProjectId" INTEGER NOT NULL,
                                "Role" TEXT NOT NULL,
                                "Allocation" DOUBLE PRECISION NOT NULL,
                                "HourlyRate" NUMERIC(18,2) NOT NULL,
                                "IsActive" BOOLEAN NOT NULL,
                                "AssignedAt" TIMESTAMP NOT NULL,
                                "RemovedAt" TIMESTAMP NULL,
                                "CreatedAt" TIMESTAMP NOT NULL,
                                "UpdatedAt" TIMESTAMP NULL
);

-- =========================
-- FOREIGN KEYS
-- =========================

ALTER TABLE "Users"
    ADD CONSTRAINT "FK_Users_Departments"
        FOREIGN KEY ("DepartmentId") REFERENCES "Departments"("Id");

ALTER TABLE "Users"
    ADD CONSTRAINT "FK_Users_Roles"
        FOREIGN KEY ("RoleId") REFERENCES "Roles"("Id");

ALTER TABLE "Users"
    ADD CONSTRAINT "FK_Users_Manager"
        FOREIGN KEY ("ManagerId") REFERENCES "Users"("Id");

ALTER TABLE "Departments"
    ADD CONSTRAINT "FK_Departments_Parent"
        FOREIGN KEY ("ParentDepartmentId") REFERENCES "Departments"("Id");

ALTER TABLE "Departments"
    ADD CONSTRAINT "FK_Departments_Manager"
        FOREIGN KEY ("ManagerId") REFERENCES "Users"("Id");

ALTER TABLE "Projects"
    ADD CONSTRAINT "FK_Projects_Departments"
        FOREIGN KEY ("DepartmentId") REFERENCES "Departments"("Id");

ALTER TABLE "Projects"
    ADD CONSTRAINT "FK_Projects_Owner"
        FOREIGN KEY ("OwnerId") REFERENCES "Users"("Id");

ALTER TABLE "UserProjects"
    ADD CONSTRAINT "FK_UserProjects_User"
        FOREIGN KEY ("UserId") REFERENCES "Users"("Id");

ALTER TABLE "UserProjects"
    ADD CONSTRAINT "FK_UserProjects_Project"
        FOREIGN KEY ("ProjectId") REFERENCES "Projects"("Id");

-- =========================
-- INDEXES (recommended)
-- =========================

CREATE INDEX "IX_Users_DepartmentId" ON "Users"("DepartmentId");
CREATE INDEX "IX_Users_RoleId" ON "Users"("RoleId");
CREATE INDEX "IX_Users_ManagerId" ON "Users"("ManagerId");

CREATE INDEX "IX_Projects_DepartmentId" ON "Projects"("DepartmentId");
CREATE INDEX "IX_Projects_OwnerId" ON "Projects"("OwnerId");

CREATE INDEX "IX_UserProjects_UserId" ON "UserProjects"("UserId");
CREATE INDEX "IX_UserProjects_ProjectId" ON "UserProjects"("ProjectId");