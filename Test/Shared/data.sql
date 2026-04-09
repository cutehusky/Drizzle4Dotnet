CREATE EXTENSION IF NOT EXISTS pgcrypto;
       
-- =========================
-- ROLES (10 rows)
-- =========================
INSERT INTO "Roles" (
    "Id","Guid","Name","Level","BaseSalary","BonusRate",
    "IsActive","CanApproveBudget","CreatedAt","UpdatedAt","Description"
)
SELECT
    i,
    gen_random_uuid(),
    'Role ' || i,
    (random()*10)::int,
    (3000 + random()*7000)::numeric(18,2),
    random(),
    true,
    (random() > 0.5),
    NOW(),
    NULL,
    'Role description ' || i
FROM generate_series(1, 10) s(i);

-- =========================
-- DEPARTMENTS (20 rows)
-- =========================
INSERT INTO "Departments" (
    "Id","Guid","Name","Code","Location","Budget","HeadCount",
    "IsActive","CreatedAt","UpdatedAt","Description",
    "ParentDepartmentId","ManagerId"
)
SELECT
    i,
    gen_random_uuid(),
    'Department ' || i,
    'DPT-' || i,
    'Location ' || i,
    (100000 + random()*900000)::numeric(18,2),
    (5 + random()*50)::int,
    true,
    NOW(),
    NULL,
    'Department desc ' || i,
    CASE WHEN i > 5 THEN (random()*5)::int + 1 ELSE NULL END,
    NULL  -- temp, fix later
FROM generate_series(1, 20) s(i);

-- =========================
-- USERS (1000 rows)
-- =========================
INSERT INTO "Users" (
    "Id","Guid","Name","Email","Age","Salary","Rating","IsActive",
    "DepartmentId","ManagerId","RoleId","CreatedAt","UpdatedAt"
)
SELECT
    i,
    gen_random_uuid(),
    'User ' || i,
    'user' || i || '@example.com',
    (18 + random()*40)::int,
    (2000 + random()*8000)::numeric(18,2),
    random()*5,
    (random() > 0.1),
    (random()*19)::int + 1,
    CASE WHEN i > 10 THEN (random()*10)::int + 1 ELSE NULL END,
    (random()*9)::int + 1,
    NOW(),
    NULL
FROM generate_series(1, 1000) s(i);

-- =========================
-- FIX Department.ManagerId (after users exist)
-- =========================
UPDATE "Departments"
SET "ManagerId" = (random()*999)::int + 1;

-- =========================
-- PROJECTS (200 rows)
-- =========================
INSERT INTO "Projects" (
    "Id","Guid","Name","Code","OwnerId","DepartmentId",
    "Budget","Progress","IsActive",
    "StartDate","EndDate","CreatedAt","UpdatedAt"
)
SELECT
    i,
    gen_random_uuid(),
    'Project ' || i,
    'PRJ-' || i,
    (random()*999)::int + 1,
    (random()*19)::int + 1,
    (50000 + random()*500000)::numeric(18,2),
    random()*100,
    (random() > 0.2),
    NOW() - (random()*365 || ' days')::interval,
    CASE WHEN random() > 0.5 THEN NOW() ELSE NULL END,
    NOW(),
    NULL
FROM generate_series(1, 200) s(i);

-- =========================
-- USER PROJECTS (~1000 rows)
-- =========================
INSERT INTO "UserProjects" (
    "Id","UserId","ProjectId","Role","Allocation",
    "HourlyRate","IsActive","AssignedAt","RemovedAt",
    "CreatedAt","UpdatedAt"
)
SELECT
    i,
    (random()*999)::int + 1,
    (random()*199)::int + 1,
    'Dev',
    random(),
    (20 + random()*100)::numeric(18,2),
    (random() > 0.2),
    NOW() - (random()*200 || ' days')::interval,
    CASE WHEN random() > 0.7 THEN NOW() ELSE NULL END,
    NOW(),
    NULL
FROM generate_series(1, 1000) s(i);