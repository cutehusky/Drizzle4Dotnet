# Drizzle4Dotnet — Comprehensive Implementation Plan

> **Status:** Draft  
> **Last Updated:** 2026-06-22  
> **Target:** Production-ready ORM with PostgreSQL-first support, extensible to other dialects.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Phase 1: Operators & SQL Functions (Foundation)](#2-phase-1-operators--sql-functions)
3. [Phase 1b: Static `Sql` Utility Class](#2b-phase-1b-static-sql-utility-class)
4. [Phase 2: Window Functions](#3-phase-2-window-functions)
5. [Phase 3: Advanced DML (Insert/Update/Delete)](#4-phase-3-advanced-dml)
6. [Phase 3b: Recursive CTEs & Advanced Subqueries](#4b-phase-3b-recursive-ctes--advanced-subqueries)
7. [Phase 4: Set Operations & Compound Queries](#5-phase-4-set-operations)
8. [Phase 5: Convenience Execution Methods](#6-phase-5-convenience-execution-methods)
9. [Phase 6: Batch Operations & Streaming](#7-phase-6-batch-operations--streaming)
10. [Phase 7: Multi-Dialect Support](#8-phase-7-multi-dialect-support)
11. [Phase 8: Schema / Migration Management](#9-phase-8-schema--migration-management)
12. [Phase 9: Observability & Tooling](#10-phase-9-observability--tooling)
13. [Phase 10: Source Generator Enhancements](#11-phase-10-source-generator-enhancements)
14. [Phase 11: GitHub Issues & Reported Bugs](#12-phase-11-github-issues--reported-bugs)
15. [Appendix: Cross-Cutting Concerns](#13-appendix-cross-cutting-concerns)

---

## 1. Architecture Overview

### Existing Architecture (Current State)

```
┌─────────────────────────────────────────────────────┐
│                    DbClient<TDialect>                │
│  Select / Insert / Update / Delete / ExecuteGetList  │
└──────────┬──────────┬──────────┬──────────┬──────────┘
           │          │          │          │
     ┌─────▼──┐ ┌────▼───┐ ┌───▼────┐ ┌───▼────┐
     │Select  │ │Insert  │ │Update  │ │Delete  │
     │Query   │ │Query   │ │Query   │ │Query   │
     └────┬───┘ └────┬───┘ └───┬────┘ └───┬────┘
          │          │         │          │
     ┌────▼──────────▼─────────▼──────────▼────┐
     │           QueryBase<TDialect>            │
     │  BuildSql() / Build() / AppendClause()   │
     └────────────────┬─────────────────────────┘
                      │
     ┌────────────────▼─────────────────────────┐
     │         SqlBuilder<TDialect>              │
     │  ISqlBuilder: Append / AddParameter       │
     └───────────────────────────────────────────┘
```

### Target Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    DbClient<TDialect>                         │
│  Connection Mgmt / Transaction / Execution / Factory         │
└──┬──────────┬──────────┬──────────┬──────────┬────────────────┘
   │          │          │          │          │
   ├──────────┴──────────┴──────────┴──────────┤
   │           Query Builders                   │
   │  Select / Insert / Update / Delete / Merge │
   │  + Union / Intersect / Except              │
   └──────────────────┬────────────────────────┘
                      │
   ┌──────────────────▼────────────────────────┐
   │        SqlBuilder<TDialect> + Hooks        │
   │  Parameter handling / Logging / Events     │
   └──────────────────┬────────────────────────┘
                      │
   ┌──────────────────▼────────────────────────┐
   │           ISqlDialect                      │
   │  PgSql / MySql / Sqlite / SqlServer        │
   └────────────────────────────────────────────┘
```

### Layer Structure

| Layer | Path | Responsibility |
|-------|------|----------------|
| **Core** | [`Drizzle4Dotnet/src/Core/`](Drizzle4Dotnet/src/Core/) | Query builders, schema types, shared interfaces |
| **Dialect** | [`Drizzle4Dotnet/src/Dialect/`](Drizzle4Dotnet/src/Dialect/) | SQL dialect implementations |
| **Source Generators** | [`SourceGenerators/SourceGenerators/`](SourceGenerators/SourceGenerators/) | Roslyn generators for tables & selects |
| **Test** | [`Test/`](Test/) | NUnit test suites |
| **Shared Demo** | [`SharedDemo/`](SharedDemo/) | Shared schema & DTO definitions |
| **Benchmark** | [`Benchmark/`](Benchmark/) | Performance benchmarks |

---

## 2. Phase 1: Operators & SQL Functions

### 2.1 Missing Comparison & Logical Operators

| Feature | Priority | Status | File |
|---------|----------|--------|------|
| `IS DISTINCT FROM` / `IS NOT DISTINCT FROM` (null-safe equality) | High | ❌ Missing | [`Operators.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs) |
| `ALL` / `ANY` / `SOME` (subquery quantifiers) | Medium | ❌ Missing | [`Operators.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs) |

### 2.2 String Functions

| Function | Example SQL | Priority | Status |
|----------|-------------|----------|--------|
| `Upper()` | `UPPER(col)` | High | ❌ Missing |
| `Lower()` | `LOWER(col)` | High | ❌ Missing |
| `Trim()` | `TRIM(col)` | Medium | ❌ Missing |
| `LTrim()` / `RTrim()` | `LTRIM(col)` / `RTRIM(col)` | Low | ❌ Missing |
| `Length()` | `LENGTH(col)` / `CHAR_LENGTH(col)` | High | ❌ Missing |
| `Substring()` | `SUBSTRING(col FROM 1 FOR 3)` | Medium | ❌ Missing |
| `Replace()` | `REPLACE(col, 'a', 'b')` | Medium | ❌ Missing |
| `Position()` | `POSITION('sub' IN col)` | Low | ❌ Missing |
| `ConcatWs()` | `CONCAT_WS(',', col1, col2)` | Low | ❌ Missing |

**Implementation:** [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs) — Add these as static methods returning [`UnaryNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/UnaryNode.cs) or custom node types.

### 2.3 Numeric & Math Functions

| Function | Priority | Status |
|----------|----------|--------|
| `Abs()` | High | ❌ Missing |
| `Ceil()` / `Floor()` | High | ❌ Missing |
| `Round()` | High | ❌ Missing |
| `Power()` / `Sqrt()` | Medium | ❌ Missing |
| `Random()` | Low | ❌ Missing |
| `Sign()` | Low | ❌ Missing |

### 2.4 Date/Time Functions

| Function | Priority | Status |
|----------|----------|--------|
| `Extract(year/month/day FROM col)` | High | ❌ Missing |
| `DateTrunc('day', col)` | High | ❌ Missing |
| `DateAdd()` / `DateDiff()` | Medium | ❌ Missing |
| `Now()` / `CurrentTimestamp()` | Medium | ❌ Missing |
| `AtTimeZone()` | Medium | ❌ Missing |
| `Age()` (PostgreSQL) | Low | ❌ Missing |

### 2.5 Conditional Expressions

| Expression | Example | Priority | Status |
|------------|---------|----------|--------|
| `Coalesce()` | `COALESCE(col, 'default')` | **Critical** | ❌ Missing |
| `NullIf()` | `NULLIF(a, b)` | High | ❌ Missing |
| `Case/When/Then/Else/End` | `CASE WHEN ... THEN ... ELSE ... END` | **Critical** | ❌ Missing (commented out in tests) |
| `IIf()` (MySQL/SQLite) | `IIF(condition, true_val, false_val)` | Low | ❌ Missing |

**Case Expression Design:**

```csharp
// Proposed API
var query = _db
    .Select(
        UsersTable.Id,
        Case()
            .When(UsersTable.IsActive.Eq(true), Sql.Value("Active"))
            .When(UsersTable.IsActive.Eq(false), Sql.Value("Inactive"))
            .Else(Sql.Value("Unknown"))
            .As("Status")
    )
    .From(users);
```

**Implementation:** Create [`CaseNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/CaseNode.cs) implementing [`IGenericSql`](Drizzle4Dotnet/src/Core/Shared/ISql.cs#L50).

### 2.6 Type Casting

| Expression | Priority | Status |
|------------|----------|--------|
| `Cast(col AS type)` / `col::type` | High | ❌ Missing |
| `CastToString()` / `CastToInt()` | Medium | ❌ Missing |

**Implementation:** Create [`CastNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/CastNode.cs). The PostgreSQL dialect would emit `::type`, others would emit `CAST(... AS ...)`.

### 2.7 JSON Functions (PostgreSQL)

| Function | Priority | Status |
|----------|----------|--------|
| `JsonExtract(col, 'path')` / `->` / `->>` | High | ❌ Missing |
| `JsonAgg(col)` / `JsonBuildObject(...)` | Medium | ❌ Missing |
| `JsonArrayLength()` | Low | ❌ Missing |
| `ToJson()` / `RowToJson()` | Low | ❌ Missing |

### 2.8 Array Functions (PostgreSQL)

| Function | Priority | Status |
|----------|----------|--------|
| `ArrayAgg(col)` | Medium | ❌ Missing |
| `Unnest(col)` | Medium | ❌ Missing |
| `ArrayLength()` | Low | ❌ Missing |
| `Any(col)` / `All(col)` (array version) | Low | ❌ Missing |

### 2.9 `NnaryNode` API — Unused Generic Parameter Bug

**File:** [`NnaryNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/NnaryNode.cs#L74-L76)

The `And<T>`, `Or<T>`, `Xor<T>` static extension methods have an unused generic parameter `T`:

```csharp
public static NnaryNode<bool, bool> And<T>(params ISql<bool>[] conditions)
```

The `T` is never referenced in the method body. These should be corrected to non-generic:

```csharp
public static NnaryNode<bool, bool> And(params ISql<bool>[] conditions)
```

### 2.10 Mixed IN Clause — Values + Subquery

**File:** [`Operators.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs#L117-L118) and [`NnaryNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/NnaryNode.cs#L28-L53)

The `In<T, TDialect>(IColumnOfDialect, params SqlValue<T, TDialect>[])` overloads are commented out. This prevents mixing literal values with subqueries in `IN` clauses:

```csharp
// Currently NOT possible:
.Where(In(UsersTable.Id, 10, 20, subQuery))

// But should be:
.Where(In<int, PgSqlSqlDialectImpl>(UsersTable.Id, 10, 20, subQuery))
```

**Fix:** Uncomment the `NnaryAnyNode`-based overloads and ensure `SqlValue<T, TDialect>` implicit conversions work correctly.

### 2.11 `FILTER (WHERE ...)` for Aggregate Functions

PostgreSQL supports `FILTER (WHERE ...)` on aggregate functions:

```sql
SELECT COUNT(*) FILTER (WHERE is_active = true) AS active_users FROM users
```

**Proposed API:**

```csharp
Count(UsersTable.Id).Filter(Where: Eq(UsersTable.IsActive, true)).As("ActiveUsers")
```

### 2.12 `AsSubQuery()` Missing on Non-VirtualTable SelectQuery

**File:** [`Query.cs`](Drizzle4Dotnet/src/Core/Query/Query.cs#L66-L69)

`AsSubQuery()` is only available on `Query<TReturn, TDialect, TVirtualTable>` but NOT on `Query<TReturn, TDialect>` (the non-virtual-table variant). This is inconsistent.

**Fix:** Add `AsSubQuery()` to the non-virtual-table variant returning `RawSubqueryTableSql<TDialect>` or similar.

### 2.13 `IS DISTINCT FROM` / `IS NOT DISTINCT FROM` (Null-Safe Equality)

**Priority: High**

```sql
SELECT * FROM users WHERE name IS DISTINCT FROM 'Alice'
```

PostgreSQL's null-safe comparison operator. Useful for comparing nullable columns without null-propagating.

---

## 2b. Phase 1b: Static `Sql` Utility Class

**Priority: Medium** — A static `Sql` utility class providing factory methods for common SQL expression patterns.

**Proposed file:** [`Drizzle4Dotnet/src/Core/Shared/Sql.cs`]

### Core Methods

| Method | Signature | Description |
|--------|-----------|-------------|
| `Sql.Value<T>(T value)` | `ISql<T>` | Creates a parameterized value expression |
| `Sql.Raw(string sql)` | `ISql` | Creates a raw SQL fragment |
| `Sql.Raw<T>(string sql)` | `ISql<T>` | Creates a typed raw SQL fragment |
| `Sql.Literal(string sql)` | `ISql` | Unsafe literal SQL (no parameterization) |
| `Sql.Null<T>()` | `ISql<T>` | NULL literal |
| `Sql.Default()` | `ISql` | DEFAULT VALUES / DEFAULT keyword |

### Usage Examples

```csharp
// Parameterized value
.Where(Eq(UsersTable.Status, Sql.Value("active")))

// Raw SQL for database-specific functions
.Select(Sql.Raw<string>("NOW()::text").As("CurrentTime"))

// Literal SQL fragment
.OrderBy(Sql.Literal("RANDOM()"))
```

### 2b.1 Operator `.As()` Consistency Audit

Verify that ALL operator return types implement `ISql<T>` so `.As(alias)` works universally:
- [`UnaryNode<T>`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/UnaryNode.cs) — implements `IOperator<T>` → `ISql<T>` ✅
- [`UnaryNode<T, TReturn>`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/UnaryNode.cs) — implements `IOperator<TReturn>` → `ISql<TReturn>` ✅
- [`BinaryNode<T>`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/BinaryNode.cs) — implements `IOperator<T>` → `ISql<T>` ✅
- [`BinaryNode<T1, T2, TReturn>`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/BinaryNode.cs) — implements `IOperator<TReturn>` → `ISql<TReturn>` ✅
- [`BinarySqlValueNode<T, TReturn>`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/BinaryNode.cs) — implements `IOperator<TReturn>` → `ISql<TReturn>` ✅
- [`NnaryNode<T, TReturn>`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/NnaryNode.cs) — implements `IOperator<TReturn>` → `ISql<TReturn>` ✅

All seem correct in the codebase. No changes needed.

---

## 3. Phase 2: Window Functions

### 3.1 Window Function Support

Window functions are essential for analytical queries.

| Function | Priority | Status |
|----------|----------|--------|
| `RowNumber().Over(...)` | **Critical** | ❌ Missing |
| `Rank().Over(...)` | High | ❌ Missing |
| `DenseRank().Over(...)` | High | ❌ Missing |
| `Ntile(n).Over(...)` | Medium | ❌ Missing |
| `Lead(col, offset, default).Over(...)` | High | ❌ Missing |
| `Lag(col, offset, default).Over(...)` | High | ❌ Missing |
| `FirstValue(col).Over(...)` | Medium | ❌ Missing |
| `LastValue(col).Over(...)` | Medium | ❌ Missing |
| `NthValue(col, n).Over(...)` | Low | ❌ Missing |

### 3.2 OVER Clause Design

```
Over()
    .PartitionBy(col1, col2)
    .OrderBy(col3.Asc(), col4.Desc())
    .RowsBetween(WindowFrame.UnboundedPreceding, WindowFrame.CurrentRow)
    .RangeBetween(...)
```

**Implementation Plan:**

1. Create [`WindowFrame.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/WindowFrame.cs) — enum/struct for frame types
2. Create [`OverNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/OverNode.cs) — `PARTITION BY` / `ORDER BY` / frame clause
3. Create window function methods in [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs) returning `WindowFunctionNode`
4. Create [`WindowFunctionNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/WindowFunctionNode.cs) — combines function + OVER clause

**Files to create/modify:**
- [`Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/WindowFunctionNode.cs`] — New
- [`Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/OverNode.cs`] — New
- [`Drizzle4Dotnet/src/Core/Shared/Operators/WindowFrame.cs`] — New
- [`Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs`] — Extend
- [`Drizzle4Dotnet/src/Core/Shared/ISql.cs`] — Possible extension

---

## 4. Phase 3: Advanced DML

### 4.1 UPSERT — INSERT ... ON CONFLICT

**Priority: Critical** — Essential for PostgreSQL production usage.

**Proposed API:**

```csharp
// ON CONFLICT DO NOTHING
var q1 = _db.Insert(UsersTable)
    .Values(insertRecord)
    .OnConflictDoNothing();

// ON CONFLICT (columns) DO UPDATE SET ...
var q2 = _db.Insert(UsersTable)
    .Values(insertRecord)
    .OnConflict(UsersTable.Id)
    .DoUpdate()
    .Set(UsersTable.Name, "Updated Name")
    .Where(UsersTable.IsActive.Eq(true));

// ON CONFLICT ON CONSTRAINT constraint_name
var q3 = _db.Insert(UsersTable)
    .Values(insertRecord)
    .OnConflictOnConstraint("users_email_key")
    .DoNothing();
```

**Implementation:**
- Modify [`InsertQuery.cs`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) to add `OnConflictDoNothing()`, `OnConflict()`, and related methods
- Create [`OnConflictClause.cs`](Drizzle4Dotnet/src/Core/Query/Insert/OnConflictClause.cs) for conflict target + action
- Wire into `BuildSql()` to emit `ON CONFLICT [...] DO [...]`

### 4.2 INSERT ... SELECT (InsertFromQuery)

**Priority: High**

**Proposed API:**

```csharp
var q = _db.Insert(UsersTable)
    .From(
        _db.Select(UsersTable.Id, UsersTable.Name)
           .From(users)
           .Where(UsersTable.IsActive.Eq(true))
    );
```

**Implementation:**
- Add `From(SelectQuery<...>)` method to [`InsertQuery.cs`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs)
- Modify `BuildSql()` to support `INSERT INTO table (...) SELECT ...` instead of `VALUES`

### 4.3 INSERT DEFAULT VALUES

**Priority: Medium**

```csharp
var q = _db.Insert(UsersTable).DefaultValues();
```

### 4.4 UPDATE with FROM / JOIN

**Priority: High** — PostgreSQL supports `UPDATE ... FROM ... WHERE`.

**Proposed API:**

```csharp
var q = _db.Update(UsersTable)
    .Set(UsersTable.Salary, 50000)
    .From(departments)
    .Where(And(
        Eq(UsersTable.DepartmentId, DepartmentsTable.Id),
        Eq(DepartmentsTable.Name, "Engineering")
    ));
```

**Implementation:**
- Add `From()` method to [`UpdateQuery.cs`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs)
- Modify `BuildSql()` to handle `FROM` clause

### 4.5 UPDATE/DELETE with USING (PostgreSQL)

**Priority: High**

```csharp
var q = _db.Delete(UsersTable)
    .Using(departments)
    .Where(And(
        Eq(UsersTable.DepartmentId, DepartmentsTable.Id),
        Eq(DepartmentsTable.Name, "Obsolete")
    ));
```

### 4.6 MERGE (SQL Standard)

**Priority: Medium** — Useful for migrations and complex sync operations.

```csharp
var q = _db.Merge(UsersTable)
    .Using(sourceTable)
    .On(Eq(UsersTable.Id, sourceTable.Id))
    .WhenMatched()
        .Update()
        .Set(UsersTable.Name, sourceTable.Name)
    .WhenNotMatched()
        .Insert();
```

**Implementation:**
- Create [`MergeQuery.cs`](Drizzle4Dotnet/src/Core/Query/Merge/MergeQuery.cs) — new query builder
- Add `Merge()` method to [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs)

### 4.7 RETURNING Clause Enhancements

**Priority: Medium** — Already implemented as [`ReturningQuery.cs`](Drizzle4Dotnet/src/Core/Query/ReturningQuery.cs), but verify it works correctly with all DML types.

- Ensure `RETURNING` on UPDATE/DELETE emits correct SQL
- Add convenience methods like `ExecuteReturningAsync<T>()` on `DbClient`

### 4.8 `DELETE USING` (PostgreSQL)

**Priority: High** — PostgreSQL supports `DELETE FROM table USING other_table WHERE ...`

```csharp
var q = _db.Delete(UsersTable)
    .Using(departments)
    .Where(And(
        Eq(UsersTable.DepartmentId, DepartmentsTable.Id),
        Eq(DepartmentsTable.Name, "Archived")
    ));
```

---

## 4b. Phase 3b: Recursive CTEs & Advanced Subqueries

### 4b.1 Recursive CTEs

**Priority: High** — Already identified as commented-out code in [`Test/Select/Base.cs`](Test/Select/Base.cs#L574-L595). Essential for tree/hierarchy queries.

**Proposed API:**

```csharp
var deptTree = _db
    .Select(DepartmentsTable.Id, DepartmentsTable.Name, DepartmentsTable.ParentDepartmentId)
    .From(departments)
    .Where(Eq(DepartmentsTable.Id, 1))           // Anchor member
    .UnionAll(
        _db.Select(DepartmentsTable.Id, DepartmentsTable.Name, DepartmentsTable.ParentDepartmentId)
            .From(departments)
            .InnerJoin(deptTree, Eq(DepartmentsTable.ParentDepartmentId, deptTree.Field<int>("Id")))
    )
    .AsRecursiveCte("dept_tree");

var query = _db
    .Select(deptTree.Selected)
    .With(deptTree)
    .From(deptTree);
```

**Implementation Plan:**
1. Add `Recursive` flag to CTE infrastructure
2. Add `UnionAll(SelectQuery)` method to support recursive CTE body
3. Create `AsRecursiveCte(alias)` method on [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs)
4. Modify [`TypedTupleGeneratedSubqueryTable`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs#L49-L112) and [`GeneratedSubqueryTable`](SourceGenerators/SourceGenerators/TableGenerator.cs) to support recursive CTEs
5. Emit `WITH RECURSIVE name AS (anchor UNION ALL recursive) SELECT ...`

### 4b.2 LATERAL Subqueries Enhancement

Currently [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs#L412-L422) (TVirtualTable variant) has:
- `InnerLateralJoin()`
- `LeftLateralJoin()`
- `CrossLateralJoin()`

**Missing:** The non-virtual-table [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs#L16-L223) does NOT have these methods. Add for consistency.

### 4b.3 Correlated Subquery in SELECT List

Already supported via `.As(alias)` on subquery objects (tested in [`Test/Select/Base.cs`](Test/Select/Base.cs#L395-L413)). Verify consistency across all query variants.

### 4b.4 Subquery Factoring / Named Subquery Blocks

**Proposed API — reusable query blocks:**

```csharp
var userSummary = _db
    .Select(UsersTable.Id, Count(UserProjectsTable.Id).As("ProjectCount"))
    .From(users)
    .LeftJoin(userProjects, Eq(UsersTable.Id, UserProjectsTable.UserId))
    .GroupBy(UsersTable.Id)
    .AsSubQuery("user_summary");

// Use the named block in multiple places
var q1 = _db.Select(userSummary.Selected).From(userSummary);
var q2 = _db.Select(UsersTable.Name, userSummary.Field<int>("ProjectCount"))
    .From(users)
    .InnerJoin(userSummary, Eq(UsersTable.Id, userSummary.Field<int>("Id")));
```

Already partially supported via [`TypedTupleAnonymousGeneratedSubqueryTable`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs#L115-L149). Need to add cleaner API.

### 4b.5 `SELECT INTO` (Create Table As)

```csharp
var q = _db.Select(UserSelect.Record)
    .From(users)
    .Into("users_backup");
```

**Implementation:** Add `Into(tableName)` method to [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs), emitting `SELECT ... INTO table_name FROM ...`.

---

## 5. Phase 4: Set Operations & Compound Queries

### 5.1 UNION / INTERSECT / EXCEPT

**Priority: High**

**Proposed API:**

```csharp
var activeUsers = _db
    .Select(UserSelect.Record)
    .From(users)
    .Where(Eq(UsersTable.IsActive, true));

var formerUsers = _db
    .Select(UserSelect.Record)
    .From(users)
    .Where(Eq(UsersTable.IsActive, false));

var union = activeUsers.Union(formerUsers);
var intersect = activeUsers.Intersect(formerUsers);
var except = activeUsers.Except(formerUsers);

// ALL variants
var unionAll = activeUsers.UnionAll(formerUsers);
```

**Implementation:**
- Create [`CompoundQuery.cs`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs) wrapping two queries with a set operation
- Add `Union()`, `UnionAll()`, `Intersect()`, `Except()` methods to [`SelectQuery.cs`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs)
- Return type should implement [`IReturning<TReturn, TDialect>`](Drizzle4Dotnet/src/Core/Shared/IReturning.cs)

### 5.2 VALUES as Derived Table

**Priority: Medium**

```csharp
var values = _db.Values(
    ("Alice", 30),
    ("Bob", 25),
    ("Charlie", 35)
).As("people(name, age)");

var q = _db.Select(...).From(values);
```

---

## 6. Phase 5: Convenience Execution Methods

### 6.1 DbClient Convenience Methods

**Priority: High**

Currently [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs) has `ExecuteGetListAsync` and `ExecuteAsync`. Add:

| Method | Description | Priority |
|--------|-------------|----------|
| `ExecuteFirstAsync<T>()` | Returns first result or default | High |
| `ExecuteSingleAsync<T>()` | Returns single result, throws if not exactly one | High |
| `ExecuteFirstOrDefaultAsync<T>()` | Returns first result or `default` | High |
| `ExecuteSingleOrDefaultAsync<T>()` | Returns single or `default` | High |
| `ExecuteScalarAsync<T>()` | Returns first column of first row | High |
| `ExecuteGetListAsync` with `CancellationToken` | Cancellation support | High |
| `ExecutePagedAsync<T>()` | Built-in pagination (page + pageSize) | Medium |
| `ExecuteReaderAsync()` | Raw reader access | Medium |

### 6.2 SelectQuery Convenience Methods

| Method | Description | Priority |
|--------|-------------|----------|
| `FirstAsync()` | Async first() | High |
| `FirstOrDefaultAsync()` | Async first or default | High |
| `SingleAsync()` | Async single() | High |
| `SingleOrDefaultAsync()` | Async single or default | High |
| `CountAsync()` | `SELECT COUNT(*) FROM (...) AS count_query` | High |
| `ExistsAsync()` | `SELECT EXISTS(...)` | High |
| `ToListAsync()` | Alias for ExecuteGetListAsync | High |

### 6.3 Paging / Pagination

**Proposed API:**

```csharp
var page = await _db
    .Select(UserSelect.Record)
    .From(users)
    .Where(Eq(UsersTable.IsActive, true))
    .OrderBy(UsersTable.Name)
    .ToPageAsync(pageNumber: 2, pageSize: 20);
// Returns (List<T> Items, int TotalCount, int Page, int PageSize, int TotalPages)
```

### 6.4 CancellationToken Support

**Priority: High** — Add `CancellationToken` parameter to all async methods in [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs).

---

## 7. Phase 6: Batch Operations & Streaming

### 7.1 Batch Insert

**Priority: High**

```csharp
var batch = _db.Insert(UsersTable)
    .Values(record1, record2, record3, ...); // Already supported
```

But add support for large batch chunking:

```csharp
// Auto-chunking into batches of 100
await batch.ExecuteBatchAsync(chunkSize: 100);
```

### 7.2 Bulk Insert (PostgreSQL COPY)

**Priority: Medium** — Use `Npgsql's COPY` protocol for high-performance bulk insert.

```csharp
await _db.BulkInsertAsync(UsersTable, listOfRecords);
```

### 7.3 Async Streaming (IAsyncEnumerable)

**Priority: Medium**

```csharp
await foreach (var user in _db
    .Select(UserSelect.Record)
    .From(users)
    .Where(Eq(UsersTable.IsActive, true))
    .AsAsyncEnumerable())
{
    Process(user);
}
```

### 7.4 Batch Update / Delete

**Priority: Medium**

```csharp
// Execute same command with different parameters
var results = await _db.Update(UsersTable)
    .Set(UsersTable.IsActive, false)
    .Where(Eq(UsersTable.DepartmentId, Sql.Parameter<int>()))
    .ExecuteBatchAsync(new[] { 1, 2, 3 });
```

---

## 8. Phase 7: Multi-Dialect Support

### 8.1 Dialect Interface Extension

Currently [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) only has:
- `BuildIdentifier()`
- `BuildTableName()`
- `BuildColumnName()`
- `BuildParameterName()`

**Need to add:**

```csharp
public interface ISqlDialect
{
    // Existing
    static abstract string BuildIdentifier(string identifier);
    static abstract string BuildTableName(string schemaName, string tableName);
    static abstract string BuildColumnName(string tableName, string columnName);
    static abstract string BuildParameterName(string parameterName);
    static abstract string BuildParameterName(int parameterIndex);
    
    // New
    static abstract string EscapeString(string value);          // String escaping
    static abstract string BuildLimitOffset(int? limit, int? offset);
    static abstract string BuildReturning();                     // RETURNING keyword or lack thereof
    static abstract string BuildConflictTarget(params string[] columns);
    static abstract string BuildUpsert();                        // ON CONFLICT or MERGE
    static abstract string BuildNow();                           // NOW(), CURRENT_TIMESTAMP
    static abstract string BuildCast(string expression, string targetType);
    static abstract string BuildOver(WindowSpec spec);
    static abstract string BuildJsonOperator(string path);      // ->, ->>, #>> etc.
    static abstract bool SupportsArrays { get; }
    static abstract bool SupportsJson { get; }
    static abstract bool SupportsReturning { get; }
    static abstract bool SupportsWindowFunctions { get; }
    static abstract bool SupportsCteWithModifiers { get; }      // WITH ... UPDATE/DELETE
}
```

### 8.2 Dialect Implementations

| Dialect | Priority | Status | File |
|---------|----------|--------|------|
| PostgreSQL | **Critical** | ✅ Done | [`PgSqlSqlDialectImpl.cs`](Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs) |
| MySQL / MariaDB | High | ❌ Missing | — |
| SQLite | High | ❌ Missing | — |
| SQL Server | High | ❌ Missing | — |

Each dialect file should be placed in [`Drizzle4Dotnet/src/Dialect/`](Drizzle4Dotnet/src/Dialect/) following the naming convention `{Name}SqlDialectImpl.cs`.

**MySQL considerations:**
- Backtick quoting instead of double-quote
- `LIMIT ? OFFSET ?` syntax
- No `RETURNING` (use `LAST_INSERT_ID()`)
- `ON DUPLICATE KEY UPDATE` for upsert
- No CTEs with write operations
- No window functions (pre 8.0)

**SQLite considerations:**
- No schema support
- `LIMIT ? OFFSET ?`
- No `RETURNING` (pre 3.35)
- `INSERT OR REPLACE` / `INSERT OR IGNORE` for upsert
- Limited JSON support

**SQL Server considerations:**
- `TOP` instead of `LIMIT` (or `OFFSET ... FETCH NEXT`)
- `OUTPUT INSERTED.*` instead of `RETURNING`
- `MERGE` for upsert
- Square bracket quoting `[identifier]`
- `@@IDENTITY` / `SCOPE_IDENTITY()`

### 8.3 Source Generator Dialect Parameterization

Currently [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs) hardcodes `PgSqlSqlDialectImpl`. This should be made extensible.

**Design Options:**

1. **Option A** (Recommended): Make the dialect a generator parameter — add a `Dialect` property to `TableAttribute` / `AliasAttribute`.
2. **Option B**: Generate for all registered dialects.
3. **Option C**: Provide a base `DbColumn` that works generically without dialect-specific types.

**Proposed Attribute Extension:**

```csharp
[Table("Users", Schema = "public", Dialect = typeof(PgSqlSqlDialectImpl))]
// or
[Table("Users")]  // Defaults to PgSqlSqlDialectImpl
```

---

## 9. Phase 8: Schema / Migration Management

### 9.1 DDL Generation

**Priority: Medium** — Generate `CREATE TABLE` / `ALTER TABLE` from table definitions.

```csharp
var createSql = _db.CreateTable(UsersTable)
    .IfNotExists()
    .Build();
```

### 9.2 Migration Support

**Priority: Medium** — Simple migration system:

- Snapshot-based: Compare current schema with previous snapshot
- Auto-detect: Add/Remove columns, change types
- Generate migration scripts

### 9.3 Index Management

```csharp
var idx = _db.CreateIndex("idx_users_email")
    .On(UsersTable)
    .Column(UsersTable.Email)
    .Unique()
    .Build();
```

---

## 10. Phase 9: Observability & Tooling

### 10.1 Query Logging / Interception

**Priority: High**

```csharp
public interface IDbInterceptor
{
    void OnQueryExecuting(string sql, IDictionary<string, object?> parameters);
    void OnQueryExecuted(string sql, IDictionary<string, object?> parameters, TimeSpan elapsed);
    void OnQueryError(string sql, IDictionary<string, object?> parameters, Exception ex);
}
```

```csharp
var db = new DbClient<PgSqlSqlDialectImpl>(connection)
    .WithInterceptor(new ConsoleLoggerInterceptor());
```

### 10.2 Query Timing / Metrics

**Priority: Medium** — Expose execution time via events or callbacks.

### 10.3 SQL Formatter / Pretty Printer

**Priority: Low** — Optional utility to format generated SQL for debugging.

### 10.4 EXPLAIN Support

**Priority: Low**

```csharp
var plan = await _db
    .Select(UserSelect.Record)
    .From(users)
    .ExplainAsync();
```

---

## 11. Phase 10: Source Generator Enhancements

### 11.1 Table Generator Improvements

**Current file:** [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs)

| Improvement | Priority | Description |
|-------------|----------|-------------|
| **Nullable support** | High | Properly handle nullable reference types in generated columns |
| **Dialect parameterization** | High | Don't hardcode `PgSqlSqlDialectImpl` |
| **Composite primary keys** | Medium | Detect and emit multiple PKs |
| **Foreign key attributes** | Medium | `ForeignKeyAttribute` for relationship metadata |
| **Default value attributes** | Medium | `DefaultValueAttribute("now()")` |
| **Auto-increment detection** | Medium | Mark SERIAL/IDENTITY columns |
| **Enum support** | Medium | Map C# enums to database enum types |
| **Column ordering** | Low | Preserve declaration order |
| **Partial column selection** | High | Generate `Select` subset methods: `UsersTable.SelectIdAndName()` |
| **Join helpers** | Medium | Generate typed join methods |

### 11.2 DbSelect Generator Improvements

**Current file:** [`DbSelectGenerator.cs`](SourceGenerators/SourceGenerators/DbSelectGenerator.cs)

| Improvement | Priority | Description |
|-------------|----------|-------------|
| **MapWithRaw support** | Medium | Already defined in [`Attributes.cs`](Drizzle4Dotnet/src/Core/Shared/Attributes.cs) but not handled by generator |
| **MapWithAlias support** | Medium | Already defined but not handled |
| **Computed columns** | Medium | Support `SELECT (a + b) AS total` in DTO |
| **Aggregate projections** | Medium | Auto-generate DTO for aggregate results |
| **Join-path generation** | Low | Auto-generate DTOs for join chains |

### 11.3 New Generator: Migration Generator

**Priority: Low** — Generate migration classes from schema snapshots.

### 11.4 New Generator: Data Seed Generator

**Priority: Low** — Generate typed seed data classes from SQL or data files.

### 11.5 Source Generator `Utils.GetDataReaderMethod()` Missing Type Mappings

**File:** [`SourceGenerators/SourceGenerators/Utils.cs`](SourceGenerators/SourceGenerators/Utils.cs#L12-L34)

The `GetDataReaderMethod()` is missing several common .NET types and has a bug for `byte[]`:

| Type | Current Mapping | Expected Mapping | Priority |
|------|----------------|------------------|----------|
| `byte` / `Byte` | ❌ Falls to `FieldValue<byte>` | `GetByte()` | High |
| `sbyte` / `SByte` | ❌ Falls to `FieldValue<sbyte>` | `GetSByte()` (via `GetFieldValue<sbyte>`) | Low |
| `ushort` / `UInt16` | ❌ Falls to `FieldValue<ushort>` | `GetInt16()` (cast from reader) | Medium |
| `uint` / `UInt32` | ❌ Falls to `FieldValue<uint>` | `GetInt32()` (cast from reader) | Medium |
| `ulong` / `UInt64` | ❌ Falls to `FieldValue<ulong>` | `GetInt64()` (cast from reader) | Medium |
| `char` / `Char` | ❌ Falls to `FieldValue<char>` | `GetString()` → `.FirstOrDefault()` | Low |
| `byte[]` | `FieldValue<byte[]>` ⚠️ | `GetFieldValue<byte[]>()` | High |
| `TimeSpan` | ❌ Falls to `FieldValue<TimeSpan>` | `GetFieldValue<TimeSpan>()` | Medium |
| `DateTimeOffset` | ❌ Falls to `FieldValue<DateTimeOffset>` | `GetFieldValue<DateTimeOffset>()` | Medium |
| `DateOnly` | ❌ Falls to `FieldValue<DateOnly>` | `GetFieldValue<DateOnly>()` | Low |
| `TimeOnly` | ❌ Falls to `FieldValue<TimeOnly>` | `GetFieldValue<TimeOnly>()` | Low |

**Bug:** The `byte[]` mapping returns `GetFieldValue<byte[]>` wrapped as `FieldValue<byte[]>`, but the code generation pattern uses `r.Get{Method}({i})`. For `FieldValue<byte[]>`, it would generate `r.GetFieldValue<byte[]>({i})` — which should actually work since `GetFieldValue<T>` is a generic method. But the mapper code would need to use `GetFieldValue<T>(i)` pattern consistently for fallback types.

**Fix:** 
1. Add explicit mappings for all missing types
2. Use `GetFieldValue<T>(i)` for types without a dedicated `Get{Type}(i)` method
3. Ensure `byte[]` uses `GetFieldValue<byte[]>()` not the non-existent `GetFieldValue<byte[]>` as string

### 11.6 `MapWithRaw` and `MapWithAlias` Attributes Not Handled by `DbSelectGenerator`

**File:** [`DbSelectGenerator.cs`](SourceGenerators/SourceGenerators/DbSelectGenerator.cs#L311-L390)

The `DbSelectGenerator` only processes `MapWithAttribute`. Two other attributes defined in [`Attributes.cs`](Drizzle4Dotnet/src/Core/Shared/Attributes.cs) are ignored:

- `MapWithAliasAttribute` — maps from a specific table alias (e.g., self-join with different alias)
- `MapWithRawAttribute` — maps from a raw SQL expression

**Proposed API:**

```csharp
[DbSelect]
public partial class UserWithManagerSelect
{
    // Direct column mapping
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string UserName { get; set; }
    
    // Column from table alias (self-join)
    [MapWithAlias(typeof(ManagersTable), "Manager", ManagersTable.ColumnNames.Name)]
    public string ManagerName { get; set; }
    
    // Raw SQL expression
    [MapWithRaw("CONCAT(first_name, ' ', last_name)", "FullName")]
    public string FullName { get; set; }
}
```

**Implementation:**
1. Add `MapWithAlias` processing to [`DbSelectGenerator.GetClassModel()`](DbSelectGenerator.cs#L311-L358)
2. Add `MapWithRaw` processing — emit raw SQL directly without table prefix
3. Generate correct SQL fragments with alias prefixes where needed

### 11.7 `TableAttribute` / `AliasAttribute` Not Exposing Constructor Args as Properties

**File:** [`Schema/Tables/Attributes.cs`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs)

Primary constructors in attributes don't auto-generate public properties in older C# versions. Although the generator accesses via `ConstructorArguments`, the attributes should expose properties for runtime reflection use cases:

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name, string schema = "") : Attribute
{
    public string Name { get; } = name;    // ❌ Missing
    public string Schema { get; } = schema; // ❌ Missing
}
```

---

## 12. Phase 11: GitHub Issues & Reported Bugs

### 12.1 `NnaryNode.And<T>()` Unused Generic Parameter

**File:** [`NnaryNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/NnaryNode.cs#L74-L76)

The `T` in `And<T>()`, `Or<T>()`, `Xor<T>()` is unused:

```csharp
// Current (buggy):
public static NnaryNode<bool, bool> And<T>(params ISql<bool>[] conditions)
// Fixed:
public static NnaryNode<bool, bool> And(params ISql<bool>[] conditions)
```

### 12.2 `SelectQuery.NonVirtualTable` Lacks LATERAL Join Methods

**File:** [`SelectQuery.cs`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs#L16-L223)

The non-virtual-table variant (lines 16-223) is missing:
- `InnerLateralJoin()`
- `LeftLateralJoin()`
- `CrossLateralJoin()`

These are only available on the `TVirtualTable` variant (lines 412-422).

### 12.3 `SelectQuery.NonVirtualTable` Lacks Advanced `ForUpdate` Overloads

The `ForUpdate()` / `ForShare()` / `ForNoKeyUpdate()` / `ForKeyShare()` methods on the non-virtual-table variant don't support `skipLocked` / `nowait` / `ofColumns` parameters, unlike the `TVirtualTable` variant.

### 12.4 `BinarySqlValueNode` Cannot Accept `ISql<T>` Right Side

**File:** [`BinaryNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/BinaryNode.cs#L4-L23)

`BinarySqlValueNode<T, TReturn>` only accepts plain `T` values, not `ISql<T>` expressions. This means `UsersTable.Salary.Eq(otherColumn)` works via `IColumnOfDialect` → `BinaryNode<T1, T2, bool>` overload, but `UsersTable.Salary.Add(otherColumn)` returns `BinarySqlValueNode` which can only accept a plain value.

**Impact:** Arithmetic operations like `Add`, `Sub`, `Mul`, `Div` between two columns return `BinarySqlValueNode` not `BinaryNode`:

```csharp
// This works: column-to-column (uses IColumnOfDialect overload)
Sub(UsersTable.Salary, managerSalaries.Field<decimal>("Salary"))

// But this doesn't:
UsersTable.Salary.Sub(UsersTable.Bonus)  // BinarySqlValueNode, not BinaryNode
```

**Fix:** Ensure all column-to-column arithmetic overloads exist and return `BinaryNode<T>`, not `BinarySqlValueNode<T, T>`.

### 12.5 `QueryBuilderExtensions` Has Code Duplication with `DbSelectGenerator`

**File:** [`QueryBuilderExtensions.cs`](Drizzle4Dotnet/src/Core/QueryBuilderExtensions.cs)

This file is marked `// <auto-generated/>` but is checked into the repository. It generates Select/SelectDistinct overloads for 1-16 columns. The same generation logic exists (commented out) in [`DbSelectGenerator.cs`](DbSelectGenerator.cs#L31-L66).

**Fix:** Either:
1. Generate this file ONCE from the source generator (uncomment the code in DbSelectGenerator), OR
2. Keep the hand-written version and remove the commented-out code in the generator

### 12.6 `Returning` on Non-Select DML: SQL Syntax Verification

**File:** [`ReturningQuery.cs`](Drizzle4Dotnet/src/Core/Query/ReturningQuery.cs)

The `ReturningQuery` wraps any `Query<TDialect>` and appends `RETURNING`. But the SQL syntax is:
- `INSERT INTO ... VALUES ... RETURNING ...` ✅ Works
- `UPDATE ... SET ... WHERE ... RETURNING ...` ✅ Should work
- `DELETE FROM ... WHERE ... RETURNING ...` ✅ Should work

But: `SELECT ... RETURNING ...` is **invalid SQL**. Need to verify that `Returning()` cannot be called on select queries.

**Fix:** Add compile-time constraint (e.g., separate `IDmlQuery` interface) or runtime validation.

### 12.7 `Benchmark/Program.cs` / `Demo1/Program.cs` — Verify API Usage Patterns

**Priority: Low** — Review these files to ensure the plan covers all usage patterns they demonstrate.

---

## 12. Appendix: Cross-Cutting Concerns

### 12.1 Native AOT Compatibility

**Priority: High** — All code must be Native AOT friendly (no runtime reflection).

Current state: ✅ Good — [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs) uses source-generated mappers.  
To maintain: No `Activator.CreateInstance`, `Expression.Compile`, or runtime type discovery.

### 12.2 Performance Optimization Targets

| Area | Current | Target | Priority |
|------|---------|--------|----------|
| SQL generation allocation | String concatenation | `StringBuilder` pooling | Medium |
| Parameter dictionary allocation | `Dictionary<string, object?>` | Reusable builders | Medium |
| Mapper compilation | Source-generated | Static delegates | ✅ Already done |
| Query object reuse | New per query | Query pool | Low |
| Async overhead | Standard await patterns | `ValueTask` where applicable | Medium |

### 12.3 Testing Strategy

| Test Category | Coverage | Priority |
|---------------|----------|----------|
| SQL generation correctness | ✅ Good (select) / ❌ Missing (others) | High |
| Query builder fluency | ✅ Good | Medium |
| Operator node rendering | ✅ Good | Medium |
| Dialect-specific output | ✅ PostgreSQL / ❌ Others | High |
| Source generator output | ❌ Missing | High |
| Integration tests (real DB) | ❌ Missing | Medium |
| Edge cases (nulls, empty lists) | ❌ Missing | Medium |
| Performance benchmarks | ✅ Existing | Medium |

### 12.4 Documentation Plan

| Document | Priority | Status |
|----------|----------|--------|
| `README.md` | **Critical** | ✅ Exists, needs update |
| `PLAN.md` (this) | High | ✅ Being created |
| `API.md` (full API reference) | High | ❌ Missing |
| Migration guide | Medium | ❌ Missing |
| Source generator documentation | Medium | ❌ Missing |
| Dialect implementation guide | Medium | ❌ Missing |
| Contributing guide | Low | ❌ Missing |

### 12.5 Dependency Management

| Dependency | Purpose | Status | Notes |
|------------|---------|--------|-------|
| `Microsoft.CodeAnalysis.*` | Source generators | ✅ Done | Roslyn 4.x |
| `Npgsql` | PostgreSQL driver | ❌ Optional | Runtime only |
| `Dapper` | Interop/comparison | ❌ Optional | Benchmarks |
| `NUnit` | Testing | ✅ Done | `Test/Test.csproj` |
| `BenchmarkDotNet` | Benchmarks | ✅ Done | `Benchmark/Benchmark.csproj` |

### 12.6 Priority Matrix Summary

| Phase | Features | Priority | Effort | Dependencies |
|-------|----------|----------|--------|--------------|
| **P1** | Operators & Functions | Critical | Medium | None |
| **P2** | Window Functions | High | Large | P1 |
| **P3** | Advanced DML | Critical | Large | None |
| **P4** | Set Operations | High | Medium | None |
| **P5** | Convenience Methods | High | Small | None |
| **P6** | Batch & Streaming | Medium | Medium | P3 |
| **P7** | Multi-Dialect | High | Large | None |
| **P8** | Schema/Migrations | Medium | Large | P7 |
| **P9** | Observability | Medium | Medium | None |
| **P10** | Source Gen Enhancements | High | Large | None |

### 12.7 Immediate Next Steps (Recommended Sprint)

Based on the priority-effort matrix, the recommended first sprint focuses on:

1. **UPSERT (ON CONFLICT)** — [`InsertQuery.cs`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) — Critical, moderate effort
2. **UPDATE with FROM** — [`UpdateQuery.cs`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs) — High, moderate effort
3. **CASE/WHEN expressions** — New [`CaseNode.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/CaseNode.cs) — High, moderate effort
4. **COALESCE / NULLIF** — [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs) — High, small effort
5. **CancellationToken support** — [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs) — High, small effort
6. **String functions (Upper, Lower, Length, Substring, Replace)** — [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs) — High, small effort
7. **ExecuteFirst/FirstOrDefault/Single** — [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs) — High, small effort
8. **Source generator nullable support** — [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs) — High, moderate effort
