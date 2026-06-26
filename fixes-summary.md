# Drizzle4Dotnet — Applied Fixes Summary

> This document catalogs the design improvements applied to the codebase  
> based on the issues identified in [`design-review.md`](design-review.md).

---

## Fix 1: Extract `IQueryExecutor<TDialect>` Interface

**Issue:** `QueryBase<TDialect>` depended directly on `DbClient<TDialect>` (concrete class), making unit testing impossible without a real database connection.

**Changes:**

| File | Change |
|------|--------|
| **NEW** [`Drizzle4Dotnet/src/Core/Shared/IQueryExecutor.cs`](Drizzle4Dotnet/src/Core/Shared/IQueryExecutor.cs) | Created interface with `ExecuteGetListAsync<T>()`, `ExecuteGetListAsync<T,TVT>()`, `ExecuteAsync()`, `ExecuteScalarAsync<T>()`, `ExecuteReaderAsync()` |
| [`QueryBase.cs`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs) | Changed field from `DbClient<TDialect> DbClient` to `IQueryExecutor<TDialect> Executor`. Added backward-compatible `[Obsolete] DbClient` property |
| [`Query.cs`](Drizzle4Dotnet/src/Core/Query/Query.cs) | All 3 `Query<>` variants: constructor parameter changed from `DbClient` to `IQueryExecutor`. `GetAwaiter()` calls `Executor.ExecuteGetListAsync()` |
| [`SelectQuery.cs`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs) | Both variants: constructor accepts `IQueryExecutor` |
| [`InsertQuery.cs`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) | Constructor accepts `IQueryExecutor` |
| [`UpdateQuery.cs`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs) | Constructor accepts `IQueryExecutor` |
| [`DeleteQuery.cs`](Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs) | Constructor accepts `IQueryExecutor` |
| [`CompoundQuery.cs`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs) | Both variants + all extension methods: `DbClient` → `Executor` |
| [`ReturningQuery.cs`](Drizzle4Dotnet/src/Core/Query/ReturningQuery.cs) | Both variants: `baseQuery.DbClient` → `baseQuery.Executor` |
| [`PgSelectQuery.cs`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs) | Both variants: `DbClient` → `IQueryExecutor` |
| [`PgInsertQuery.cs`](Drizzle4Dotnet/src/PgSql/PgInsertQuery.cs) | Constructor: `DbClient` → `IQueryExecutor` |
| [`PgUpdateQuery.cs`](Drizzle4Dotnet/src/PgSql/PgUpdateQuery.cs) | Constructor: `DbClient` → `IQueryExecutor` |
| [`PgDeleteQuery.cs`](Drizzle4Dotnet/src/PgSql/PgDeleteQuery.cs) | Constructor: `DbClient` → `IQueryExecutor` |
| [`MySqlSelectQuery.cs`](Drizzle4Dotnet/src/MySql/MySqlSelectQuery.cs) | Both variants: `DbClient` → `IQueryExecutor` |
| [`MySqlInsertQuery.cs`](Drizzle4Dotnet/src/MySql/MySqlInsertQuery.cs) | Constructor: `DbClient` → `IQueryExecutor` |
| [`MySqlUpdateQuery.cs`](Drizzle4Dotnet/src/MySql/MySqlUpdateQuery.cs) | Constructor: `DbClient` → `IQueryExecutor` |
| [`MySqlDeleteQuery.cs`](Drizzle4Dotnet/src/MySql/MySqlDeleteQuery.cs) | Constructor: `DbClient` → `IQueryExecutor` |
| [`MySqlReplaceQuery.cs`](Drizzle4Dotnet/src/MySql/MySqlReplaceQuery.cs) | Constructor: `DbClient` → `IQueryExecutor` |

**Impact:** Queries are now decoupled from the concrete `DbClient`. You can mock `IQueryExecutor<TDialect>` for unit tests.

---

## Fix 2: Add `ExecuteScalarAsync<T>()` and `ExecuteReaderAsync()`

**Issue:** Missing execution methods — no `ExecuteScalar` or `ExecuteReader` support.

**Changes:**

| File | Change |
|------|--------|
| [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs) | Added `ExecuteScalarAsync<T>(ISql query)` — returns first column of first row as `T?`. Added `ExecuteReaderAsync(ISql query)` — returns `DbDataReader` (caller disposes) |
| [`IQueryExecutor.cs`](Drizzle4Dotnet/src/Core/Shared/IQueryExecutor.cs) | Added `Task<T?> ExecuteScalarAsync<T>(ISql query)` and `Task<DbDataReader> ExecuteReaderAsync(ISql query)` to interface |

---

## Fix 3: Extract Shared Command Preparation to Reduce Duplication

**Issue:** `ExecuteGetListAsync`, `ExecuteGetListAsync<T,TVT>`, and `ExecuteAsync` all duplicated the same parameter setup code (create command, build SQL, add parameters).

**Changes:**

| File | Change |
|------|--------|
| [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs) | Extracted `CreateCommandAsync(ISql query)` → creates `DbCommand`, builds SQL via `SqlBuilder<TDialect>`, populates parameters. All three `Execute*` methods now call `await CreateCommandAsync(query)` instead of duplicating the setup |

**Impact:** Removed ~40 lines of duplicated code. Adding new execution methods is now trivial.

---

## Fix 4: Move DDL Factories to Standalone `Ddl` Class

**Issue:** DDL factory methods (`CreateTable`, `DropTable`, `DropIndex`, etc.) were `protected static` methods on `DbClient`, requiring a `DbClient` instance to use DDL generation. They don't need any instance state.

**Changes:**

| File | Change |
|------|--------|
| **NEW** [`Ddl.cs`](Drizzle4Dotnet/src/Core/Schema/Migration/Ddl.cs) | Created standalone `static class Ddl` with all DDL factory methods: `CreateTable()`, `DropTable()`, `AlterTable()`, `CreateIndex()`, `DropIndex()` |
| [`DbClient.cs`](Drizzle4Dotnet/src/Core/DbClient.cs) | Removed DDL factory methods and `GetTableDefinition<TTable>()`, `MapClrTypeToSql()`, `ClrTypeToSqlType()` — they are no longer tied to `DbClient` |

**Impact:** DDL can now be used without a `DbClient` instance: `Ddl.CreateTable(def)` instead of `DbClient.CreateTable(def)`. Type mapping is separated into its own utility class.

---

## Fix 5: Extract `TypeMapper` Utility Class

**Issue:** CLR-to-SQL type mapping (`ClrTypeToSqlType`) was a protected method on `DbClient`, not reusable.

**Changes:**

| File | Change |
|------|--------|
| **NEW** [`TypeMapper.cs`](Drizzle4Dotnet/src/Core/Schema/Migration/TypeMapper.cs) | Created `static class TypeMapper` with `ClrTypeToSqlType(Type)`, `MapClrTypeToSql(string, Type)` — methods formerly on `DbClient` |

---

## Fix 6: Add Clause Interfaces (`ISupportWhere`, `ISupportOrderBy`, `ISupportLimit`, `ISupportCte`)

**Issue:** No shared interface contracts for `Where()`, `OrderBy()`, `Limit()`, `Offset()`, `With()` — each query type re-implements these independently.

**Changes:**

| File | Change |
|------|--------|
| **NEW** [`QueryClauseInterfaces.cs`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs) | Created `ISupportWhere<TQuery>` (Where methods), `ISupportOrderBy<TQuery>` (OrderBy method), `ISupportLimit<TQuery>` (Limit/Offset methods), `ISupportCte<TQuery, TDialect>` (With method) |

**Impact:** These interfaces enable generic programming patterns — e.g., `void Filter<T>(T q) where T : ISupportWhere<T>`.

---

## Summary of Files Created

| File | Purpose |
|------|---------|
| [`Drizzle4Dotnet/src/Core/Shared/IQueryExecutor.cs`](Drizzle4Dotnet/src/Core/Shared/IQueryExecutor.cs) | Query execution abstraction — decouples queries from `DbClient` |
| [`Drizzle4Dotnet/src/Core/Schema/Migration/Ddl.cs`](Drizzle4Dotnet/src/Core/Schema/Migration/Ddl.cs) | Standalone DDL factory methods |
| [`Drizzle4Dotnet/src/Core/Schema/Migration/TypeMapper.cs`](Drizzle4Dotnet/src/Core/Schema/Migration/TypeMapper.cs) | CLR-to-SQL type mapping utility |
| [`Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs) | Clause support interfaces (`ISupportWhere`, `ISupportOrderBy`, `ISupportLimit`, `ISupportCte`) |

## Summary of Files Modified

| File | Change Summary |
|------|---------------|
| `QueryBase.cs` | `DbClient` → `IQueryExecutor` field |
| `Query.cs` | Constructor + GetAwaiter → uses `IQueryExecutor` |
| `SelectQuery.cs` | Constructor → `IQueryExecutor` |
| `InsertQuery.cs` | Constructor → `IQueryExecutor` |
| `UpdateQuery.cs` | Constructor → `IQueryExecutor` |
| `DeleteQuery.cs` | Constructor → `IQueryExecutor` |
| `CompoundQuery.cs` | Constructors + extensions → `IQueryExecutor` |
| `ReturningQuery.cs` | Constructor + GetAwaiter → `IQueryExecutor` |
| `DbClient.cs` | Implements `IQueryExecutor`, extracted `CreateCommandAsync`, added `ExecuteScalarAsync`/`ExecuteReaderAsync`, removed DDL methods (moved to `Ddl`) |
| `PgSelectQuery.cs` | Constructor → `IQueryExecutor` |
| `PgInsertQuery.cs` | Constructor → `IQueryExecutor` |
| `PgUpdateQuery.cs` | Constructor → `IQueryExecutor` |
| `PgDeleteQuery.cs` | Constructor → `IQueryExecutor` |
| `MySqlSelectQuery.cs` | Constructor → `IQueryExecutor` |
| `MySqlInsertQuery.cs` | Constructor → `IQueryExecutor` |
| `MySqlUpdateQuery.cs` | Constructor → `IQueryExecutor` |
| `MySqlDeleteQuery.cs` | Constructor → `IQueryExecutor` |
| `MySqlReplaceQuery.cs` | Constructor → `IQueryExecutor` |
