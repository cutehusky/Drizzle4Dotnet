# Drizzle4Dotnet — Complete Feature Inventory

> **Project:** Drizzle4Dotnet — A modern, type-safe SQL builder for .NET inspired by Drizzle ORM (TypeScript)  
> **Target:** .NET 10.0, Native AOT compatible  
> **Dialects:** PostgreSQL (PgSql), MySQL/MariaDB, Microsoft SQL Server (Mssql), Oracle

---

## Table of Contents

1. [Core Query Builders (CRUD)](#1-core-query-builders-crud)
2. [SELECT Features (All Dialects)](#2-select-features-all-dialects)
3. [INSERT Features (All Dialects)](#3-insert-features-all-dialects)
4. [UPDATE Features (All Dialects)](#4-update-features-all-dialects)
5. [DELETE Features (All Dialects)](#5-delete-features-all-dialects)
6. [PostgreSQL (PgSql) Dialect](#6-postgresql-pgsql-dialect)
7. [MySQL/MariaDB Dialect](#7-mysqlmariadb-dialect)
8. [Microsoft SQL Server (Mssql) Dialect](#8-microsoft-sql-server-mssql-dialect)
9. [Oracle Dialect](#9-oracle-dialect)
10. [Core SQL Operators](#10-core-sql-operators)
11. [Core SQL Functions](#11-core-sql-functions)
12. [PostgreSQL Operators & Functions](#12-postgresql-operators--functions)
13. [MySQL Operators & Functions](#13-mysql-operators--functions)
14. [MSSQL Operators & Functions](#14-mssql-operators--functions)
15. [Oracle Operators & Functions](#15-oracle-operators--functions)
16. [Window Functions](#16-window-functions)
17. [Set Operations / Compound Queries](#17-set-operations--compound-queries)
18. [CTEs / Recursive CTEs](#18-ctes--recursive-ctes)
19. [Subqueries & Virtual Tables](#19-subqueries--virtual-tables)
20. [Schema Definition System](#20-schema-definition-system)
21. [Migration System](#21-migration-system)
22. [Source Generators](#22-source-generators)
23. [CLI Tool](#23-cli-tool)
24. [Execution / DbClient](#24-execution--dbclient)
25. [Expression Nodes / AST](#25-expression-nodes--ast)
26. [Utility Classes & Interfaces](#26-utility-classes--interfaces)

---

## 1. Core Query Builders (CRUD)

All queries follow the **fluent builder pattern** with method chaining. Each query is directly `await`-able.

| Method | Description | SQL Output |
|---|---|---|
| `db.Select(col1, col2, ...)` | Start SELECT query (1–16 tuple columns) | `SELECT col1, col2` |
| `db.SelectDistinct(col1, ...)` | SELECT DISTINCT | `SELECT DISTINCT col1, ...` |
| `db.Insert(table)` | Start INSERT query | `INSERT INTO table ...` |
| `db.Update(table)` | Start UPDATE query | `UPDATE table ...` |
| `db.Delete(table)` | Start DELETE query | `DELETE FROM table ...` |
| `query.Returning(col1, ...)` | Add RETURNING (1–16 columns) | `... RETURNING col1, ...` |

**Fluent chaining:**
```csharp
await db.Select(users.Id, users.Name)
    .From(users)
    .Where(users.Age.Gt(18))
    .OrderBy(users.Name)
    .Limit(10);
```

**Awaitable query:**
```csharp
// Non-returning query (INSERT/UPDATE/DELETE):
await db.Insert(users).Value(...);

// Returning query (SELECT, or DML with RETURNING):
List<(int, string)> result = await db.Select(users.Id, users.Name).From(users);
```

---

## 2. SELECT Features (All Dialects)

| Method | Clause | SQL Sample |
|---|---|---|
| `.From(table)` | FROM | `FROM "users"` |
| `.Where(condition)` | WHERE | `WHERE ("age" > @p0)` |
| `.Where(cond1, cond2, ...)` | WHERE (AND) | `WHERE ("age" > @p0) AND ("status" = @p1)` |
| `.GroupBy(column)` | GROUP BY | `GROUP BY "dept_id"` |
| `.GroupBy(col1, col2, ...)` | GROUP BY (multi) | `GROUP BY "dept_id", "status"` |
| `.Having(condition)` | HAVING | `HAVING (COUNT(*) > @p0)` |
| `.OrderBy(col, asc: true)` | ORDER BY ASC | `ORDER BY "name" ASC` |
| `.OrderBy(col, asc: false)` | ORDER BY DESC | `ORDER BY "name" DESC` |
| `.OrderBy((col1, true), (col2, false))` | ORDER BY (multiple) | `ORDER BY "dept" ASC, "name" DESC` |
| `.Limit(n)` | LIMIT | `LIMIT @p0` |
| `.Offset(n)` | OFFSET | `OFFSET @p0` |
| `.Distinct()` | DISTINCT | `SELECT DISTINCT ...` |
| `.InnerJoin(table, on)` | INNER JOIN | `INNER JOIN "depts" ON ("users"."dept_id" = "depts"."id")` |
| `.LeftJoin(table, on)` | LEFT JOIN | `LEFT JOIN "depts" ON (...)` |
| `.RightJoin(table, on)` | RIGHT JOIN | `RIGHT JOIN "depts" ON (...)` |
| `.CrossJoin(table)` | CROSS JOIN | `CROSS JOIN "depts"` |
| `.With(cteTable)` | WITH (CTE) | `WITH "cte" AS (...) SELECT ...` |
| `.WithRecursive(cteTables)` | WITH RECURSIVE | `WITH RECURSIVE "cte" AS (...) SELECT ...` |

---

## 3. INSERT Features (All Dialects)

| Method | Description | SQL Sample |
|---|---|---|
| `.Value(IInsertRecord)` | Single row (typed record) | `INSERT INTO "users" ("name", "age") VALUES (@p0, @p1)` |
| `.Values(IInsertRecord[])` | Multiple rows (typed records) | `INSERT INTO "users" (...) VALUES (@p0, @p1), (@p2, @p3)` |
| `.Value(Dictionary)` | Single row (dictionary) | `INSERT INTO "users" (...) VALUES (@p0, @p1)` |
| `.Values(Dictionary[])` | Multiple rows (dictionaries) | `INSERT INTO "users" (...) VALUES (...), (...)` |
| `.DefaultValues()` | INSERT DEFAULT VALUES | `INSERT INTO "users" DEFAULT VALUES` |
| `.From(subquery)` | INSERT ... SELECT | `INSERT INTO "users" (...) SELECT ... FROM ...` |
| `.With(cteTable)` | CTE support | `WITH "cte" AS (...) INSERT INTO ...` |

---

## 4. UPDATE Features (All Dialects)

| Method | Description | SQL Sample |
|---|---|---|
| `.Set(column, value)` | Set column to scalar | `SET "name" = @p0` |
| `.Set(column, ISql<T>)` | Set column to expression | `SET "count" = ("count" + 1)` |
| `.Set(IUpdateRecord)` | Set from typed record | `SET "name" = @p0, "age" = @p1` |
| `.Set(Dictionary)` | Set from dictionary | `SET "name" = @p0, "age" = @p1` |
| `.Where(condition)` | WHERE clause | `WHERE ("id" = @p0)` |
| `.With(cteTable)` | CTE support | `WITH "cte" AS (...) UPDATE ...` |

---

## 5. DELETE Features (All Dialects)

| Method | Description | SQL Sample |
|---|---|---|
| `.Where(condition)` | WHERE clause | `DELETE FROM "users" WHERE ("id" = @p0)` |
| `.With(cteTable)` | CTE support | `WITH "cte" AS (...) DELETE FROM ...` |

---

## 6. PostgreSQL (PgSql) Dialect

### 6.1 Dialect Identity

| Property | Value |
|---|---|
| Namespace | `Drizzle4Dotnet.PgSql` |
| Dialect class | [`PgSqlSqlDialectImpl`](Drizzle4Dotnet/src/PgSql/PgSqlSqlDialectImpl.cs:7) |
| Identifier quoting | Double quotes: `"identifier"` |
| Parameter prefix | `@p0`, `@p1` (Npgsql compatible) |
| Table ref | `"schema"."table"` |
| Column ref | `"table"."column"` |

### 6.2 Feature Flags

| Flag | Value |
|---|---|
| `SupportsReturning` | ✅ Yes |
| `SupportsArrays` | ✅ Yes |
| `SupportsJson` | ✅ Yes |
| `SupportsWindowFunctions` | ✅ Yes |
| `SupportsCte` | ✅ Yes |
| `SupportsRecursiveCte` | ✅ Yes |
| `SupportsDeleteUsing` | ✅ Yes |
| `SupportsIsDistinctFrom` | ✅ Yes |
| `SupportsFilteredAggregates` | ✅ Yes |
| `SupportsFullOuterJoin` | ✅ Yes |
| `SupportsNaturalJoin` | ✅ Yes |
| `SupportsLateralJoin` | ✅ Yes |
| `SupportsApplyJoin` | ❌ No |

### 6.3 PgSql-Specific Query Types

#### PgSelectQuery
Extends `SelectQuery` with:
- **`DistinctOn(params IGenericSql[])`** — `SELECT DISTINCT ON (col1, col2) ...`
- **LATERAL joins**: `InnerLateralJoin()`, `LeftLateralJoin()`, `CrossLateralJoin()`
- **Row locking**: `ForUpdate()`, `ForNoKeyUpdate()`, `ForShare()`, `ForKeyShare()`
- **Lock options**: `.Nowait()`, `.SkipLocked()`, `.OfTable(table)`
- **Multiple lock clauses**: Supports combining multiple FOR clauses

**SQL syntax:**
```sql
SELECT ... FROM "users"
CROSS LATERAL JOIN LATERAL (SELECT ...) AS "sub"
WHERE ...
FOR UPDATE NOWAIT OF "users";
```

#### PgInsertQuery
Extends `InsertQuery` with upsert support:
- **`OnConflict(columns)`** — `ON CONFLICT (col1, col2)`
- **`OnConflictOnConstraint(name)`** — `ON CONFLICT ON CONSTRAINT name`
- **`DoNothing()`** — `ON CONFLICT ... DO NOTHING`
- **`DoUpdate()`** — `ON CONFLICT ... DO UPDATE SET ...`
- **`SetOnConflict(col, value)`** — SET column = value in ON CONFLICT
- **`SetOnConflictExcluded(col)`** — SET column = EXCLUDED.column
- **`WhereConflictTarget(condition)`** — ON CONFLICT WHERE ...
- **`WhereOnConflictSet(condition)`** — DO UPDATE SET WHERE ...

**SQL syntax:**
```sql
INSERT INTO "users" ("name", "email") VALUES (@p0, @p1)
ON CONFLICT ("email")
DO UPDATE SET "name" = EXCLUDED."name";
```

#### PgUpdateQuery
Extends `UpdateQuery` with:
- **`From(IGenericTable)`** — `UPDATE "users" SET ... FROM "other_table" WHERE ...`

**SQL syntax:**
```sql
UPDATE "users" SET "name" = @p0
FROM "departments"
WHERE "users"."dept_id" = "departments"."id";
```

#### PgDeleteQuery
Extends `DeleteQuery` with:
- **`Using(IGenericTable)`** — `DELETE FROM "users" USING "other_table" WHERE ...`
- **`Returning(params col)`** — `DELETE FROM ... RETURNING col1, col2`

**SQL syntax:**
```sql
DELETE FROM "users"
USING "departments"
WHERE "users"."dept_id" = "departments"."id";
```

---

## 7. MySQL/MariaDB Dialect

### 7.1 Dialect Identity

| Property | Value |
|---|---|
| Namespace | `Drizzle4Dotnet.MySql` |
| Dialect class | [`MySqlSqlDialectImpl`](Drizzle4Dotnet/src/MySql/MySqlSqlDialectImpl.cs) |
| Identifier quoting | Backticks: `` `identifier` `` |
| Parameter prefix | `@p0`, `@p1` (MySqlConnector compatible) |
| Table ref | `` `schema`.`table` `` |
| Column ref | `` `table`.`column` `` |

### 7.2 Feature Flags

| Flag | Value |
|---|---|
| `SupportsReturning` | ❌ No |
| `SupportsArrays` | ❌ No |
| `SupportsJson` | ✅ Yes |
| `SupportsWindowFunctions` | ✅ Yes (8.0+) |
| `SupportsCte` | ✅ Yes (8.0+) |
| `SupportsRecursiveCte` | ✅ Yes (8.0+) |
| `SupportsDeleteUsing` | ✅ Yes (uses JOIN) |
| `SupportsIsDistinctFrom` | ❌ No (`<=>` null-safe equals instead) |
| `SupportsFilteredAggregates` | ❌ No |
| `SupportsFullOuterJoin` | ❌ No |
| `SupportsNaturalJoin` | ✅ Yes |
| `SupportsLateralJoin` | ❌ No |
| `SupportsApplyJoin` | ❌ No |

### 7.3 MySql-Specific Query Types

#### MySqlInsertQuery
Extends `InsertQuery` with:
- **`OnDuplicateKeyUpdate(column, value)`** — `ON DUPLICATE KEY UPDATE col = VALUES(col)`
- **`OnDuplicateKeyUpdateAll()`** — Updates all columns on duplicate
- **`Ignore()`** — `INSERT IGNORE INTO ...`
- **`Set(col, value)`** — `INSERT INTO table SET col = value, col2 = value2` (MySQL-specific syntax)

**SQL syntax:**
```sql
INSERT INTO `users` (`name`, `email`) VALUES (@p0, @p1)
ON DUPLICATE KEY UPDATE `name` = VALUES(`name`);
```

#### MySqlReplaceQuery
- **`REPLACE INTO table (cols) VALUES (vals)`** — MySQL-specific upsert (DELETE + INSERT)

**SQL syntax:**
```sql
REPLACE INTO `users` (`name`, `email`) VALUES (@p0, @p1);
```

#### MySqlSelectQuery
Extends `SelectQuery` with:
- **`ForUpdate()`** — `SELECT ... FOR UPDATE`
- **`ForShare()`** — `SELECT ... FOR SHARE`
- Lock options: `.Nowait()`, `.SkipLocked()`

#### MySqlUpdateQuery
Extends `UpdateQuery` with:
- **`Limit(int)`** — `UPDATE ... LIMIT n`
- **`OrderBy(col, asc)`** — `UPDATE ... ORDER BY ...`
- **Join support**: `InnerJoin()`, `LeftJoin()`, `RightJoin()`

#### MySqlDeleteQuery
Extends `DeleteQuery` with:
- **`Limit(int)`** — `DELETE ... LIMIT n`
- **`OrderBy(col, asc)`** — `DELETE ... ORDER BY ...`
- **Join support**: `InnerJoin()`, `LeftJoin()`, `RightJoin()`

---

## 8. Microsoft SQL Server (Mssql) Dialect

### 8.1 Dialect Identity

| Property | Value |
|---|---|
| Namespace | `Drizzle4Dotnet.Mssql` |
| Dialect class | [`MssqlSqlDialectImpl`](Drizzle4Dotnet/src/Mssql/MssqlSqlDialectImpl.cs:12) |
| Identifier quoting | Square brackets: `[identifier]` |
| Parameter prefix | `@p0`, `@p1` (Microsoft.Data.SqlClient compatible) |
| Table ref | `[schema].[table]` or `[table]` (default dbo) |
| Column ref | `[table].[column]` |
| Limit/Offset | `OFFSET n ROWS FETCH NEXT m ROWS ONLY` |

### 8.2 Feature Flags

| Flag | Value |
|---|---|
| `SupportsReturning` | ✅ Yes (OUTPUT clause) |
| `SupportsArrays` | ❌ No |
| `SupportsJson` | ✅ Yes (2016+) |
| `SupportsWindowFunctions` | ✅ Yes (2005+) |
| `SupportsCte` | ✅ Yes (2005+) |
| `SupportsRecursiveCte` | ✅ Yes (2005+) |
| `SupportsDeleteUsing` | ❌ No (uses JOIN) |
| `SupportsIsDistinctFrom` | ❌ No |
| `SupportsFilteredAggregates` | ❌ No |
| `SupportsFullOuterJoin` | ✅ Yes |
| `SupportsNaturalJoin` | ❌ No |
| `SupportsLateralJoin` | ❌ No |
| `SupportsApplyJoin` | ✅ Yes (CROSS/OUTER APPLY) |

### 8.3 Mssql-Specific Query Types

#### MssqlSelectQuery
Extends `SelectQuery` with:
- **`Top(n)`** — `SELECT TOP (n) ...`
- **`TopWithTies(n)`** — `SELECT TOP (n) WITH TIES ...`
- **`TopPercent(n)`** — `SELECT TOP (n) PERCENT ...`
- **`CrossApply(table)`** — `CROSS APPLY` (MSSQL alternative to LATERAL JOIN)
- **`OuterApply(table)`** — `OUTER APPLY`
- **`FullJoin(table, on)`** — `FULL OUTER JOIN`
- **`WithHint(hint)`** — Table hints: `WITH (NOLOCK)`, `WITH (TABLOCK)`, etc.

**SQL syntax:**
```sql
SELECT TOP (10) [users].[id], [users].[name]
FROM [users] WITH (NOLOCK)
CROSS APPLY (SELECT ...) AS [sub]
WHERE ...
ORDER BY [name] ASC
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;
```

**Note:** MSSQL requires `ORDER BY` for `OFFSET/FETCH`. If no ORDER BY is provided but LIMIT/OFFSET is used, a workaround `ORDER BY (SELECT 0)` is added automatically.

#### MssqlInsertQuery
Extends `InsertQuery` with:
- **`OutputInserted(params column)`** — `OUTPUT INSERTED.[col1], INSERTED.[col2]` (MSSQL's equivalent of RETURNING)

**SQL syntax:**
```sql
INSERT INTO [users] ([name], [email])
OUTPUT INSERTED.[id], INSERTED.[name]
VALUES (@p0, @p1);
```

#### MssqlMergeQuery
**`MERGE`** statement with:
- **`Using(ISql)`** — Source table/subquery
- **`On(condition)`** — Match condition
- **`WhenMatchedThenUpdate(Dictionary)`** — `WHEN MATCHED THEN UPDATE SET ...`
- **`WhenNotMatchedThenInsert(columns, values)`** — `WHEN NOT MATCHED THEN INSERT ...`
- **`WhenNotMatchedBySourceThenDelete()`** — `WHEN NOT MATCHED BY SOURCE THEN DELETE`
- **`OutputInserted(columns)`** — `OUTPUT INSERTED.col`
- **`OutputDeleted(columns)`** — `OUTPUT DELETED.col`
- **Join support**: `InnerJoin()`, `LeftJoin()`, `RightJoin()`, `CrossJoin()`

**SQL syntax:**
```sql
MERGE [users] AS target
USING [source_table] AS source
ON (target.[id] = source.[id])
WHEN MATCHED THEN UPDATE SET target.[name] = source.[name]
WHEN NOT MATCHED THEN INSERT ([name]) VALUES (source.[name])
OUTPUT INSERTED.[id];
```

#### MssqlUpdateQuery / MssqlDeleteQuery
- Both use `TOP(n)` via [`MssqlTopNode`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlTopNode.cs) for limiting rows.
- `OUTPUT DELETED/INSERTED` support for UPDATE/DELETE.

---

## 9. Oracle Dialect

### 9.1 Dialect Identity

| Property | Value |
|---|---|
| Namespace | `Drizzle4Dotnet.Oracle` |
| Dialect class | [`OracleSqlDialectImpl`](Drizzle4Dotnet/src/Oracle/OracleSqlDialectImpl.cs:12) |
| Identifier quoting | Double quotes: `"identifier"` |
| Parameter prefix | `:p0`, `:p1` (Oracle.ManagedDataAccess.Core compatible) |
| Table ref | `"schema"."table"` |
| Column ref | `"table"."column"` |
| Limit/Offset | `OFFSET n ROWS FETCH NEXT m ROWS ONLY` (12c+) |

### 9.2 Feature Flags

| Flag | Value |
|---|---|
| `SupportsReturning` | ✅ Yes (RETURNING ... INTO) |
| `SupportsArrays` | ❌ No |
| `SupportsJson` | ✅ Yes (12c+) |
| `SupportsWindowFunctions` | ✅ Yes (9i+) |
| `SupportsCte` | ✅ Yes (9i+) |
| `SupportsRecursiveCte` | ✅ Yes (11gR2+) |
| `SupportsDeleteUsing` | ❌ No |
| `SupportsIsDistinctFrom` | ❌ No |
| `SupportsFilteredAggregates` | ❌ No |
| `SupportsFullOuterJoin` | ✅ Yes |
| `SupportsNaturalJoin` | ✅ Yes |
| `SupportsLateralJoin` | ✅ Yes (12c+) |
| `SupportsApplyJoin` | ❌ No |

### 9.3 Oracle-Specific Query Types

#### OracleSelectQuery
Extends `SelectQuery` with:
- **`ForUpdate()`** — `SELECT ... FOR UPDATE`
- **`ForUpdateOf(columns)`** — `SELECT ... FOR UPDATE OF col1, col2`
- **`ForUpdateNoWait()`** — `FOR UPDATE NOWAIT`
- **`ForUpdateWait(seconds)`** — `FOR UPDATE WAIT n`
- **`ForUpdateSkipLocked()`** — `FOR UPDATE SKIP LOCKED`

**SQL syntax:**
```sql
SELECT "id", "name" FROM "users"
WHERE "status" = :p0
FOR UPDATE OF "users" SKIP LOCKED
OFFSET 0 ROWS FETCH NEXT 10 ROWS ONLY;
```

#### OracleInsertQuery
- **`ReturningInto(params column)`** — Oracle's `RETURNING col1, col2 INTO :out1, :out2` syntax for capturing inserted values.
- **`OnConflict(columns)`** / `OnConflictOnConstraint(name)` — Oracle MERGE-based upsert
- **`DoNothing()`** / `DoUpdate()` — Upsert actions
- **`SetOnConflict(col, value)`** / `SetOnConflictExcluded(col)` — SET in upsert

#### OracleMergeQuery
- Full MERGE support similar to MSSQL:
  **`Using()`**, **`On()`**, **`WhenMatchedThenUpdate()`**, **`WhenNotMatchedThenInsert()`**
- Oracle-specific: `WhenMatchedThenDelete()` (DELETE WHERE clause in WHEN MATCHED)

**SQL syntax:**
```sql
MERGE INTO "users" target
USING (SELECT :p0 AS "id" FROM DUAL) source
ON (target."id" = source."id")
WHEN MATCHED THEN UPDATE SET target."name" = :p1
  DELETE WHERE target."status" = 'INACTIVE'
WHEN NOT MATCHED THEN INSERT ("id", "name") VALUES (source."id", :p1);
```

#### OracleUpdateQuery
- Standard UPDATE with support for `RETURNING ... INTO` clause.

#### OracleDeleteQuery
- Standard DELETE with support for `RETURNING ... INTO` clause.

---

## 10. Core SQL Operators

### 10.1 Comparison Operators

All operators have two forms:
- **Static**: `Operators.Eq(column, value)` / `Operators.Eq(column1, column2)`
- **Extension**: `column.Eq(value)` / `column.Eq(otherColumn)`

| Operator | Static Method | Extension Method | SQL Output |
|---|---|---|---|
| Equal | `Eq(c, value)` / `Eq(c1, c2)` | `.Eq(value)` / `.Eq(other)` | `col = @p0` / `col1 = col2` |
| Not Equal | `Ne(c, value)` / `Ne(c1, c2)` | `.Ne(value)` / `.Ne(other)` | `col <> @p0` / `col1 <> col2` |
| Less Than | `Lt(c, value)` / `Lt(c1, c2)` | `.Lt(value)` / `.Lt(other)` | `col < @p0` |
| Greater Than | `Gt(c, value)` / `Gt(c1, c2)` | `.Gt(value)` / `.Gt(other)` | `col > @p0` |
| Less or Equal | `Ltq(c, value)` / `Ltq(c1, c2)` | `.Ltq(value)` / `.Ltq(other)` | `col <= @p0` |
| Greater or Equal | `Gtq(c, value)` / `Gtq(c1, c2)` | `.Gtq(value)` / `.Gtq(other)` | `col >= @p0` |

### 10.2 Logical Operators

| Operator | Static Method | Extension Method | SQL Output |
|---|---|---|---|
| AND (binary) | `And(c1, c2)` | — | `(cond1) AND (cond2)` |
| AND (N-ary) | `And(params conditions)` | — | `(cond1) AND (cond2) AND ...` |
| OR (binary) | `Or(c1, c2)` | — | `(cond1) OR (cond2)` |
| OR (N-ary) | `Or(params conditions)` | — | `(cond1) OR (cond2) OR ...` |
| XOR | `Xor(c1, c2)` / `Xor(params)` | — | `(cond1) XOR (cond2)` |
| NOT | `Not(condition)` | — | `NOT (condition)` |

### 10.3 Null & Collection Operators

| Operator | Static Method | Extension Method | SQL Output |
|---|---|---|---|
| IS NULL | `IsNull(column)` | `.IsNull()` | `col IS NULL` |
| IS NOT NULL | `IsNotNull(column)` | `.IsNotNull()` | `col IS NOT NULL` |
| IN (values) | `In(c, values)` | `.In(values)` | `col IN (@p0, @p1, @p2)` |
| NOT IN (values) | `NotIn(c, values)` | `.NotIn(values)` | `col NOT IN (@p0, @p1)` |
| IN (subquery) | `In(c, subquery)` | `.In(subquery)` | `col IN (SELECT ...)` |
| EXISTS | `Exists(subquery)` | `.Exists()` (on subquery) | `EXISTS (SELECT ...)` |

### 10.4 Range Operators

| Operator | Static Method | Extension Method | SQL Output |
|---|---|---|---|
| BETWEEN | `Between(c, low, high)` | `.Between(low, high)` | `col BETWEEN @p0 AND @p1` |
| NOT BETWEEN | `NotBetween(c, low, high)` | `.NotBetween(low, high)` | `col NOT BETWEEN @p0 AND @p1` |

### 10.5 String Pattern Operators

| Operator | Extension Method | SQL Output |
|---|---|---|
| LIKE | `.Like(pattern)` | `col LIKE @p0` |
| NOT LIKE | `.NotLike(pattern)` | `col NOT LIKE @p0` |
| Contains | `.Contains(value)` | `col LIKE '%' \|\| @p0 \|\| '%'` |
| StartsWith | `.StartsWith(value)` | `col LIKE @p0 \|\| '%'` |
| EndsWith | `.EndsWith(value)` | `col LIKE '%' \|\| @p0` |

### 10.6 Arithmetic Operators

| Operator | Static Method | Extension Method | SQL Output |
|---|---|---|---|
| Addition | `Add(a, b)` | — | `(col + @p0)` |
| Subtraction | `Sub(a, b)` | — | `(col - @p0)` |
| Multiplication | `Mul(a, b)` | — | `(col * @p0)` |
| Division | `Div(a, b)` | — | `(col / @p0)` |
| Modulo | `Mod(a, b)` | — | `(col % @p0)` |
| Negation | `Neg(a)` | — | `(-col)` |

### 10.7 String Concatenation

| Method | SQL Output |
|---|---|
| `Concat(a, b)` | `(col1 \|\| col2)` |

---

## 11. Core SQL Functions

### 11.1 Aggregate Functions

| Function | SQL Output |
|---|---|
| `Functions.Count(column)` | `COUNT("col")` |
| `Functions.CountDistinct(column)` | `COUNT(DISTINCT "col")` |
| `Functions.Sum(column)` | `SUM("col")` |
| `Functions.Avg(column)` | `AVG("col")` |
| `Functions.Min(column)` | `MIN("col")` |
| `Functions.Max(column)` | `MAX("col")` |
| `Functions.StdDev(column)` | `STDDEV("col")` |
| `Functions.Variance(column)` | `VARIANCE("col")` |
| `Functions.VarSample(column)` | `VAR_SAMP("col")` |
| `Functions.VarPop(column)` | `VAR_POP("col")` |
| `Functions.StdDevSample(column)` | `STDDEV_SAMP("col")` |
| `Functions.StdDevPop(column)` | `STDDEV_POP("col")` |

**Filtered aggregates** (PgSql only):
```csharp
Functions.Count(col).Filter(condition)  // COUNT(col) FILTER (WHERE condition)
```

### 11.2 String Functions

| Function | SQL Output |
|---|---|
| `Functions.Upper(col)` | `UPPER("col")` |
| `Functions.Lower(col)` | `LOWER("col")` |
| `Functions.Trim(col)` | `TRIM("col")` |
| `Functions.LTrim(col)` | `LTRIM("col")` |
| `Functions.RTrim(col)` | `RTRIM("col")` |
| `Functions.Length(col)` | `LENGTH("col")` |
| `Functions.Substring(col, start, len)` | `SUBSTRING("col" FROM @p0 FOR @p1)` |
| `Functions.Replace(col, old, new)` | `REPLACE("col", @p0, @p1)` |
| `Functions.Concat(col1, col2, ...)` | `CONCAT("col1", "col2")` |

### 11.3 Numeric Functions

| Function | SQL Output |
|---|---|
| `Functions.Abs(col)` | `ABS("col")` |
| `Functions.Ceil(col)` | `CEIL("col")` |
| `Functions.Floor(col)` | `FLOOR("col")` |
| `Functions.Round(col)` | `ROUND("col")` |
| `Functions.Round(col, decimals)` | `ROUND("col", @p0)` |
| `Functions.Power(col, exp)` | `POWER("col", @p0)` |
| `Functions.Sqrt(col)` | `SQRT("col")` |
| `Functions.Sign(col)` | `SIGN("col")` |

### 11.4 Date/Time Functions

| Function | SQL Output |
|---|---|
| `Functions.Now()` | `NOW()` |
| `Functions.CurrentTimestamp` | `CURRENT_TIMESTAMP` |
| `Functions.CurrentDate` | `CURRENT_DATE` |

### 11.5 Conditional Functions

| Function | SQL Output |
|---|---|
| `Functions.Coalesce(col1, col2, ...)` | `COALESCE("col1", "col2")` |
| `Functions.NullIf(col1, col2)` | `NULLIF("col1", "col2")` |

### 11.6 CASE Expression

```csharp
// Static builder:
Functions.Case()
    .When(condition1, result1)
    .When(condition2, result2)
    .Else(defaultResult)
    .End();

// SQL:
// CASE WHEN (cond1) THEN @p0 WHEN (cond2) THEN @p1 ELSE @p2 END
```

### 11.7 Type Casting

| Function | SQL Output |
|---|---|
| `Functions.Cast(expr, typeName)` | `CAST(expr AS type)` |
| `Functions.CastToString(expr)` | `CAST(expr AS TEXT)` (PgSql) / `CAST(expr AS VARCHAR)` |
| `Functions.CastToInt(expr)` | `CAST(expr AS INTEGER)` |
| `Functions.CastToLong(expr)` | `CAST(expr AS BIGINT)` |
| `Functions.CastToDouble(expr)` | `CAST(expr AS DOUBLE PRECISION)` |
| `Functions.CastToDateTime(expr)` | `CAST(expr AS TIMESTAMP)` |

---

## 12. PostgreSQL Operators & Functions

### 12.1 PgOperators (PostgreSQL-Specific)

| Operator | Method | SQL Output |
|---|---|---|
| IS DISTINCT FROM | `col.IsDistinctFrom(value)` | `col IS DISTINCT FROM @p0` |
| IS NOT DISTINCT FROM | `col.IsNotDistinctFrom(value)` | `col IS NOT DISTINCT FROM @p0` |
| ALL (subquery) | `Operators.All(col, subquery)` | `col ALL (SELECT ...)` |
| ANY (subquery) | `Operators.Any(col, subquery)` | `col ANY (SELECT ...)` |
| SOME (subquery) | `Operators.Some(col, subquery)` | `col SOME (SELECT ...)` |
| JSON contains | `col.JsonContains(value)` | `col @> @p0` |
| JSON contained by | `col.JsonContainedBy(value)` | `col <@ @p0` |
| JSON exists key | `col.JsonExists(key)` | `col ? @p0` |
| JSON any key exists | `col.JsonAnyExists(keys)` | `col ?\| @p0` |
| JSON all keys exist | `col.JsonAllExist(keys)` | `col ?& @p0` |
| JSON path extract (->) | `col.JsonPath(expr)` | `col -> @p0` |
| JSON path extract text (->>) | `col.JsonPathText(expr)` | `col ->> @p0` |
| JSON path extract (#>) | `col.JsonObjectPath(expr)` | `col #> @p0` |
| JSON path extract text (#>>) | `col.JsonObjectPathText(expr)` | `col #>> @p0` |
| Full-text search (@@) | `col.TextSearch(query)` | `col @@ @p0` |
| Text search with config | `col.TextSearchWithConfig(config, query)` | `col @@ @p0` |
| Prefix match (^@) | `col.PrefixMatch(value)` | `col ^@ @p0` |
| Trigram similarity (%) | `col.TrigramSimilar(value)` | `col % @p0` |
| Trigram word similarity (%%%) | `col.TrigramWordSimilar(value)` | `col %%% @p0` |
| Distance (<->) | `col.DistanceTo(value)` | `col <-> @p0` |

### 12.2 PgFunctions (PostgreSQL-Specific)

| Category | Function | SQL Output |
|---|---|---|
| **String** | `PgFunctions.Position(substring, col)` | `POSITION(@p0 IN "col")` |
| **Date/Time** | `PgFunctions.Extract(field, col)` | `EXTRACT('year' FROM "col")` |
| | `PgFunctions.DateTrunc(part, col)` | `DATE_TRUNC('month', "col")` |
| | `PgFunctions.DateAdd(interval, col)` | `"col" + INTERVAL '1 day'` |
| | `PgFunctions.DateDiff(interval, col1, col2)` | `DATE_PART('day', "col2" - "col1")` |
| | `PgFunctions.AtTimeZone(col, tz)` | `"col" AT TIME ZONE 'UTC'` |
| | `PgFunctions.Age(col)` | `AGE("col")` |
| | `PgFunctions.Age(col1, col2)` | `AGE("col1", "col2")` |
| **JSON** | `PgFunctions.JsonExtract(col, path)` | `JSON_EXTRACT_PATH("col", @p0)` |
| | `PgFunctions.JsonExtractText(col, path)` | `JSON_EXTRACT_PATH_TEXT("col", @p0)` |
| | `PgFunctions.JsonAgg(expr)` | `JSON_AGG(expr)` |
| | `PgFunctions.JsonBuildObject(keys, vals)` | `JSON_BUILD_OBJECT(key1, val1, key2, val2)` |
| | `PgFunctions.JsonArrayLength(col)` | `JSON_ARRAY_LENGTH("col")` |
| | `PgFunctions.ToJson(expr)` | `TO_JSON(expr)` |
| | `PgFunctions.RowToJson(record)` | `ROW_TO_JSON(record)` |
| **Array** | `PgFunctions.ArrayAgg(expr)` | `ARRAY_AGG(expr)` |
| | `PgFunctions.Unnest(array)` | `UNNEST("array_col")` |
| | `PgFunctions.ArrayLength(array)` | `ARRAY_LENGTH("array_col", 1)` |
| | `PgFunctions.ArrayAny(expr)` | `(expr)` (any/some wrapper) |
| | `PgFunctions.ArrayAll(expr)` | `(expr)` (all wrapper) |
| **Numeric** | `PgFunctions.Random()` | `RANDOM()` |
| **Other** | `PgFunctions.ConcatWs(sep, val1, val2)` | `CONCAT_WS(@p0, @p1, @p2)` |
| | `PgFunctions.CastPg<T>(expr)` | `expr::type` (PostgreSQL cast syntax) |

### 12.3 PgSql Static Factories

| Factory | SQL Output |
|---|---|
| `PgSqlStatics.Interval("1 day")` | `INTERVAL '1 day'` |
| `PgSqlStatics.TimeZone("UTC")` | `TIME ZONE 'UTC'` |
| `PgSqlStatics.Excluded(column)` | `EXCLUDED."column"` (for ON CONFLICT) |

---

## 13. MySQL Operators & Functions

### 13.1 MySqlOperators (MySQL-Specific)

| Operator | Method | SQL Output |
|---|---|---|
| Null-safe equals | `col.NullSafeEq(value)` | `col <=> @p0` |
| Regex match | `col.RegexMatch(pattern)` | `col REGEXP @p0` |
| Regex not match | `col.NotRegexMatch(pattern)` | `col NOT REGEXP @p0` |
| RLIKE | `col.Rlike(pattern)` | `col RLIKE @p0` |

### 13.2 MySqlFunctions (MySQL-Specific)

| Category | Function | SQL Output |
|---|---|---|
| **Date/Time** | `MySqlFunctions.Now()` | `NOW()` |
| | `MySqlFunctions.CurDate()` | `CURDATE()` |
| | `MySqlFunctions.CurTime()` | `CURTIME()` |
| | `MySqlFunctions.DateAdd(unit, col, expr)` | `DATE_ADD("col", INTERVAL @p0 unit)` |
| | `MySqlFunctions.DateSub(unit, col, expr)` | `DATE_SUB("col", INTERVAL @p0 unit)` |
| | `MySqlFunctions.DateDiff(col1, col2)` | `DATEDIFF("col1", "col2")` |
| | `MySqlFunctions.DateFormat(col, format)` | `DATE_FORMAT("col", @p0)` |
| | `MySqlFunctions.Extract(unit, col)` | `EXTRACT(unit FROM "col")` |
| **JSON** | `MySqlFunctions.JsonExtract(col, path)` | `JSON_EXTRACT("col", @p0)` |
| | `MySqlFunctions.JsonUnquote(expr)` | `JSON_UNQUOTE(expr)` |
| | `MySqlFunctions.JsonContains(col, val)` | `JSON_CONTAINS("col", @p0)` |
| | `MySqlFunctions.JsonArrayAppend(col, path, val)` | `JSON_ARRAY_APPEND("col", @p0, @p1)` |
| | `MySqlFunctions.JsonObject(key, val)` | `JSON_OBJECT(key, val)` |
| | `MySqlFunctions.JsonArray(vals...)` | `JSON_ARRAY(vals...)` |
| **Numeric** | `MySqlFunctions.Rand()` | `RAND()` |
| | `MySqlFunctions.Truncate(col, decimals)` | `TRUNCATE("col", @p0)` |
| **Info** | `MySqlFunctions.Database()` | `DATABASE()` |
| | `MySqlFunctions.User()` | `USER()` |
| | `MySqlFunctions.Version()` | `VERSION()` |
| **String** | `MySqlFunctions.ConcatWs(sep, vals...)` | `CONCAT_WS(@p0, @p1, @p2)` |
| | `MySqlFunctions.GroupConcat(expr)` | `GROUP_CONCAT(expr)` |
| | `MySqlFunctions.GroupConcatDistinct(expr)` | `GROUP_CONCAT(DISTINCT expr)` |
| | `MySqlFunctions.FindInSet(str, strlist)` | `FIND_IN_SET(@p0, "col")` |

### 13.3 MySql Static Factories

| Factory | SQL Output |
|---|---|
| `MySqlStatics.Interval(value, unit)` | `INTERVAL @p0 unit` |

---

## 14. MSSQL Operators & Functions

### 14.1 MssqlFunctions (MSSQL-Specific)

| Category | Function | SQL Output |
|---|---|---|
| **Date/Time** | `MssqlFunctions.GetDate()` | `GETDATE()` |
| | `MssqlFunctions.SysDateTime()` | `SYSDATETIME()` |
| | `MssqlFunctions.DateAdd(part, val, col)` | `DATEADD(day, @p0, "col")` |
| | `MssqlFunctions.DateDiff(part, col1, col2)` | `DATEDIFF(day, "col1", "col2")` |
| | `MssqlFunctions.DatePart(part, col)` | `DATEPART(year, "col")` |
| | `MssqlFunctions.Year(col)` | `YEAR("col")` |
| | `MssqlFunctions.Month(col)` | `MONTH("col")` |
| | `MssqlFunctions.Day(col)` | `DAY("col")` |
| | `MssqlFunctions.Format(col, format)` | `FORMAT("col", @p0)` |
| | `MssqlFunctions.EOMonth(col)` | `EOMONTH("col")` |
| **JSON** | `MssqlFunctions.JsonValue(col, path)` | `JSON_VALUE("col", @p0)` |
| | `MssqlFunctions.JsonQuery(col, path)` | `JSON_QUERY("col", @p0)` |
| | `MssqlFunctions.JsonModify(col, path, val)` | `JSON_MODIFY("col", @p0, @p1)` |
| | `MssqlFunctions.OpenJson(content)` | `OPENJSON(@p0)` |
| **Numeric** | `MssqlFunctions.Rand()` | `RAND()` |
| | `MssqlFunctions.Round(col, decimals)` | `ROUND("col", @p0)` |
| | `MssqlFunctions.Ceiling(col)` | `CEILING("col")` |
| **Info** | `MssqlFunctions.DbName()` | `DB_NAME()` |
| | `MssqlFunctions.UserName()` | `USER_NAME()` |
| | `MssqlFunctions.ObjectName(id)` | `OBJECT_NAME(@p0)` |
| **String** | `MssqlFunctions.CharIndex(substr, col)` | `CHARINDEX(@p0, "col")` |
| | `MssqlFunctions.CharIndex(substr, col, start)` | `CHARINDEX(@p0, "col", @p1)` |
| | `MssqlFunctions.ConcatWs(sep, vals...)` | `CONCAT_WS(@p0, @p1, @p2)` |
| | `MssqlFunctions.StringAgg(expr, sep)` | `STRING_AGG(expr, @p0)` |
| | `MssqlFunctions.StringAggWithinGroup(expr, sep, order)` | `STRING_AGG(expr, @p0) WITHIN GROUP (ORDER BY ...)` |
| | `MssqlFunctions.Trim(col)` | `TRIM("col")` |
| | `MssqlFunctions.LTrim(col)` | `LTRIM("col")` |
| | `MssqlFunctions.RTrim(col)` | `RTRIM("col")` |
| | `MssqlFunctions.Len(col)` | `LEN("col")` |
| | `MssqlFunctions.Replace(col, old, new)` | `REPLACE("col", @p0, @p1)` |

### 14.2 Mssql Static Factories

| Factory | SQL Output |
|---|---|
| `MssqlStatics.NewId()` | `NEWID()` |
| `MssqlStatics.NewSequentialId()` | `NEWSEQUENTIALID()` |
| `MssqlStatics.CurrentTimestamp` | `CURRENT_TIMESTAMP` |
| `MssqlStatics.GetAnsiNull()` | `GETANSINULL()` |

### 14.3 Mssql Special Nodes

| Node | SQL Output |
|---|---|
| [`MssqlTopNode(n)`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlTopNode.cs) | `TOP (n)` / `TOP (n) WITH TIES` / `TOP (n) PERCENT` |
| [`MssqlTableHintNode(hint)`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlTableHintNode.cs) | `WITH (NOLOCK)` / `WITH (TABLOCK)` / `WITH (READUNCOMMITTED)` |
| [`MssqlOutputNode(cols)`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlOutputNode.cs) | `OUTPUT INSERTED.col, DELETED.col` |
| [`MssqlSequenceNode(name)`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlSequenceNode.cs) | `NEXT VALUE FOR [sequence]` |

---

## 15. Oracle Operators & Functions

### 15.1 OracleOperators (Oracle-Specific)

| Operator | Method | SQL Output |
|---|---|---|
| CONCAT | `col.OracleConcat(val)` | `col \|\| @p0` |
| String concatenation (`||`) | `OracleOperators.Concat(a, b)` | `("col1" \|\| "col2")` |

### 15.2 OracleFunctions (Oracle-Specific)

| Category | Function | SQL Output |
|---|---|---|
| **Date/Time** | `OracleFunctions.SysDate()` | `SYSDATE` |
| | `OracleFunctions.CurrentDate` | `CURRENT_DATE` |
| | `OracleFunctions.CurrentTimestamp` | `CURRENT_TIMESTAMP` |
| | `OracleFunctions.SysTimestamp()` | `SYSTIMESTAMP` |
| | `OracleFunctions.AddMonths(col, n)` | `ADD_MONTHS("col", :p0)` |
| | `OracleFunctions.LastDay(col)` | `LAST_DAY("col")` |
| | `OracleFunctions.MonthsBetween(col1, col2)` | `MONTHS_BETWEEN("col1", "col2")` |
| | `OracleFunctions.NextDay(col, day)` | `NEXT_DAY("col", :p0)` |
| | `OracleFunctions.Round(col, fmt)` | `ROUND("col", :p0)` |
| | `OracleFunctions.Trunc(col, fmt)` | `TRUNC("col", :p0)` |
| | `OracleFunctions.Extract(unit, col)` | `EXTRACT(year FROM "col")` |
| | `OracleFunctions.ToChar(col, fmt)` | `TO_CHAR("col", :p0)` |
| | `OracleFunctions.ToDate(str, fmt)` | `TO_DATE(:p0, :p1)` |
| | `OracleFunctions.ToTimestamp(str, fmt)` | `TO_TIMESTAMP(:p0, :p1)` |
| | `OracleFunctions.Interval(num, unit)` | `INTERVAL ':p0' unit` |
| **JSON** | `OracleFunctions.JsonValue(col, path)` | `JSON_VALUE("col", :p0)` |
| | `OracleFunctions.JsonQuery(col, path)` | `JSON_QUERY("col", :p0)` |
| | `OracleFunctions.JsonExists(col, path)` | `JSON_EXISTS("col", :p0)` |
| | `OracleFunctions.JsonArrayagg(expr)` | `JSON_ARRAYAGG(expr)` |
| | `OracleFunctions.JsonObjectagg(key, val)` | `JSON_OBJECTAGG(key : val)` |
| | `OracleFunctions.JsonTable(col, path)` | `JSON_TABLE("col", :p0)` |
| **Numeric** | `OracleFunctions.Round(col, decimals)` | `ROUND("col", :p0)` |
| | `OracleFunctions.Trunc(col, decimals)` | `TRUNC("col", :p0)` |
| | `OracleFunctions.Mod(col, divisor)` | `MOD("col", :p0)` |
| | `OracleFunctions.Remainder(col, divisor)` | `REMAINDER("col", :p0)` |
| | `OracleFunctions.Power(col, exp)` | `POWER("col", :p0)` |
| | `OracleFunctions.Sqrt(col)` | `SQRT("col")` |
| **String** | `OracleFunctions.Concat(col1, col2)` | `CONCAT("col1", "col2")` |
| | `OracleFunctions.ConcatWs(sep, vals...)` | `CONCAT_WS(:p0, "col1", "col2")` |
| | `OracleFunctions.Instr(col, substr)` | `INSTR("col", :p0)` |
| | `OracleFunctions.Length(col)` | `LENGTH("col")` |
| | `OracleFunctions.LengthB(col)` | `LENGTHB("col")` |
| | `OracleFunctions.Substr(col, start, len)` | `SUBSTR("col", :p0, :p1)` |
| | `OracleFunctions.Trim(col)` | `TRIM("col")` |
| | `OracleFunctions.LTrim(col, chars)` | `LTRIM("col", :p0)` |
| | `OracleFunctions.RTrim(col, chars)` | `RTRIM("col", :p0)` |
| | `OracleFunctions.Translate(col, from, to)` | `TRANSLATE("col", :p0, :p1)` |
| | `OracleFunctions.Replace(col, old, new)` | `REPLACE("col", :p0, :p1)` |
| | `OracleFunctions.RegexpReplace(col, pat, rep)` | `REGEXP_REPLACE("col", :p0, :p1)` |
| | `OracleFunctions.Listagg(col, sep)` | `LISTAGG("col", :p0)` |
| | `OracleFunctions.ListaggWithinGroup(col, sep, order)` | `LISTAGG("col", :p0) WITHIN GROUP (ORDER BY ...)` |
| **Info** | `OracleFunctions.User()` | `USER` |
| | `OracleFunctions.SysContext(namespace, param)` | `SYS_CONTEXT(:p0, :p1)` |
| | `OracleFunctions.Decode(expr, search, result)` | `DECODE(expr, :p0, :p1)` |
| **Analytic** | `OracleFunctions.RatioToReport(col)` | `RATIO_TO_REPORT("col") OVER ()` |
| | `OracleFunctions.CumeDist()` | `CUME_DIST() OVER (...)` |
| | `OracleFunctions.PercentRank()` | `PERCENT_RANK() OVER (...)` |
| | `OracleFunctions.PercentileCont(p)` | `PERCENTILE_CONT(:p0) WITHIN GROUP (ORDER BY ...)` |
| | `OracleFunctions.PercentileDisc(p)` | `PERCENTILE_DISC(:p0) WITHIN GROUP (ORDER BY ...)` |
| | `OracleFunctions.Median(col)` | `MEDIAN("col")` |
| | `OracleFunctions.StdDev(col)` | `STDDEV("col") OVER (...)` |
| | `OracleFunctions.Variance(col)` | `VARIANCE("col") OVER (...)` |

### 15.3 Oracle Special Nodes

| Node | SQL Output |
|---|---|
| [`OracleReturningNode(cols)`](Drizzle4Dotnet/src/Oracle/Operators/Nodes/OracleReturningNode.cs) | `RETURNING col1, col2 INTO :out1, :out2` |
| [`OracleRowIdNode`](Drizzle4Dotnet/src/Oracle/Operators/Nodes/OracleRowIdNode.cs) | `ROWID` |
| [`OracleSequenceNode(name)`](Drizzle4Dotnet/src/Oracle/Operators/Nodes/OracleSequenceNode.cs) | `"sequence".NEXTVAL` |

---

## 16. Window Functions

### 16.1 Available Window Functions

| Function | SQL Output |
|---|---|
| `PgFunctions.RowNumber()` | `ROW_NUMBER() OVER (...)` |
| `PgFunctions.Rank()` | `RANK() OVER (...)` |
| `PgFunctions.DenseRank()` | `DENSE_RANK() OVER (...)` |
| `PgFunctions.Ntile(n)` | `NTILE(@p0) OVER (...)` |
| `PgFunctions.Lead(col)` | `LEAD("col") OVER (...)` |
| `PgFunctions.Lead(col, offset, default)` | `LEAD("col", @p0, @p1) OVER (...)` |
| `PgFunctions.Lag(col)` | `LAG("col") OVER (...)` |
| `PgFunctions.Lag(col, offset, default)` | `LAG("col", @p0, @p1) OVER (...)` |
| `PgFunctions.FirstValue(col)` | `FIRST_VALUE("col") OVER (...)` |
| `PgFunctions.LastValue(col)` | `LAST_VALUE("col") OVER (...)` |
| `PgFunctions.NthValue(col, n)` | `NTH_VALUE("col", @p0) OVER (...)` |

### 16.2 OVER Clause Builder

```csharp
// Full fluent builder:
PgOver.Create()
    .PartitionBy(col1, col2)
    .OrderBy(col3, false)
    .RowsBetween(WindowFrame.UnboundedPreceding, WindowFrame.CurrentRow)

// SQL:
// OVER (PARTITION BY "col1", "col2" ORDER BY "col3" DESC ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)
```

### 16.3 Frame Specifications

| Boundary | Description |
|---|---|
| `WindowFrame.UnboundedPreceding` | `UNBOUNDED PRECEDING` |
| `WindowFrame.Preceding(n)` | `n PRECEDING` |
| `WindowFrame.CurrentRow` | `CURRENT ROW` |
| `WindowFrame.UnboundedFollowing` | `UNBOUNDED FOLLOWING` |
| `WindowFrame.Following(n)` | `n FOLLOWING` |

| Frame Mode | Method |
|---|---|
| ROWS | `.RowsBetween(start, end)` |
| RANGE | `.RangeBetween(start, end)` |
| GROUPS | `.GroupsBetween(start, end)` |

### 16.4 Using Window Functions

```csharp
// Method 1: Over() extension on aggregate functions
Functions.Sum(col).Over().PartitionBy(deptId).OrderBy(salary, false);

// Method 2: Dedicated window functions
PgFunctions.RowNumber().Over().OrderBy(salary, false);
```

---

## 17. Set Operations / Compound Queries

### 17.1 Basic Set Operations

| Method | SQL Operator |
|---|---|
| `query1.Union(query2)` | `(SELECT ...) UNION (SELECT ...)` |
| `query1.UnionAll(query2)` | `(SELECT ...) UNION ALL (SELECT ...)` |
| `query1.Intersect(query2)` | `(SELECT ...) INTERSECT (SELECT ...)` |
| `query1.Except(query2)` | `(SELECT ...) EXCEPT (SELECT ...)` |
| `query1.IntersectAll(query2)` | `(SELECT ...) INTERSECT ALL (SELECT ...)` |
| `query1.ExceptAll(query2)` | `(SELECT ...) EXCEPT ALL (SELECT ...)` |

### 17.2 CompoundQuery Features

- **`Limit(n)`** — Applies to the compound result
- **`Offset(n)`** — Applies to the compound result
- **`OrderBy(col, asc)`** — Orders the compound result
- **`With(cte)` / `WithRecursive(cte)`** — CTE support
- **`AsSubQuery(alias)`** — Wraps compound as subquery
- **`AsRecursiveCte(alias)`** — Wraps as recursive CTE

### 17.3 SQL Syntax

```sql
WITH "cte" AS (
  SELECT "id", "name" FROM "users"
  UNION ALL
  SELECT "id", "name" FROM "archived_users"
)
SELECT * FROM "cte"
ORDER BY "name" ASC
LIMIT @p0;
```

---

## 18. CTEs / Recursive CTEs — Deep Dive

### 18.1 CTE Table Types

| Type | Description | How It Works |
|---|---|---|
| `TypedTupleGeneratedCteTable<T,D>` | CTE from typed tuple columns | Wraps `TypedTupleGeneratedSubqueryTable` but overrides `BuildSql()` to render `"alias" AS (...)`, and `BuildRefSql()` to render just `"alias"` |
| `TypedTupleAnonymousGeneratedCteTable<TShape,T,D>` | CTE with custom shape via lambda | Same as above but stores a `TShape` object (the result of the shape lambda) for typed field access |
| `RecursiveCteTable<T,D>` | Recursive CTE from compound query | Wraps a compound query (anchor UNION ALL recursive) with an `IGetFieldByName` that delegates field lookup to the underlying `ITypedTupleSelectedColumns` |
| `RawSubqueryTableSql<TDialect>.AsCte()` | Raw SQL wrapped as CTE | Toggles `_isCte` flag — when true, `BuildSql()` renders `"alias" AS (...)` and `BuildRefSql()` renders just `"alias"` |

### 18.2 CTE Architecture and Flow

A CTE is created from a subquery through the `.AsCte()` method. The key insight is that `IVirtualTable` and `ICteTable` are separate interfaces but linked through the `.AsCte()` conversion:

```
IGenericSql (query)
  → .AsSubQuery("alias") → IVirtualTable (has .AsCte())
    → .AsCte() → ICteTable
```

The `IVirtualTable.BuildSql()` always renders `(subquery) AS "alias"` — suitable for FROM/JOIN clauses.
The `ICteTable.BuildSql()` renders `"alias" AS (subquery)` — suitable for the WITH clause.
The `ICteTable.BuildRefSql()` renders just `"alias"` — for referencing the CTE by name in FROM.

### 18.3 Tying CTE to Queries

```csharp
// Step 1: Create a CTE table from a subquery
var activeCte = query.AsSubQuery("active_users").AsCte();

// Step 2: Reference the CTE in a query using .With()
var result = await db.Select(activeCte.Field<long>("id"), activeCte.Field<string>("name"))
    .From(activeCte)           // FROM references the CTE by alias
    .With(activeCte);          // WITH defines the CTE

// SQL generated:
// WITH "active_users" AS (
//   SELECT "id", "name" FROM "users" WHERE "age" > 18
// )
// SELECT "active_users"."id", "active_users"."name" FROM "active_users"
```

### 18.4 Recursive CTE from Compound Query

```csharp
var cte = compoundQuery.AsRecursiveCte("org_tree");

var result = await db.Select(cte.Field<long>("id"), cte.Field<string>("name"))
    .From(cte)
    .WithRecursive(cte);

// SQL:
// WITH RECURSIVE "org_tree" AS (
//   SELECT ... FROM ... WHERE parent_id IS NULL
//   UNION ALL
//   SELECT ... FROM ... INNER JOIN "org_tree" ...
// )
// SELECT "org_tree"."id", "org_tree"."name" FROM "org_tree"
```

### 18.5 Generated CTE Tables (from TableGenerator)

For each table class, the TableGenerator generates both `GeneratedSubqueryTable` and `GeneratedCteTable`:

```csharp
// GeneratedSubqueryTable — for FROM/JOIN usage
public class GeneratedSubqueryTable : IVirtualTable<PgSqlSqlDialectImpl>
{
    public VirtualColumn<long, PgSqlSqlDialectImpl> Id { get; set; }
    // ...
    public GeneratedCteTable AsCte() => new GeneratedCteTable(AliasName, BaseSql);
}

// GeneratedCteTable — for WITH clause
public class GeneratedCteTable : GeneratedSubqueryTable, ICteTable<PgSqlSqlDialectImpl>
{
    // BuildSql override for CTE: "alias" AS (subquery)
    public override void BuildSql(ISqlBuilder sqlBuilder) { ... }
    // BuildRefSql override: just "alias"
    public override void BuildRefSql(ISqlBuilder sqlBuilder) { ... }
}
```

This enables typed CTE usage:
```csharp
var cte = db.Select(users.Id, users.Name).From(users).AsSubQuery("u").AsCte();
await db.Select(users.ResultAll).From(users).With(cte);
```

---

## 19. Subqueries & Virtual Tables — Deep Dive

### 19.1 The TVirtualTable Type Parameter

`TVirtualTable` is a **generic type parameter** threaded through the entire query system that carries the concrete subquery table type. This is the core mechanism enabling type-safe subquery composition.

```csharp
// SelectQuery carries TVirtualTable:
public class SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>
    where TVirtualTable : IVirtualTable<TDialect>

// AsSubQuery returns the concrete TVirtualTable:
public TVirtualTable AsSubQuery(string alias)
{
    return (TVirtualTable)TVirtualTable.Create(this, alias, SelectedColumns);
}
```

The `TVirtualTable` parameter propagates through the query chain:

```
db.Select(users.Id, users.Name)
  → returns SelectQuery<(long,string), PgSqlSqlDialectImpl, TypedTupleGeneratedSubqueryTable<(long,string), PgSqlSqlDialectImpl>, PgSqlSelectQuery<...>>
  
  .From(users)
  → still same type
  
  .AsSubQuery("u")
  → returns TypedTupleGeneratedSubqueryTable<(long,string), PgSqlSqlDialectImpl>
```

The concrete `TypedTupleGeneratedSubqueryTable<TReturn, TDialect>` implements:
- `IVirtualTable<TDialect>` — usable in FROM/JOIN
- `IGetFieldByName` — provides `.Field<T>(name)` for column access

### 19.2 How Typed Tuple Returns Work

When you call `db.Select(col1, col2)`, the extension method creates a `TypedTupleSelectedColumns<T1, T2, TDialect>` which:

1. **BuildSql()**: Renders the column SQL (e.g., `"Users"."Id", "Users"."Name"`)
2. **Mapper(DbDataReader)**: Reads columns by ordinal from the data reader and constructs a `(T1, T2)` value tuple
3. **IGetFieldByName**: Allows field lookup by column name for subquery usage

The Mapper is how `await query` returns `List<(long, string)>` — the `Query<TReturn, TDialect, TVirtualTable>.GetAwaiter()` calls `Executor.ExecuteGetListAsync(this)`, which iterates the reader and calls `SelectedColumns.Mapper(reader)` for each row.

### 19.3 Shape Mapping in Subqueries

The `AsSubQuery<T>(alias, shapeSelector)` overload provides **anonymous shape mapping**:

```csharp
var subQuery = query.AsSubQuery("sub", f => new {
    UserId = f.Field<long>("id"),
    FullName = f.Field<string>("name")
});
```

This creates a `TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, TDialect>`:

```csharp
public class TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, TDialect>
    : TypedTupleGeneratedSubqueryTable<TReturn, TDialect>
{
    public TShape Shape { get; }  // The result of the lambda

    // Constructor takes a Func<IGetFieldByName, TShape> and invokes it with `this`:
    public TypedTupleAnonymousGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns,
        Func<IGetFieldByName, TShape> shapeFunc
    ) : base(aliasName, baseSql, selectedColumns)
    {
        Shape = shapeFunc(this);  // Passes itself as IGetFieldByName
    }
}
```

The shape function receives the subquery table (as `IGetFieldByName`), and each `f.Field<T>("name")` call creates a `VirtualColumn<T, TDialect>` scoped to the subquery alias. The result is a strongly-typed shape object (`TShape`) whose properties are `VirtualColumn` instances you can use in subsequent query building:

```csharp
var result = await db.Select(subQuery.Shape.UserId, subQuery.Shape.FullName)
    .From(subQuery);
```

### 19.4 IGetFieldByName Field Resolution

The `IGetFieldByName` interface provides dynamic field access:

```csharp
public interface IGetFieldByName
{
    IAliasedSql<T> Field<T>(string columnName);
    IAliasedSql<T> Field<T>(IAliasedSql<T> column);
}
```

Implementations (in `TypedTupleSelectedColumns`, `GeneratedSubqueryTable`, `RecursiveCteTable`) look up columns by name and return the correct type:

```csharp
// In TypedTupleSelectedColumns<T1, T2, TDialect>:
public IAliasedSql<T> Field<T>(string columnName)
{
    if (columnName == _col1.Identifier) return _col1 as IAliasedSql<T>;
    if (columnName == _col2.Identifier) return _col2 as IAliasedSql<T>;
    throw new ArgumentException($"Column '{columnName}' not found in selection.");
}
```

This provides runtime type checking — if you request the wrong type for a column, the cast fails at runtime.

### 19.5 IVirtualTable.Create Factory Pattern

The `TVirtualTable.Create(IGenericSql, string, object)` static factory is how subquery tables are materialized:

```csharp
// In Query<TReturn, TDialect, TVirtualTable>:
public TVirtualTable AsSubQuery(string alias)
{
    return (TVirtualTable)TVirtualTable.Create(this, alias, SelectedColumns);
}

// In TypedTupleGeneratedSubqueryTable:
public static IVirtualTable<TDialect> Create(IGenericSql baseSql, string aliasName, object selectedColumns)
    => new TypedTupleGeneratedSubqueryTable<TReturn, TDialect>(
        aliasName,
        baseSql,
        (ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>>)selectedColumns);

// In GeneratedSubqueryTable (from TableGenerator):
public static IVirtualTable<PgSqlSqlDialectImpl> Create(IGenericSql baseSql, string aliasName, object _)
    => new GeneratedSubqueryTable(aliasName, baseSql);
```

The `TVirtualTable` type parameter determines which concrete `Create()` is called, enabling the correct subquery table type to be instantiated.

### 19.6 Subquery Table Type Summary

| Scenario | TVirtualTable Concrete Type | TShape Type | Usage |
|---|---|---|---|
| `db.Select(col1)` | `TypedTupleGeneratedSubqueryTable<T1, TDialect>` | N/A | `.Field<T1>(name)` |
| `db.Select(col1, col2).AsSubQuery("s")` | `TypedTupleGeneratedSubqueryTable<(T1,T2), D>` | N/A | `.Field<T1>("col1")` |
| `query.AsSubQuery("s", f => new {...})` | `TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, D>` | Anonymous type | `.Shape.Property` |
| Generated `Table.ResultAll` | `GeneratedSubqueryTable` (per-table) | N/A | `.Id`, `.Name` (typed VirtualColumns) |
| Raw SQL subquery | `RawSubqueryTableSql<TDialect>` | N/A | `.Field<T>(name)` |

### 19.7 Complete Subquery/CTE Flow Example

```csharp
// 1. Create a base query
var baseQuery = db.Select(users.Id, users.Name, users.Age)
    .From(users)
    .Where(users.Age.Gt(18));

// 2. Wrap as subquery with shape
var adultSub = baseQuery.AsSubQuery("adults", f => new {
    Id = f.Field<long>("id"),
    Name = f.Field<string>("name")
});

// 3. Use subquery in another query
var result = await db.Select(adultSub.Shape.Id, adultSub.Shape.Name)
    .From(adultSub);

// SQL: SELECT "adults"."id", "adults"."name" FROM (SELECT ... FROM "users" WHERE "age" > @p0) AS "adults"

// 4. Or use as CTE
var adultCte = adultSub.AsCte();
var result2 = await db.Select(adultCte.Field<long>("id"), adultCte.Field<string>("name"))
    .From(adultCte)
    .With(adultCte);

// SQL: WITH "adults" AS (SELECT ...) SELECT "adults"."id", "adults"."name" FROM "adults"
```

---

## 20. Schema Definition System

### 20.1 Table Attributes

| Attribute | Target | Properties |
|---|---|---|
| `[Table(name, schema = "")]` | Class | `Name`, `Schema`, `Dialect` (required Type) |
| `[Alias(table, alias)]` | Class | `Table` (Type), `Alias` (string) |
| `[Virtual]` | Class | — (marks subquery/CTE class) |

### 20.2 Column Attributes

| Attribute | Target | Properties |
|---|---|---|
| `[Column(name)]` | Property | `Name`, `DefaultValue`, `AutoIncrement`, `NotNull`, `Nullable`, `Check`, `Comment`, `PrimaryKey` |
| `[PrimaryKey]` | Property | — |
| `[DefaultValue(value)]` | Property | `Value` (raw SQL expression) |
| `[AutoIncrement]` | Property | — |
| `[NotNull]` | Property | — |
| `[Nullable]` | Property | — |
| `[Check(expression)]` | Property | `Expression` |
| `[Comment(comment)]` | Property | `Comment` |
| `SqlTypeAttribute` (abstract) | Property | `SqlType` (override for dialect-specific types) |

### 20.3 Constraint Attributes (Table-Level)

| Attribute | Repeatable | Properties |
|---|---|---|
| `[ForeignKeyConstraint(name, cols, foreignTable, foreignCols)]` | ✅ Yes | `ConstraintName`, `Columns`, `ForeignTable`, `ForeignColumns`, `ForeignSchema` |
| `[UniqueConstraint(columns)]` | ✅ Yes | `Columns`, `ConstraintName` |
| `[PrimaryKeyTableConstraint(columns)]` | ✅ Yes | `Columns`, `ConstraintName` |
| `[CheckTableConstraint(expression)]` | ✅ Yes | `Expression`, `ConstraintName` |
| `[Index(name, columns)]` | ✅ Yes | `IndexName`, `Columns`, `IsUnique`, `IndexType`, `Where` |

### 20.4 Table/Column Interfaces

```csharp
IGenericTable<TDialect>        // BuildRefSql()
  ├── ITable<TDialect>         // static abstract TableRefName
  │    ├── IDbTable<TDialect>  // static abstract TableName + SchemaName
  │    └── ITableAlias<TDialect>  // instance Alias
  ├── IVirtualTable<TDialect>  // static Create() + BuildSql()
  └── ICteTable<TDialect>      // BuildSql() (for WITH clause)

IColumn<T> : IAliasedSql<T>    // BuildSql() + Identifier
IColumnOfTable<TTable>         // Identifier property only

DbColumn<T, TTable, TDialect>  // Database column (from source generation)
VirtualColumn<T, TDialect>     // Virtual column (subquery/alias references)
```

---

## 21. Migration System

### 21.1 Core Data Model

| Class | Description |
|---|---|
| [`TableDefinition`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinition.cs:232) | Complete table: name, schema, columns, constraints, indexes |
| [`ColumnDefinition`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinition.cs:41) | Column: name, data type, nullable, PK, auto-increment, default, check, comment |
| [`TableConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:10) (abstract) | Base class for typed constraints |
| [`ForeignKeyConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:56) | FOREIGN KEY (cols) REFERENCES tbl (fcols) |
| [`UniqueConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:106) | UNIQUE (cols) |
| [`PrimaryKeyTableConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:132) | PRIMARY KEY (cols) |
| [`CheckTableConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:158) | CHECK (expr) |
| [`RawTableConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:34) | Raw SQL fallback |
| [`TableIndex`](Drizzle4Dotnet/src/Core/Schema/Migration/TableConstraint.cs:185) | Index: name, schema, table, columns, unique, type, where |
| [`ISqlDataType`](Drizzle4Dotnet/src/Core/Schema/Migration/SqlDataType.cs:8) | Interface for SQL data types |
| [`RawSqlDataType`](Drizzle4Dotnet/src/Core/Schema/Migration/SqlDataType.cs:22) | Raw string data type |

### 21.2 DDL Query Builders

| Builder | Key Methods | SQL Sample |
|---|---|---|
| [`CreateTableQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/Query/CreateTableQuery.cs) | `.IfNotExists()`, `.Temporary()` | `CREATE TABLE "schema"."table" (col TYPE, ...)` |
| [`DropTableQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/Query/DropTableQuery.cs) | `.IfExists()`, `.Cascade()` | `DROP TABLE IF EXISTS "schema"."table" CASCADE` |
| [`AlterTableQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/Query/AlterTableQuery.cs) | `AddColumn()`, `DropColumn()`, `AlterColumnType()`, `SetNotNull()`, `DropNotNull()`, `SetDefault()`, `DropDefault()`, `RenameColumn()`, `AddConstraint()`, `DropConstraint()` | `ALTER TABLE "schema"."table" ADD COLUMN ...` |
| [`CreateIndexQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/Query/CreateIndexQuery.cs) | `.On(columns)`, `.Unique()`, `.Using(type)`, `.Where(cond)`, `.IfNotExists()` | `CREATE UNIQUE INDEX "idx" ON "schema"."table" ("col") WHERE ...` |
| [`DropIndexQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/Query/DropIndexQuery.cs) | `.IfExists()`, `.Cascade()` | `DROP INDEX IF EXISTS "idx"` |
| [`CreateSchemaQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/Query/CreateSchemaQuery.cs) | `.IfNotExists()` | `CREATE SCHEMA IF NOT EXISTS "schema"` |
| [`CreateDatabaseQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/Query/CreateDatabaseQuery.cs) | `.IfNotExists()` | `CREATE DATABASE IF NOT EXISTS "db"` |

### 21.3 Snapshot System

| Class | Description |
|---|---|
| [`SchemaSnapshot`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:10) | JSON-serializable schema state |
| [`SnapshotTable`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:415) | Serializable table: name, schema, columns, constraints, indexes |
| [`SnapshotColumn`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:501) | Serializable column: name, type, nullable, PK, auto-increment, default |
| [`SnapshotIndex`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:435) | Serializable index |
| [`SnapshotConstraint`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:450) | Serializable constraint (FK, UNIQUE, PK, CHECK, RAW) |

### 21.4 Diff & Migration Pipeline

```csharp
SchemaSnapshot oldSnapshot = SchemaSnapshot.Deserialize(json);
SchemaSnapshot newSnapshot = OrmSchemaExporter.CreateSchemaSnapshot<TDialect>("v2", tableTypes);

SchemaDiff diff = oldSnapshot.Compare(newSnapshot);
MigrationPlan plan = diff.ToMigrationPlan("v2");
string sql = plan.ToSql<TDialect>();
```

| Component | Description |
|---|---|
| [`SchemaDiff`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:515) | List of `TableChange` (Added/Removed/Modified) |
| [`TableChange`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinition.cs:190) | Change descriptor with column changes, constraint changes, index changes |
| [`ColumnChange`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinition.cs:152) | Column-level change (Added/Removed/TypeChanged/NullabilityChanged/DefaultChanged) |
| [`MigrationPlan`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:731) | Ordered list of `MigrationStep` |
| [`MigrationStep`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:700) | Single DDL step with type, description, and SQL |
| [`TableDefinitionComparer`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinitionComparer.cs:11) | Low-level table comparison |

### 21.5 ORM Schema Export

```csharp
// Extract TableDefinition from generated table types:
TableDefinition def = OrmSchemaExporter.GetTableDefinition<UsersTable, PgSqlSqlDialectImpl>();
List<TableDefinition> defs = OrmSchemaExporter.GetTableDefinitions<PgSqlSqlDialectImpl>(typeof(UsersTable), typeof(DepartmentsTable));
SchemaSnapshot snapshot = OrmSchemaExporter.CreateSchemaSnapshot<PgSqlSqlDialectImpl>("v1", typeof(UsersTable));
```

### 21.6 DDL Factory

```csharp
Ddl.CreateTable(tableDef)
Ddl.DropTable("users", "public")
Ddl.AlterTable("users", "public")
Ddl.CreateIndex("idx_users_email", "users", "public")
Ddl.DropIndex("idx_users_email")
Ddl.CreateSchema("my_schema")
Ddl.CreateDatabase("my_db")
```

---

## 22. Source Generators — Deep Dive

The source generation system uses Roslyn incremental generators to produce strongly-typed code at compile time. Three generators work together to eliminate boilerplate and provide compile-time type safety.

### 22.1 TableGenerator ([`SourceGenerators/SourceGenerators/TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs))

**Input**: Partial classes with `[Table]` attribute and inner `Columns` class with `[Column]` properties.

**Output per table class** (e.g., `UsersTable` → generated `UsersTable.g.cs`):

#### 1. DbColumn Static Properties

Each `[Column]` property in the user-written `Columns` inner class generates a static `DbColumn<T, TTable, TDialect>` property:

```csharp
// User writes:
public partial class UsersTable
{
    public static class Columns
    {
        [PgSqlBigInt]
        [Column("Id")]
        [PrimaryKey]
        public static long Id { get; set; }
    }
}

// Source generator produces:
partial class UsersTable : IDbTable<PgSqlSqlDialectImpl>
{
    public static string TableName { get => "Users"; }
    public static string SchemaName { get => "public"; }
    public static string TableRefName { get => "\"Users\""; }

    [PgSqlBigInt]
    [PrimaryKey]
    public static DbColumn<long, UsersTable, PgSqlSqlDialectImpl> Id { get; set; }
    
    // ... other columns

    static UsersTable() {
        Id = new DbColumn<long, UsersTable, PgSqlSqlDialectImpl>("Id");
        // ... other columns
        _sql = PgSqlSqlDialectImpl.BuildTableName("public", "Users");
    }

    public static class ColumnNames
    {
        public const string Id = "Id";
        // ...
    }
}
```

The `DbColumn<T, TTable, TDialect>` is the runtime representation. Its constructor calls `TDialect.BuildColumnName(TTable.TableRefName, columnName)` to get the dialect-appropriate SQL string (e.g., `"Users"."Id"` for PgSql, `` `Users`.`Id` `` for MySql, `[Users].[Id]` for Mssql).

#### 2. Generated Select/Result Types

Each table gets pre-built selection types for common query patterns:

```csharp
// Pre-built ISelectedColumns for all-column selection:
public static ISelectedColumns<SelectResult, PgSqlSqlDialectImpl, GeneratedSubqueryTable> ResultAll { get; }
public static ISelectedColumns<SelectModel, PgSqlSqlDialectImpl, GeneratedSubqueryTable> ModelAll { get; }

// Struct tuple result (for value-typed selection):
public readonly record struct SelectResult(long Id, Guid Guid, string Name, ...);

// Class model result (for reference-typed selection):
public class SelectModel {
    public long Id { get; set; }
    public Guid Guid { get; set; }
    public string Name { get; set; }
    // ...
}
```

Usage:
```csharp
// Select all columns as struct tuple:
await db.Select(UsersTable.ResultAll).From(users);

// Select all columns as model class:
await db.Select(UsersTable.ModelAll).From(users);
```

#### 3. Generated Subquery and CTE Tables

Each table generates a `GeneratedSubqueryTable` class implementing `IVirtualTable<TDialect>`:

```csharp
public class GeneratedSubqueryTable : IVirtualTable<PgSqlSqlDialectImpl>
{
    // VirtualColumn for each table column, accessible after construction:
    public VirtualColumn<long, PgSqlSqlDialectImpl> Id { get; set; }
    public VirtualColumn<string, PgSqlSqlDialectImpl> Name { get; set; }

    // Constructor populates VirtualColumns with the alias:
    public GeneratedSubqueryTable(string aliasName, IGenericSql baseSql) {
        Id = new VirtualColumn<long, PgSqlSqlDialectImpl>(aliasName, "Id");
        Name = new VirtualColumn<string, PgSqlSqlDialectImpl>(aliasName, "Name");
    }

    // BuildSql renders: (subquery) AS "alias"
    public void BuildSql(ISqlBuilder sqlBuilder) { ... }

    // AsCte() returns a GeneratedCteTable wrapping this subquery
    public GeneratedCteTable AsCte() => new GeneratedCteTable(AliasName, BaseSql);

    // Static factory required by IVirtualTable<TDialect>:
    public static IVirtualTable<PgSqlSqlDialectImpl> Create(IGenericSql baseSql, string aliasName, object _)
        => new GeneratedSubqueryTable(aliasName, baseSql);
}
```

The `GeneratedCteTable` extends `GeneratedSubqueryTable` and overrides `BuildSql()` to render `"alias" AS (subquery)` for CTE syntax.

#### 4. Typed Insert Records (IInsertRecord)

Generated **`InsertModel`** (class) and **`InsertRecord`** (readonly record struct):

```csharp
// Generated class-based insert model:
public class InsertModel : IInsertRecord<UsersTable, PgSqlSqlDialectImpl>
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long? ManagerId { get; set; }

    public void Writer(Dictionary<string, object?> values)
    {
        if (Id != null) values["\"Id\""] = Id;
        if (Name != null) values["\"Name\""] = Name;
        if (ManagerId != null) values["\"ManagerId\""] = ManagerId;
    }
}

// Generated struct-based insert record:
public readonly record struct InsertRecord(long Id = default, string Name = default, long? ManagerId = default)
    : IInsertRecord<UsersTable, PgSqlSqlDialectImpl>
{
    public void Writer(Dictionary<string, object?> values) { ... }
}
```

Usage:
```csharp
await db.Insert(users).Value(new UsersTable.InsertRecord(Id: 1, Name: "Alice"));
await db.Insert(users).Value(new UsersTable.InsertModel { Name = "Bob", Age = 30 });
```

The `Writer()` method populates a dictionary with **dialect-quoted column names** as keys — the `BuildIdentifier("Id")` call produces `"Id"` for PgSql, `` `Id` `` for MySql, `[Id]` for Mssql.

#### 5. Typed Update Records (IUpdateRecord)

Generated **`UpdateModel`** (class) and **`UpdateRecord`** (readonly record struct) using `Optional<T>`:

```csharp
public readonly record struct UpdateRecord(
    Optional<long> Id = default,
    Optional<string> Name = default,
    Optional<long?> ManagerId = default
) : IUpdateRecord<UsersTable, PgSqlSqlDialectImpl>
{
    public void Writer(Dictionary<string, object?> values)
    {
        if (Id.HasValue) values["\"Id\""] = Id.Value;
        if (Name.HasValue) values["\"Name\""] = Name.Value;
        if (ManagerId.HasValue) values["\"ManagerId\""] = ManagerId.Value;
    }
}
```

The `Optional<T>` pattern is critical — it distinguishes "set to null" from "don't set":
- `Optional<long?>.From(null)` → sets column to NULL
- `default(Optional<long?>)` → skips the column entirely

Usage:
```csharp
await db.Update(users)
    .Set(new UsersTable.UpdateRecord(Name: "Updated Name"))
    .Where(users.Id.Eq(1));
```

#### 6. Alias Table Generation

Classes with `[Alias]` attribute get a generated table that copies the source table's column definitions but uses a different alias:

```csharp
[Alias(typeof(UsersTable), "Manager")]
public partial class ManagersTable { }

// Generated:
partial class ManagersTable : ITableAlias<PgSqlSqlDialectImpl>
{
    public string Alias { get => "Manager"; }
    public static string TableRefName { get => "\"Users\" AS \"Manager\""; }

    public static DbColumn<long, ManagersTable, PgSqlSqlDialectImpl> Id { get; set; }
    // ... all columns from UsersTable, but typed as ManagersTable

    static ManagersTable() {
        Id = new DbColumn<long, ManagersTable, PgSqlSqlDialectImpl>("Id");
        _sql = $"{\"Users\"} AS {\"Manager\"}";
    }
}
```

This enables self-joins:
```csharp
await db.Select(users.Id, managers.Name)
    .From(users)
    .LeftJoin(managers, users.ManagerId.Eq(managers.Id));
```

### 22.2 DbSelectGenerator ([`SourceGenerators/SourceGenerators/DbSelectGenerator.cs`](SourceGenerators/SourceGenerators/DbSelectGenerator.cs))

#### Pre-Generated TypedTupleSelectedColumns (Post-Initialization)

The DbSelectGenerator registers a **post-initialization output** that generates `TypedTupleSelectedColumns` for 1 to 16 columns at compile time. This is distributed as part of the library, not per-user code:

```csharp
// Generated for ALL combinations of 1–16 columns:
public class TypedTupleSelectedColumns<T1, ..., T16, TDialect>
    : ITypedTupleSelectedColumns<(T1, ..., T16), TDialect, TypedTupleGeneratedSubqueryTable<(T1, ..., T16), TDialect>>
```

Each class provides:
- **`BuildSql(ISqlBuilder)`** — renders comma-separated column references
- **`Mapper(DbDataReader)`** — reads columns by ordinal, returns typed `ValueTuple`
- **`As(alias)`** — creates new instance with `VirtualColumn` scoped to alias
- **`Field<T>(name)`** and **`Field<T>(column)`** — `IGetFieldByName` implementation

#### DbSelect Attribute — Custom Selection Types

The `[DbSelect]` attribute on user-defined classes enables custom query result mapping:

#### TypedTupleSelectedColumns (1–16 columns)

The DbSelectGenerator registers a post-initialization output that generates `TypedTupleSelectedColumns` classes for 1 to 16 columns. This is the mechanism behind typed tuple returns.

```csharp
// Generated for 2 columns:
public class TypedTupleSelectedColumns<T1, T2, TDialect>
    : ITypedTupleSelectedColumns<(T1, T2), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2), TDialect>>
    where TDialect : ISqlDialect
{
    private readonly IAliasedSql<T1> _col1;
    private readonly IAliasedSql<T2> _col2;

    public TypedTupleSelectedColumns(IAliasedSql<T1> col1, IAliasedSql<T2> col2)
    { _col1 = col1; _col2 = col2; }

    // Renders: col1, col2
    public void BuildSql(ISqlBuilder sqlBuilder) {
        _col1.BuildSql(sqlBuilder);
        sqlBuilder.Append(", ");
        _col2.BuildSql(sqlBuilder);
    }

    // Mapper reads columns by ordinal:
    public (T1, T2) Mapper(DbDataReader r) => (
        r.GetFieldValue<T1>(0),
        r.GetFieldValue<T2>(1)
    );

    // Field lookup by name (for IGetFieldByName):
    public IAliasedSql<T> Field<T>(string columnName) { ... }
    public IAliasedSql<T> Field<T>(IAliasedSql<T> column) { ... }

    // As(alias) creates a new instance with alias-scoped VirtualColumns:
    public ITypedTupleSelectedColumns<(T1, T2), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2), TDialect>>
        As(string alias) => new TypedTupleSelectedColumns<T1, T2, TDialect>(
            new VirtualColumn<T1, TDialect>(alias, _col1.Identifier),
            new VirtualColumn<T2, TDialect>(alias, _col2.Identifier)
        );
}
```

This is what enables the `Select(col1, col2)` and `Returning(col1, col2)` extension methods to return typed tuples:

```csharp
// Select returns SelectQuery<(long, string), PgSqlSqlDialectImpl, TypedTupleGeneratedSubqueryTable<(long, string), PgSqlSqlDialectImpl>>
var query = db.Select(users.Id, users.Name).From(users);

// Await returns List<(long, string)> with built-in mapper
List<(long Id, string Name)> results = await query;
```

#### DbSelect Attribute Processing

The DbSelectGenerator also processes classes marked with `[DbSelect]` to generate custom selection types:

```csharp
[DbSelect(Dialect = typeof(PgSqlSqlDialectImpl))]
public partial class UserDepartmentSelect
{
    [MapWith(typeof(UsersTable), "Id")] public long UserId { get; set; }
    [MapWith(typeof(UsersTable), "Name")] public string UserName { get; set; }
    [MapWith(typeof(DepartmentsTable), "Name")] public string DepartmentName { get; set; }
}

// Generated:
public partial class UserDepartmentSelect
{
    // ISelection with Record (tuple) and Mapping (model):
    public static ISelection<SelectModel, SelectRecord, PgSqlSqlDialectImpl> Selection { get; }
}
```

### 22.3 MigrationSchemaGenerator ([`SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs`](SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs))

- **Input**: Table types with schema attributes
- **Output**: Migration schema snapshots and C# migration code
- Generates `*_Migration.g.cs` files with embedded SQL scripts for each migration step

---

## 23. CLI Tool

### 23.1 Commands

| Command | Aliases | Description |
|---|---|---|
| `generate` | `gen`, `g` | Generate migration SQL + journal JSON |
| `snapshot` | `snap`, `s` | Create schema snapshot JSON |
| `apply` | `a` | Apply pending migrations to database |
| `status` | `st` | Show migration status |
| `debug` | `d` | Debug/inspection of schema types |

### 23.2 CLI Options

| Option | Description |
|---|---|
| `--provider, -p` | Database provider (`pgsql`, `mysql`, `mssql`, `sqlite`, `oracle`) |
| `--name, -n` | Migration/snapshot name (auto-generated if omitted) |
| `--types, -t` | Table types (comma-separated, e.g., `"Namespace.UsersTable,Namespace.DeptsTable"`) |
| `--output, -o` | Output directory (default: `./Migrations/{provider}`) |
| `--snapshot, -s` | Path to existing snapshot JSON for diff comparison |
| `--assembly, -a` | Path to assembly DLL |
| `--project, --proj` | Path to .csproj (builds before generating) |
| `--connection, -c` | Connection string (for apply/status) |
| `--verbose` | Enable verbose output |

### 23.3 Generate Flow

1. Parse CLI options
2. Build project or load assembly
3. Auto-discover table types (scan for `*Table` classes if `--types` not specified)
4. Load existing snapshot JSON if available
5. Extract `TableDefinition` via `OrmSchemaExporter` reflection
6. Create `SchemaSnapshot.FromTableDefinitions()`
7. Compare with existing snapshot via `SchemaSnapshot.Compare()`
8. Generate `MigrationPlan` via `SchemaDiff.ToMigrationPlan()`
9. Write migration SQL file + migration journal JSON + new snapshot JSON

---

## 24. Execution / DbClient

### 24.1 Database Client Hierarchy

```
DbClient<TDialect> : IQueryExecutor<TDialect>, IAsyncDisposable
  ├── PgSqlDbClient
  ├── MySqlDbClient
  ├── MssqlDbClient
  └── OracleDbClient

DbClientWithTransaction<TInstance, TDialect> : DbClient<TDialect>
  └── Transaction support: BeginTransactionAsync(), CommitAsync(), RollbackAsync()
      RunInTransactionAsync(Func<TInstance, Task>)
```

### 24.2 Query Execution

| Method | Description |
|---|---|
| `ExecuteGetListAsync<T, TVT>(IReturning)` | Execute returning query → `List<T>` |
| `ExecuteAsync(IGenericSql)` | Execute non-query (INSERT/UPDATE/DELETE) |
| `ExecuteScalarAsync<T>(IGenericSql)` | Execute scalar query → `T?` |
| `ExecuteReaderAsync(IGenericSql)` | Execute query → `DbDataReader` |

### 24.3 SQL Builder

| Component | Description |
|---|---|
| [`ISqlBuilder`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:9) | Interface: `Append()`, `AddParameter()`, `Build()` |
| [`SqlBuilder<TDialect>`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:17) | Implementation: `StringBuilder` + parameter dictionary |

---

## 25. Expression Nodes / AST

| Node | Type Params | SQL Pattern | Used For |
|---|---|---|---|
| [`BinaryNode<T1,T2,TResult>`](Drizzle4Dotnet/src/Core/Operators/Nodes/BinaryNode.cs) | 3 | `c1 OP c2` | Comparisons, arithmetic, AND/OR |
| [`UnaryNode<T,TResult>`](Drizzle4Dotnet/src/Core/Operators/Nodes/UnaryNode.cs) | 2 | `OP c` or `c OP` | NOT, IS NULL, IS NOT NULL, EXISTS |
| [`NnaryNode<T,TResult>`](Drizzle4Dotnet/src/Core/Operators/Nodes/NnaryNode.cs) | 2 | `c1 OP c2 OP ...` | AND/OR/XOR (N-ary) |
| [`TrinaryNode<T,TResult>`](Drizzle4Dotnet/src/Core/Operators/Nodes/TrinaryNode.cs) | 2 | `c1 OP1 c2 OP2 c3` | BETWEEN, NOT BETWEEN |
| [`FunctionCallNode<TReturn>`](Drizzle4Dotnet/src/Core/Operators/Nodes/FunctionCallNode.cs) | 1 | `FUNC(args)` | All SQL functions |
| [`SqlValueNode<T>`](Drizzle4Dotnet/src/Core/Operators/Nodes/SqlValueNode.cs) | 1 | Parameter placeholder | Literal values |
| [`CaseNode<T>`](Drizzle4Dotnet/src/Core/Operators/Nodes/CaseNode.cs) | 1 | `CASE WHEN ... END` | CASE expressions |
| [`CastNode<T>`](Drizzle4Dotnet/src/Core/Operators/Nodes/CastNode.cs) | 1 | `CAST(expr AS type)` | Type casting |
| [`FilteredAggregateNode`](Drizzle4Dotnet/src/Core/Operators/Nodes/FilteredAggregateNode.cs) | 0 | `AGG(...) FILTER (WHERE ...)` | Filtered aggregates |

### Dialect-Specific Nodes

| Node | Dialect | SQL Pattern |
|---|---|---|
| [`MssqlTopNode`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlTopNode.cs) | MSSQL | `TOP (n) [WITH TIES] [PERCENT]` |
| [`MssqlTableHintNode`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlTableHintNode.cs) | MSSQL | `WITH (NOLOCK)` |
| [`MssqlOutputNode`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlOutputNode.cs) | MSSQL | `OUTPUT INSERTED.col, DELETED.col` |
| [`MssqlSequenceNode`](Drizzle4Dotnet/src/Mssql/Operators/Nodes/MssqlSequenceNode.cs) | MSSQL | `NEXT VALUE FOR [sequence]` |
| [`OracleReturningNode`](Drizzle4Dotnet/src/Oracle/Operators/Nodes/OracleReturningNode.cs) | Oracle | `RETURNING col INTO :out` |
| [`OracleRowIdNode`](Drizzle4Dotnet/src/Oracle/Operators/Nodes/OracleRowIdNode.cs) | Oracle | `ROWID` |
| [`OracleSequenceNode`](Drizzle4Dotnet/src/Oracle/Operators/Nodes/OracleSequenceNode.cs) | Oracle | `"seq".NEXTVAL` |
| [`MySqlIntervalNode`](Drizzle4Dotnet/src/MySql/Operators/Nodes/MySqlIntervalNode.cs) | MySQL | `INTERVAL @p0 unit` |

---

## 26. Utility Classes & Interfaces

### 26.1 Core Interfaces

| Interface | Purpose |
|---|---|
| [`IGenericSql`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:47) | Base SQL expression: `BuildSql(ISqlBuilder)` |
| [`ISql<TReturn>`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:52) | Typed SQL expression |
| [`ISql`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:56) | Marker interface for untyped SQL |
| [`IAliasedSql<T>`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:60) | SQL with `Identifier` (alias/column name) |
| [`IAwaitableQuery<TReturn>`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs:43) | Awaitable returning query |
| [`IAwaitableQuery`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs:48) | Awaitable non-returning query |
| [`IReturning<T,D,VT>`](Drizzle4Dotnet/src/Core/Shared/IReturning.cs:7) | Query with typed results + `Mapper` |
| [`IWriteRecord`](Drizzle4Dotnet/src/Core/Shared/IWriteRecord.cs:3) | Write record: `Writer(Dictionary)` |
| [`ISelectedColumns<T,D,VT>`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:8) | Column selection: `BuildSql()` + `Mapper(DbDataReader)` |
| [`ITypedTupleSelectedColumns<T,D,VT>`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:19) | Tuple column selection with `As(alias)` + `IGetFieldByName` |
| [`ISelection<TModel,TRecord,D,VT>`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:31) | Static abstract `Record` + `Mapping` for generated select types |
| [`IGetFieldByName`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:13) | Dynamic field access on subquery tables |

#### ISelectedColumns — The Central Selection Abstraction

[`ISelectedColumns<TReturn, TDialect, TVirtualTable>`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:8) is the interface that ties SQL generation to result mapping:

```csharp
public interface ISelectedColumns<TReturn, TDialect, TVirtualTable> : ISql
    where TDialect : ISqlDialect
    where TVirtualTable : IVirtualTable<TDialect>
{
    // BuildSql renders the SELECT column list (e.g., "col1", "col2")
    void BuildSql(ISqlBuilder sqlBuilder);  // inherited from ISql

    // Mapper reads DbDataReader and constructs TReturn
    TReturn Mapper(DbDataReader r);
}
```

There are three implementations:

1. **`TypedTupleSelectedColumns<T1..T16, TDialect>`** — for `db.Select(col1, col2, ...)` returning value tuples
2. **`GeneratedResultSelection` / `GeneratedModelSelection`** — generated per-table for all-column select (e.g., `UsersTable.ResultAll`)
3. **`[DbSelect]` generated types** — for custom query result mapping with `[MapWith]` attributes

#### ISelection — Static Abstract Selection Factory

[`ISelection<TModel, TRecord, TDialect, TVirtualTable>`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs:31) uses C# 11 `static abstract` to provide pre-built Record and Mapping selections:

```csharp
public interface ISelection<TReturnModel, TReturnRecord, TDialect, TVirtualTable>
    where TDialect : ISqlDialect
    where TVirtualTable : IVirtualTable<TDialect>
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect, TVirtualTable> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect, TVirtualTable> Mapping { get; }
}

// Usage in generated DbSelect types:
public static ISelection<SelectModel, SelectRecord, PgSqlSqlDialectImpl, GeneratedSubqueryTable> Selection { get; }
//   .Selection.Record → returns ISelectedColumns<SelectRecord, ...> for struct-style mapping
//   .Selection.Mapping → returns ISelectedColumns<SelectModel, ...> for class-style mapping
```

### 26.2 Static Utility Classes

| Class | Methods |
|---|---|
| [`Sql`](Drizzle4Dotnet/src/Core/Shared/Sql.cs) | `Value<T>()`, `Raw()`, `Raw<T>()`, `Literal()`, `Literal<T>()`, `Null<T>()`, `Default()` |
| [`SqlStatics`](Drizzle4Dotnet/src/Core/Shared/SqlStatics.cs:9) | `BuildClause()`, `BuildSqlSetClause()`, `BuildSetValues()`, `BuildInsertColumnList()`, `BuildInsertRowValues()`, `BuildSqlJoins()`, `BuildSqlCte()`, `BuildSqlOrderBy()` |
| [`SqlDialectDefaults`](Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs:7) | Default LIMIT/OFFSET builder, default string escaping, default feature flags |
| [`SqlExtensions`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:88) | `AsSubQuery()`, `As()` (aliasing) |

### 26.3 Data Type Mapping

Each dialect provides `ClrToSqlTypeMap` mapping CLR types to dialect-specific [`ISqlDataType`](Drizzle4Dotnet/src/Core/Schema/Migration/SqlDataType.cs:8) implementations:

| CLR Type | PgSql | MySql | Mssql | Oracle |
|---|---|---|---|---|
| `int` | `Integer` | `Int` | `Int` | `Integer` |
| `long` | `BigInt` | `BigInt` | `BigInt` | `BigInt` |
| `short` | `SmallInt` | `SmallInt` | `SmallInt` | `SmallInt` |
| `byte` | `SmallInt` | `TinyInt` | `TinyInt` | `TinyInt` |
| `string` | `Text` | `Text` | `Text` | `Text` |
| `bool` | `Boolean` | `Boolean` | `Boolean` | `Boolean` |
| `decimal` | `Numeric` | `Decimal` | `Decimal` | `Decimal` |
| `float` | `Real` | `Float` | `Real` | `BinaryFloat` |
| `double` | `DoublePrecision` | `Double` | `Float` | `BinaryDouble` |
| `DateTime` | `Timestamp` | `DateTime` | `DateTime2` | `Timestamp` |
| `DateOnly` | `Date` | `Date` | `Date` | `Date` |
| `TimeOnly` | `Time` | `Time` | `Time` | `Time` |
| `Guid` | `Uuid` | `Guid` | `UniqueIdentifier` | `Uuid` |
| `byte[]` | `Bytea` | `Blob` | `VarBinary` | `Blob` |
| `char` | `Char` | `Char` | `NChar` | `Char` |

---

## Feature Count Summary

| Category | Features |
|---|---|
| Core Query Builders (CRUD) | 7 |
| SELECT Features (cross-dialect) | 20 |
| INSERT Features (cross-dialect) | 8 |
| UPDATE Features (cross-dialect) | 7 |
| DELETE Features (cross-dialect) | 2 |
| PostgreSQL (PgSql) Features | 30+ |
| MySQL/MariaDB Features | 20+ |
| MSSQL Features | 25+ |
| Oracle Features | 25+ |
| Core SQL Operators | 30+ |
| Core SQL Functions | 35+ |
| PgSql Operators & Functions | 40+ |
| MySQL Operators & Functions | 25+ |
| MSSQL Operators & Functions | 30+ |
| Oracle Operators & Functions | 50+ |
| Window Functions | 11 + frame specs |
| Set Operations | 8 |
| CTEs / Recursive CTEs | 6 |
| Subqueries & Virtual Tables | 8 |
| Schema Attributes | 15+ |
| Migration System | 20+ |
| Source Generators | 3 generators |
| CLI Tool | 5 commands |
| Execution / DbClient | 4 implementations |
| Expression Nodes / AST | 10+ node types |
| **Total** | **~450+ features** |
