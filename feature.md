# Drizzle4Dotnet — Feature Inventory

> **Project:** Drizzle4Dotnet — A modern, type-safe SQL builder for .NET inspired by Drizzle ORM (TypeScript)  
> **Target:** .NET 10.0, Native AOT compatible  
> **Dialects:** PostgreSQL, MySQL/MariaDB  

---

## Table of Contents

1. [Query Builders (CRUD)](#1-query-builders-crud)
2. [SELECT Features](#2-select-features)
3. [INSERT Features](#3-insert-features)
4. [UPDATE Features](#4-update-features)
5. [DELETE Features](#5-delete-features)
6. [Dialect Support](#6-dialect-support)
7. [SQL Operators](#7-sql-operators)
8. [SQL Functions](#8-sql-functions)
9. [PostgreSQL-Specific Features](#9-postgresql-specific-features)
10. [MySQL-Specific Features](#10-mysql-specific-features)
11. [Window Functions](#11-window-functions)
12. [Set Operations / Compound Queries](#12-set-operations--compound-queries)
13. [CTEs / Recursive CTEs](#13-ctes--recursive-ctes)
14. [Subqueries](#14-subqueries)
15. [Schema / Migration System](#15-schema--migration-system)
16. [Source Generators](#16-source-generators)
17. [Execution / DbClient](#17-execution--dbclient)
18. [Expression Nodes / AST](#18-expression-nodes--ast)
19. [Utility Classes](#19-utility-classes)
20. [Shared / Demo / Test Infrastructure](#20-shared--demo--test-infrastructure)

---

## 1. Query Builders (CRUD)

| Feature | Description | Files |
|---------|-------------|-------|
| **Fluent Query Builder** | Method-chaining fluent API for building SQL queries | [`Drizzle4Dotnet/src/Core/Query/QueryBase.cs`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs) |
| **Build() method** | Compiles query to (SQL string, parameter dictionary) tuple | [`QueryBase.Build()`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs:20) |
| **C# `await` support** | Query is directly awaitable via `TaskAwaiter` | [`Query.GetAwaiter()`](Drizzle4Dotnet/src/Core/Query/Query.cs:14), [`Query<TReturn>.GetAwaiter()`](Drizzle4Dotnet/src/Core/Query/Query.cs:43) |
| **`Select<TReturn>(selectedColumns)`** | Start building a SELECT query with typed result | [`DbClient.Select()`](Drizzle4Dotnet/src/Core/DbClient.cs) |
| **`Insert<TTable>(table)`** | Start building an INSERT query | [`InsertQuery`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) |
| **`Update<TTable>(table)`** | Start building an UPDATE query | [`UpdateQuery`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs) |
| **`Delete<TTable>(table)`** | Start building a DELETE query | [`DeleteQuery`](Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs) |
| **`Returning()` extension** | Adds RETURNING clause to DML queries (INSERT/UPDATE/DELETE) with up to 16 columns | [`ReturningExtensions`](Drizzle4Dotnet/src/Core/Query/ReturningExtensions.cs) |
| **`ReturningQuery`** | Typed returning query wrapper for result mapping | [`ReturningQuery`](Drizzle4Dotnet/src/Core/Query/ReturningQuery.cs) |

---

## 2. SELECT Features

| Feature | Description | Files |
|---------|-------------|-------|
| **`From(table)`** | FROM clause with any `IGenericTable<TDialect>` | [`SelectQuery.From()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:138) |
| **`Where(conditions)`** | WHERE clause with AND conjunction | [`SelectQuery.Where()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:145) |
| **`GroupBy(columns)`** | GROUP BY clause | [`SelectQuery.GroupBy()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:157) |
| **`Having(conditions)`** | HAVING clause | [`SelectQuery.Having()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:169) |
| **`OrderBy(col, asc)`** | ORDER BY clause with ASC/DESC | [`SelectQuery.OrderBy()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:181) |
| **`Limit(n)`** | LIMIT clause | [`SelectQuery.Limit()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:187) |
| **`Offset(n)`** | OFFSET clause | [`SelectQuery.Offset()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:193) |
| **`Distinct()`** | SELECT DISTINCT | [`SelectQuery.Distinct()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:231) |
| **`Into(tableName)`** | SELECT ... INTO table | [`SelectQuery.Into()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:225) |
| **Joins** | INNER, LEFT, RIGHT, FULL, CROSS joins | [`SelectQuery.InnerJoin()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:209) |
| **Join ON clause** | Each join supports ON condition | [`SelectQuery.BuildSqlJoins()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:93) |
| **Typed result projections** | `ISelectedColumns<TReturn, TDialect>` for compile-time type safety | [`ISelectedColumns`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs) |
| **Mapper function** | Built-in `DbDataReader` → `TReturn` mapper | [`ISelectedColumns.Mapper`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:10) |
| **`AsSubQuery(alias)`** | Wraps SELECT as a subquery table | [`Query<TReturn>.AsSubQuery()`](Drizzle4Dotnet/src/Core/Query/Query.cs:48) |
| **With CTE** | `With(cteTable)` — attach CTE table to SELECT | [`SelectQuery.With()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:34) |
| **With Recursive CTE** | `WithRecursive(cteTables)` — attach recursive CTEs | [`SelectQuery.WithRecursive()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:40) |
| **Typed tuple columns** | Generic `TypedTupleSelectedColumns` for N-column projections (1–16 columns) | [`TypedTupleSelectedColumns`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs) |

---

## 3. INSERT Features

| Feature | Description | Files |
|---------|-------------|-------|
| **`Value(record)`** | Insert a single row via `IInsertRecord` | [`InsertQuery.Value()`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:28) |
| **`Values(records[])`** | Insert multiple rows | [`InsertQuery.Values()`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:36) |
| **`Value(dictionary)`** | Insert via column→value dictionary | [`InsertQuery.Value(Dictionary)`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:47) |
| **`DefaultValues()`** | INSERT DEFAULT VALUES | [`InsertQuery.DefaultValues()`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:89) |
| **`From(subquery)`** | INSERT ... SELECT from subquery | [`InsertQuery.From()`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:80) |
| **`With(cteTable)`** | CTE support for INSERT | [`InsertQuery.With()`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:22) |
| **`IInsertRecord<TTable, TDialect>`** | Type-safe record interface for insert values | [`IInsertRecord`](Drizzle4Dotnet/src/Core/Query/Insert/IInsertRecord.cs) |
| **Multi-row INSERT** | `VALUES (row1), (row2), ...` | [`InsertQuery.BuildSql()`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs:95) |

---

## 4. UPDATE Features

| Feature | Description | Files |
|---------|-------------|-------|
| **`Set(column, value)`** | Set column to a scalar value | [`UpdateQuery.Set()`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:24) |
| **`Set(column, ISql<T>)`** | Set column to an SQL expression | [`UpdateQuery.Set(column, ISql)`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:42) |
| **`Set(record)`** | Set multiple columns via `IUpdateRecord` | [`UpdateQuery.Set(record)`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:36) |
| **`Set(dictionary)`** | Set via column→value dictionary | [`UpdateQuery.Set(Dictionary)`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:48) |
| **`Where(conditions)`** | WHERE clause | [`UpdateQuery.Where()`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:57) |
| **`With(cteTable)`** | CTE support for UPDATE | [`UpdateQuery.With()`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:30) |
| **Expression values in SET** | ISql expressions rendered inline instead of parameterized | [`UpdateQuery.BuildSqlSet()`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:89) |

---

## 5. DELETE Features

| Feature | Description | Files |
|---------|-------------|-------|
| **`Where(conditions)`** | WHERE clause | [`DeleteQuery.Where()`](Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs:26) |
| **`With(cteTable)`** | CTE support for DELETE | [`DeleteQuery.With()`](Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs:20) |

---

## 6. Dialect Support

| Feature | Description | Files |
|---------|-------------|-------|
| **`ISqlDialect` interface** | Abstract SQL dialect with static abstract members | [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) |
| **`PgSqlSqlDialectImpl`** | PostgreSQL dialect — double-quote identifiers, RETURNING support, array/json support | [`PgSqlSqlDialectImpl`](Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs) |
| **`MySqlSqlDialectImpl`** | MySQL dialect — backtick identifiers, no RETURNING, ON DUPLICATE KEY UPDATE | [`MySqlSqlDialectImpl`](Drizzle4Dotnet/src/Dialect/MySqlSqlDialectImpl.cs) |
| **`SqlDialectDefaults`** | Shared default implementations for dialects | [`SqlDialectDefaults`](Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs) |
| **`BuildIdentifier()`** | Dialect-specific identifier quoting | [`ISqlDialect.BuildIdentifier()`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:9) |
| **`BuildTableName()`** | Schema-qualified table name building | [`ISqlDialect.BuildTableName()`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:11) |
| **`BuildColumnName()`** | Table-qualified column name building | [`ISqlDialect.BuildColumnName()`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:13) |
| **`BuildParameterName()`** | Parameter name formatting (`@p0`, `@p1`, etc.) | [`ISqlDialect.BuildParameterName()`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:15) |
| **`BuildLimitOffset()`** | LIMIT/OFFSET clause generation | [`ISqlDialect.BuildLimitOffset()`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:23) |
| **Feature flags** | `SupportsReturning`, `SupportsArrays`, `SupportsJson`, `SupportsWindowFunctions`, `SupportsCte`, `SupportsRecursiveCte`, `SupportsDeleteUsing`, `SupportsIsDistinctFrom`, `SupportsFilteredAggregates` | [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:29) |
| **`BuildOnDuplicateKeyUpdate()`** | Upsert clause building | [`ISqlDialect.BuildOnDuplicateKeyUpdate()`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:43) |
| **`EscapeString()`** | String escaping for SQL literals | [`ISqlDialect.EscapeString()`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:49) |

---

## 7. SQL Operators

| Feature | Description | Files |
|---------|-------------|-------|
| **`Eq`, `Ne`, `Lt`, `Gt`, `Ltq`, `Gtq`** | Comparison operators — column-to-value and column-to-column | [`Operators.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs) |
| **`And`, `Or`, `Xor`** | Logical operators — binary and N-ary (params) | [`Operators.And/Or/Xor`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:71) |
| **`Not`** | Unary NOT operator | [`Operators.Not()`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:149) |
| **`Like`, `NotLike`** | Pattern matching operators | [`Operators.Like/NotLike`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:80) |
| **`Contains`, `StartsWith`, `EndsWith`** | String pattern convenience methods | [`Operators.Contains/StartsWith/EndsWith`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:84) |
| **`In`, `NotIn`** | IN/NOT IN with value lists or subqueries | [`Operators.In/NotIn`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:113) |
| **`IsNull`, `IsNotNull`** | NULL checks | [`Operators.IsNull/IsNotNull`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:103) |
| **`Between`, `NotBetween`** | Range operators | [`Operators.Between/NotBetween`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:226) |
| **`Exists`** | EXISTS subquery operator | [`Operators.Exists()`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:222) |
| **`Add`, `Sub`, `Mul`, `Div`, `Mod`** | Arithmetic operators | [`Operators`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:153) |
| **`Concat`** | String concatenation (`\|\|`) | [`Operators.Concat()`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:205) |

---

## 8. SQL Functions

| Category | Functions | Files |
|----------|-----------|-------|
| **Aggregate** | `Count`, `CountDistinct`, `Sum`, `Avg`, `Min`, `Max`, `StdDev`, `Variance`, `VarSample`, `VarPop`, `StdDevSample`, `StdDevPop` | [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs:12) |
| **String** | `Upper`, `Lower`, `Trim`, `LTrim`, `RTrim`, `Length`, `Substring`, `Replace` | [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs:75) |
| **Numeric/Math** | `Abs`, `Ceil`, `Floor`, `Round`, `Round(decimals)`, `Power`, `Sqrt`, `Sign` | [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs:138) |
| **Date/Time** | `Now`, `CurrentTimestamp`, `CurrentDate` | [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs:192) |
| **Conditional** | `Coalesce`, `NullIf`, `IIf`, `Case` (via `CaseNode`) | [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs:204) |
| **Type Casting** | `Cast`, `CastToString`, `CastToInt`, `CastToLong`, `CastToDouble`, `CastToDateTime` | [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs:233) |

---

## 9. PostgreSQL-Specific Features

| Feature | Description | Files |
|---------|-------------|-------|
| **`PgInsertQuery`** | INSERT with ON CONFLICT (upsert): `OnConflict()`, `DoNothing()`, `DoUpdate()`, `SetOnConflict()`, `SetOnConflictExcluded()`, `OnConflictOnConstraint()`, `WhereConflictTarget()`, `WhereOnConflictSet()` | [`PgInsertQuery`](Drizzle4Dotnet/src/PgSql/PgInsertQuery.cs) |
| **`PgSelectQuery`** | SELECT with LATERAL joins, row-level locking | [`PgSelectQuery`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs) |
| **LATERAL Joins** | `InnerLateralJoin`, `LeftLateralJoin`, `CrossLateralJoin` | [`PgSelectQuery`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs:53) |
| **Row Locking** | `ForUpdate`, `ForNoKeyUpdate`, `ForShare`, `ForKeyShare` — all with NOWAIT/SKIP LOCKED, OF table support | [`PgSelectQuery`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs:68) |
| **Multiple Lock Clauses** | Supports combining multiple lock clauses in one SELECT | [`PgSelectQuery._lockClauses`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs:42) |
| **`PgLockSpec`** | Lock specification struct | [`PgLockSpec`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs:13) |
| **`PgUpdateQuery`** | UPDATE with `FROM` clause (UPDATE ... FROM joins) | [`PgUpdateQuery`](Drizzle4Dotnet/src/PgSql/PgUpdateQuery.cs) |
| **`PgDeleteQuery`** | DELETE with `USING` clause (DELETE ... USING joins) | [`PgDeleteQuery`](Drizzle4Dotnet/src/PgSql/PgDeleteQuery.cs) |
| **`PgOperators`** | PostgreSQL-specific operators: `IsDistinctFrom`, `IsNotDistinctFrom`, `All`, `Any`, `Some` (subquery quantifiers) | [`PgOperators`](Drizzle4Dotnet/src/PgSql/PgOperators.cs) |
| **`PgFunctions`** | PostgreSQL-specific functions: | [`PgFunctions`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs) |
| | **String:** `Position()` (POSITION ... IN syntax) | |
| | **Date/Time:** `Extract()`, `DateTrunc()`, `DateAdd()`, `DateDiff()`, `AtTimeZone()`, `Age()` | |
| | **JSON:** `JsonExtract()`, `JsonExtractText()`, `JsonAgg()`, `JsonBuildObject()`, `JsonArrayLength()`, `ToJson()`, `RowToJson()` | |
| | **Array:** `ArrayAgg()`, `Unnest()`, `ArrayLength()`, `ArrayAny()`, `ArrayAll()` | |
| | **Other:** `Random()`, `CastPg()` (:: syntax), `ConcatWs()` | |
| | **Window:** `RowNumber()`, `Rank()`, `DenseRank()`, `Ntile()`, `Lead()`, `Lag()`, `FirstValue()`, `LastValue()`, `NthValue()` | |
| **`PgSqlStatics`** | Static factories: `Interval()`, `TimeZone()`, `Excluded()` | [`PgSqlStatics`](Drizzle4Dotnet/src/PgSql/PgSqlStatics.cs) |
| **`PgIntervalNode`** | INTERVAL 'amount unit' expression node | [`PgIntervalNode`](Drizzle4Dotnet/src/PgSql/Nodes/PgIntervalNode.cs) |
| **`PgTimeZoneNode`** | Time zone literal node | [`PgTimeZoneNode`](Drizzle4Dotnet/src/PgSql/Nodes/PgTimeZoneNode.cs) |
| **`PgExcludedNode`** | EXCLUDED.column reference for ON CONFLICT | [`PgExcludedNode`](Drizzle4Dotnet/src/PgSql/Nodes/PgExcludedNode.cs) |
| **`PositionNode`** | POSITION(substring IN column) syntax | [`PositionNode`](Drizzle4Dotnet/src/PgSql/Nodes/PositionNode.cs) |
| **`PgDbClient`** | PostgreSQL DbClient implementation with Npgsql | [`PgSqlDbClient`](Drizzle4Dotnet/src/PgSql/PgSqlDbClient.cs) |

---

## 10. MySQL-Specific Features

| Feature | Description | Files |
|---------|-------------|-------|
| **`MySqlInsertQuery`** | INSERT with `OnDuplicateKeyUpdate()`, `OnDuplicateKeyUpdateAll()`, `Ignore()`, `Set()` (INSERT ... SET syntax) | [`MySqlInsertQuery`](Drizzle4Dotnet/src/MySql/MySqlInsertQuery.cs) |
| **`MySqlReplaceQuery`** | REPLACE INTO query (MySQL-specific upsert) | [`MySqlReplaceQuery`](Drizzle4Dotnet/src/MySql/MySqlReplaceQuery.cs) |
| **`MySqlSelectQuery`** | SELECT with `ForUpdate`, `ForShare` (NOWAIT/SKIP LOCKED) | [`MySqlSelectQuery`](Drizzle4Dotnet/src/MySql/MySqlSelectQuery.cs) |
| **`MySqlUpdateQuery`** | UPDATE with JOIN support, `Limit()`, `OrderBy()` | [`MySqlUpdateQuery`](Drizzle4Dotnet/src/MySql/MySqlUpdateQuery.cs) |
| **`MySqlDeleteQuery`** | DELETE with JOIN support, `Limit()`, `OrderBy()` | [`MySqlDeleteQuery`](Drizzle4Dotnet/src/MySql/MySqlDeleteQuery.cs) |
| **`MySqlFunctions`** | MySQL-specific functions | [`MySqlFunctions`](Drizzle4Dotnet/src/MySql/MySqlFunctions.cs) |
| **`MySqlOperators`** | MySQL-specific operators | [`MySqlOperators`](Drizzle4Dotnet/src/MySql/MySqlOperators.cs) |
| **`MySqlDbClient`** | MySQL DbClient implementation | [`MySqlDbClient`](Drizzle4Dotnet/src/MySql/MySqlDbClient.cs) |
| **`MySqlColumn`** | MySQL-specific column type | [`MySqlColumn`](Drizzle4Dotnet/src/MySql/MySqlColumn.cs) |
| **`MySqlTable`** | MySQL-specific table type | [`MySqlTable`](Drizzle4Dotnet/src/MySql/MySqlTable.cs) |
| **`MySqlIntervalNode`** | MySQL INTERVAL syntax (`INTERVAL 1 DAY`) | [`MySqlIntervalNode`](Drizzle4Dotnet/src/MySql/Nodes/MySqlIntervalNode.cs) |
| **`MySqlPositionNode`** | MySQL POSITION syntax | [`MySqlPositionNode`](Drizzle4Dotnet/src/MySql/Nodes/MySqlPositionNode.cs) |

---

## 11. Window Functions

| Feature | Description | Files |
|---------|-------------|-------|
| **`PgWindowFunctionNode<TReturn>`** | Represents `function(args) OVER (...)` | [`PgWindowFunctionNode`](Drizzle4Dotnet/src/PgSql/Nodes/PgWindowFunctionNode.cs) |
| **`.Over()` extension** | Appends OVER clause to any `IFunctionCallNode<T>` | [`PgWindowFunctionExtensions.Over()`](Drizzle4Dotnet/src/PgSql/Nodes/PgWindowFunctionNode.cs:47) |
| **Window functions** | `RowNumber()`, `Rank()`, `DenseRank()`, `Ntile()`, `Lead()`, `Lag()`, `FirstValue()`, `LastValue()`, `NthValue()` | [`PgFunctions`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs:160) |
| **`PgOverNode`** | OVER clause builder — PARTITION BY, ORDER BY, frame | [`PgOverNode`](Drizzle4Dotnet/src/PgSql/Nodes/PgOverNode.cs) |
| **`PgOverBuilder`** | Fluent builder: `PgOver.Create().PartitionBy(...).OrderBy(...).RowsBetween(...)` | [`PgOverBuilder`](Drizzle4Dotnet/src/PgSql/Nodes/PgOverNode.cs:51) |
| **`PgWindowFrame`** | Frame specification: `RowsBetween`, `RangeBetween`, `GroupsBetween` | [`PgWindowFrame`](Drizzle4Dotnet/src/PgSql/Nodes/PgWindowFrame.cs) |
| **Frame boundaries** | `UnboundedPreceding`, `Preceding(n)`, `CurrentRow`, `UnboundedFollowing`, `Following(n)` | [`PgWindowFrame`](Drizzle4Dotnet/src/PgSql/Nodes/PgWindowFrame.cs) |

---

## 12. Set Operations / Compound Queries

| Feature | Description | Files |
|---------|-------------|-------|
| **`CompoundQuery<TReturn, TDialect>`** | Combines two queries with UNION, INTERSECT, EXCEPT | [`CompoundQuery`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs) |
| **`Union()`** | UNION two queries | [`CompoundQueryExtensions.Union()`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:94) |
| **`UnionAll()`** | UNION ALL | [`CompoundQueryExtensions.UnionAll()`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:100) |
| **`Intersect()`** | INTERSECT | [`CompoundQueryExtensions.Intersect()`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:106) |
| **`Except()`** | EXCEPT | [`CompoundQueryExtensions.Except()`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:112) |
| **`AsRecursiveCte(alias)`** | Wraps compound query as a recursive CTE | [`CompoundQueryExtensions.AsRecursiveCte()`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:209) |
| **TVirtualTable versions** | Set operations with virtual table support for subquery composition | [`CompoundQuery<TReturn, TDialect, TVirtualTable>`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs:49) |

---

## 13. CTEs / Recursive CTEs

| Feature | Description | Files |
|---------|-------------|-------|
| **`ICteTable<TDialect>`** | Interface for CTE tables | [`ICteTable`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:17) |
| **`RecursiveCteTable`** | Recursive CTE table implementation | [`RecursiveCteTable`](Drizzle4Dotnet/src/Core/Schema/Tables/RecursiveCteTable.cs) |
| **`TypedTupleGeneratedCteTable`** | Typed CTE table from tuple projections | [`TypedTupleGeneratedCteTable`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:152) |
| **`TypedTupleAnonymousGeneratedCteTable`** | Typed CTE with custom shape | [`TypedTupleAnonymousGeneratedCteTable`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:174) |
| **`BuildSqlCte()`** | Builds WITH / WITH RECURSIVE clause before main statement | [`QueryBase.BuildSqlCte()`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs:32) |
| **`With(cteTable)`** | Attach CTE to SELECT/INSERT/UPDATE/DELETE | [`SelectQuery.With()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:34) |
| **`WithRecursive(cteTables)`** | Attach recursive CTEs | [`SelectQuery.WithRecursive()`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:40) |
| **`.AsCte()`** | Converts subquery table to CTE table | [`TypedTupleGeneratedSubqueryTable.AsCte()`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:73) |

---

## 14. Subqueries

| Feature | Description | Files |
|---------|-------------|-------|
| **`AsSubQuery(alias)`** | Wraps any query as a subquery table | [`Query<TReturn>.AsSubQuery()`](Drizzle4Dotnet/src/Core/Query/Query.cs:48) |
| **`AsSubQuery<T>(alias, selector)`** | Wraps query with typed column selector | [`Query<TReturn>.AsSubQuery<T>()`](Drizzle4Dotnet/src/Core/Query/Query.cs:54) |
| **`RawSubqueryTableSql<TDialect>`** | Raw SQL subquery table | [`RawSubqueryTableSql`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:155) |
| **`TypedTupleGeneratedSubqueryTable`** | Typed subquery table from tuple projections | [`TypedTupleGeneratedSubqueryTable`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:49) |
| **`TypedTupleAnonymousGeneratedSubqueryTable`** | Typed subquery with custom shape via lambda | [`TypedTupleAnonymousGeneratedSubqueryTable`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:115) |
| **`IVirtualTable<TDialect>`** | Interface for subquery/virtual tables | [`IVirtualTable`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:21) |
| **`IGetFieldByName`** | Dynamic field access on subquery tables | [`IGetFieldByName`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:18) |

---

## 15. Schema / Migration System

| Feature | Description | Files |
|---------|-------------|-------|
| **`TableAttribute`** | Marks a class as database table definition (name, schema, dialect, constraints) | [`TableAttribute`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:9) |
| **`AliasAttribute`** | Marks a class as table alias for self-joins | [`AliasAttribute`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:37) |
| **`VirtualAttribute`** | Marks a class as virtual table (subquery/CTE) | [`VirtualAttribute`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:55) |
| **`ColumnAttribute`** | Column definition: name, data type, default, auto-increment, nullability, check, comment, primary key | [`ColumnAttribute`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:8) |
| **`PrimaryKeyAttribute`** | Marks a property as primary key | [`PrimaryKeyAttribute`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:61) |
| **`ITable<TDialect>`** | Table interface with static abstract `TableRefName` | [`ITable`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:12) |
| **`IDbTable<TDialect>`** | Database table interface with `TableName`, `SchemaName` | [`IDbTable`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:27) |
| **`ITableAlias<TDialect>`** | Table alias interface with `Alias` | [`ITableAlias`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:33) |
| **`DbColumn<T, TTable, TDialect>`** | Generic typed column type | [`DbColumn`](Drizzle4Dotnet/src/Core/Schema/Columns/DbColumn.cs) |
| **`VirtualColumn<T, TDialect>`** | Virtual column for subquery references | [`VirtualColumn`](Drizzle4Dotnet/src/Core/Schema/Columns/VirtualColumn.cs) |
| **`IColumn<T>`** | Column interface | [`IColumn`](Drizzle4Dotnet/src/Core/Schema/Columns/IColumn.cs) |
| **`CreateTableQuery`** | CREATE TABLE DDL builder — columns, constraints, IF NOT EXISTS, TEMPORARY | [`CreateTableQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/CreateTableQuery.cs:8) |
| **`DropTableQuery`** | DROP TABLE DDL — IF EXISTS, CASCADE | [`DropTableQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/CreateTableQuery.cs:117) |
| **`AlterTableQuery`** | ALTER TABLE DDL — Add/Drop/Alter column, Set/Drop NOT NULL, Set/Drop Default, Rename column, Add/Drop constraint | [`AlterTableQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/CreateTableQuery.cs:325) |
| **`CreateIndexQuery`** | CREATE INDEX DDL — columns, UNIQUE, IF NOT EXISTS, USING type, partial WHERE | [`CreateIndexQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/CreateTableQuery.cs:171) |
| **`DropIndexQuery`** | DROP INDEX DDL — IF EXISTS, CASCADE | [`DropIndexQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/CreateTableQuery.cs:275) |
| **`ColumnDefinition`** | Column metadata for DDL generation | [`ColumnDefinition`](Drizzle4Dotnet/src/Core/Schema/Migration/ColumnDefinition.cs) |
| **`TableDefinition`** | Table metadata for DDL generation | [`TableDefinition`](Drizzle4Dotnet/src/Core/Schema/Migration/ColumnDefinition.cs) |
| **`SchemaSnapshot`** | Serializable schema snapshot (JSON) — tables, columns, data types, nullability, defaults | [`SchemaSnapshot`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:9) |
| **`SchemaSnapshot.Compare()`** | Diff two snapshots → `SchemaDiff` | [`SchemaSnapshot.Compare()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:71) |
| **`SchemaDiff`** | Schema differences — table added/removed/modified, column added/removed/type/null/default changed | [`SchemaDiff`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:218) |
| **`SchemaDiff.ToMigrationPlan()`** | Converts diff to executable migration plan | [`SchemaDiff.ToMigrationPlan()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:232) |
| **`MigrationPlan`** | Ordered list of migration steps with SQL generation | [`MigrationPlan`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:337) |
| **`MigrationStep`** | Single migration step — create/drop/alter table/index | [`MigrationStep`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:301) |
| **`MigrationManager<TDialect>`** | Applies migrations — ensures migration table, tracks applied, applies plans, creates migrations from diffs | [`MigrationManager`](Drizzle4Dotnet/src/Core/Schema/Migration/MigrationManager.cs) |
| **`AppliedMigration`** | Record of applied migration (name, timestamp, checksum) | [`AppliedMigration`](Drizzle4Dotnet/src/Core/Schema/Migration/MigrationManager.cs:139) |
| **`OrmSchemaExporter`** | Exports ORM schema to `SchemaSnapshot` | [`OrmSchemaExporter`](Drizzle4Dotnet/src/Core/Schema/Migration/OrmSchemaExporter.cs) |
| **`TableDefinitionComparer`** | Compares table definitions for equality/diff | [`TableDefinitionComparer`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinitionComparer.cs) |
| **CLR→SQL type mapping** | Maps C# types to SQL data types for DDL | [`DbClient.ClrTypeToSqlType()`](Drizzle4Dotnet/src/Core/DbClient.cs:112) |
| **`GetTableDefinition<TTable>()`** | Extracts `TableDefinition` from generated table type via reflection | [`DbClient.GetTableDefinition()`](Drizzle4Dotnet/src/Core/DbClient.cs:49) |

---

## 16. Source Generators

| Feature | Description | Files |
|---------|-------------|-------|
| **`TableGenerator`** | Roslyn incremental generator — produces strong-typed Table classes with Columns and ColumnNames from `[Table]`/`[Column]` attributes | [`TableGenerator`](SourceGenerators/SourceGenerators/TableGenerator.cs) |
| **`DbSelectGenerator`** | Roslyn incremental generator — produces `ISelection<...>` Record/Mapping types from `[DbSelect]` attribute | [`DbSelectGenerator`](SourceGenerators/SourceGenerators/DbSelectGenerator.cs) |
| **`MigrationSchemaGenerator`** | Roslyn incremental generator — produces schema snapshots and migration code | [`MigrationSchemaGenerator`](SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs) |
| **`DbSelectAttribute`** | Marks a class for select result generation | [`Attributes.cs`](Drizzle4Dotnet/src/Core/Shared/Attributes.cs) |
| **`MapWithAttribute`** | Maps query result to model class | [`Attributes.cs`](Drizzle4Dotnet/src/Core/Shared/Attributes.cs) |
| **`MapWithAliasAttribute`** | Maps with column alias support | [`Attributes.cs`](Drizzle4Dotnet/src/Core/Shared/Attributes.cs) |
| **Generated `Record`/`Model` types** | `ISelection<TModel, TRecord, TDialect>` with `.Record` and `.Mapping` properties | [`ISelection`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:36) |

---

## 17. Execution / DbClient

| Feature | Description | Files |
|---------|-------------|-------|
| **`DbClient<TDialect>`** | Abstract base client — connection management, query execution | [`DbClient`](Drizzle4Dotnet/src/Core/DbClient.cs) |
| **`ExecuteGetListAsync<T>(query)`** | Executes query, maps results using generated mapper | [`DbClient.ExecuteGetListAsync()`](Drizzle4Dotnet/src/Core/DbClient.cs:145) |
| **`ExecuteAsync(query)`** | Executes non-query (INSERT/UPDATE/DELETE) | [`DbClient.ExecuteAsync()`](Drizzle4Dotnet/src/Core/DbClient.cs:201) |
| **`ExecuteGetListAsync<T, TVirtualTable>(query)`** | Execute query with virtual table result type | [`DbClient.ExecuteGetListAsync<T, TVirtualTable>()`](Drizzle4Dotnet/src/Core/DbClient.cs:173) |
| **`DbClientWithTransaction<TInstance, TDialect>`** | Transaction support — `BeginTransactionAsync()`, `CommitAsync()`, `RollbackAsync()`, `RunInTransactionAsync()` | [`DbClientWithTransaction`](Drizzle4Dotnet/src/Core/DbClient.cs:232) |
| **`PgSqlDbClient`** | PostgreSQL concrete DbClient | [`PgSqlDbClient`](Drizzle4Dotnet/src/PgSql/PgSqlDbClient.cs) |
| **`MySqlDbClient`** | MySQL concrete DbClient | [`MySqlDbClient`](Drizzle4Dotnet/src/MySql/MySqlDbClient.cs) |
| **`IAsyncDisposable`** | DbClient implements `IAsyncDisposable` | [`DbClient`](Drizzle4Dotnet/src/Core/DbClient.cs:9) |
| **`SqlBuilder<TDialect>`** | Mutable SQL string builder with parameter management | [`SqlBuilder`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:16) |
| **`ISqlBuilder`** | Builder interface — `Append()`, `AddParameter()` | [`ISqlBuilder`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:9) |

---

## 18. Expression Nodes / AST

| Node Type | SQL Pattern | Files |
|-----------|-------------|-------|
| **`BinaryNode<T1, T2, TResult>`** | `expr1 operator expr2` (comparisons, arithmetic) | [`BinaryNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/BinaryNode.cs) |
| **`UnaryNode<T, TResult>`** | `operator expr` or `expr operator` (NOT, IS NULL, EXISTS) | [`UnaryNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/UnaryNode.cs) |
| **`NnaryNode<T, TResult>`** | `expr1 operator expr2 operator ...` (AND, OR, XOR with multiple args) | [`NnaryNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/NnaryNode.cs) |
| **`TrinaryNode<T, TResult>`** | `expr1 operator1 expr2 operator2 expr3` (BETWEEN) | [`TrinaryNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/TrinaryNode.cs) |
| **`FunctionCallNode<TReturn>`** | `FUNCTION_NAME(arg1, arg2, ...)` | [`FunctionCallNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/FunctionCallNode.cs) |
| **`SqlValueNode<T>`** | Parameterized value placeholder | [`SqlValueNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/SqlValueNode.cs) |
| **`CaseNode<T>`** | `CASE WHEN ... THEN ... ELSE ... END` | [`CaseNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/CaseNode.cs) |
| **`CastNode<T>`** | `CAST(expr AS type)` or `expr::type` (PostgreSQL) | [`CastNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/CastNode.cs) |
| **`FilteredAggregateNode`** | `AGG(...) FILTER (WHERE ...)` | [`FilteredAggregateNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Nodes/FilteredAggregateNode.cs) |
| **`SqlNullNode<T>`** | `NULL` literal | [`SqlNullNode`](Drizzle4Dotnet/src/Core/Shared/Sql.cs:54) |
| **`SqlDefaultNode`** | `DEFAULT` keyword | [`SqlDefaultNode`](Drizzle4Dotnet/src/Core/Shared/Sql.cs:65) |
| **`RawSql`** | Raw SQL fragment with parameter substitution | [`RawSql`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:100) |
| **`AliasedSql<T>`** | `(expr) AS alias` | [`AliasedSql`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:68) |
| **`BinarySqlListValueNode`** | `col IN (val1, val2, ...)` | [`BinarySqlListValueNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs) |
| **`NnaryAnyNode`** | `col IN (param1, param2, ...)` | [`NnaryAnyNode`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs) |

---

## 19. Utility Classes

| Utility | Description | Files |
|---------|-------------|-------|
| **`Sql`** | Static factory: `Value<T>()`, `Raw()`, `Raw<T>()`, `Literal()`, `Literal<T>()`, `Null<T>()`, `Default()` | [`Sql`](Drizzle4Dotnet/src/Core/Shared/Sql.cs) |
| **`SqlExtensions`** | Extension: `AsSubQuery()`, `As()` (aliasing) | [`SqlExtensions`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:91) |
| **`ISql<T>` / `ISql` / `IGenericSql`** | Core SQL expression interfaces | [`ISql`](Drizzle4Dotnet/src/Core/Shared/ISql.cs) |
| **`IOperator<T>`** | Operator result interface | [`IOperator`](Drizzle4Dotnet/src/Core/Shared/Operators/IOperator.cs) |
| **`IReturning<T, TDialect>`** | Interface for queries with typed results | [`IReturning`](Drizzle4Dotnet/src/Core/Shared/IReturning.cs) |
| **`IWriteRecord`** | Interface for write records (insert/update) | [`IWriteRecord`](Drizzle4Dotnet/src/Core/Shared/IWriteRecord.cs) |
| **`Utils`** | General utility methods | [`Utils`](Drizzle4Dotnet/src/Core/Shared/Utils.cs) |

---

## 20. Shared / Demo / Test Infrastructure

| Component | Description | Files |
|-----------|-------------|-------|
| **SharedDemo/PgSql** | PostgreSQL schema definitions and DTOs | [`SharedDemo/PgSql/`](SharedDemo/PgSql/) |
| **SharedDemo/MySql** | MySQL schema definitions and DTOs | [`SharedDemo/MySql/`](SharedDemo/MySql/) |
| **Test/Select** | SELECT query tests (PgSql + MySql, compound queries) | [`Test/Select/`](Test/Select/) |
| **Test/Insert** | INSERT query tests | [`Test/Insert/`](Test/Insert/) |
| **Test/Update** | UPDATE query tests | [`Test/Update/`](Test/Update/) |
| **Test/Delete** | DELETE query tests | [`Test/Delete/`](Test/Delete/) |
| **Test/Migration** | Migration system tests | [`Test/Migration/`](Test/Migration/) |
| **Benchmark** | Performance benchmarks | [`Benchmark/`](Benchmark/) |
| **Demo1** | Demo application | [`Demo1/`](Demo1/) |
| **PLAN.md** | PostgreSQL implementation plan | [`PLAN.md`](PLAN.md) |
| **PLAN-mysql.md** | MySQL implementation plan | [`PLAN-mysql.md`](PLAN-mysql.md) |

---

## Feature Count Summary

| Category | Feature Count |
|----------|---------------|
| Query Builders (CRUD) | 7 |
| SELECT Features | 22 |
| INSERT Features | 8 |
| UPDATE Features | 7 |
| DELETE Features | 2 |
| Dialect Support | 12 |
| SQL Operators | 20 |
| SQL Functions | 32 |
| PostgreSQL-Specific | 23 |
| MySQL-Specific | 10 |
| Window Functions | 13 |
| Set Operations | 8 |
| CTEs / Recursive CTEs | 9 |
| Subqueries | 7 |
| Schema / Migration | 26 |
| Source Generators | 7 |
| Execution / DbClient | 10 |
| Expression Nodes / AST | 13 |
| Utility Classes | 9 |
| **Total** | **~245+ features** |
