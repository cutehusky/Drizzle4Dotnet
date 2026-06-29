# Drizzle4Dotnet — MySQL/MariaDB Implementation Status & Plan

> **Status:** ✅ Implemented (Full MySQL dialect)  
> **Last Updated:** 2026-06-29  
> **Target:** MySQL 8.0+ / MariaDB 10.2+ with MySqlConnector

---

## 1. Implementation Status Overview

| Layer | Status | Details |
|-------|--------|---------|
| **Dialect** | ✅ Complete | [`MySqlSqlDialectImpl`](Drizzle4Dotnet/src/MySql/MySqlSqlDialectImpl.cs) — backtick quoting, `@p0` params |
| **Data Types** | ✅ Complete | [`MySqlDataType`](Drizzle4Dotnet/src/MySql/Schema/MySqlDataType.cs) — Int, BigInt, SmallInt, TinyInt, Text, Boolean, Decimal, Float, Double, DateTime, Date, Time, Guid, Blob, Char |
| **DbClient** | ✅ Complete | [`MySqlDbClient`](Drizzle4Dotnet/src/MySql/MySqlDbClient.cs) — MySqlConnector |
| **SELECT** | ✅ Complete | MySqlSelectQuery — FOR UPDATE, FOR SHARE, NOWAIT/SKIP LOCKED |
| **INSERT** | ✅ Complete | MySqlInsertQuery — ON DUPLICATE KEY UPDATE, INSERT IGNORE, SET syntax |
| **REPLACE** | ✅ Complete | MySqlReplaceQuery — REPLACE INTO |
| **UPDATE** | ✅ Complete | MySqlUpdateQuery — JOIN support, LIMIT, ORDER BY |
| **DELETE** | ✅ Complete | MySqlDeleteQuery — JOIN support, LIMIT, ORDER BY |
| **Functions** | ✅ Complete | DateTime: Now, CurDate, CurTime, DateAdd, DateSub, DateDiff, DateFormat, Extract; JSON: JsonExtract, JsonUnquote, JsonContains, JsonArrayAppend, JsonObject, JsonArray; Numeric: Rand, Truncate; Info: Database, User, Version; String: ConcatWs, GroupConcat, GroupConcatDistinct, FindInSet |
| **Operators** | ✅ Complete | Null-safe equals (`<=>`), Regex (`REGEXP`, `NOT REGEXP`, `RLIKE`) |
| **Special Nodes** | ✅ Complete | `MySqlIntervalNode`, `MySqlPositionNode`, `MySqlValuesNode` |

## 2. Current File Structure

```
Drizzle4Dotnet/src/MySql/
├── MySqlDbClient.cs                # Connection + factory methods
├── MySqlQueryBuilder.cs            # Offline query builder
├── MySqlQueryBuilderExtensions.cs  # 1–16 column Select/SelectDistinct extensions
├── MySqlSqlDialectImpl.cs          # Dialect: backtick quoting, LIMIT pair mode, type map
├── Schema/
│   ├── MySqlColumn.cs              # MySqlColumn type
│   ├── MySqlDataType.cs            # MySqlDataType static structs
│   └── MySqlTable.cs               # MySqlTable base class
├── Operators/
│   ├── MySqlFunctions.cs           # Entry point
│   ├── MySqlFunctions.DateTime.cs  # Now, CurDate, CurTime, DateAdd, DateSub, DateDiff, DateFormat, Extract
│   ├── MySqlFunctions.Info.cs      # Database, User, Version
│   ├── MySqlFunctions.Json.cs      # JsonExtract, JsonUnquote, JsonContains, JsonArrayAppend, JsonObject, JsonArray
│   ├── MySqlFunctions.Numeric.cs   # Rand, Truncate
│   ├── MySqlFunctions.String.cs    # ConcatWs, GroupConcat, GroupConcatDistinct, FindInSet
│   ├── MySqlOperators.cs           # Null-safe equals operator
│   ├── MySqlOperators.NullSafe.cs  # `<=>` operator
│   ├── MySqlOperators.Regex.cs     # REGEXP, NOT REGEXP, RLIKE
│   └── Nodes/
│       ├── MySqlIntervalNode.cs    # INTERVAL @p0 unit (without quotes)
│       ├── MySqlPositionNode.cs    # POSITION(substr IN str)
│       └── MySqlValuesNode.cs      # VALUES(col) for ON DUPLICATE KEY UPDATE
└── Query/
    ├── MySqlDeleteQuery.cs         # DELETE with JOIN, LIMIT, ORDER BY
    ├── MySqlInsertQuery.cs         # INSERT with ON DUPLICATE KEY UPDATE, IGNORE, SET syntax
    ├── MySqlReplaceQuery.cs        # REPLACE INTO
    ├── MySqlSelectQuery.cs         # SELECT with FOR UPDATE/SHARE, NOWAIT/SKIP LOCKED
    └── MySqlUpdateQuery.cs         # UPDATE with JOIN, LIMIT, ORDER BY
```

## 3. Complete Feature Matrix

### 3.1 SELECT Features — Status: ✅ All Implemented

| Feature | MySQL Syntax | Implementation |
|---------|-------------|----------------|
| FROM | `` FROM `table` `` | ✅ Base |
| WHERE | `` WHERE (cond) `` | ✅ Base |
| GROUP BY / HAVING | `GROUP BY ... HAVING ...` | ✅ Base |
| ORDER BY | `ORDER BY col ASC/DESC` | ✅ Base |
| LIMIT / OFFSET | `LIMIT n OFFSET m` | ✅ Via dialect `BuildLimitOffset()` |
| DISTINCT | `SELECT DISTINCT ...` | ✅ Base |
| INNER/LEFT/RIGHT JOIN | `JOIN ... ON ...` | ✅ Base |
| CROSS JOIN | `CROSS JOIN ...` | ✅ Base |
| FOR UPDATE / FOR SHARE | `SELECT ... FOR UPDATE` | ✅ `ForUpdate()`, `ForShare()` |
| Lock Options | `NOWAIT`, `SKIP LOCKED` | ✅ `.Nowait()`, `.SkipLocked()` |
| CTEs | `WITH ... AS ...` | ✅ Base (MySQL 8.0+) |
| Recursive CTEs | `WITH RECURSIVE ...` | ✅ Base (MySQL 8.0+) |
| Subqueries | `(SELECT ...) AS alias` | ✅ Base |

### 3.2 INSERT Features — Status: ✅ All Implemented

| Feature | MySQL Syntax | Implementation |
|---------|-------------|----------------|
| Single row | `` INSERT INTO `t` (cols) VALUES (vals) `` | ✅ Base |
| Multi row | `INSERT ... VALUES (...), (...)` | ✅ Base |
| INSERT ... SELECT | `INSERT ... SELECT ...` | ✅ Base |
| INSERT IGNORE | `INSERT IGNORE INTO ...` | ✅ `Ignore()` |
| INSERT ... SET | `` INSERT INTO `t` SET col = val `` | ✅ `Set(col, value)` (MySQL-specific syntax) |
| ON DUPLICATE KEY UPDATE | `ON DUPLICATE KEY UPDATE col = VALUES(col)` | ✅ `OnDuplicateKeyUpdate(col, value)` |
| ON DUPLICATE KEY UPDATE ALL | Updates all columns | ✅ `OnDuplicateKeyUpdateAll()` |
| REPLACE INTO | `REPLACE INTO ...` | ✅ `MySqlReplaceQuery` |

### 3.3 UPDATE Features — Status: ✅ All Implemented

| Feature | MySQL Syntax | Implementation |
|---------|-------------|----------------|
| SET | `` SET `col` = val `` | ✅ Base |
| SET expression | `` SET `col` = (expr) `` | ✅ Base |
| JOIN in UPDATE | `` UPDATE t JOIN other ON ... SET ... `` | ✅ MySqlUpdateQuery |
| WHERE | `WHERE (cond)` | ✅ Base |
| LIMIT | `UPDATE ... LIMIT n` | ✅ MySqlUpdateQuery |
| ORDER BY | `UPDATE ... ORDER BY ...` | ✅ MySqlUpdateQuery |
| CTE | `WITH cte AS (...) UPDATE ...` | ✅ Base |

### 3.4 DELETE Features — Status: ✅ All Implemented

| Feature | MySQL Syntax | Implementation |
|---------|-------------|----------------|
| WHERE | `` DELETE FROM ... WHERE (cond) `` | ✅ Base |
| JOIN in DELETE | `` DELETE t FROM t JOIN ... `` | ✅ MySqlDeleteQuery |
| LIMIT | `DELETE ... LIMIT n` | ✅ MySqlDeleteQuery |
| ORDER BY | `DELETE ... ORDER BY ...` | ✅ MySqlDeleteQuery |
| CTE | `WITH cte AS (...) DELETE ...` | ✅ Base |

### 3.5 MySQL Functions — Status: ✅ All Implemented

| Category | Functions |
|----------|-----------|
| DateTime | `Now()`, `CurDate()`, `CurTime()`, `DateAdd(unit, col, expr)`, `DateSub(unit, col, expr)`, `DateDiff(col1, col2)`, `DateFormat(col, format)`, `Extract(unit, col)` |
| JSON | `JsonExtract(col, path)`, `JsonUnquote(expr)`, `JsonContains(col, val)`, `JsonArrayAppend(col, path, val)`, `JsonObject(key, val)`, `JsonArray(vals...)` |
| String | `ConcatWs(sep, vals...)`, `GroupConcat(expr)`, `GroupConcatDistinct(expr)`, `FindInSet(str, strlist)` |
| Numeric | `Rand()`, `Truncate(col, decimals)` |
| Info | `Database()`, `User()`, `Version()` |

### 3.6 MySQL Operators — Status: ✅ All Implemented

| Operator | SQL |
|----------|-----|
| Null-safe equals | `` `col` <=> @p0 `` |
| Regex match | `` `col` REGEXP @p0 `` |
| Regex not match | `` `col` NOT REGEXP @p0 `` |
| RLIKE | `` `col` RLIKE @p0 `` |

### 3.7 MySQL-Specific Syntax Notes

| Feature | MySQL Difference |
|---------|-----------------|
| No RETURNING clause | Use `LAST_INSERT_ID()` or `SELECT` after INSERT |
| No FULL OUTER JOIN | Not supported in MySQL |
| No arrays | Not supported |
| No IS DISTINCT FROM | Use `<=>` null-safe equals instead |
| No FILTER (WHERE) for aggregates | Use `CASE WHEN` inside aggregate |
| LIMIT pair mode | `LIMIT n, m` syntax supported via `UseLimitPairMode` |
| No DEFAULT VALUES | Use `INSERT INTO t () VALUES ()` instead |
| Identifier quoting | Backticks `` `identifier` `` |

## 4. Remaining Work / Gaps

| Item | Priority | Notes |
|------|----------|-------|
| LATERAL join support | Low | MySQL 8.0.14+ supports `LATERAL`; not yet implemented |
| Window functions | Low | MySQL 8.0+ supports; core FilteredAggregate + Over() not MySQL-tested |
| FULL OUTER JOIN workaround | Low | Can be simulated via LEFT + RIGHT JOIN + UNION |
| `INSERT () VALUES ()` for DEFAULT | Low | MySqlInsertQuery should auto-detect and use this instead of DEFAULT VALUES |
| MySqlStatics class | Medium | Missing static factory methods comparable to `PgSqlStatics` |
