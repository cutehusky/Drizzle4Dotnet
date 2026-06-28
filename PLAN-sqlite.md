# Drizzle4Dotnet — SQLite Implementation Plan

> **Status:** Draft  
> **Last Updated:** 2026-06-28  
> **Target:** Full SQLite dialect support, paralleling existing PostgreSQL and MySQL implementations.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Phase S1: SQLite Dialect (Foundation)](#2-phase-s1-sqlite-dialect-foundation)
3. [Phase S2: SQLite Types & Interfaces](#3-phase-s2-sqlite-types--interfaces)
4. [Phase S3: SQLite Functions](#4-phase-s3-sqlite-functions)
5. [Phase S4: SQLite Operators & Nodes](#5-phase-s4-sqlite-operators--nodes)
6. [Phase S5: SQLite DML — SELECT, INSERT, UPDATE, DELETE](#6-phase-s5-sqlite-dml--select-insert-update-delete)
7. [Phase S6: SQLite DML — ON CONFLICT (UPSERT) & RETURNING](#7-phase-s6-sqlite-dml--on-conflict-upsert--returning)
8. [Phase S7: SQLite Schema / Column Types](#8-phase-s7-sqlite-schema--column-types)
9. [Phase S8: SQLite Connection & Execution](#8-phase-s8-sqlite-connection--execution)
10. [Phase S9: Source Generator SQLite Support](#9-phase-s9-source-generator-sqlite-support)
11. [Phase S10: Migration / DDL Layer for SQLite](#10-phase-s10-migration--ddl-layer-for-sqlite)
12. [Phase S11: Testing Strategy](#11-phase-s11-testing-strategy)
13. [Phase S12: NuGet Packaging & Demo](#12-phase-s12-nuget-packaging--demo)
14. [Appendix: SQLite SQL Syntax Reference](#14-appendix-sqlite-sql-syntax-reference)
15. [Appendix: Implementation Order & Dependencies](#15-appendix-implementation-order--dependencies)

---

## 1. Architecture Overview

### 1.1 SQLite vs PostgreSQL/MySQL — Key Differences

| Aspect | PostgreSQL | MySQL | SQLite |
|--------|------------|-------|--------|
| **Identifier quoting** | `"double quotes"` | `` `backticks` `` | `"double quotes"` (or backticks) |
| **Parameter prefix** | `@p0`, `@p1` | `@p0`, `@p1` | `@p0`, `@p1` (Microsoft.Data.Sqlite) |
| **Schema support** | `schema.table` | `db.table` | ❌ No schema — single database file |
| **LIMIT / OFFSET** | `LIMIT ? OFFSET ?` | `LIMIT ?, ?` or `LIMIT ? OFFSET ?` | `LIMIT ? OFFSET ?` (standard) |
| **RETURNING clause** | ✅ Supported (8.2+) | ❌ Not supported | ✅ Supported (3.35+) |
| **UPSERT** | `ON CONFLICT [...] DO UPDATE/NOTHING` | `ON DUPLICATE KEY UPDATE` / `REPLACE` | `ON CONFLICT [...] DO UPDATE/NOTHING` (3.24+) |
| **INSERT DEFAULT VALUES** | `INSERT INTO t DEFAULT VALUES` | `INSERT INTO t () VALUES ()` | ❌ Not supported (use `DEFAULT` per-column) |
| **UPDATE/DELETE with JOIN** | `UPDATE ... FROM` / `DELETE ... USING` | `UPDATE t JOIN ...` / `DELETE t FROM t JOIN` | ❌ No JOIN in UPDATE/DELETE (use correlated subqueries) |
| **CTEs (WITH)** | ✅ Supported | ✅ MySQL 8.0+ | ✅ Supported (3.8.3+) |
| **Recursive CTEs** | ✅ Supported | ✅ MySQL 8.0+ | ✅ Supported (3.8.3+) |
| **Window functions** | ✅ Supported | ⚠️ MySQL 8.0+ partial | ✅ Supported (3.25+) |
| **JSON functions** | Native JSON + operators | `JSON_EXTRACT()`, etc. | ✅ `json_extract()`, etc. (3.38+; built-in) |
| **Array types** | ✅ Native array type | ❌ Not supported | ❌ Not supported |
| **`IS DISTINCT FROM`** | ✅ Supported | ❌ Not supported | ❌ Not supported |
| **`FILTER (WHERE ...)`** | ✅ Supported | ❌ Not supported | ❌ Not supported |
| **FULL OUTER JOIN** | ✅ Supported | ❌ Not supported | ❌ Not supported (3.39+ supports it) |
| **NATURAL JOIN** | ✅ Supported | ✅ Supported | ✅ Supported |
| **LATERAL JOIN** | ✅ Supported | ✅ MySQL 8.0.14+ | ❌ Not supported |
| **APPLY (CROSS/OUTER)** | ❌ Not supported | ❌ Not supported | ❌ Not supported |
| **`NOW()`** | `NOW()` | `NOW()` | `datetime('now')` |
| **`RANDOM()`** | `RANDOM()` | `RAND()` | `RANDOM()` |
| **String concat** | `\|\|` operator | `CONCAT()` function | `\|\|` operator |
| **Auto-increment** | `GENERATED AS IDENTITY` / `SERIAL` | `AUTO_INCREMENT` | `INTEGER PRIMARY KEY` (auto-increment by default) |
| **`last_insert_rowid()`** | Use `RETURNING` | `LAST_INSERT_ID()` | `last_insert_rowid()` |
| **`ROWID`** | — | — | Implicit `rowid` on all tables |
| **`VACUUM`** | `VACUUM` | `OPTIMIZE TABLE` | `VACUUM` |
| **`ANALYZE`** | `ANALYZE` | `ANALYZE TABLE` | `ANALYZE` |
| **`ATTACH DATABASE`** | — | — | ✅ Supported |
| **`PRAGMA` statements** | — | — | ✅ Supported |
| **Transaction DDL** | DDL not transactional in some cases | DDL not transactional | ✅ DDL is transactional |
| **`EXPLAIN QUERY PLAN`** | `EXPLAIN ANALYZE` | `EXPLAIN` | `EXPLAIN QUERY PLAN` |

### 1.2 Target File Structure

```
Drizzle4Dotnet/
├── src/
│   ├── Core/                           # Existing — minimal changes needed
│   │   ├── Shared/
│   │   │   ├── SqlDialectDefaults.cs   # Add UseLimitPairMode flag (already exists)
│   │   │   └── ISqlDialect.cs          # Already abstract enough
│   │   └── ...
│   ├── Dialect/
│   │   ├── PgSqlSqlDialectImpl.cs      # Existing
│   │   ├── MySqlSqlDialectImpl.cs      # Existing
│   │   └── SqliteSqlDialectImpl.cs     # NEW — SQLite dialect
│   └────── Sqlite/                          # NEW — SQLite-specific namespace
│           ├── SqliteDbClient.cs            # DbClient + factory methods
│           ├── SqliteQueryBuilder.cs        # Offline query builder
│           ├── SqliteTable.cs               # Table/alias interfaces
│           ├── SqliteColumn.cs              # Column types
│           ├── SqliteStatics.cs             # Static helpers (last_insert_rowid, etc.)
│           ├── SqliteFunctions.cs           # SQLite-specific functions (empty stub if none)
│           ├── SqliteFunctions.DateTime.cs  # datetime(), date(), time(), strftime(), julianday()
│           ├── SqliteFunctions.String.cs    # instr(), substr(), replace(), trim(), etc.
│           ├── SqliteFunctions.Numeric.cs   # random(), abs(), round(), etc.
│           ├── SqliteFunctions.Json.cs      # json_extract(), json_object(), etc.
│           ├── SqliteFunctions.Info.cs      # last_insert_rowid(), sqlite_version(), changes()
│           ├── SqliteOperators.cs           # SQLite-specific operators (regexp? via MATCH)
│           ├── Nodes/
│           │   └── SqliteRowIdNode.cs       # _rowid_ expression wrapper (optional)
│           └── Query/
│               ├── SqliteSelectQuery.cs     # SELECT with RETURNING, DISTINCT, etc.
│               ├── SqliteInsertQuery.cs     # INSERT with ON CONFLICT, RETURNING
│               ├── SqliteUpdateQuery.cs     # UPDATE with RETURNING
│               └── SqliteDeleteQuery.cs     # DELETE with RETURNING

Test/
├── Select/
│   └── SqliteSelectTests.cs            # NEW — SQLite SELECT tests
├── Insert/
│   └── SqliteInsertTests.cs             # NEW — SQLite INSERT tests
├── Update/
│   └── SqliteUpdateTests.cs             # NEW — SQLite UPDATE tests
├── Delete/
│   └── SqliteDeleteTests.cs             # NEW — SQLite DELETE tests
├── Migration/
│   └── SqliteMigrationTests.cs          # NEW — SQLite DDL/migration tests

SharedDemo/
└── Sqlite/
    ├── Schema.cs                        # SQLite-specific schema definitions
    ├── DbDto.cs                         # SQLite DTOs
    ├── schema.sql                       # DDL for demo setup
    └── data.sql                         # Sample data

SourceGenerators/
├── SourceGenerators/
│   ├── TableGenerator.cs               # Add SQLite dialect support
│   ├── MigrationSchemaGenerator.cs      # Add SQLite type generation
│   └── ...
```

### 1.3 SQLite ADO.NET Provider

The ORM uses `System.Data.Common.DbConnection` / `DbCommand`, so any ADO.NET-compatible SQLite provider works:

| Provider | NuGet Package | Notes |
|----------|--------------|-------|
| **Microsoft.Data.Sqlite** | `Microsoft.Data.Sqlite` | ✅ Recommended. Lightweight, cross-platform, `@named` parameters. |
| System.Data.SQLite | `System.Data.SQLite` | Full-featured, supports both `?` and `@named` params. |

**Recommendation:** Use **Microsoft.Data.Sqlite** — it's the modern, lightweight, cross-platform provider from Microsoft. It uses `@name` parameter prefix (same convention as Npgsql and MySqlConnector), so the existing `SqlBuilder` parameter naming strategy works unchanged.

NuGet dependency to add to `Drizzle4Dotnet.csproj`:
```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
```

---

## 2. Phase S1: SQLite Dialect (Foundation)

### 2.1 Create `SqliteSqlDialectImpl.cs`

**File:** [`Drizzle4Dotnet/src/Dialect/SqliteSqlDialectImpl.cs`](Drizzle4Dotnet/src/Dialect/SqliteSqlDialectImpl.cs)

Implements [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs).

```csharp
public class SqliteSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================
    
    // SQLite uses double-quote identifiers like PostgreSQL
    public static string BuildIdentifier(string identifier)
        => $"\"{identifier}\"";

    // SQLite has no schema — ignore schemaName
    public static string BuildTableName(string schemaName, string tableName)
        => $"\"{tableName}\"";

    public static string BuildColumnName(string refName, string columnName)
        => $"\"{refName}\".\"{columnName}\"";

    // Parameters: @p0, @p1, ... (Microsoft.Data.Sqlite uses @named params)
    public static string BuildParameterName(string parameterName)
        => $"@{parameterName}";
        
    public static string BuildParameterName(int parameterIndex)
        => $"@p{parameterIndex}";
    
    // ======================================================================
    // Limit / Offset
    // ======================================================================
    
    // Standard SQL: LIMIT ? OFFSET ?
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => SqlDialectDefaults.BuildLimitOffset(sqlBuilder, limit, offset);

    // SQLite does NOT support LIMIT/OFFSET on UPDATE/DELETE directly
    // (Unlike MySQL). Throw NotSupportedException.
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => throw new NotSupportedException("SQLite does not support LIMIT/OFFSET for UPDATE/DELETE statements.");
    
    // ======================================================================
    // Feature Flags
    // ======================================================================
    
    public static bool SupportsReturning => true;           // SQLite 3.35+ (2021)
    public static bool SupportsArrays => false;             // Not supported
    public static bool SupportsJson => true;                // SQLite 3.38+ (built-in)
    public static bool SupportsWindowFunctions => true;     // SQLite 3.25+ (2018)
    public static bool SupportsCte => true;                 // SQLite 3.8.3+
    public static bool SupportsRecursiveCte => true;        // SQLite 3.8.3+
    public static bool SupportsDeleteUsing => false;        // Not supported (use subqueries)
    public static bool SupportsIsDistinctFrom => false;     // Not supported
    public static bool SupportsFilteredAggregates => false; // Not supported
    public static bool SupportsFullOuterJoin => false;      // SQLite 3.39+ (2022) — keep false for maximum compat
    public static bool SupportsNaturalJoin => true;         // Supported
    public static bool SupportsLateralJoin => false;        // Not supported
    public static bool SupportsApplyJoin => false;          // Not supported
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    // SQLite uses '' escaping like standard SQL
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
}
```

### 2.2 Register the dialect

- Update [`SqlDialectDefaults`](Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs) if needed — currently it already has `UseLimitPairMode` flag.
- No changes needed to [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) — it already supports all SQLite features via feature flags.

---

## 3. Phase S2: SQLite Types & Interfaces

### 3.1 Create [`SqliteTable.cs`](Drizzle4Dotnet/src/Sqlite/SqliteTable.cs)

```csharp
namespace Drizzle4Dotnet.Sqlite;

public interface ISqliteGenericTable : IGenericTable<SqliteSqlDialectImpl> { }
public interface ISqliteTable : ITable<SqliteSqlDialectImpl> { }
public interface ISqliteCteTable : ICteTable<SqliteSqlDialectImpl> { }
public interface ISqliteVirtualTable : IVirtualTable<SqliteSqlDialectImpl> { }
public interface ISqliteDbTable : IDbTable<SqliteSqlDialectImpl> { }
public interface ISqliteTableAlias : ITableAlias<SqliteSqlDialectImpl> { }
```

### 3.2 Create [`SqliteColumn.cs`](Drizzle4Dotnet/src/Sqlite/SqliteColumn.cs)

```csharp
namespace Drizzle4Dotnet.Sqlite;

public class SqliteColumn<T, TTable> : DbColumn<T, TTable, SqliteSqlDialectImpl>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    public SqliteColumn(string columnName) : base(columnName) { }
}

public class SqliteVirtualColumn<T> : VirtualColumn<T, SqliteSqlDialectImpl>
{
    public SqliteVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
```

---

## 4. Phase S3: SQLite Functions

### 4.1 Create [`SqliteStatics.cs`](Drizzle4Dotnet/src/Sqlite/SqliteStatics.cs)

Static helper methods mirroring [`PgSqlStatics`](Drizzle4Dotnet/src/Dialect/PgSql/PgSqlStatics.cs) pattern.

```csharp
public static class SqliteStatics
{
    /// <summary>Returns the rowid of the last inserted row.</summary>
    public static ISql<long> LastInsertRowId() => new RawSql<long>("SELECT last_insert_rowid()");
    
    /// <summary>Returns the SQLite library version.</summary>
    public static ISql<string> SqliteVersion() => new RawSql<string>("SELECT sqlite_version()");
    
    /// <summary>Returns the number of rows changed by the last statement.</summary>
    public static ISql<int> Changes() => new RawSql<int>("SELECT changes()");
}
```

### 4.2 Create [`SqliteFunctions.DateTime.cs`](Drizzle4Dotnet/src/Sqlite/SqliteFunctions.DateTime.cs)

SQLite datetime functions use a different syntax from PostgreSQL/MySQL:

| Function | SQLite |
|----------|--------|
| `NOW()` | `datetime('now')` |
| `CURRENT_DATE` | `date('now')` |
| `CURRENT_TIME` | `time('now')` |
| Date add/sub | `date('now', '+1 day')` — modifier syntax |
| Date extract | `strftime('%Y', 'now')` |

Implementation approach: Use `FunctionCallNode` / `RawSql` wrappers.

```csharp
public static class SqliteFunctions
{
    public static class DateTime
    {
        /// <summary>Returns current date and time as 'YYYY-MM-DD HH:MM:SS'.</summary>
        public static ISql<string> Now() => new RawSql<string>("datetime('now')");
        
        /// <summary>Returns current date as 'YYYY-MM-DD'.</summary>
        public static ISql<string> CurrentDate() => new RawSql<string>("date('now')");
        
        /// <summary>Returns current time as 'HH:MM:SS'.</summary>
        public static ISql<string> CurrentTime() => new RawSql<string>("time('now')");
        
        /// <summary>Formats a datetime value using strftime format string.</summary>
        public static ISql<string> Strftime(string format, ISql<string> dateTime)
            => new FunctionCallNode("strftime", Sql.Value(format), dateTime);
        
        /// <summary>Adds a time interval to a date (uses SQLite modifier syntax).</summary>
        public static ISql<string> DateAdd(ISql<string> date, string modifier)
            => new RawSql<string>(...);
        
        /// <summary>Extracts year from a date.</summary>
        public static ISql<int> ExtractYear(ISql<string> date)
            => new RawSql<int>($"CAST(strftime('%Y', ...) AS INTEGER)");
    }
}
```

### 4.3 Create [`SqliteFunctions.String.cs`](Drizzle4Dotnet/src/Sqlite/SqliteFunctions.String.cs)

SQLite uses `||` for string concatenation — shared with PostgreSQL. Wrap common functions:

- `instr(haystack, needle)` → `ISql<int> Instr(...)`
- `substr(str, start, length?)` → `ISql<string> Substr(...)`
- `replace(str, from, to)` → `ISql<string> Replace(...)`
- `trim(str)`, `ltrim(str)`, `rtrim(str)` → trim wrappers
- `length(str)` → `ISql<int> Length(...)`
- `upper(str)`, `lower(str)` → case conversion
- `like(str, pattern)` → `ISql<bool> Like(...)`
- `glob(str, pattern)` → `ISql<bool> Glob(...)` (SQLite-specific)

### 4.4 Create [`SqliteFunctions.Numeric.cs`](Drizzle4Dotnet/src/Sqlite/SqliteFunctions.Numeric.cs)

- `random()` → `ISql<long> Random()` (returns -2^63 to 2^63-1)
- `abs(value)` → `ISql<T> Abs<T>(...)`
- `round(value, digits?)` → `ISql<double> Round(...)`
- `total_changes()` → `ISql<int> TotalChanges()`

### 4.5 Create [`SqliteFunctions.Json.cs`](Drizzle4Dotnet/src/Sqlite/SqliteFunctions.Json.cs)

SQLite 3.38+ has built-in JSON functions (no separate extension needed):

- `json(value)` → validate/minify JSON
- `json_extract(value, path)` → extract JSON value
- `json_object(key, value, ...)` → build JSON object
- `json_array(value, ...)` → build JSON array
- `json_set(value, path, value)` → set JSON value
- `json_remove(value, path)` → remove JSON key
- `json_type(value, path?)` → get JSON type
- `json_valid(value)` → check if valid JSON
- `json_group_array(value)` → aggregate to JSON array
- `json_group_object(key, value)` → aggregate to JSON object

### 4.6 Create [`SqliteFunctions.Info.cs`](Drizzle4Dotnet/src/Sqlite/SqliteFunctions.Info.cs)

- `last_insert_rowid()` → `ISql<long>`
- `sqlite_version()` → `ISql<string>`
- `changes()` → `ISql<int>` (number of rows affected)
- `total_changes()` → `ISql<int>` (cumulative changes)

---

## 5. Phase S4: SQLite Operators & Nodes

### 5.1 Create [`SqliteOperators.cs`](Drizzle4Dotnet/src/Sqlite/SqliteOperators.cs)

SQLite shares many common operators with standard SQL and PostgreSQL:

- **`||` (string concat)** — same as PostgreSQL, shared via [`Operators.String`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.String.cs)
- **`MATCH` (full-text search)** — SQLite-specific, could be added as an extension
- **`REGEXP`** — requires regex extension; optional
- **`GLOB`** — pattern matching with Unix wildcards
- **`LIKE`** — standard SQL, already handled

Most standard operators (comparison, arithmetic, logical, string) are already in [`Core/Shared/Operators/`](Drizzle4Dotnet/src/Core/Shared/Operators/) and work with SQLite unchanged.

### 5.2 Create [`Nodes/SqliteRowIdNode.cs`](Drizzle4Dotnet/src/Sqlite/Nodes/SqliteRowIdNode.cs) (Optional)

```csharp
public class SqliteRowIdNode : IOperator<long>
{
    private readonly string? _tableAlias;
    
    public SqliteRowIdNode(string? tableAlias = null)
    {
        _tableAlias = tableAlias;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_tableAlias != null)
            sqlBuilder.Append(SqliteSqlDialectImpl.BuildColumnName(_tableAlias, "rowid"));
        else
            sqlBuilder.Append("rowid");
    }
}
```

---

## 6. Phase S5: SQLite DML — SELECT, INSERT, UPDATE, DELETE

### 6.1 Key SQLite SQL Differences

| Operation | SQLite Syntax | Notes |
|-----------|--------------|-------|
| **SELECT** | Standard | Supports DISTINCT, GROUP BY, HAVING, WINDOW, ORDER BY, LIMIT/OFFSET |
| **INSERT** | Standard + ON CONFLICT | Supports `INSERT INTO ... VALUES`, `INSERT INTO ... SELECT` |
| **INSERT DEFAULT** | ❌ Not supported | Must provide explicit values or use `DEFAULT` keyword per-column |
| **UPDATE** | Standard + RETURNING | No JOIN support; use correlated subqueries |
| **DELETE** | Standard + RETURNING | No JOIN/USING support; use correlated subqueries |
| **REPLACE** | ✅ `INSERT OR REPLACE` | Equivalent to MySQL's REPLACE |
| **INSERT OR IGNORE** | ✅ `INSERT OR IGNORE` | Equivalent to MySQL's INSERT IGNORE |
| **INSERT OR ROLLBACK** | ✅ SQLite-specific | — |
| **INSERT OR ABORT** | ✅ SQLite-specific | Default behavior |
| **INSERT OR FAIL** | ✅ SQLite-specific | — |
| **RETURNING** | ✅ Supported (3.35+) | Full support, like PostgreSQL |

### 6.2 Create [`SqliteSelectQuery.cs`](Drizzle4Dotnet/src/Sqlite/Query/SqliteSelectQuery.cs)

```csharp
public class SqliteSelectQuery<TReturn, TVirtualTable> 
    : SelectQuery<TReturn, SqliteSqlDialectImpl, TVirtualTable, SqliteSelectQuery<TReturn, TVirtualTable>>,
      INaturalJoin<SqliteSelectQuery<TReturn, TVirtualTable>, SqliteSqlDialectImpl>
    where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
{
    public SqliteSelectQuery(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<SqliteSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ====== NATURAL JOINS (supported by SQLite) ======
    
    public SqliteSelectQuery<TReturn, TVirtualTable> NaturalJoin(
        IGenericTable<SqliteSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL");

    public SqliteSelectQuery<TReturn, TVirtualTable> NaturalLeftJoin(
        IGenericTable<SqliteSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL LEFT");

    // SQLite does NOT support:
    // - LATERAL joins
    // - FULL OUTER JOIN (before 3.39)
    // - DISTINCT ON
    // - Lock clauses (FOR UPDATE, etc.)
}
```

**Design Decision:** SQLite does not support locking clauses like `FOR UPDATE` — the base `BuildSqlLock()` in [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs) is a no-op by default, so no override is needed.

### 6.3 Create [`SqliteInsertQuery.cs`](Drizzle4Dotnet/src/Sqlite/Query/SqliteInsertQuery.cs)

SQLite's INSERT syntax is very similar to PostgreSQL's, with `ON CONFLICT` support and `RETURNING`. However, SQLite does NOT support `INSERT ... DEFAULT VALUES`. Also, SQLite supports `INSERT OR REPLACE`, `INSERT OR IGNORE`, etc.

```csharp
public class SqliteInsertQuery<TTable> 
    : InsertQuery<TTable, SqliteSqlDialectImpl, SqliteInsertQuery<TTable>>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    private string? _orAction; // "OR REPLACE", "OR IGNORE", "OR ROLLBACK", "OR ABORT", "OR FAIL"
    
    // ON CONFLICT support (similar to PgSqlInsertQuery)
    private List<string>? _conflictTargetColumns;
    private string? _conflictAction;
    private readonly Dictionary<string, object?> _conflictUpdates = new();
    
    // ... OnConflict(), DoNothing(), DoUpdate(), SetOnConflict() methods ...
    // ... modeled after PgInsertQuery but simpler (no constraint target, no WHERE on conflict) ...
    
    protected override void BuildInsertKeywords(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INSERT");
        if (_orAction != null)
        {
            sqlBuilder.Append(' ').Append(_orAction);
        }
        sqlBuilder.Append(" INTO ");
    }

    // SQLite does not support DEFAULT VALUES
    protected override void ValidateQuery()
    {
        if (UseDefaultValues)
            throw new NotSupportedException("SQLite does not support INSERT ... DEFAULT VALUES. " +
                "Use explicit values with the DEFAULT keyword per-column instead.");
        base.ValidateQuery();
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        base.BuildSql(sqlBuilder);
        // Append ON CONFLICT clause
        BuildOnConflict(sqlBuilder);
    }
}
```

**`ON CONFLICT` in SQLite** vs **PostgreSQL**:

| Feature | PostgreSQL | SQLite |
|---------|------------|--------|
| Column list target | ✅ `ON CONFLICT (col)` | ✅ `ON CONFLICT (col)` |
| Constraint name target | ✅ `ON CONFLICT ON CONSTRAINT name` | ❌ Not supported |
| WHERE on conflict target | ✅ `WHERE condition` | ❌ Not supported |
| DO UPDATE SET | ✅ `DO UPDATE SET col=val` | ✅ `DO UPDATE SET col=val` |
| WHERE on SET (partial index) | ✅ `WHERE condition` | ❌ Not supported |
| DO NOTHING | ✅ | ✅ |

SQLite's `ON CONFLICT` is a subset of PostgreSQL's. The implementation can reuse the [`PgConflictHelper`](Drizzle4Dotnet/src/Dialect/PgSql/Query/PgInsertQuery.cs:278) pattern but with fewer features.

### 6.4 Create [`SqliteUpdateQuery.cs`](Drizzle4Dotnet/src/Sqlite/Query/SqliteUpdateQuery.cs)

```csharp
public class SqliteUpdateQuery<TTable> 
    : UpdateQuery<TTable, SqliteSqlDialectImpl, SqliteUpdateQuery<TTable>>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    public SqliteUpdateQuery(TTable table, IQueryExecutor<SqliteSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // SQLite does NOT support:
    // - UPDATE ... FROM (no JOIN support)
    // - LIMIT/OFFSET on UPDATE
    // - ORDER BY on UPDATE
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);
        SqlStatics.BuildSqlSetClause<SqliteSqlDialectImpl>(sqlBuilder, SetValues);

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
        
        // No LIMIT/OFFSET — not supported by SQLite for UPDATE
        // No FROM clause — not supported by SQLite for UPDATE
    }
}
```

**Alternative for UPDATE with JOIN:** Users can use correlated subqueries:
```sql
UPDATE t1 SET col = (SELECT val FROM t2 WHERE t2.id = t1.id)
WHERE EXISTS (SELECT 1 FROM t2 WHERE t2.id = t1.id)
```

### 6.5 Create [`SqliteDeleteQuery.cs`](Drizzle4Dotnet/src/Sqlite/Query/SqliteDeleteQuery.cs)

```csharp
public class SqliteDeleteQuery<TTable> 
    : DeleteQuery<TTable, SqliteSqlDialectImpl, SqliteDeleteQuery<TTable>>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    public SqliteDeleteQuery(TTable table, IQueryExecutor<SqliteSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // SQLite does NOT support:
    // - DELETE ... USING (no JOIN support)
    // - LIMIT/OFFSET on DELETE
    // - ORDER BY on DELETE
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("DELETE FROM ");
        Table.BuildRefSql(sqlBuilder);

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
        
        // No USING clause, no ORDER BY, no LIMIT/OFFSET
    }
}
```

---

## 7. Phase S6: SQLite DML — ON CONFLICT (UPSERT) & RETURNING

### 7.1 RETURNING Support

SQLite 3.35+ supports `RETURNING` clause on INSERT, UPDATE, and DELETE — identical to PostgreSQL syntax:

```sql
INSERT INTO users (name, email) VALUES ('Alice', 'a@b.com') RETURNING id;
UPDATE users SET name = 'Bob' WHERE id = 1 RETURNING id, name;
DELETE FROM users WHERE id = 1 RETURNING *;
```

Since SQLite supports `RETURNING`, the existing [`ReturningQuery`](Drizzle4Dotnet/src/Core/Query/ReturningQuery.cs) and [`QueryReturningExtensions`](Drizzle4Dotnet/src/Core/Query/QueryReturningExtensions.cs) work out of the box with `SqliteSqlDialectImpl`.

The `SupportsReturning = true` flag in the dialect enables RETURNING on all DML operations through the existing core mechanism.

### 7.2 ON CONFLICT (Upsert)

SQLite's `ON CONFLICT` syntax (3.24+) is similar to PostgreSQL's:

```sql
INSERT INTO users (name, email) VALUES ('Alice', 'a@b.com')
ON CONFLICT(email) DO UPDATE SET name = excluded.name;

INSERT INTO users (name, email) VALUES ('Bob', 'b@c.com')
ON CONFLICT(email) DO NOTHING;
```

Implementation follows [`PgInsertQuery`](Drizzle4Dotnet/src/Dialect/PgSql/Query/PgInsertQuery.cs) pattern but simplified:

- Column-list conflict target only (no constraint name target)
- No `WHERE` on conflict target or SET
- Uses `excluded.` prefix (same as PostgreSQL)
- Create `SqliteExcludedNode` analogous to [`PgExcludedNode`](Drizzle4Dotnet/src/Dialect/PgSql/Operators/Nodes/PgExcludedNode.cs)

### 7.3 INSERT OR REPLACE / INSERT OR IGNORE

SQLite supports these as shorthand for `ON CONFLICT DO REPLACE/NOTHING`:

```sql
INSERT OR REPLACE INTO users (id, name) VALUES (1, 'Alice');
INSERT OR IGNORE INTO users (id, name) VALUES (1, 'Bob');
```

These should be exposed as chainable methods:

```csharp
public SqliteInsertQuery<TTable> OrReplace()
{
    _orAction = "OR REPLACE";
    return this;
}

public SqliteInsertQuery<TTable> OrIgnore()
{
    _orAction = "OR IGNORE";
    return this;
}
```

---

## 8. Phase S7: SQLite Schema / Column Types

### 8.1 SQLite Storage Classes

SQLite has only 5 storage classes:

| Storage Class | Mapped CLR Types | Notes |
|--------------|-----------------|-------|
| `INTEGER` | `int`, `long`, `short`, `byte`, `bool` | Signed up to 8 bytes |
| `REAL` | `float`, `double` | 8-byte IEEE floating point |
| `TEXT` | `string`, `char`, `Guid` | UTF-8, UTF-16BE, UTF-16LE |
| `BLOB` | `byte[]` | Stored exactly as input |
| `NULL` | `null` | — |

**Type Affinity System:** SQLite uses *type affinity* rather than strict types. The declared type is converted to one of the 5 affinities:
- `INT` / `INTEGER` → INTEGER affinity
- `TEXT` / `CHAR` / `VARCHAR` / `CLOB` → TEXT affinity
- `BLOB` → BLOB affinity (no conversion)
- `REAL` / `FLOAT` / `DOUBLE` → REAL affinity
- `NUMERIC` / `DECIMAL` / `BOOLEAN` / `DATE` / `DATETIME` → NUMERIC affinity

**Recommendation:** Use `INTEGER`, `REAL`, `TEXT`, `BLOB`, and `NUMERIC` as the primary SQLite types in DDL generation.

### 8.2 SQLite Type Map for DDL

Add to [`OrmSchemaExporter`](Drizzle4Dotnet/src/Core/Schema/Migration/OrmSchemaExporter.cs):

```csharp
public static readonly Dictionary<Type, string> SqliteTypeMap = new()
{
    [typeof(int)] = "INTEGER",
    [typeof(long)] = "INTEGER",
    [typeof(short)] = "INTEGER",
    [typeof(byte)] = "INTEGER",
    [typeof(string)] = "TEXT",
    [typeof(bool)] = "INTEGER",       // 0 or 1
    [typeof(decimal)] = "NUMERIC",
    [typeof(float)] = "REAL",
    [typeof(double)] = "REAL",
    [typeof(DateTime)] = "TEXT",      // ISO-8601 format
    [typeof(DateOnly)] = "TEXT",      // 'YYYY-MM-DD'
    [typeof(TimeOnly)] = "TEXT",      // 'HH:MM:SS'
    [typeof(Guid)] = "TEXT",          // hex string
    [typeof(byte[])] = "BLOB",
    [typeof(char)] = "TEXT",
};
```

### 8.3 Auto-increment

In SQLite:
- `INTEGER PRIMARY KEY` — auto-increments by default (rowid alias)
- `INTEGER PRIMARY KEY AUTOINCREMENT` — prevents reuse of rowid values

For the `[Column]` attribute's `AutoIncrement = true`:
- DDL output: `"id" INTEGER PRIMARY KEY AUTOINCREMENT`

### 8.4 SQLite Table Schema (Demo)

Create [`SharedDemo/Sqlite/Schema.cs`](SharedDemo/Sqlite/Schema.cs) with SQLite-specific schema definitions using `SqliteColumn<T, TTable>` and `SqliteSqlDialectImpl`.

---

## 9. Phase S8: SQLite Connection & Execution

### 9.1 Create [`SqliteDbClient.cs`](Drizzle4Dotnet/src/Sqlite/SqliteDbClient.cs)

```csharp
public class SqliteDbClient : DbClientWithTransaction<SqliteDbClient, SqliteSqlDialectImpl>
{
    public SqliteDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public SqliteSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns)
        where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
        => new(selectedColumns, this);

    public SqliteSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns)
        where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
        => new(selectedColumns, this).Distinct();

    public SqliteInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
        => new(table, this);

    public SqliteUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
        => new(table, this);

    public SqliteDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
        => new(table, this);

    protected override SqliteDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
        => new(conn, transaction);
}
```

### 9.2 Create [`SqliteQueryBuilder.cs`](Drizzle4Dotnet/src/Sqlite/SqliteQueryBuilder.cs)

For offline SQL building without a database connection (mirrors [`PgSqlQueryBuilder`](Drizzle4Dotnet/src/Dialect/PgSql/PgSqlQueryBuilder.cs) / [`MySqlQueryBuilder`](Drizzle4Dotnet/src/Dialect/MySql/MySqlQueryBuilder.cs)):

```csharp
public class SqliteQueryBuilder
{
    private static readonly IQueryExecutor<SqliteSqlDialectImpl>? _nullExecutor = null;

    public SqliteSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns)
        where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
        => new(selectedColumns, _nullExecutor!);

    // ... Insert, Update, Delete, SelectDistinct ...
}
```

### 9.3 NuGet Dependencies

Add `Microsoft.Data.Sqlite` to [`Drizzle4Dotnet.csproj`](Drizzle4Dotnet/Drizzle4Dotnet.csproj):

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
</ItemGroup>
```

Or keep the provider dependency optional by loading at runtime via `DbProviderFactory`:

```csharp
// Recommended approach — don't hard-code Microsoft.Data.Sqlite
var factory = DbProviderFactories.GetFactory("Microsoft.Data.Sqlite");
var conn = factory.CreateConnection();
conn.ConnectionString = "Data Source=mydb.sqlite";
```

---

## 10. Phase S9: Source Generator SQLite Support

### 10.1 Update [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs)

The source generator needs to support SQLite dialect by:

1. Recognizing `typeof(SqliteSqlDialectImpl)` in the `[Table]` attribute's `Dialect` property
2. Generating `SqliteColumn<T, TTable>` instead of `PgColumn<T, TTable>` or `MySqlColumn<T, TTable>`
3. Setting `TableRefName` based on `SqliteSqlDialectImpl`

The existing generator already compares dialect types — add a third branch:

```csharp
if (dialectType == "SqliteSqlDialectImpl" || dialectType == "Drizzle4Dotnet.Dialect.SqliteSqlDialectImpl")
{
    // Generate with SqliteColumn<,>, SqliteSqlDialectImpl
}
```

### 10.2 Update [`MigrationSchemaGenerator.cs`](SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs)

Add SQLite type mapping generation:

```csharp
private static string GetSqliteColumnType(string clrTypeName, bool isNullable)
{
    return clrTypeName switch
    {
        "int" or "long" or "short" or "byte" => "INTEGER",
        "string" => "TEXT",
        "bool" => "INTEGER",
        "decimal" => "NUMERIC",
        "float" or "double" => "REAL",
        "DateTime" or "DateOnly" or "TimeOnly" => "TEXT",
        "Guid" => "TEXT",
        "byte[]" => "BLOB",
        "char" => "TEXT",
        _ => "TEXT"
    };
}
```

---

## 11. Phase S10: Migration / DDL Layer for SQLite

### 11.1 SQLite DDL Characteristics

- **DDL is transactional** — `CREATE TABLE`, `ALTER TABLE`, etc. can be rolled back
- **Limited `ALTER TABLE`** — can only `RENAME TABLE`, `RENAME COLUMN`, `ADD COLUMN` (no DROP COLUMN, no ALTER COLUMN)
- **No `CREATE INDEX IF NOT EXISTS`** — use `CREATE INDEX IF NOT EXISTS` (supported)
- **No `DROP INDEX IF EXISTS`** — use `DROP INDEX IF EXISTS` (supported)
- **No `CREATE TEMPORARY TABLE`** — `CREATE TEMP TABLE` is supported
- **Foreign keys** — supported but not enforced by default (`PRAGMA foreign_keys = ON`)

### 11.2 Migration Strategy

The existing [`CreateTableQuery`](Drizzle4Dotnet/src/Core/Schema/Migration/CreateTableQuery.cs) builds DDL from `TableDefinition`. It currently generates dialect-agnostic SQL. For SQLite:

1. The existing DDL builder works as-is for `CREATE TABLE` with SQLite-compatible type names
2. Use `SqliteTypeMap` when extracting schema from ORM types
3. `ALTER TABLE` limitations mean schema migration may need table rebuilds (CREATE NEW → COPY DATA → DROP OLD → RENAME)

### 11.3 MigrationManager Compatibility

The existing [`MigrationManager`](Drizzle4Dotnet/src/Core/Schema/Migration/MigrationManager.cs) should work with SQLite since it:
- Uses `DbConnection` / `DbCommand` (provider-agnostic)
- Executes raw SQL strings
- Compares schema snapshots

However, the `TableDefinitionComparer` may need adjustments for SQLite's type affinity system (where `INT` and `INTEGER` are treated the same).

### 11.4 SQLite-Specific DDL Extensions

Consider adding SQLite-specific DDL helpers:

```csharp
public static class SqliteDdl
{
    /// <summary>Creates a PRAGMA statement.</summary>
    public static SqlPragma Pragma(string key, string? value = null) => new(key, value);
    
    /// <summary>VACUUM the database.</summary>
    public static VacuumQuery Vacuum() => new();
    
    /// <summary>ANALYZE the database.</summary>
    public static AnalyzeQuery Analyze(string? tableName = null) => new(tableName);
}
```

---

## 12. Phase S11: Testing Strategy

### 12.1 Test File Structure

```
Test/
├── Select/
│   └── SqliteSelectTests.cs
├── Insert/
│   └── SqliteInsertTests.cs
├── Update/
│   └── SqliteUpdateTests.cs
├── Delete/
│   └── SqliteDeleteTests.cs
└── Migration/
    └── SqliteMigrationTests.cs
```

### 12.2 Test Categories

| Category | What to Test | Example |
|----------|-------------|---------|
| **SELECT** | Basic SELECT, WHERE, JOIN, GROUP BY, HAVING, ORDER BY, LIMIT/OFFSET, DISTINCT, CTE, subqueries, NATURAL JOIN | `SELECT id, name FROM users WHERE age > 18` |
| **Compound SELECT** | UNION, UNION ALL, INTERSECT, EXCEPT | `SELECT id FROM users UNION SELECT id FROM admins` |
| **INSERT** | Basic INSERT, multi-row INSERT, INSERT ... SELECT, INSERT OR REPLACE, INSERT OR IGNORE, ON CONFLICT DO UPDATE/NOTHING, RETURNING | `INSERT INTO users (name) VALUES ('Alice') RETURNING id` |
| **UPDATE** | Basic UPDATE, WHERE, RETURNING, correlated subquery updates | `UPDATE users SET name = 'Bob' WHERE id = 1 RETURNING id` |
| **DELETE** | Basic DELETE, WHERE, RETURNING, correlated subquery deletes | `DELETE FROM users WHERE id = 1 RETURNING id` |
| **CTEs** | WITH, WITH RECURSIVE | Common table expressions |
| **Window functions** | ROW_NUMBER(), RANK(), etc. | `ROW_NUMBER() OVER (ORDER BY id)` |
| **JSON functions** | json_extract, json_object, etc. | `json_extract(data, '$.name')` |
| **DDL / Migration** | CREATE TABLE, DROP TABLE, ALTER TABLE, CREATE/DROP INDEX | Schema comparison and migration |
| **Error cases** | Unsupported features throw proper exceptions | `LIMIT/OFFSET on UPDATE/DELETE`, `INSERT DEFAULT VALUES`, `LATERAL JOIN` |

### 12.3 Test Infrastructure

Following [`Test/Select/Base.cs`](Test/Select/Base.cs) pattern:

```csharp
public class SqliteTestBase
{
    protected static SqliteQueryBuilder QueryBuilder => new();
    
    // For in-memory database testing:
    protected static SqliteConnection CreateInMemoryConnection()
    {
        var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        return conn;
    }
    
    protected static SqliteDbClient CreateClient(SqliteConnection? conn = null)
    {
        conn ??= CreateInMemoryConnection();
        return new SqliteDbClient(conn);
    }
}
```

Use `:memory:` database for unit tests — fast, no cleanup needed.

### 12.4 Example Test

```csharp
[Fact]
public void Select_WithWhere_GeneratesCorrectSql()
{
    var query = QueryBuilder
        .Select(UsersTable.Select)
        .From(UsersTable.Instance)
        .Where(UsersTable.Age > Sql.Value(18));
    
    var (sql, parameters) = query.Build();
    
    Assert.Contains("SELECT", sql);
    Assert.Contains("FROM \"users\"", sql);
    Assert.Contains("WHERE", sql);
}
```

---

## 13. Phase S12: NuGet Packaging & Demo

### 13.1 Demo Project

Create [`SharedDemo/Sqlite/Schema.cs`](SharedDemo/Sqlite/Schema.cs) with SQLite schema:

```csharp
[Table("users", Dialect = typeof(SqliteSqlDialectImpl))]
public class UsersTable : ISqliteDbTable
{
    public const string TableName = "users";
    public const string SchemaName = "";  // No schema in SQLite
    public const string TableRefName = "users";
    
    public static SqliteColumn<int, UsersTable> Id = new("id");
    public static SqliteColumn<string, UsersTable> Name = new("name");
    public static SqliteColumn<string, UsersTable> Email = new("email");
    
    public static ITypedTupleSelectedColumns<(int, string, string), SqliteSqlDialectImpl, ...> Select { get; }
}
```

### 13.2 Usage Example

```csharp
// Using Microsoft.Data.Sqlite
using var conn = new SqliteConnection("Data Source=mydb.sqlite");
conn.Open();

var db = new SqliteDbClient(conn);

// INSERT with RETURNING
var newId = await db.Insert(UsersTable)
    .Value(new { Name = "Alice", Email = "alice@example.com" })
    .Returning(UsersTable.Id)
    .ExecuteAsync();

// SELECT
var users = await db.Select(UsersTable.Select)
    .From(UsersTable)
    .Where(UsersTable.Age > Sql.Value(18))
    .OrderBy(UsersTable.Name)
    .ExecuteAsync();

// UPDATE with RETURNING and ON CONFLICT
var updated = await db.Update(UsersTable)
    .Set(UsersTable.Name, "Bob")
    .Where(UsersTable.Id == Sql.Value(1))
    .Returning(UsersTable.Id, UsersTable.Name)
    .ExecuteAsync();

// DELETE with RETURNING
var deleted = await db.Delete(UsersTable)
    .Where(UsersTable.Id == Sql.Value(1))
    .Returning(UsersTable.Id)
    .ExecuteAsync();

// UPSERT with ON CONFLICT
await db.Insert(UsersTable)
    .Value(new { Id = 1, Name = "Alice", Email = "a@b.com" })
    .OnConflict(UsersTable.Email)
    .DoUpdate()
    .SetOnConflictExcluded(UsersTable.Name)
    .ExecuteAsync();
```

### 13.3 Update [`Drizzle4Dotnet.csproj`](Drizzle4Dotnet/Drizzle4Dotnet.csproj)

Add SQLite provider package reference (conditional or unconditional):
```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.0" />
</ItemGroup>
```

### 13.4 Update Solution File

Add new source files to [`Drizzle4Dotnet.sln`](Drizzle4Dotnet.sln) and test projects.

---

## 14. Appendix: SQLite SQL Syntax Reference

### 14.1 DML Syntax

```sql
-- SELECT
SELECT [DISTINCT] select_expr [, ...]
  FROM table_reference [, ...]
  [JOIN ...]
  [WHERE expr]
  [GROUP BY expr [, ...]]
  [HAVING expr]
  [WINDOW window_name AS (window_spec)]
  [ORDER BY expr [ASC|DESC] [, ...]]
  [LIMIT n [OFFSET m]]

-- INSERT
INSERT [OR REPLACE|OR IGNORE|OR ROLLBACK|OR ABORT|OR FAIL] INTO table
  [(column [, ...])]
  {VALUES (expr [, ...]) [, ...] | select_statement}
  [ON CONFLICT [(conflict_target)] DO UPDATE SET col=expr [, ...] | DO NOTHING]
  [RETURNING * | expr [[AS] alias] [, ...]]

-- UPDATE
UPDATE table
  SET col=expr [, ...]
  [FROM other_table]  -- ❌ Not supported in SQLite
  [WHERE expr]
  [RETURNING * | expr [[AS] alias] [, ...]]

-- DELETE
DELETE FROM table
  [WHERE expr]
  [RETURNING * | expr [[AS] alias] [, ...]]

-- REPLACE (shorthand for INSERT OR REPLACE)
REPLACE INTO table [(column [, ...])] VALUES (expr [, ...])
```

### 14.2 Supported JOIN Types

| JOIN Type | SQLite Support |
|-----------|---------------|
| `INNER JOIN` | ✅ |
| `LEFT [OUTER] JOIN` | ✅ |
| `RIGHT [OUTER] JOIN` | ❌ |
| `FULL [OUTER] JOIN` | ⚠️ 3.39+ |
| `CROSS JOIN` | ✅ |
| `NATURAL JOIN` | ✅ |
| `NATURAL LEFT JOIN` | ✅ |
| `LATERAL JOIN` | ❌ |

### 14.3 Functions Quick Reference

| Category | SQLite Function | SQL | Notes |
|----------|----------------|-----|-------|
| **Date/Time** | Current datetime | `datetime('now')` | Returns 'YYYY-MM-DD HH:MM:SS' |
| | Current date | `date('now')` | Returns 'YYYY-MM-DD' |
| | Current time | `time('now')` | Returns 'HH:MM:SS' |
| | Format | `strftime(format, datetime)` | Like PostgreSQL's `to_char()` |
| | Modifier | `date('now', '+1 day')` | Add/subtract intervals |
| | Julian day | `julianday('now')` | Fractional days since -4713-11-24 |
| **String** | Concatenation | `a \|\| b` | Same as PostgreSQL |
| | Position | `instr(haystack, needle)` | Like `strpos()` in PostgreSQL |
| | Substring | `substr(str, start, len)` | 1-based index |
| | Replace | `replace(str, from, to)` | Standard |
| | Trim | `trim(str)`, `ltrim()`, `rtrim()` | Standard |
| | Length | `length(str)` | Character count |
| | Upper/Lower | `upper(str)`, `lower(str)` | Standard |
| **Numeric** | Random | `random()` | Returns -2^63 to 2^63-1 |
| | Absolute | `abs(x)` | Standard |
| | Round | `round(x, d)` | Standard |
| | Total changes | `total_changes()` | Cumulative row count |
| **Info** | Last insert rowid | `last_insert_rowid()` | Like MySQL's `LAST_INSERT_ID()` |
| | SQLite version | `sqlite_version()` | String |
| | Changes | `changes()` | Rows affected by last statement |
| **Aggregate** | Group concat | `group_concat(expr, sep)` | Like `string_agg()` in PostgreSQL |
| | JSON group array | `json_group_array(expr)` | Aggregate to JSON array |
| | JSON group object | `json_group_object(key, val)` | Aggregate to JSON object |

### 14.4 PRAGMA Statements

```sql
PRAGMA foreign_keys = ON;       -- Enable foreign key enforcement
PRAGMA journal_mode = WAL;      -- Write-Ahead Logging mode
PRAGMA synchronous = NORMAL;    -- Balance safety/speed
PRAGMA cache_size = -64000;     -- 64MB cache
PRAGMA busy_timeout = 5000;     -- 5 second busy timeout
PRAGMA encoding = 'UTF-8';      -- Text encoding
```

---

## 15. Appendix: Implementation Order & Dependencies

### Phase Dependency Graph

```
Phase S1: Dialect (Foundation)
    └── No dependencies
    └── Files: SqliteSqlDialectImpl.cs

Phase S2: Types & Interfaces
    └── Depends on: S1
    └── Files: SqliteTable.cs, SqliteColumn.cs

Phase S3: Functions
    └── Depends on: S1
    └── Files: SqliteStatics.cs, SqliteFunctions.*.cs

Phase S4: Operators & Nodes
    └── Depends on: S1
    └── Files: SqliteOperators.cs, Nodes/*.cs

Phase S5: DML Queries
    └── Depends on: S1, S2
    └── Files: Query/SqliteSelectQuery.cs, Query/SqliteInsertQuery.cs,
               Query/SqliteUpdateQuery.cs, Query/SqliteDeleteQuery.cs

Phase S6: Upsert & Returning
    └── Depends on: S5
    └── Enhanced: SqliteInsertQuery.cs (ON CONFLICT), plus RETURNING test

Phase S7: Schema / Columns
    └── Depends on: S1
    └── Files: OrmSchemaExporter update, type maps

Phase S8: Connection & Execution
    └── Depends on: S1, S2, S5
    └── Files: SqliteDbClient.cs, SqliteQueryBuilder.cs

Phase S9: Source Generator
    └── Depends on: S1 (needs the dialect type reference)
    └── Modified: TableGenerator.cs, MigrationSchemaGenerator.cs

Phase S10: Migration / DDL
    └── Depends on: S7, S8
    └── Tests, DDL adjustments

Phase S11: Testing
    └── Depends on: S1-S10
    └── Test files for all query types

Phase S12: Packaging & Demo
    └── Depends on: S1-S11
    └── Demo project, NuGet packaging, solution file updates
```

### Recommended Implementation Order

```
Step 1:  Phase S1  → SqliteSqlDialectImpl.cs
Step 2:  Phase S2  → SqliteTable.cs, SqliteColumn.cs
Step 3:  Phase S8  → SqliteDbClient.cs, SqliteQueryBuilder.cs (early for testing)
Step 4:  Phase S5  → SqliteSelectQuery.cs (test SELECT first)
Step 5:  Phase S5  → SqliteInsertQuery.cs (test INSERT)
Step 6:  Phase S6  → Add ON CONFLICT, RETURNING to insert
Step 7:  Phase S5  → SqliteUpdateQuery.cs (test UPDATE)
Step 8:  Phase S5  → SqliteDeleteQuery.cs (test DELETE)
Step 9:  Phase S3  → SqliteStatics.cs + SqliteFunctions.*.cs
Step 10: Phase S4  → SqliteOperators.cs + Nodes
Step 11: Phase S7  → Type maps, schema support
Step 12: Phase S9  → Source generator updates
Step 13: Phase S10 → Migration / DDL layer
Step 14: Phase S11 → Comprehensive tests
Step 15: Phase S12 → Demo, packaging
```

### Effort Estimate

| Phase | Files | Estimated Effort | Complexity |
|-------|-------|-----------------|------------|
| S1: Dialect | 1 | 0.5 day | Low |
| S2: Types & Interfaces | 2 | 0.25 day | Low |
| S3: Functions | 5-6 | 1 day | Medium |
| S4: Operators & Nodes | 2-3 | 0.5 day | Low |
| S5: DML Queries | 4 | 2 days | High |
| S6: Upsert & Returning | 1-2 | 1 day | Medium |
| S7: Schema / Columns | 1-2 | 0.5 day | Low |
| S8: Connection & Execution | 2 | 0.5 day | Low |
| S9: Source Generator | 2 | 1 day | Medium |
| S10: Migration / DDL | 1-2 | 1 day | Medium |
| S11: Testing | 5 | 2 days | Medium |
| S12: Packaging & Demo | 2-3 | 0.5 day | Low |
| **Total** | **~28-32** | **~10.75 days** | — |
