# Design Review — Drizzle4Dotnet

> **Review Date**: 2026-06-29 UTC+7
> **Scope**: Full class hierarchy, architecture patterns, and code organization across all layers (Core SQL, Schema, Query, Migration, Dialect, CLI)

---

## 1. Strengths

### 1.1 Strong Type Safety via Generics

The system leverages C# generics extensively to provide compile-time type safety:

- [`DbColumn<T, TTable, TDialect>`](Drizzle4Dotnet/src/Core/Schema/Columns/DbColumn.cs:6) ties column type, owning table, and dialect together — eliminating mismatches between column types and their values at compile time.
- [`Query<TReturn, TDialect, TVirtualTable>`](Drizzle4Dotnet/src/Core/Query/Query.cs:25) ensures subquery composition preserves column types through the `TVirtualTable` parameter.
- Tuple overloads (1–16 columns) for [`Returning()`](Drizzle4Dotnet/src/Core/Query/QueryReturningExtensions.cs:9) and `Select()` enable type-safe selection without boxing or `dynamic`.

### 1.2 Static Abstract Interface Pattern (C# 11)

The [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:5) interface uses `static abstract` members, enabling:

- **Zero-overhead dispatch**: Dialect methods are resolved at compile time — no virtual calls or runtime dispatch.
- **Compile-time feature gating**: `TDialect.SupportsReturning` etc. are known at JIT time, enabling dead code elimination.
- **Type-safe factory methods**: `TDialect.BuildIdentifier()`, `TDialect.BuildParameterName()` are called without a dialect instance.

### 1.3 Fluent Builder Pattern

All query types follow a consistent fluent API with method chaining:

```csharp
_db.Select()
   .From(users)
   .Where(users.Name.Eq("Alice"))
   .OrderBy(users.Id, asc: false)
   .Limit(10)
```

The CRTP pattern (`TSelf : SelectQuery<..., TSelf>`) ensures fluent methods return the concrete subtype, preserving dialect-specific extension methods.

### 1.4 Clean Separation of Concerns

- **SQL Building** ([`SqlStatics`](Drizzle4Dotnet/src/Core/Shared/SqlStatics.cs:9)): Pure static utility methods with no side effects.
- **Query Building** ([`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:7), etc.): State management + validation.
- **Execution** ([`DbClient<TDialect>`](Drizzle4Dotnet/src/Core/DbClient.cs:8)): ADO.NET interaction only.
- **Migration** ([`SchemaSnapshot`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:10), [`TableDefinitionComparer`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinitionComparer.cs:11)): Schema introspection and diffing.
- **CLI** ([`Program`](Drizzle4Dotnet.Cli/Program.cs:22)): Command dispatch and option parsing.

### 1.5 Comprehensive Migration System

The snapshot-diff-migration pipeline is well-designed:

1. [`SchemaSnapshot`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:10) captures full schema state (columns, constraints, indexes) in JSON.
2. [`SchemaSnapshot.Compare()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:118) produces detailed [`SchemaDiff`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:515) with typed changes.
3. [`SchemaDiff.ToMigrationPlan()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:524) generates ordered DDL steps.
4. Structured constraint/index objects ([`TableConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:10), [`TableIndex`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:185)) enable proper reconstruction rather than raw SQL strings.

### 1.6 Reflection-Free Schema Export Path

The [`OrmSchemaExporter`](Drizzle4Dotnet/src/Core/Schema/Migration/OrmSchemaExporter.cs:14) uses reflection to extract schema from source-generated classes, but the generated classes themselves are strongly typed — reflection is only needed at the CLI boundary for dynamic assembly loading.

---

## 2. Weaknesses & Concerns

### 2.1 CRTP Boilerplate and Complexity

The Curiously Recurring Template Pattern is used extensively:

```csharp
public class SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>
    : Query<TReturn, TDialect, TVirtualTable>
    where TSelf : SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>
```

This adds significant type parameter complexity. Every dialect-specific query class must propagate these four type parameters, and the constraints create a tight coupling between base and derived classes. Consider whether the benefit (fluent `TSelf` return types) outweighs the cognitive overhead.

### 2.2 Excessive Generic Parameters

Some classes carry 3–4 generic parameters:

| Class | Parameters |
|---|---|
| [`SelectQuery<T,D,VT,TS>`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:7) | `TReturn`, `TDialect`, `TVirtualTable`, `TSelf` |
| [`InsertQuery<TTable,D,TS>`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:7) | `TTable`, `TDialect`, `TSelf` |
| [`UpdateQuery<TTable,D,TS>`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:7) | `TTable`, `TDialect`, `TSelf` |
| [`CompoundQuery<T,D,VT>`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:12) | `TReturn`, `TDialect`, `TVirtualTable` |

While this provides type safety, it makes the codebase harder to navigate and increases compilation times. The `TVirtualTable` parameter is particularly problematic — it flows through the entire query hierarchy but is only needed for `AsSubQuery()`.

### 2.3 Duplicate CTE Implementation

CTE support is implemented identically in 4 query types:

- [`SelectQuery.With()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:35)
- [`InsertQuery.With()`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:24)
- [`UpdateQuery.With()`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:33)
- [`CompoundQuery.With()`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:27)

Each has nearly identical code for `With()`, `WithRecursive()`, `CteTables`, and `Recursive` fields. Consider extracting CTE support into a reusable mixin pattern or moving it entirely into [`QueryBase`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs:8) since all queries support CTEs.

### 2.4 String Concatenation in SQL Building

Several places use string concatenation for SQL fragments instead of `ISqlBuilder.Append`:

- [`ForeignKeyConstraint.BuildSql()`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:86): Uses `string.Join(", ", Columns.Select(c => $"\"{c}\""))` — the quoting is hardcoded as double-quotes, which is incorrect for MySQL (backticks) and SQL Server (brackets).
- [`TableIndex.BuildSql()`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:229) and constraint `BuildSql()` methods don't use the dialect-aware builder for identifier quoting.

This breaks cross-dialect compatibility in migration DDL generation. Constraint and index SQL should go through the dialect's `BuildIdentifier()` method.

### 2.5 Raw SQL Type Coupling in Schema Snapshot

The [`RawColumnDefinition`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:485) class is used when deserializing snapshots, but it stores data types as raw strings (`RawSqlDataType`). This means:

- Round-tripping a snapshot loses the specific `ISqlDataType` implementation (e.g., `PgSqlDataType.Integer` becomes `RawSqlDataType("INTEGER")`).
- No way to validate or transform data types after deserialization.

### 2.6 Mixed Naming Conventions

- **Method casing**: `AsSubQuery()`, `BuildSql()` vs. `ToMigrationPlan()`, `ToSql()` — some use PascalCase, others use mixed.
- **Field naming**: `_sql`, `_identifier` vs. `_sb`, `_parameters` — inconsistent private field naming across files.
- **File naming**: `ISql.cs` contains `ISqlBuilder`, `ISql`, `RawSql`, etc. — files are named by role, not by containing type.

### 2.7 Missing Async Cancellation

`IQueryExecutor` methods (`ExecuteGetListAsync`, `ExecuteAsync`, `ExecuteScalarAsync`, `ExecuteReaderAsync`) don't accept `CancellationToken`. This prevents cancellation of long-running queries, which is a standard practice in async ADO.NET code.

### 2.8 CLI Command Duplication

The [`Program.cs`](Drizzle4Dotnet.Cli/Program.cs:22) has separate `HandleGenerate`, `HandleSnapshot`, `HandleStatus`, `HandleApply`, `HandleDebug` methods, each with duplicated option parsing logic. This could be refactored into a command pattern with shared option parsing infrastructure.

---

## 3. Architecture Observations

### 3.1 Dialect Feature Flags — Strengths and Gaps

The feature flag system in [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:32) is powerful, but:

- **Strengths**: Allows query builders to conditionally enable/disable SQL features at compile time.
- **Gaps**: Feature flags are not used consistently. For example, `SupportsCte` is declared but CTE methods are available on all query types regardless of dialect. Dialect-specific queries (PgSqlSelectQuery, MySqlSelectQuery, etc.) are responsible for only exposing supported features, but the base [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:7) unconditionally implements `ISupportCte`.

### 3.2 Virtual Table Pattern

The `TVirtualTable` parameter creates an interesting trade-off:

- **Pros**: Subqueries preserve column type information, enabling chained `.Field<T>()` access.
- **Cons**: Every query class must carry this parameter, and dialect-specific subclasses must specify it. The concrete type is determined by source generators, creating a tight coupling between code generation and the runtime type system.

### 3.3 Transaction Handling

The [`DbClientWithTransaction<TInstance, TDialect>`](Drizzle4Dotnet/src/Core/DbClient.cs:102) uses the CRTP pattern to return the concrete client type from `BeginTransactionAsync()`. This is clean but requires each concrete client to implement `CreateInstance()`. Consider whether a simpler composition-over-inheritance approach (e.g., a separate `TransactionScope` wrapper) would be more maintainable.

---

## 4. Specific File-Level Issues

### 4.1 [ISql.cs](Drizzle4Dotnet/src/Core/Shared/ISql.cs)

- Contains 7+ unrelated types (`ISqlBuilder`, `SqlBuilder`, `IGenericSql`, `ISql`, `IAliasedSql`, `AliasedSql`, `RawSql`, `RawSql<TReturn>`, `RawSubqueryTableSql`, `SqlExtensions`). These should be split into separate files by responsibility.
- `RawSubqueryTableSql.Create()` throws `NotImplementedException()` — dead code or placeholder that should be addressed.

### 4.2 [TableDefinitionComparer.cs](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinitionComparer.cs)

- Constraint and index comparison is not implemented — `Compare()` only checks column changes. Constraints and indexes are handled by [`SchemaSnapshot.Compare()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:118) but not by the lower-level `TableDefinitionComparer`.
- The `AlterAction` struct and `AlterActionType` enum are private nested types, making them untestable in isolation.

### 4.3 [SelectQuery.cs](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs)

- `DistinctOn()` is defined in [`ISupportDistinctOn`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:119) but not implemented in the base `SelectQuery` — only PostgreSQL-specific subclasses implement it. The interface should be moved or the base should provide a default no-op.
- `GroupBy` and `Having` are unique to `SelectQuery` but are not defined as clause interfaces — they're directly on the class. Consider extracting `ISupportGroupBy` and `ISupportHaving` for consistency.

### 4.4 [SchemaSnapshot.cs](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs)

- At ~757 lines, this file is too large and mixes several concerns: serialization (Serialize/Deserialize), comparison (Compare, CompareColumnSets, CompareConstraintSets, CompareIndexSets), conversion (ToTableDef, ToColDef, ToConstraint, ToSnapshotIndex, ToSnapshotConstraint), and migration planning (ToMigrationPlan, MigrationPlan, MigrationStep).
- [`SchemaDiff.ToMigrationPlan()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:524) contains complex switch/case logic that should be broken into smaller methods.

### 4.5 [DbColumn.cs](Drizzle4Dotnet/src/Core/Schema/Columns/DbColumn.cs)

- Uses `TDialect.BuildColumnName()` in the constructor (called at static initialization time), which means static column properties must access the dialect type at class load time. This works with source generation but could cause issues with lazy initialization or reflection-based loading.

---

## 5. Recommendations

### 5.1 Short-Term (High Impact, Low Effort)

1. **Split [ISql.cs](Drizzle4Dotnet/src/Core/Shared/ISql.cs)** into separate files per type.
2. **Add `CancellationToken`** to all `IQueryExecutor` async methods.
3. **Fix identifier quoting** in [`TableConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs) subclasses to use the dialect system instead of hardcoded double-quotes.
4. **Remove or implement** `RawSubqueryTableSql.Create()`.
5. **Centralize CTE implementation** into [`QueryBase`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs:8) to eliminate duplication.

### 5.2 Medium-Term

1. **Split [SchemaSnapshot.cs](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs)** into separate files: `SchemaSnapshot.cs`, `SchemaDiff.cs`, `MigrationPlan.cs`, `SnapshotTypes.cs` (SnapshotTable, SnapshotColumn, SnapshotIndex, SnapshotConstraint).
2. **Extract `GroupBy`/`Having`** into clause interfaces (`ISupportGroupBy`, `ISupportHaving`).
3. **Refactor CLI commands** to use a shared option parsing base class or command pattern.
4. **Add constraint/index comparison** to [`TableDefinitionComparer`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinitionComparer.cs:11).

### 5.3 Long-Term

1. **Evaluate CRTP necessity**: Consider whether removing `TSelf` and returning the base query type (losing some fluent subtype safety) would simplify the codebase enough to justify the trade-off.
2. **Simplify `TVirtualTable`**: Consider separating subquery/CTE capabilities into a separate system rather than threading it through all query types.
3. **Explore interceptors/pipelines**: Add pre/post execution hooks to `IQueryExecutor` for logging, auditing, or caching.
4. **Batch DDL execution**: The migration plan generates individual statements but doesn't batch them. Consider adding transaction wrapping or batch execution support.

---

## 6. Summary

| Category | Rating | Notes |
|---|---|---|
| **Type Safety** | ⭐⭐⭐⭐⭐ | Excellent use of generics for compile-time safety |
| **Architecture** | ⭐⭐⭐⭐ | Clean separation of concerns, but some over-engineering |
| **Extensibility** | ⭐⭐⭐⭐ | Dialect system makes adding new DB providers straightforward |
| **Migration System** | ⭐⭐⭐⭐⭐ | Comprehensive snapshot-diff-migration pipeline |
| **Code Organization** | ⭐⭐⭐ | Large files, mixed concerns, inconsistent naming |
| **Performance** | ⭐⭐⭐⭐⭐ | Static abstract dispatch, no reflection in hot paths |
| **Testability** | ⭐⭐⭐ | Good separation (IQueryExecutor), but nested types untestable |
| **Async Support** | ⭐⭐⭐ | Missing CancellationToken, otherwise solid |

The class design is **generally strong**, with particular excellence in type safety, the static abstract dialect pattern, and the migration pipeline. The main improvement areas are **code organization** (splitting large files, centralizing duplicate code) and **completeness** (fixing TODOs, adding constraint comparison).
