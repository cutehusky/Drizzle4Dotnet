# Drizzle4Dotnet — Total Project Roadmap

> **Status:** Active Development  
> **Last Updated:** 2026-06-29  
> **Target:** v1.0 Release

---

## Table of Contents

1. [Current State Summary](#1-current-state-summary)
2. [Phase 1: Full Unit Test Coverage](#2-phase-1-full-unit-test-coverage)
3. [Phase 2: Batch API Support](#3-phase-2-batch-api-support)
4. [Phase 3: Prepared Statement Support](#4-phase-3-prepared-statement-support)
5. [Phase 4: Better JSON/JSONB Handling](#5-phase-4-better-jsonjsonb-handling)
6. [Phase 5: Cross-Dialect Test Suites](#6-phase-5-cross-dialect-test-suites)
7. [Phase 6: Performance & AOT Optimization](#7-phase-6-performance--aot-optimization)
8. [Phase 7: Documentation & Release](#8-phase-7-documentation--release)
9. [Timeline & Milestones](#9-timeline--milestones)

---

## 1. Current State Summary

| Area | Status | Details |
|------|--------|---------|
| **Core Library** | ✅ Stable | SQL builder, expression nodes, operators, functions, schema/migration |
| **PgSql Dialect** | ✅ Complete | Full PostgreSQL support with Npgsql |
| **MySql Dialect** | ✅ Complete | Full MySQL/MariaDB support with MySqlConnector |
| **MSSQL Dialect** | ✅ Complete | Full SQL Server support with Microsoft.Data.SqlClient |
| **Oracle Dialect** | ✅ Complete | Full Oracle support with Oracle.ManagedDataAccess.Core |
| **SQLite Dialect** | 🚧 Planned | Not yet implemented |
| **Source Generators** | ✅ Stable | TableGenerator, DbSelectGenerator, MigrationSchemaGenerator |
| **CLI Tool** | ✅ Stable | Generate, snapshot, apply, status, debug commands |
| **Migration System** | ✅ Stable | Snapshot-based diff and migration plan generation |
| **Unit Tests** | ⚠️ Partial | PgSql/MySql query tests exist; MSSQL/Oracle missing; core component tests missing |
| **Batch API** | ❌ Missing | No batch insert/update/delete support |
| **Prepared Statements** | ❌ Missing | No prepared statement / command caching |

### 1.1 Existing Documentation

| Document | Description | Status |
|----------|-------------|--------|
| [`class.md`](class.md) | Comprehensive class design | ✅ |
| [`feature.md`](feature.md) | Complete feature inventory | ✅ |
| [`design-review.md`](design-review.md) | Design analysis & recommendations | ✅ |
| [`test-report.md`](test-report.md) | Test coverage report & plan | ✅ |
| [`PLAN-pgsql.md`](PLAN-pgsql.md) | PgSql implementation status | ✅ |
| [`PLAN-mssql.md`](PLAN-mssql.md) | MSSQL implementation status | ✅ |
| [`PLAN-mysql.md`](PLAN-mysql.md) | MySQL implementation status | ✅ |
| [`PLAN-oracle.md`](PLAN-oracle.md) | Oracle implementation status | ✅ |
| [`PLAN-sqlite.md`](PLAN-sqlite.md) | SQLite implementation plan | ✅ |

---

## 2. Phase 1: Full Unit Test Coverage

### 2.1 Core Component Tests

Target: 100% coverage of all core utility classes and expression nodes.

#### 2.1.1 SqlBuilder Tests

| Test | Description |
|------|-------------|
| `SqlBuilder_Append_AccumulatesString` | Verify `Append()` accumulates correctly |
| `SqlBuilder_AppendChar_AccumulatesChar` | Verify char overload works |
| `SqlBuilder_AddParameter_ReturnsParameterName` | `@p0`, `@p1` sequential naming |
| `SqlBuilder_AddParameter_StoresValue` | Parameter dictionary correctness |
| `SqlBuilder_AddParameter_DifferentDialects` | Test `@p0` vs `:p0` naming per dialect |
| `SqlBuilder_Build_ReturnsSqlAndParameters` | Complete build cycle |
| `SqlBuilder_Build_ClearsState` | Ensure idempotent build |
| `SqlBuilder_Chaining_ReturnsThis` | Fluent API returns `ISqlBuilder` |
| `SqlBuilder_MultipleParameters_MixedTypes` | int, string, bool, null parameters |

#### 2.1.2 SqlStatics Tests

| Test | Description |
|------|-------------|
| `BuildClause_EmptyItems_NoOutput` | Empty list → no header appended |
| `BuildClause_SingleItem_NoSeparator` | Single item → no separator needed |
| `BuildClause_MultipleItems_WithSeparator` | Multiple items joined correctly |
| `BuildClause_WithParentheses_WrapsEachItem` | Each condition wrapped in `()` |
| `BuildSqlSetClause_Empty_NoOutput` | Empty dictionary → no SET |
| `BuildSqlSetClause_ScalarValues_Parameterized` | Scalar values → `@p0` parameters |
| `BuildSqlSetClause_ExpressionValues_Inline` | `ISql` expressions rendered inline |
| `BuildSetValues_Empty_NoOutput` | Empty updates list |
| `BuildSetValues_MultipleColumns_CommaSeparated` | Multiple SET pairs |
| `BuildInsertColumnList_Empty_NoOutput` | Empty column list |
| `BuildInsertColumnList_MultipleColumns_CommaSeparated` | Column list in `()` |
| `BuildInsertColumnList_DialectQuoting_UsesBuildIdentifier` | `"col"`, `` `col` ``, `[col]` |
| `BuildInsertRowValues_SingleRow` | `VALUES (@p0, @p1)` |
| `BuildInsertRowValues_MultipleRows` | `VALUES (...), (...)` |
| `BuildInsertRowValues_WithDefaultValue` | Missing column → `DEFAULT`/`NULL` |
| `BuildInsertRowValues_ExpressionValue_Inline` | `ISql` inlined in VALUES |
| `BuildSqlJoins_Empty_NoOutput` | Empty join list |
| `BuildSqlJoins_SingleJoin` | `INNER JOIN t ON (cond)` |
| `BuildSqlJoins_MultipleJoins` | `INNER JOIN ... LEFT JOIN ...` |
| `BuildSqlJoins_CrossJoin_NoOn` | `CROSS JOIN t` (no ON) |
| `BuildSqlCte_Empty_NoOutput` | No CTEs → no WITH |
| `BuildSqlCte_SingleCte` | `WITH "cte" AS (...) ` |
| `BuildSqlCte_MultipleCtes_CommaSeparated` | `WITH "a" AS (...), "b" AS (...)` |
| `BuildSqlCte_Recursive` | `WITH RECURSIVE "cte" AS (...)` |
| `BuildSqlOrderBy_Empty_NoOutput` | Empty order list |
| `BuildSqlOrderBy_SingleColumn_Asc` | `ORDER BY "col" ASC` |
| `BuildSqlOrderBy_MultipleColumns_MixedDir` | `ORDER BY "a" ASC, "b" DESC` |

#### 2.1.3 Expression Node Tests

| Node | Test Cases |
|------|-----------|
| `BinaryNode` | Eq/Neq with values, Eq/Neq with columns, arithmetic ops, type param validation |
| `UnaryNode` | NOT, IS NULL, IS NOT NULL, EXISTS, prefix vs postfix |
| `NnaryNode` | AND/OR/XOR with 2 args, 3 args, N args |
| `TrinaryNode` | BETWEEN, NOT BETWEEN with two bounds, null bounds |
| `FunctionCallNode` | No args, multiple args, DISTINCT, ORDER BY in agg, FILTER (WHERE) |
| `CaseNode` | Single WHEN, multiple WHEN, ELSE, nested CASE, no ELSE |
| `CastNode` | CAST to TEXT/INTEGER/BIGINT/DOUBLE/TIMESTAMP, PostgreSQL `::` syntax |
| `SqlValueNode` | int, string, bool, null, DateTime, Guid, decimal |
| `FilteredAggregateNode` | Agg + FILTER, chained from FunctionCallNode |

#### 2.1.4 Alias & Virtual Column Tests

| Test | Description |
|------|-------------|
| `AliasedSql_BuildSql_WrapsInParentheses` | `(expr) AS alias` |
| `AliasedSql_Identifier_ReturnsAlias` | `Identifier` property returns alias |
| `VirtualColumn_BuildSql_QualifiedName` | `"alias"."column"` |
| `VirtualColumn_Identifier_ReturnsColumnName` | `Identifier` returns column name |
| `VirtualColumn_DialectQuoting_UsesBuildColumnName` | Correct quoting per dialect |

### 2.2 Dialect-Specific Query Tests

#### 2.2.1 MSSQL Test Suite (🔴 High Priority)

**File: [`Test/Select/MssqlSelectTests.cs`](Test/Select/MssqlSelectTests.cs)** (to be created)

| Category | Test Cases |
|----------|-----------|
| TOP | `Top_Basic`, `Top_WithTies`, `Top_Percent`, `Top_WithDistinct`, `Top_WithWhere` |
| OFFSET/FETCH | `OffsetFetch_Basic`, `OffsetFetch_WithOrderBy`, `OffsetFetch_WithoutOrderBy_AddsWorkaround` |
| CROSS/OUTER APPLY | `CrossApply_Basic`, `OuterApply_Basic`, `CrossApply_WithWhere` |
| FULL OUTER JOIN | `FullJoin_Basic`, `FullJoin_WithWhere` |
| Table Hints | `WithHint_Nolock`, `WithHint_Tablock`, `MultipleHints` |
| ORDER BY requirement | `OffsetWithoutOrderBy_AddsSelect0` |

**File: [`Test/Insert/MssqlInsertTests.cs`](Test/Insert/MssqlInsertTests.cs)**

| Category | Test Cases |
|----------|-----------|
| OUTPUT INSERTED | `OutputInserted_SingleColumn`, `OutputInserted_MultipleColumns`, `OutputInserted_AllColumns` |
| OUTPUT clause position | `Output_BetweenColumnsAndValues`, `Output_WithDefaultValues`, `Output_WithInsertSelect` |

**File: [`Test/Merge/MssqlMergeTests.cs`](Test/Merge/MssqlMergeTests.cs)**

| Category | Test Cases |
|----------|-----------|
| Basic MERGE | `Merge_BasicUsingOn`, `Merge_UsingSubquery` |
| WHEN MATCHED | `WhenMatchedUpdate`, `WhenMatchedUpdateMultiple` |
| WHEN NOT MATCHED | `WhenNotMatchedInsert`, `WhenNotMatchedInsertMultiple` |
| BY SOURCE | `WhenNotMatchedBySourceDelete`, `WhenNotMatchedBySourceUpdate` |
| OUTPUT | `MergeOutputInserted`, `MergeOutputDeleted`, `MergeOutputBoth` |
| JOIN in USING | `MergeWithJoin` |

#### 2.2.2 Oracle Test Suite (🔴 High Priority)

**File: [`Test/Select/OracleSelectTests.cs`](Test/Select/OracleSelectTests.cs)**

| Category | Test Cases |
|----------|-----------|
| FOR UPDATE | `ForUpdate_Basic`, `ForUpdate_Of`, `ForUpdate_NoWait`, `ForUpdate_Wait`, `ForUpdate_SkipLocked` |
| Pagination | `OffsetFetch_Basic`, `OffsetFetch_WithOrderBy` |
| DUAL table | `ScalarSelect_AppendsFromDual` (if applicable) |

**File: [`Test/Insert/OracleInsertTests.cs`](Test/Insert/OracleInsertTests.cs)**

| Category | Test Cases |
|----------|-----------|
| RETURNING INTO | `ReturningInto_SingleColumn`, `ReturningInto_MultipleColumns` |
| MERGE-based upsert | `OnConflictDoNothing`, `OnConflictDoUpdate` |

**File: [`Test/Merge/OracleMergeTests.cs`](Test/Merge/OracleMergeTests.cs)**

| Category | Test Cases |
|----------|-----------|
| Basic MERGE | `Merge_Basic` |
| DELETE WHERE | `WhenMatchedDeleteWhere` (Oracle-specific) |
| Full flow | `Merge_WhenMatchedUpdate_WhenNotMatchedInsert_DeleteWhere` |

#### 2.2.3 SQLite Test Suite (When Implemented)

See [PLAN-sqlite.md](PLAN-sqlite.md) for SQLite implementation plan. Once SQLite dialect is built, full test suite parallels PgSql patterns.

### 2.3 Operator Tests

**File: [`Test/Operators/ComparisonOperatorTests.cs`](Test/Operators/ComparisonOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `Eq_Value_GeneratesEquals` | `col = @p0` |
| `Eq_Column_GeneratesEquals` | `col1 = col2` |
| `Ne_Value_GeneratesNotEquals` | `col <> @p0` |
| `Lt_Gt_Ltq_Gtq_EachOperator` | Each comparison operator |
| `ChainedOperators_MultipleConditions` | `.Eq().And().Gt()` chaining |

**File: [`Test/Operators/LogicalOperatorTests.cs`](Test/Operators/LogicalOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `And_Binary_GeneratesAnd` | `(cond1) AND (cond2)` |
| `And_Nary_GeneratesMultipleAnd` | `(c1) AND (c2) AND (c3)` |
| `Or_BinaryAndNary` | OR variants |
| `Xor_Basic` | `(c1) XOR (c2)` |
| `Not_WrapsInParentheses` | `NOT (condition)` |

**File: [`Test/Operators/StringOperatorTests.cs`](Test/Operators/StringOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `Like_GeneratesLike` | `col LIKE @p0` |
| `NotLike_GeneratesNotLike` | `col NOT LIKE @p0` |
| `Contains_WrapsWithWildcards` | `col LIKE '%' \|\| @p0 \|\| '%'` |
| `StartsWith_AppendsWildcard` | `col LIKE @p0 \|\| '%'` |
| `EndsWith_PrependsWildcard` | `col LIKE '%' \|\| @p0` |

**File: [`Test/Operators/CollectionOperatorTests.cs`](Test/Operators/CollectionOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `IsNull_GeneratesIsNull` | `col IS NULL` |
| `IsNotNull_GeneratesIsNotNull` | `col IS NOT NULL` |
| `In_ValueList_GeneratesIn` | `col IN (@p0, @p1, @p2)` |
| `NotIn_ValueList_GeneratesNotIn` | `col NOT IN (@p0, @p1)` |
| `In_Subquery_GeneratesSubqueryIn` | `col IN (SELECT ...)` |
| `Exists_GeneratesExists` | `EXISTS (SELECT ...)` |

**File: [`Test/Operators/RangeOperatorTests.cs`](Test/Operators/RangeOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `Between_GeneratesBetween` | `col BETWEEN @p0 AND @p1` |
| `NotBetween_GeneratesNotBetween` | `col NOT BETWEEN @p0 AND @p1` |

**File: [`Test/Operators/ArithmeticOperatorTests.cs`](Test/Operators/ArithmeticOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `Add_Sub_Mul_Div_Mod_EachOperator` | Each arithmetic operator |
| `Neg_GeneratesNegation` | `(-col)` |
| `ComplexArithmetic_Chained` | `((col1 + col2) * col3)` |

**File: [`Test/Operators/PgSqlOperatorTests.cs`](Test/Operators/PgSqlOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `IsDistinctFrom_GeneratesCorrectSql` | `col IS DISTINCT FROM @p0` |
| `JsonContains_GeneratesAtGt` | `col @> @p0` |
| `JsonExists_GeneratesQuestion` | `col ? @p0` |
| `TextSearch_GeneratesAtAt` | `col @@ @p0` |
| `TrigramSimilar_GeneratesPercent` | `col % @p0` |

**File: [`Test/Operators/MySqlOperatorTests.cs`](Test/Operators/MySqlOperatorTests.cs)**

| Test | Description |
|------|-------------|
| `NullSafeEq_GeneratesSafeEquals` | `` `col` <=> @p0 `` |
| `RegexMatch_GeneratesRegexp` | `` `col` REGEXP @p0 `` |

### 2.4 Function Tests

**File: [`Test/Functions/AggregateFunctionTests.cs`](Test/Functions/AggregateFunctionTests.cs)**

| Test | Description |
|------|-------------|
| `Count_Basic` | `COUNT("col")` |
| `CountDistinct` | `COUNT(DISTINCT "col")` |
| `Sum_Avg_Min_Max_Each` | Each aggregate |
| `Sum_WithFilter` | `SUM("col") FILTER (WHERE ...)` |
| `StdDev_Variance_Each` | Statistical aggregates |

**File: [`Test/Functions/StringFunctionTests.cs`](Test/Functions/StringFunctionTests.cs)**

| Test | Description |
|------|-------------|
| `Upper_Lower_Trim_Each` | Each string function |
| `Length_Substring_Replace` | Positional string functions |
| `Concat_MultipleArgs` | `CONCAT("a", "b", "c")` |

### 2.5 Migration & DDL Tests

**File: [`Test/Migration/DDLQueryBuilderTests.cs`](Test/Migration/DDLQueryBuilderTests.cs)**

| Test | Description |
|------|-------------|
| `CreateTable_Basic` | `CREATE TABLE "s"."t" ("col" TYPE)` |
| `CreateTable_WithIfNotExists` | `CREATE TABLE IF NOT EXISTS ...` |
| `CreateTable_Temporary` | `CREATE TEMPORARY TABLE ...` |
| `CreateTable_WithAllColumnOptions` | PK, NOT NULL, DEFAULT, CHECK, AUTO_INCREMENT |
| `CreateTable_WithConstraints` | FOREIGN KEY, UNIQUE, PRIMARY KEY constraints |
| `CreateTable_WithIndexes` | Inline index definitions |
| `DropTable_Basic` | `DROP TABLE "s"."t"` |
| `DropTable_IfExists` | `DROP TABLE IF EXISTS ...` |
| `DropTable_Cascade` | `DROP TABLE ... CASCADE` |
| `AlterTable_AddColumn` | `ALTER TABLE ... ADD COLUMN ...` |
| `AlterTable_DropColumn` | `ALTER TABLE ... DROP COLUMN ...` |
| `AlterTable_AlterType` | `ALTER TABLE ... ALTER COLUMN TYPE ...` |
| `AlterTable_SetNotNull` | `ALTER TABLE ... ALTER COLUMN SET NOT NULL` |
| `AlterTable_DropDefault` | `ALTER TABLE ... ALTER COLUMN DROP DEFAULT` |
| `AlterTable_AddConstraint` | `ALTER TABLE ... ADD CONSTRAINT ...` |
| `AlterTable_DropConstraint` | `ALTER TABLE ... DROP CONSTRAINT ...` |
| `AlterTable_RenameColumn` | `ALTER TABLE ... RENAME COLUMN ...` |
| `CreateIndex_Basic` | `CREATE INDEX ... ON ... (...)` |
| `CreateIndex_Unique` | `CREATE UNIQUE INDEX ...` |
| `CreateIndex_UsingType` | `CREATE INDEX ... ON ... USING GIN (...)` |
| `CreateIndex_WithWhere` | `CREATE INDEX ... ON ... WHERE ...` |
| `DropIndex_Basic` | `DROP INDEX ...` |
| `CreateSchema_Basic` | `CREATE SCHEMA ...` |
| `CreateDatabase_Basic` | `CREATE DATABASE ...` |

---

## 3. Phase 2: Batch API Support

### 3.1 Motivation

Currently, each `await query` executes a single SQL statement. For bulk operations (inserting 10,000 rows, updating multiple records), the overhead of individual round-trips is prohibitive. Batch API enables:

- **Bulk INSERT**: Single command with multiple value rows
- **Batch INSERT/UPDATE/DELETE**: Multiple statements in one round-trip
- **Transactional batch**: All-or-nothing execution

### 3.2 Architecture

```
BatchQuery<TDialect>
├── BatchInsert<TTable, TDialect>
├── BatchUpdate<TTable, TDialect>
└── BatchDelete<TTable, TDialect>

IExecutableBatch
├── ExecuteAsync()       # Execute all statements
└── Build()              # Get SQL + parameters
```

### 3.3 API Design

#### Batch Insert

```csharp
// Current: individual round-trips
await db.Insert(users).Value(r1);
await db.Insert(users).Value(r2);  // 2 round-trips

// Proposed: batch
await db.Batch(users)
    .Insert(r1)
    .Insert(r2)
    .Insert(r3)     // adds rows to VALUES
    .ExecuteAsync(); // 1 round-trip
```

**Generated SQL (multi-row INSERT):**
```sql
INSERT INTO "users" ("name", "email") VALUES (@p0, @p1), (@p2, @p3), (@p4, @p5);
```

#### Batch Mixed Operations

```csharp
await db.Batch()
    .Insert(users, r1)
    .Update(users, u => u.Set(...).Where(...))
    .Delete(users, d => d.Where(...))
    .ExecuteAsync();
```

**Generated SQL (statement batching):**
```sql
INSERT INTO "users" ("name") VALUES (@p0);
UPDATE "users" SET "name" = @p1 WHERE "id" = @p2;
DELETE FROM "users" WHERE "id" = @p3;
```

### 3.4 Implementation Plan

| Step | Component | Files | Effort |
|------|-----------|-------|--------|
| 3.4.1 | `BatchQuery` base class | `Drizzle4Dotnet/src/Core/Query/Batch/BatchQuery.cs` | 2 days |
| 3.4.2 | `BatchInsert` | `Drizzle4Dotnet/src/Core/Query/Batch/BatchInsert.cs` | 1 day |
| 3.4.3 | `BatchUpdate` | `Drizzle4Dotnet/src/Core/Query/Batch/BatchUpdate.cs` | 1 day |
| 3.4.4 | `BatchDelete` | `Drizzle4Dotnet/src/Core/Query/Batch/BatchDelete.cs` | 1 day |
| 3.4.5 | `IExecutableBatch` interface | `Drizzle4Dotnet/src/Core/Shared/IBatch.cs` | 0.5 day |
| 3.4.6 | Batch execution in DbClient | `DbClient.ExecuteBatchAsync()` | 1 day |
| 3.4.7 | Fluent API entry points | `QueryBuilder.Batch()` methods | 0.5 day |
| 3.4.8 | Dialect batching support | `ISqlDialect.SupportsBatch` flag | 0.5 day |
| 3.4.9 | Unit tests | `Test/Batch/*BatchTests.cs` | 2 days |
| **Total** | | | **~9.5 days** |

### 3.5 Interface Design

```csharp
/// <summary>
/// Represents a batch of SQL statements that can be executed together.
/// </summary>
public interface IBatchQuery : IGenericSql
{
    /// <summary>Number of statements in the batch.</summary>
    int StatementCount { get; }
}

/// <summary>
/// Extension method on DbClient for starting a batch.
/// </summary>
public static BatchQuery<TDialect> Batch<TDialect>(this IQueryExecutor<TDialect> executor)
    where TDialect : ISqlDialect;
```

### 3.6 DbClient Execution

```csharp
public abstract class DbClient<TDialect> : IQueryExecutor<TDialect>
{
    /// <summary>
    /// Executes a batch query. Default implementation concatenates
    /// statements with ';' separator. Dialects can override for
    /// provider-specific batch support.
    /// </summary>
    public virtual async Task ExecuteBatchAsync(IBatchQuery batch)
    {
        await using var cmd = await CreateCommandAsync(batch);
        await cmd.ExecuteNonQueryAsync();
    }
}
```

### 3.7 PostgreSQL-Specific: COPY for Bulk Insert

For very large datasets, PostgreSQL's `COPY` protocol is significantly faster than multi-row INSERT:

```csharp
// Proposed future API:
await db.BulkInsert(users)
    .FromReader(externalDataReader)
    .ExecuteCopyAsync();  // Uses Npgsql's COPY binary protocol
```

This is a future enhancement (v1.1+).

---

## 4. Phase 3: Prepared Statement Support

### 4.1 Motivation

Prepared statements improve performance for repeated query execution by:
- **Caching query plans**: The database compiles the SQL once, reuses the plan
- **Reducing network traffic**: Only parameter data is sent on subsequent executions
- **Preventing SQL injection**: Parameterized queries are inherently safe

### 4.2 Architecture

```
PreparedQuery<TDialect>
├── PrepareAsync()           # Send to DB for preparation
├── ExecuteAsync(params)     # Execute with parameter values
├── ExecuteGetListAsync<T>(params)
├── ExecuteScalarAsync<T>(params)
└── DisposeAsync()           # DEALLOCATE the prepared statement
```

### 4.3 API Design

```csharp
// Prepare a query once
var prep = await db.Prepare(
    db.Select(users.Id, users.Name)
      .From(users)
      .Where(users.Id.Eq(Sql.Parameter<long>()))  // placeholder parameter
);

// Execute multiple times with different values
var result1 = await prep.ExecuteGetListAsync(42);      // WHERE id = 42
var result2 = await prep.ExecuteGetListAsync(100);     // WHERE id = 100

// Clean up
await prep.DisposeAsync();
```

#### Named Parameter Support

```csharp
var prep = await db.Prepare(
    db.Select(users.Id, users.Name)
      .From(users)
      .Where(And(
          users.Id.Eq(Sql.Parameter<long>("userId")),
          users.IsActive.Eq(Sql.Parameter<bool>("isActive"))
      ))
);

// Execute with named parameters
var result = await prep.ExecuteGetListAsync(new {
    userId = 42,
    isActive = true
});
```

### 4.4 Dialect Differences

| Dialect | Prepared Statement Support | Syntax |
|---------|--------------------------|--------|
| PostgreSQL | ✅ `PREPARE`/`EXECUTE`/`DEALLOCATE` | `PREPARE name (int) AS SELECT ...` |
| MySQL | ✅ Client-side prepared statements | `MySqlCommand` with `CommandType.Text` (automatic) |
| MSSQL | ✅ `sp_prepare`/`sp_execute`/`sp_unprepare` | T-SQL system stored procedures |
| Oracle | ✅ Cursor variables / `DBMS_SQL` | `OPEN :cursor FOR ...` |
| SQLite | ✅ `sqlite3_prepare_v2` | Automatic via `SqliteCommand` |

### 4.5 Implementation Plan

| Step | Component | Files | Effort |
|------|-----------|-------|--------|
| 4.5.1 | `SqlParameter<T>` expression node | `Drizzle4Dotnet/src/Core/Operators/Nodes/SqlParameterNode.cs` | 1 day |
| 4.5.2 | `PreparedQuery<TDialect>` class | `Drizzle4Dotnet/src/Core/Query/PreparedQuery.cs` | 2 days |
| 4.5.3 | `IPreparedStatement` interface | `Drizzle4Dotnet/src/Core/Shared/IPreparedStatement.cs` | 0.5 day |
| 4.5.4 | Parameter extraction from query tree | Walk expression tree to find `SqlParameter` nodes | 1 day |
| 4.5.5 | DbClient.PrepareAsync() | `PrepareAsync(IGenericSql)` → `PreparedQuery` | 1 day |
| 4.5.6 | PgSql prepared statement impl | `PREPARE`/`EXECUTE`/`DEALLOCATE` via Npgsql | 1 day |
| 4.5.7 | MySql prepared statement impl | Auto-prepared via MySqlConnector | 0.5 day |
| 4.5.8 | MSSQL prepared statement impl | `sp_prepare`/`sp_execute` | 1 day |
| 4.5.9 | Oracle prepared statement impl | Implicit cursor handling | 1 day |
| 4.5.10 | Unit tests | `Test/*PreparedQueryTests.cs` | 2 days |
| 4.5.11 | Integration tests | `Test/Integration/*PreparedExecuteTests.cs` | 2 days |
| **Total** | | | **~13 days** |

### 4.6 SqlParameter Expression Node

```csharp
/// <summary>
/// Represents a parameter placeholder in a prepared statement.
/// Acts as a typed placeholder that gets replaced with actual values
/// at execution time.
/// </summary>
public class SqlParameterNode<T> : ISql<T>
{
    public string? Name { get; }       // Optional named parameter
    public int Index { get; set; }     // Positional index (assigned during preparation)

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        // During preparation: renders as type placeholder
        // During execution: renders as parameter reference
        sqlBuilder.Append(sqlBuilder.AddParameter(null)); // placeholder value
    }
}

// Factory method on Sql static class:
public static SqlParameterNode<T> Parameter<T>(string? name = null)
    => new(name);
```

### 4.7 PreparedQuery Class

```csharp
public class PreparedQuery<TDialect> : IAsyncDisposable
    where TDialect : ISqlDialect
{
    private readonly IQueryExecutor<TDialect> _executor;
    private readonly string _preparedSql;
    private readonly IReadOnlyList<SqlParameterInfo> _parameters;
    private bool _isPrepared;

    /// <summary>Prepare the statement on the server.</summary>
    public async Task PrepareAsync() { ... }

    /// <summary>Execute with positional parameter values.</summary>
    public Task ExecuteAsync(params object?[] parameterValues) { ... }

    /// <summary>Execute with named parameter values.</summary>
    public Task ExecuteAsync(Dictionary<string, object?> namedParameters) { ... }

    /// <summary>Execute returning query with positional parameter values.</summary>
    public Task<List<T>> ExecuteGetListAsync<T>(params object?[] parameterValues) { ... }

    /// <summary>DEALLOCATE the prepared statement.</summary>
    public async ValueTask DisposeAsync() { ... }
}
```

### 4.8 Usage Examples

```csharp
// ===== Basic usage =====
var prep = await db.Prepare(
    db.Insert(users).Value(new UsersTable.InsertRecord(
        Name: Sql.Parameter<string>("name"),
        Email: Sql.Parameter<string>("email"),
        IsActive: Sql.Parameter<bool>("active")
    ))
);

await prep.ExecuteAsync("Alice", "alice@example.com", true);
await prep.ExecuteAsync("Bob", "bob@example.com", false);
await prep.DisposeAsync();

// ===== SELECT with prepared statement =====
var selectPrep = await db.Prepare(
    db.Select(users.Id, users.Name)
      .From(users)
      .Where(users.Id.Eq(Sql.Parameter<long>()))
);

var results = await selectPrep.ExecuteGetListAsync<(long, string)>(42);
var results2 = await selectPrep.ExecuteGetListAsync<(long, string)>(100);

// ===== UPDATE with prepared statement =====
var updatePrep = await db.Prepare(
    db.Update(users)
      .Set(users.Name, Sql.Parameter<string>("name"))
      .Where(users.Id.Eq(Sql.Parameter<long>("id")))
);

await updatePrep.ExecuteAsync(new Dictionary<string, object?> {
    ["name"] = "Updated Name",
    ["id"] = 42L
});
```

---

## 5. Phase 4: Better JSON/JSONB Handling

### 5.1 Motivation

JSON/JSONB support is currently fragmented across dialects. Each dialect has its own set of JSON functions and operators, but there is no unified, type-safe way to work with JSON columns in queries. The goal is to create a consistent, cross-dialect JSON API while preserving dialect-specific capabilities.

**Current state:**
- PostgreSQL has rich JSON operators (`->`, `->>`, `#>`, `#>>`, `@>`, `?`, `?|`, `?&`) and JSONB functions
- MySQL has `JSON_EXTRACT()`, `JSON_UNQUOTE()`, `JSON_CONTAINS()`, and `->`/`->>` operators
- MSSQL has `JSON_VALUE()`, `JSON_QUERY()`, `JSON_MODIFY()`, `OPENJSON()`
- Oracle has `JSON_VALUE()`, `JSON_QUERY()`, `JSON_EXISTS()`, `JSON_TABLE()`, `JSON_ARRAYAGG()`, `JSON_OBJECTAGG()`
- No cross-dialect unified JSON column type
- No compile-time JSON path validation
- No JSON column type in schema generation

### 5.2 Proposed Architecture

```
JsonColumn<T, TTable, TDialect> : DbColumn<T, TTable, TDialect>
  ├── .Path(path)          → ISql<T>   # Extract at JSON path
  ├── .Text(path)          → ISql<string> # Extract as text
  ├── .Int(path)           → ISql<int>    # Extract as integer
  ├── .Long(path)          → ISql<long>
  ├── .Double(path)        → ISql<double>
  ├── .Bool(path)          → ISql<bool>
  ├── .ArrayLength(path)   → ISql<int>
  ├── .ArrayContains(val)  → ISql<bool>
  ├── .KeyExists(key)      → ISql<bool>
  └── .ObjectField(key)    → ISql<T>

CrossDialectJsonFunctions<TDialect>
  ├── JsonExtract(col, path)
  ├── JsonArrayAgg(expr)
  ├── JsonObjectAgg(key, expr)
  ├── JsonArray(val1, val2, ...)
  ├── JsonObject(key1, val1, ...)
  ├── JsonMergePatch(target, patch)
  └── JsonStripNulls(expr)
```

### 5.3 API Design

```csharp
// Schema definition with JSON column
[PgSqlJsonB]
[Column("Metadata")]
public static string Metadata { get; set; }

// Generated as:
public static DbColumn<string, UsersTable, PgSqlSqlDialectImpl> Metadata { get; set; }

// But we want to use it as a JSON column:
var metadata = users.Metadata.AsJson(); // → JsonColumn<string, UsersTable, PgSqlSqlDialectImpl>

// Then access sub-properties:
db.Select(
    metadata.Text("$.address.city"),
    metadata.Int("$.age"),
    metadata.Bool("$.isActive")
)
.From(users)
.Where(metadata.ArrayContains("$.tags", "vip"));
```

#### Generated SQL per dialect

**PostgreSQL:**
```sql
SELECT
  "users"."Metadata" -> 'address' ->> 'city',    -- text path
  CAST("users"."Metadata" -> 'age' AS INTEGER),   -- int path
  "users"."Metadata" -> 'isActive'                 -- bool path
FROM "users"
WHERE "users"."Metadata" -> 'tags' ? 'vip';        -- array contains
```

**MySQL:**
```sql
SELECT
  JSON_UNQUOTE(JSON_EXTRACT("users"."Metadata", '$.address.city')),
  CAST(JSON_EXTRACT("users"."Metadata", '$.age') AS SIGNED),
  JSON_EXTRACT("users"."Metadata", '$.isActive')
FROM "users"
WHERE JSON_CONTAINS("users"."Metadata", '"vip"', '$.tags');
```

**MSSQL:**
```sql
SELECT
  JSON_VALUE("users"."Metadata", '$.address.city'),
  CAST(JSON_VALUE("users"."Metadata", '$.age') AS INT),
  JSON_QUERY("users"."Metadata", '$.isActive')
FROM "users"
WHERE JSON_VALUE("users"."Metadata", '$.tags') LIKE '%"vip"%';  -- approximation
```

**Oracle:**
```sql
SELECT
  JSON_VALUE("users"."Metadata", '$.address.city'),
  CAST(JSON_VALUE("users"."Metadata", '$.age') AS INTEGER),
  JSON_QUERY("users"."Metadata", '$.isActive')
FROM "users"
WHERE JSON_EXISTS("users"."Metadata", '$.tags?(@ == "vip")');
```

### 5.4 JsonPath Expression Builder

A type-safe JSON path builder to prevent path string errors at compile time:

```csharp
// Type-safe path builder
var cityPath = JsonPath.Root().Field("address").Field("city");
// → "$.address.city"

var tag0Path = JsonPath.Root().Field("tags").Index(0);
// → "$.tags[0]"

var wildcardPath = JsonPath.Root().Field("items").Wildcard().Field("id");
// → "$.items[*].id"

// Used with JsonColumn:
metadata.Text(cityPath)
metadata.Int(JsonPath.Root().Field("age"))
```

### 5.5 JSON Column in Schema Generation

```csharp
// Schema attribute for JSON column type
[PgSqlJsonB]
[Column("Metadata")]
public static string Metadata { get; set; }

// Source generator enhancement:
public static JsonDbColumn<string, UsersTable, PgSqlSqlDialectImpl> Metadata { get; set; }
// JsonDbColumn exposes .Path(), .Text(), .Int(), .ArrayContains(), etc.
```

### 5.6 Implementation Plan

| Step | Component | Effort |
|------|-----------|--------|
| 5.6.1 | `JsonColumn<T,TTable,TDialect>` class | 2 days |
| 5.6.2 | `JsonPath` builder with type-safe path construction | 2 days |
| 5.6.3 | `AsJson()` extension on `DbColumn<string,...>` | 1 day |
| 5.6.4 | Cross-dialect JSON functions (`JsonExtract`, `JsonArrayAgg`, etc.) | 3 days |
| 5.6.5 | PgSql JSON/JSONB implementation (operators + functions) | 2 days |
| 5.6.6 | MySql JSON implementation | 2 days |
| 5.6.7 | MSSQL JSON implementation | 2 days |
| 5.6.8 | Oracle JSON implementation | 2 days |
| 5.6.9 | Source generator enhancement for `JsonDbColumn` | 2 days |
| 5.6.10 | Unit tests for all JSON operations | 3 days |
| **Total** | | **~21 days** |

### 5.7 JSON Data Type Matrix

| Feature | PostgreSQL | MySQL | MSSQL | Oracle |
|---------|-----------|-------|-------|--------|
| **Storage type** | `JSON` / `JSONB` | `JSON` | no native type (nvarchar) | `JSON` (12c+) |
| **Index support** | GIN on JSONB | Virtual column + index | Computed column + index | Functional index |
| **Path extract** | `->` / `->>` | `JSON_EXTRACT()` / `->` | `JSON_VALUE()` / `JSON_QUERY()` | `JSON_VALUE()` / `JSON_QUERY()` |
| **Contains** | `@>` operator | `JSON_CONTAINS()` | None | `JSON_TEXTCONTAINS()` |
| **Key exists** | `?` operator | None | None | `JSON_EXISTS()` |
| **Array agg** | `json_agg()` / `jsonb_agg()` | `JSON_ARRAYAGG()` (8.0+) | None (use `STRING_AGG` + concat) | `JSON_ARRAYAGG()` (19c+) |
| **Object agg** | `jsonb_object_agg()` | `JSON_OBJECTAGG()` (8.0+) | None | `JSON_OBJECTAGG()` (19c+) |
| **Table/rows** | `jsonb_to_recordset()` / `json_array_elements()` | `JSON_TABLE()` (8.0+) | `OPENJSON()` (2016+) | `JSON_TABLE()` (12c+) |
| **Modify** | `jsonb_set()` | `JSON_SET()` / `JSON_REPLACE()` | `JSON_MODIFY()` | `JSON_MERGEPATCH()` (19c+) |

### 5.8 Current Gaps vs Target

| Capability | Current | Target |
|------------|---------|--------|
| Unified JSON API | ❌ No `JsonColumn` type | ✅ `JsonColumn<T,TTable,TDialect>` with `.Path()`, `.Text()`, `.Int()`, etc. |
| JSON path safety | ❌ Raw string paths | ✅ `JsonPath` builder with compile-time checks |
| Cross-dialect consistency | ⚠️ Different APIs per dialect | ✅ Single API, dialect-specific SQL generation |
| JSON in schema | ❌ `string` column only | ✅ `JsonDbColumn` generated for JSON columns |
| JSON_TABLE / OPENJSON | ❌ Not supported | ✅ `JsonTable()` function for unnesting JSON arrays |
| JSON aggregation | ⚠️ Partial per-dialect | ✅ Unified `JsonArrayAgg()`, `JsonObjectAgg()` |

---

## 6. Phase 5: Cross-Dialect Test Suites

### 5.1 MSSQL Full Test Suite

| File | Tests | Effort |
|------|-------|--------|
| `Test/Select/MssqlSelectTests.cs` | 40+ | 3 days |
| `Test/Insert/MssqlInsertTests.cs` | 15+ | 1 day |
| `Test/Update/MssqlUpdateTests.cs` | 15+ | 1 day |
| `Test/Delete/MssqlDeleteTests.cs` | 15+ | 1 day |
| `Test/Merge/MssqlMergeTests.cs` | 20+ | 2 days |
| `Test/Migration/MssqlMigrationTests.cs` | 30+ | 2 days |

### 5.2 Oracle Full Test Suite

| File | Tests | Effort |
|------|-------|--------|
| `Test/Select/OracleSelectTests.cs` | 30+ | 2 days |
| `Test/Insert/OracleInsertTests.cs` | 15+ | 1 day |
| `Test/Update/OracleUpdateTests.cs` | 10+ | 1 day |
| `Test/Delete/OracleDeleteTests.cs` | 10+ | 1 day |
| `Test/Merge/OracleMergeTests.cs` | 15+ | 2 days |
| `Test/Migration/OracleMigrationTests.cs` | 20+ | 2 days |

### 5.3 SQLite Test Suite

To be implemented alongside the SQLite dialect (see [PLAN-sqlite.md](PLAN-sqlite.md)).

---

## 7. Phase 6: Performance & AOT Optimization

### 6.1 AOT Compatibility Audit

| Concern | Current Status | Action |
|---------|---------------|--------|
| Runtime reflection in schema export | ✅ Only in CLI (OrmSchemaExporter) | Acceptable — CLI is not AOT |
| Source generators | ✅ AOT-safe | Already compatible |
| Expression compilation | ✅ Static abstract dispatch | No runtime code gen |
| `string.Join` in SQL building | ⚠️ Used in some places | Replace with `ISqlBuilder.Append` loops |
| LINQ in hot paths | ⚠️ `.Select()`, `.Any()` in query building | Replace with loops |

### 6.2 StringBuilder Pooling

```csharp
// Future optimization:
internal static class StringBuilderPool
{
    [ThreadStatic] private static StringBuilder? _cached;
    
    public static StringBuilder Rent() => Interlocked.Exchange(ref _cached, null) ?? new();
    public static void Return(StringBuilder sb) { sb.Clear(); _cached = sb; }
}
```

### 6.3 Parameter Dictionary Pooling

```csharp
// Future optimization: use ArrayPool for small parameter dictionaries
// or pre-allocate known-size dictionaries in generated code.
```

### 6.4 Microbenchmarks

| Benchmark | Current | Target | File |
|-----------|---------|--------|------|
| Simple SELECT build | — | < 1μs | `Benchmark/QueryBuildBenchmark.cs` |
| Complex SELECT (joins, CTE) | — | < 5μs | `Benchmark/QueryBuildBenchmark.cs` |
| INSERT with 100 rows | — | < 10μs | `Benchmark/InsertBenchmark.cs` |
| SqlBuilder.Append (1M ops) | — | < 100ms | `Benchmark/SqlBuilderBenchmark.cs` |

---

## 8. Phase 7: Documentation & Release

### 7.1 API Documentation

| Document | Status | Description |
|----------|--------|-------------|
| `API.md` | 🚧 Planned | Full auto-generated API reference |
| XML doc comments | ⚠️ Partial | Complete all `///` comments on public types |
| Migration guide | 🚧 Planned | How to upgrade between versions |
| Quickstart tutorial | 🚧 Planned | Step-by-step getting started guide |

### 7.2 NuGet Packaging

| Package | Contents |
|---------|----------|
| `Drizzle4Dotnet` | Core library + all dialects |
| `Drizzle4Dotnet.SourceGenerators` | Roslyn incremental generators (auto-included) |
| `Drizzle4Dotnet.Cli` | CLI tool (global tool or local tool) |

### 7.3 CI/CD Pipeline

| Step | Tool |
|------|------|
| Build + Test | GitHub Actions / `dotnet build && dotnet test` |
| Code coverage | Coverlet + ReportGenerator (target: >80%) |
| Benchmark tracking | BenchmarkDotNet with historical comparison |
| NuGet publish | GitHub Release workflow |

---

## 9. Timeline & Milestones

### Estimated Effort Summary

| Phase | Description | Effort | Dependencies |
|-------|-------------|--------|--------------|
| **P1** | Full Unit Test Coverage | 20 days | None |
| **P2** | Batch API Support | 10 days | P1 (for testing) |
| **P3** | Prepared Statement Support | 13 days | P1 (for testing) |
| **P4** | Better JSON/JSONB Handling | 21 days | P1 (for testing) |
| **P5** | Cross-Dialect Test Suites | 15 days | P1 (patterns) |
| **P6** | Performance & AOT Optimization | 10 days | P1, P2, P3 (stable API) |
| **P7** | Documentation & Release | 10 days | All above |
| **Total** | | **~99 days** | |

### Milestone Schedule

```
M1: Core Test Coverage Complete   [P1]         Week 4
M2: Batch API Ready               [P2]         Week 6
M3: Prepared Statement Ready      [P3]         Week 8
M4: JSON/JSONB API Complete       [P4]         Week 12
M5: All Dialect Tests Complete    [P5]         Week 14
M6: Performance Optimized         [P6]         Week 16
M7: v1.0 Release                  [P7]         Week 18
```

### Priority Order

1. **🔴 Must-have for v1.0**: P1 (core tests), P5 (MSSQL/Oracle tests)
2. **🟡 Should-have for v1.0**: P2 (batch API), P4 (JSON/JSONB), P7 (documentation)
3. **🟠 Nice-to-have for v1.0**: P3 (prepared statements), P6 (optimization)
4. **🟢 Future (v1.1+)**: SQLite dialect, COPY bulk insert, connection pooling integration
