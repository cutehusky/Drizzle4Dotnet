# Drizzle4Dotnet — MySQL Implementation Plan

> **Status:** Draft  
> **Last Updated:** 2026-06-22  
> **Target:** Full MySQL/MariaDB dialect support, paralleling existing PostgreSQL implementation.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Phase M1: MySQL Dialect (Foundation)](#2-phase-m1-mysql-dialect-foundation)
3. [Phase M2: MySQL Types & Interfaces](#3-phase-m2-mysql-types--interfaces)
4. [Phase M3: MySQL Functions](#4-phase-m3-mysql-functions)
5. [Phase M4: MySQL Operators & Nodes](#5-phase-m4-mysql-operators--nodes)
6. [Phase M5: MySQL DML — INSERT ... ON DUPLICATE KEY UPDATE](#6-phase-m5-mysql-dml--insert--on-duplicate-key-update)
7. [Phase M6: MySQL DML — REPLACE, DELETE/UPDATE with JOIN](#7-phase-m6-mysql-dml--replace-deleteupdate-with-join)
8. [Phase M7: MySQL Schema / Column Types](#8-phase-m7-mysql-schema--column-types)
9. [Phase M8: MySQL Connection & Execution](#9-phase-m8-mysql-connection--execution)
10. [Phase M9: Source Generator MySQL Support](#10-phase-m9-source-generator-mysql-support)
11. [Phase M10: Testing Strategy](#11-phase-m10-testing-strategy)
12. [Phase M11: NuGet Packaging](#12-phase-m11-nuget-packaging)
13. [Appendix: MySQL SQL Syntax Reference](#13-appendix-mysql-sql-syntax-reference)
14. [Appendix: Implementation Order & Dependencies](#14-appendix-implementation-order--dependencies)

---

## 1. Architecture Overview

### 1.1 MySQL vs PostgreSQL — Key Differences

| Aspect | PostgreSQL | MySQL / MariaDB |
|--------|------------|-----------------|
| **Identifier quoting** | `"double quotes"` | `` `backticks` `` |
| **Parameter prefix** | `@p0`, `@p1` (Npgsql) | `@p0`, `@p1` (MySqlConnector) — same convention ✅ |
| **Schema support** | `schema.table` | Database = schema; `db.table` or just `table` |
| **LIMIT / OFFSET** | `LIMIT ? OFFSET ?` | `LIMIT ?, ?` or `LIMIT ? OFFSET ?` |
| **RETURNING clause** | ✅ Supported (8.2+) | ❌ Not supported (use `LAST_INSERT_ID()`) |
| **UPSERT** | `ON CONFLICT [...] DO UPDATE/NOTHING` | `ON DUPLICATE KEY UPDATE` / `REPLACE INTO` |
| **INSERT DEFAULT VALUES** | `INSERT INTO t DEFAULT VALUES` | ❌ Not supported (use `INSERT INTO t () VALUES ()`) |
| **UPDATE with JOIN** | `UPDATE t SET ... FROM other WHERE ...` | `UPDATE t JOIN other ON ... SET ... WHERE ...` |
| **DELETE with JOIN** | `DELETE FROM t USING other WHERE ...` | `DELETE t FROM t JOIN other ON ... WHERE ...` |
| **CTEs (WITH)** | ✅ Supported | ⚠️ MySQL 8.0+ ✅; MariaDB 10.2+ ✅ |
| **Recursive CTEs** | ✅ Supported | ✅ MySQL 8.0+ / MariaDB 10.2+ |
| **Window functions** | ✅ Supported (8.4+) | ⚠️ MySQL 8.0+ ✅; MariaDB 10.2+ partial |
| **JSON functions** | Native JSON + operators (`->`, `->>`) | `JSON_EXTRACT()`, `JSON_UNQUOTE()`, `->` / `->>` (MySQL 8.0+) |
| **Array types** | ✅ Native array type | ❌ Not supported |
| **`IS DISTINCT FROM`** | ✅ Supported | ❌ Not supported |
| **`FILTER (WHERE ...)`** | ✅ Supported | ❌ Not supported |
| **`NOW()` / `RANDOM()`** | `NOW()` / `RANDOM()` | `NOW()` / `RAND()` |
| **String concat** | `\|\|` operator | `CONCAT()` function (or `\|\|` with PIPES_AS_CONCAT mode) |
| **`LAST_INSERT_ID()`** | Use `RETURNING` | `SELECT LAST_INSERT_ID()` |
| **Interval syntax** | `INTERVAL '1 day'` | `INTERVAL 1 DAY` (without quotes) |

### 1.2 Target File Structure

```
Drizzle4Dotnet/
├── src/
│   ├── Core/                          # Existing — no changes needed
│   │   ├── DbClient.cs
│   │   ├── Query/...
│   │   ├── Schema/...
│   │   └── Shared/...
│   ├── Dialect/
│   │   ├── PgSqlSqlDialectImpl.cs     # Existing
│   │   └── MySqlSqlDialectImpl.cs     # NEW — MySQL dialect
│   └── MySql/                          # NEW — MySQL-specific namespace
│       ├── MySqlTable.cs               # Table/alias interfaces
│       ├── MySqlColumn.cs              # Column types
│       ├── MySqlFunctions.cs           # MySQL-specific functions
│       ├── MySqlOperators.cs           # MySQL-specific operators
│       └── Nodes/
│           └── LastInsertIdNode.cs     # LAST_INSERT_ID() wrapper
│
Test/
├── Select/
│   ├── Base.cs                        # Existing — PostgreSQL tests
│   └── MySql/                          # NEW — MySQL tests
│       └── SelectTests.cs
├── Insert/
│   └── MySql/
│       └── InsertTests.cs
├── Delete/
│   └── MySql/
│       └── DeleteTests.cs
├── Update/
│   └── MySql/
│       └── UpdateTests.cs
│
SharedDemo/
├── Schema.cs                          # Add MySql-specific schema
├── data.sql
└── schema.sql
```

### 1.3 MySQL/MariaDB Provider Options

The ORM uses `System.Data.Common.DbConnection` / `DbCommand`, so any ADO.NET-compatible MySQL provider works:

| Provider | NuGet | Notes |
|----------|-------|-------|
| **MySqlConnector** | `MySqlConnector` | Recommended — modern, async, high-performance |
| **MySQL.Data** | `MySql.Data` | Oracle's official provider |
| **MariaDB** | `MariaDB.EntityFrameworkCore` | MariaDB-specific |

**Recommendation:** Support `MySqlConnector` as primary, via `DbConnection` abstraction.

---

## 2. Phase M1: MySQL Dialect (Foundation)

### 2.1 `MySqlSqlDialectImpl`

**File:** [`Drizzle4Dotnet/src/Dialect/MySqlSqlDialectImpl.cs`](Drizzle4Dotnet/src/Dialect/MySqlSqlDialectImpl.cs) — **New file**

**Pattern:** Follow [`PgSqlSqlDialectImpl.cs`](Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs).

```csharp
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Dialect;

public class MySqlSqlDialectImpl : ISqlDialect
{
    public static string BuildIdentifier(string identifier)
    {
        return $"`{identifier}`";                          // Backtick quoting
    }

    public static string BuildTableName(string schemaName, string tableName)
    {
        // MySQL: `database`.`table` or just `table`
        return string.IsNullOrEmpty(schemaName) 
            ? $"`{tableName}`" 
            : $"`{schemaName}`.`{tableName}`";
    }

    public static string BuildColumnName(string refName, string columnName)
    {
        return $"`{refName}`.`{columnName}`";               // Backtick quoting
    }

    public static string BuildParameterName(string parameterName)
    {
        return $"@{parameterName}";                         // Same as PostgreSQL
    }

    public static string BuildParameterName(int parameterIndex)
    {
        return $"@p{parameterIndex}";                       // Same convention
    }
}
```

### 2.2 Dialect Interface Extensions Needed for MySQL

The current [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) is minimal. MySQL requires additional dialect methods that the existing codebase will call. These should be added to the interface (with default implementations or static abstract methods):

| Method | Purpose | MySQL Behavior |
|--------|---------|----------------|
| `BuildLimitOffset(int? limit, int? offset)` | LIMIT/OFFET clause | `LIMIT {limit} OFFSET {offset}` |
| `BuildReturning()` | RETURNING clause | Empty string (not supported) |
| `BuildConflictTarget(params string[] columns)` | Upsert conflict target | `ON DUPLICATE KEY UPDATE` (no target columns) |
| `BuildUpsertAction()` | Upsert action | `UPDATE col=VALUES(col)` pattern |
| `BuildLastInsertId()` | Get last inserted ID | `SELECT LAST_INSERT_ID()` |
| `EscapeString(string value)` | String escaping | MySQL string escape rules |
| `SupportsReturning` | Whether dialect supports RETURNING | `false` |
| `SupportsArrays` | Whether dialect supports array types | `false` |
| `SupportsJson` | Whether dialect supports JSON (native) | `true` (MySQL 8.0+) |
| `SupportsWindowFunctions` | Whether dialect supports window functions | `true` (MySQL 8.0+) |
| `SupportsCte` | Whether dialect supports CTEs | `true` (MySQL 8.0+) |
| `SupportsRecursiveCte` | Whether dialect supports recursive CTEs | `true` (MySQL 8.0+) |

**Implementation Priority:** Add these to [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) **before** implementing MySQL-specific features, as the query builders need to call them.

### 2.3 Proposed `ISqlDialect` Extension

```csharp
public interface ISqlDialect
{
    // Existing
    static abstract string BuildIdentifier(string identifier);
    static abstract string BuildTableName(string schemaName, string tableName);
    static abstract string BuildColumnName(string tableName, string columnName);
    static abstract string BuildParameterName(string parameterName);
    static abstract string BuildParameterName(int parameterIndex);
    
    // New — Limit/Offset
    static abstract string BuildLimitOffset(int? limit, int? offset);
    
    // New — Feature flags
    static abstract bool SupportsReturning { get; }
    static abstract bool SupportsArrays { get; }
    static abstract bool SupportsJson { get; }
    static abstract bool SupportsWindowFunctions { get; }
    static abstract bool SupportsCte { get; }
    static abstract bool SupportsRecursiveCte { get; }
    static abstract bool SupportsDeleteUsing { get; }
    static abstract bool SupportsIsDistinctFrom { get; }
    static abstract bool SupportsFilteredAggregates { get; }
    
    // New — Upsert
    static abstract string BuildOnDuplicateKeyUpdate(IReadOnlyList<string> columns);
    
    // New — String escaping
    static abstract string EscapeString(string value);
}
```

### 2.4 Default Interface Implementation Strategy

Since `ISqlDialect` uses `static abstract` members (C# 11 / .NET 7+), there are no default implementations. Each dialect must implement all members. However, we can create a **base class** pattern:

```csharp
public abstract class SqlDialectBase : ISqlDialect
{
    public static abstract string BuildIdentifier(string identifier);
    public static abstract string BuildTableName(string schemaName, string tableName);
    public static abstract string BuildColumnName(string tableName, string columnName);
    public static abstract string BuildParameterName(string parameterName);
    public static abstract string BuildParameterName(int parameterIndex);
    
    // Default implementations can go here as virtual, but with static abstract
    // we unfortunately can't provide defaults. Each dialect must implement.
}
```

Alternatively, adopt a **helper class** pattern — a non-static utility that provides default behaviors:

```csharp
public static class SqlDialectDefaults
{
    public static string BuildLimitOffset(int? limit, int? offset)
    {
        if (limit.HasValue && offset.HasValue)
            return $" LIMIT {limit} OFFSET {offset}";
        if (limit.HasValue)
            return $" LIMIT {limit}";
        return "";
    }
    
    public static string BuildReturning() => "";
    public static bool SupportsReturning => false;
    // etc.
}
```

**Recommendation:** Use the helper class approach to reduce duplication across dialects.

---

## 3. Phase M2: MySQL Types & Interfaces

### 3.1 `MySqlTable.cs`

**File:** [`Drizzle4Dotnet/src/MySql/MySqlTable.cs`](Drizzle4Dotnet/src/MySql/MySqlTable.cs) — **New file**

Follow the pattern of [`PgTable.cs`](Drizzle4Dotnet/src/PgSql/PgTable.cs):

```csharp
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

public interface IMySqlGenericTable : IGenericTable<MySqlSqlDialectImpl> { }
public interface IMySqlTable : ITable<MySqlSqlDialectImpl> { }
public interface IMySqlCteTable : ICteTable<MySqlSqlDialectImpl> { }
public interface IMySqlVirtualTable : IVirtualTable<MySqlSqlDialectImpl> { }
public interface IMySqlDbTable : IDbTable<MySqlSqlDialectImpl> { }
public interface IMySqlTableAlias : ITableAlias<MySqlSqlDialectImpl> { }

public class MySqlSubqueryTable : RawSubqueryTableSql<MySqlSqlDialectImpl>,
    IMySqlGenericTable, IMySqlCteTable, IMySqlVirtualTable
{
    public MySqlSubqueryTable(RawSql<MySqlSqlDialectImpl> sql, string alias, bool isCte = false) 
        : base(sql, alias, isCte) { }
}
```

### 3.2 `MySqlColumn.cs`

**File:** [`Drizzle4Dotnet/src/MySql/MySqlColumn.cs`](Drizzle4Dotnet/src/MySql/MySqlColumn.cs) — **New file**

Follow the pattern of [`PgColumn.cs`](Drizzle4Dotnet/src/PgSql/PgColumn.cs):

```csharp
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

public class MySqlColumn<T, TTable> : DbColumn<T, TTable, MySqlSqlDialectImpl>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    public MySqlColumn(string columnName) : base(columnName) { }
}

public class MySqlVirtualColumn<T> : VirtualColumn<T, MySqlSqlDialectImpl>
{
    public MySqlVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
```

### 3.3 Generated Table Pattern (MySQL)

When a table class is generated via [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs), it currently hardcodes `PgSqlSqlDialectImpl`. The MySQL variant would look like:

```csharp
// Generated by source generator with [Table("users", Dialect = typeof(MySqlSqlDialectImpl))]
public partial class UsersTable : IMySqlDbTable
{
    public const string TableRefName = "Users";
    public const string TableName = "users";
    public const string SchemaName = "";

    // Column constants
    public static class ColumnNames
    {
        public const string Id = "id";
        public const string Name = "name";
        // etc.
    }

    // Column instances
    public static readonly MySqlColumn<int, UsersTable> Id = new("id");
    public static readonly MySqlColumn<string, UsersTable> Name = new("name");
    // etc.

    // Implementation
    public void BuildSql(ISqlBuilder sqlBuilder) => 
        sqlBuilder.Append(MySqlSqlDialectImpl.BuildTableName(SchemaName, TableName));
    public void BuildRefSql(ISqlBuilder sqlBuilder) => BuildSql(sqlBuilder);
}
```

---

## 4. Phase M3: MySQL Functions

### 4.1 `MySqlFunctions.cs`

**File:** [`Drizzle4Dotnet/src/MySql/MySqlFunctions.cs`](Drizzle4Dotnet/src/MySql/MySqlFunctions.cs) — **New file**

Following the pattern of [`PgFunctions.cs`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs), but for MySQL-specific functions and syntax variations.

```csharp
namespace Drizzle4Dotnet.MySql;

public static class MySqlFunctions
{
    // ======================================================================
    // String Functions (MySQL-specific)
    // ======================================================================
    
    // MySQL CONCAT (variable args) — MySQL uses CONCAT(), not ||
    public static FunctionCallNode<string> Concat(params IGenericSql[] args)
        => new FunctionCallNode<string>("CONCAT", args);
    public static FunctionCallNode<string> Concat(params string[] args)
        => new FunctionCallNode<string>("CONCAT", 
            args.Select(a => new SqlValueNode<string>(a)).Cast<IGenericSql>().ToArray());
    
    // ConcatWs with separator (standard, same function name as Pg)
    public static FunctionCallNode<string> ConcatWs(IGenericSql separator, params IGenericSql[] columns) { ... }
    
    // MySQL-specific: CHAR_LENGTH (returns character count), LENGTH (returns byte count)
    public static UnaryNode<T, long> CharLength<T>(ISql<T> c1) 
        => new(c1, "CHAR_LENGTH", true);
    public static UnaryNode<T, long> CharLength<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect
        => new(c1, "CHAR_LENGTH", true);
    
    // LOCATE(substr, str) / LOCATE(substr, str, pos) — MySQL-specific
    public static FunctionCallNode<long> Locate(IGenericSql substr, IGenericSql str)
        => new FunctionCallNode<long>("LOCATE", substr, str);
    public static FunctionCallNode<long> Locate(IGenericSql substr, IGenericSql str, int pos)
        => new FunctionCallNode<long>("LOCATE", substr, str, new SqlValueNode<int>(pos));
    
    // MySQL SUBSTRING_INDEX(str, delim, count)
    public static FunctionCallNode<string> SubstringIndex(IGenericSql str, IGenericSql delim, int count)
        => new FunctionCallNode<string>("SUBSTRING_INDEX", str, delim, new SqlValueNode<int>(count));
    
    // MySQL POSITION — same as PostgreSQL (POSITION(substr IN str))
    // Can reuse the same PositionNode pattern
    
    
    // ======================================================================
    // Date/Time Functions (MySQL-specific)
    // ======================================================================
    
    // MySQL NOW() ✅ (same as PostgreSQL)
    // Uses core Functions.Now() — no override needed
    
    // MySQL CURDATE() / CURTIME()
    public static FunctionCallNode<DateTime> CurDate()
        => new FunctionCallNode<DateTime>("CURDATE");
    public static FunctionCallNode<TimeSpan> CurTime()
        => new FunctionCallNode<TimeSpan>("CURTIME");
    
    // DATE_ADD(date, INTERVAL expr unit) — MySQL syntax
    public static FunctionCallNode<DateTime> DateAdd(ISql<DateTime> date, int amount, string unit)
        => new FunctionCallNode<DateTime>("DATE_ADD", date, new IntervalMySqlNode(amount, unit));
    public static FunctionCallNode<DateTime> DateSub(ISql<DateTime> date, int amount, string unit)
        => new FunctionCallNode<DateTime>("DATE_SUB", date, new IntervalMySqlNode(amount, unit));
    
    // DATE_FORMAT(date, format) — MySQL-specific
    public static FunctionCallNode<string> DateFormat(ISql<DateTime> date, string format)
        => new FunctionCallNode<string>("DATE_FORMAT", date, new SqlValueNode<string>(format));
    
    // UNIX_TIMESTAMP / FROM_UNIXTIME
    public static UnaryNode<DateTime, long> UnixTimestamp(ISql<DateTime> date)
        => new(date, "UNIX_TIMESTAMP", true);
    public static UnaryNode<long, DateTime> FromUnixTime(ISql<long> timestamp)
        => new(timestamp, "FROM_UNIXTIME", true);
    
    // EXTRACT — same SQL standard, works in both
    // Uses core Extract via FunctionCallNode or PgFunctions.Extract pattern
    
    // STR_TO_DATE(str, format) — MySQL-specific
    public static FunctionCallNode<DateTime> StrToDate(IGenericSql str, string format)
        => new FunctionCallNode<DateTime>("STR_TO_DATE", str, new SqlValueNode<string>(format));
    
    
    // ======================================================================
    // Numeric / Math Functions (MySQL-specific)
    // ======================================================================
    
    // RAND() — MySQL uses RAND(), PostgreSQL uses RANDOM()
    public static FunctionCallNode<T> Rand<T>()
        => new FunctionCallNode<T>("RAND");
    
    // MySQL TRUNCATE (not TRUNC)
    public static FunctionCallNode<T> Truncate<T>(ISql<T> c1, int decimals)
        => new FunctionCallNode<T>("TRUNCATE", c1, new SqlValueNode<int>(decimals));
    
    // MySQL-specific: BIT_COUNT, CRC32, etc.
    public static UnaryNode<T, long> BitCount<T>(ISql<T> c1)
        => new(c1, "BIT_COUNT", true);
    public static UnaryNode<T, long> Crc32<T>(ISql<T> c1)
        => new(c1, "CRC32", true);
    
    
    // ======================================================================
    // JSON Functions (MySQL)
    // ======================================================================
    
    // JSON_EXTRACT(col, path) -> JSON_EXTRACT(col, '$.path')
    // MySQL 8.0+ also supports -> and ->> operators
    public static FunctionCallNode<V> JsonExtract<T, V>(ISql<T> c1, string path)
        => new FunctionCallNode<V>("JSON_EXTRACT", c1, new SqlValueNode<string>(path));
    public static FunctionCallNode<string> JsonExtractText<T>(ISql<T> c1, string path)
        => new FunctionCallNode<string>("JSON_UNQUOTE", 
            new FunctionCallNode<string>("JSON_EXTRACT", c1, new SqlValueNode<string>(path)));
    
    // JSON_EXTRACT using -> operator (MySQL 8.0+)
    public static BinaryNode<T, string, V> JsonArrow<T, V>(ISql<T> c1, string path)
        => new(c1, new SqlValueNode<string>(path), " -> ");
    public static BinaryNode<T, string, string> JsonArrowText<T>(ISql<T> c1, string path)
        => new(c1, new SqlValueNode<string>(path), " ->> ");
    
    // JSON_ARRAY_AGG / JSON_OBJECT_AGG (MySQL 8.0+)
    public static UnaryNode<T> JsonArrayAgg<T>(ISql<T> c1)
        => new(c1, "JSON_ARRAYAGG", true);  // Note: no underscore in MySQL
    public static FunctionCallNode<string> JsonObjectAgg(IGenericSql key, IGenericSql value)
        => new FunctionCallNode<string>("JSON_OBJECTAGG", key, value);
    
    // JSON_ARRAY / JSON_OBJECT constructors
    public static FunctionCallNode<string> JsonArray(params IGenericSql[] values)
        => new FunctionCallNode<string>("JSON_ARRAY", values);
    public static FunctionCallNode<string> JsonObject(params IGenericSql[] keyValuePairs)
        => new FunctionCallNode<string>("JSON_OBJECT", keyValuePairs);
    
    // JSON_CONTAINS(col, value) — MySQL-specific
    public static FunctionCallNode<bool> JsonContains(IGenericSql c1, IGenericSql value)
        => new FunctionCallNode<bool>("JSON_CONTAINS", c1, value);
    
    // JSON_LENGTH(col) — MySQL-specific
    public static UnaryNode<T, int> JsonLength<T>(ISql<T> c1)
        => new(c1, "JSON_LENGTH", true);
    
    // JSON_KEYS(col) — MySQL-specific
    public static UnaryNode<T> JsonKeys<T>(ISql<T> c1)
        => new(c1, "JSON_KEYS", true);
    
    
    // ======================================================================
    // Info / Utility Functions
    // ======================================================================
    
    // LAST_INSERT_ID() — MySQL-specific
    public static FunctionCallNode<long> LastInsertId()
        => new FunctionCallNode<long>("LAST_INSERT_ID");
    
    // DATABASE() — current database name
    public static FunctionCallNode<string> Database()
        => new FunctionCallNode<string>("DATABASE");
    
    // VERSION() — MySQL version
    public static FunctionCallNode<string> Version()
        => new FunctionCallNode<string>("VERSION");
    
    // FOUND_ROWS() — used with SQL_CALC_FOUND_ROWS
    public static FunctionCallNode<long> FoundRows()
        => new FunctionCallNode<long>("FOUND_ROWS");
    
    
    // ======================================================================
    // Conditional Expressions (MySQL-specific)
    // ======================================================================
    
    // IF(condition, true_val, false_val) — MySQL-specific (not IIF)
    public static FunctionCallNode<T> If<T>(IGenericSql condition, IGenericSql trueVal, IGenericSql falseVal)
        => new FunctionCallNode<T>("IF", condition, trueVal, falseVal);
    
    // IFNULL(expr, default) — MySQL-specific (like COALESCE with 2 args)
    public static FunctionCallNode<T> IfNull<T>(IGenericSql expr, IGenericSql defaultVal)
        => new FunctionCallNode<T>("IFNULL", expr, defaultVal);
}
```

### 4.2 `IntervalMySqlNode.cs`

**File:** [`Drizzle4Dotnet/src/MySql/Nodes/IntervalMySqlNode.cs`](Drizzle4Dotnet/src/MySql/Nodes/IntervalMySqlNode.cs) — **New file**

MySQL interval syntax differs from PostgreSQL:
- PostgreSQL: `INTERVAL '1 day'`
- MySQL: `INTERVAL 1 DAY` (no quotes around the value+unit)

```csharp
public readonly struct IntervalMySqlNode : IOperator<DateTime>
{
    private readonly int _amount;
    private readonly string _unit;

    public IntervalMySqlNode(int amount, string unit)
    {
        _amount = amount;
        _unit = unit;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INTERVAL ");
        sqlBuilder.Append(_amount.ToString());
        sqlBuilder.Append(' ');
        sqlBuilder.Append(_unit);
    }
}
```

---

## 5. Phase M4: MySQL Operators & Nodes

### 5.1 `MySqlOperators.cs`

**File:** [`Drizzle4Dotnet/src/MySql/MySqlOperators.cs`](Drizzle4Dotnet/src/MySql/MySqlOperators.cs) — **New file**

MySQL-specific operators that differ from or extend the core [`Operators`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs):

```csharp
namespace Drizzle4Dotnet.MySql;

public static class MySqlOperators
{
    // ======================================================================
    // Comparison Operators
    // ======================================================================
    
    // MySQL <=> (null-safe equality operator) — equivalent to IS NOT DISTINCT FROM
    const string _operatorNullSafeEqual = " <=> ";
    
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, _operatorNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), _operatorNullSafeEqual);
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2, TDialect>(
        this IColumnOfDialect<T1, TDialect> c1, IColumnOfDialect<T2, TDialect> c2)
        where TDialect : ISqlDialect
        => new(c1, c2, _operatorNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T, TDialect>(
        this IColumnOfDialect<T, TDialect> c1, T value)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<T>(value), _operatorNullSafeEqual);
    
    
    // ======================================================================
    // String Operators
    // ======================================================================
    
    // MySQL uses CONCAT() function, not || operator by default
    // However, || can be used if PIPES_AS_CONCAT mode is enabled
    // For safety, we provide CONCAT() via MySqlFunctions.Concat
    
    
    // ======================================================================
    // Regular Expression Operators
    // ======================================================================
    
    const string _operatorRegexp = " REGEXP ";
    const string _operatorNotRegexp = " NOT REGEXP ";
    const string _operatorRLike = " RLIKE ";  // Synonym for REGEXP
    
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, _operatorRegexp);
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), _operatorRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, _operatorNotRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), _operatorNotRegexp);
    
    public static BinaryNode<string, string, bool> Regexp<TDialect>(
        this IColumnOfDialect<string, TDialect> c1, string pattern)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<string>(pattern), _operatorRegexp);
    public static BinaryNode<string, string, bool> NotRegexp<TDialect>(
        this IColumnOfDialect<string, TDialect> c1, string pattern)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<string>(pattern), _operatorNotRegexp);
}
```

### 5.2 MySQL-Specific Nodes

**MySQL does not have `IS DISTINCT FROM`** — the equivalent is `<=>` (null-safe equality) provided above.

**MySQL does not have `FILTER (WHERE ...)`** on aggregate functions — use `SUM(CASE WHEN ...)` pattern instead or `IF()` function.

**MySQL `POSITION` function** — same SQL standard syntax as PostgreSQL (`POSITION(substr IN str)`), so [`PositionNode`](Drizzle4Dotnet/src/PgSql/Nodes/PositionNode.cs) can be shared or moved to Core.

### 5.3 String Concatenation Strategy

PostgreSQL uses `||` for string concatenation. MySQL supports `||` only in `PIPES_AS_CONCAT` SQL mode.

**Recommendation:** 

1. Keep `||` operator in core [`Operators.Concat()`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs#L205-L206) as-is.
2. For MySQL, provide `MySqlFunctions.Concat()` as the canonical way to concatenate strings.
3. Add a `SupportsPipeConcat` flag to [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) to let query builders optionally switch behavior.

---

## 6. Phase M5: MySQL DML — INSERT ... ON DUPLICATE KEY UPDATE

### 6.1 MySQL UPSERT Pattern

MySQL uses `INSERT ... ON DUPLICATE KEY UPDATE` instead of PostgreSQL's `ON CONFLICT [...] DO UPDATE`.

**Proposed API:**

```csharp
// MySQL: ON DUPLICATE KEY UPDATE
var q = _db.Insert(UsersTable)
    .Values(new { Id = 1, Name = "Alice", Email = "alice@example.com" })
    .OnDuplicateKeyUpdate()
    .Set(UsersTable.Name, "Updated Alice")
    .Set(UsersTable.Email, "newalice@example.com");
```

**Generated SQL:**
```sql
INSERT INTO `users` (`id`, `name`, `email`) 
VALUES (@p0, @p1, @p2)
ON DUPLICATE KEY UPDATE 
    `name` = VALUES(`name`), 
    `email` = VALUES(`email`)
```

### 6.2 Implementation

**Files to modify:**
- [`InsertQuery.cs`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) — Add MySQL-specific overloads or dialect-conditional behavior

**Design Options:**

**Option A (Recommended):** Add a `OnDuplicateKeyUpdate()` method to [`InsertQuery`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) that works for any dialect, but only emits SQL when the dialect is MySQL/MariaDB. For PostgreSQL, it would be a no-op (or throw).

**Option B:** Create a separate `MySqlInsertQuery` class that extends `InsertQuery` with MySQL-specific methods.

**Option C:** Make the UPSERT behavior entirely dialect-driven — the dialect provides the SQL fragment for the conflict clause.

**Recommended: Option C** — Add to [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs):
```csharp
static abstract string BuildOnDuplicateKeyUpdate(IReadOnlyList<string> columns);
```

For MySQL:
```csharp
public static string BuildOnDuplicateKeyUpdate(IReadOnlyList<string> columns)
{
    var assignments = columns.Select(c => $"`{c}` = VALUES(`{c}`)");
    return "ON DUPLICATE KEY UPDATE " + string.Join(", ", assignments);
}
```

For PostgreSQL: throw `NotSupportedException` or return empty.

### 6.3 REPLACE INTO

MySQL also supports `REPLACE INTO` which is `DELETE` + `INSERT`:

```csharp
var q = _db.Replace(UsersTable)
    .Values(new { Id = 1, Name = "Alice" });
```

**Generated SQL:**
```sql
REPLACE INTO `users` (`id`, `name`) VALUES (@p0, @p1)
```

**Implementation:**
- Add `Replace()` method to [`IQueryBuilder<TDialect>`](Drizzle4Dotnet/src/Core/DbClient.cs#L11-L25) and [`DbClient<TDialect>`](Drizzle4Dotnet/src/Core/DbClient.cs#L65)
- Create or adapt `InsertQuery` to emit `REPLACE` instead of `INSERT` when called via `Replace()`

### 6.4 INSERT ... SET (MySQL Syntax)

MySQL supports `INSERT INTO table SET col=value, col=value` syntax:

```csharp
var q = _db.Insert(UsersTable)
    .Set(UsersTable.Name, "Alice")
    .Set(UsersTable.Email, "alice@example.com");
```

**Generated SQL:**
```sql
INSERT INTO `users` SET `name` = @p0, `email` = @p1
```

### 6.5 INSERT IGNORE

```csharp
var q = _db.Insert(UsersTable)
    .Ignore()
    .Values(new { Id = 1, Name = "Alice" });
```

**Generated SQL:**
```sql
INSERT IGNORE INTO `users` (`id`, `name`) VALUES (@p0, @p1)
```

---

## 7. Phase M6: MySQL DML — REPLACE, DELETE/UPDATE with JOIN

### 7.1 UPDATE with JOIN

MySQL syntax differs significantly from PostgreSQL:

**PostgreSQL:**
```sql
UPDATE users SET salary = 50000
FROM departments 
WHERE users.department_id = departments.id 
  AND departments.name = 'Engineering'
```

**MySQL:**
```sql
UPDATE users
JOIN departments ON users.department_id = departments.id
SET users.salary = 50000
WHERE departments.name = 'Engineering'
```

**Proposed API:**
```csharp
var q = _db.Update(UsersTable)
    .Set(UsersTable.Salary, 50000)
    .Join(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
    .Where(Eq(DepartmentsTable.Name, "Engineering"));
```

**Implementation:**
- Modify [`UpdateQuery.BuildSql()`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs) to handle `JOIN` syntax for MySQL
- Add `Join()`, `InnerJoin()`, `LeftJoin()` methods (similar to SelectQuery's join methods)
- The dialect provides the join SQL style via `BuildUpdateJoin()`

### 7.2 DELETE with JOIN

**PostgreSQL:**
```sql
DELETE FROM users
USING departments
WHERE users.department_id = departments.id 
  AND departments.name = 'Archived'
```

**MySQL:**
```sql
DELETE users FROM users
JOIN departments ON users.department_id = departments.id
WHERE departments.name = 'Archived'
```

**Proposed API:**
```csharp
var q = _db.Delete(UsersTable)
    .Join(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
    .Where(Eq(DepartmentsTable.Name, "Archived"));
```

**Implementation:**
- Modify [`DeleteQuery.BuildSql()`](Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs) to handle JOIN syntax for MySQL
- Add `Join()` methods to `DeleteQuery`

### 7.3 LIMIT on UPDATE/DELETE

MySQL supports `LIMIT` on UPDATE and DELETE statements:

```sql
DELETE FROM users WHERE is_active = 0 LIMIT 100
UPDATE users SET status = 'inactive' WHERE last_login < '2020-01-01' LIMIT 1000
```

**Proposed API:**
```csharp
var q = _db.Delete(UsersTable)
    .Where(Eq(UsersTable.IsActive, false))
    .Limit(100);

var q2 = _db.Update(UsersTable)
    .Set(UsersTable.Status, "inactive")
    .Where(Lt(UsersTable.LastLogin, new DateTime(2020, 1, 1)))
    .Limit(1000);
```

**Implementation:**
- Add `Limit(int count)` to `DeleteQuery` and `UpdateQuery`
- The dialect controls whether LIMIT is emitted via `SupportsDeleteLimit` / `SupportsUpdateLimit`

### 7.4 ORDER BY on UPDATE/DELETE (MySQL)

MySQL also supports `ORDER BY` on UPDATE/DELETE when used with `LIMIT`:

```sql
DELETE FROM users WHERE is_active = 0 ORDER BY id LIMIT 100
```

**Proposed API:**
```csharp
var q = _db.Delete(UsersTable)
    .Where(Eq(UsersTable.IsActive, false))
    .OrderBy(UsersTable.Id)
    .Limit(100);
```

---

## 8. Phase M7: MySQL Schema / Column Types

### 8.1 MySQL Type Mappings

| MySQL Type | C# Type | DbType | Notes |
|-----------|---------|--------|-------|
| `TINYINT(1)` | `bool` | `Boolean` | MySQL treats TINYINT(1) as bool |
| `TINYINT` | `sbyte` | `SByte` | |
| `SMALLINT` | `short` | `Int16` | |
| `MEDIUMINT` | `int` | `Int32` | No direct C# equivalent |
| `INT` | `int` | `Int32` | |
| `BIGINT` | `long` | `Int64` | |
| `FLOAT` | `float` | `Single` | |
| `DOUBLE` | `double` | `Double` | |
| `DECIMAL(p,s)` | `decimal` | `Decimal` | |
| `CHAR(n)` | `string` | `StringFixedLength` | |
| `VARCHAR(n)` | `string` | `String` | |
| `TEXT` | `string` | `String` | |
| `BLOB` | `byte[]` | `Binary` | |
| `DATE` | `DateOnly` | `Date` | |
| `DATETIME` | `DateTime` | `DateTime` | |
| `TIMESTAMP` | `DateTime` | `DateTime` | MySQL auto-converts to UTC |
| `TIME` | `TimeSpan` | `Time` | |
| `YEAR` | `int` | `Int32` | |
| `JSON` | `string` | `String` | |
| `ENUM('a','b','c')` | `string` | `String` | Or custom enum mapping |
| `SET('a','b')` | `string` | `String` | |

### 8.2 Column Attributes for MySQL

```csharp
// Proposed extended attributes for MySQL-specific column metadata
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class MySqlColumnAttribute : Attribute
{
    public string? Charset { get; set; }        // utf8mb4, latin1, etc.
    public string? Collation { get; set; }      // utf8mb4_unicode_ci
    public bool Unsigned { get; set; }          // UNSIGNED numeric types
    public bool AutoIncrement { get; set; }     // AUTO_INCREMENT
    public string? OnUpdate { get; set; }       // ON UPDATE CURRENT_TIMESTAMP
    public string? Comment { get; set; }        // Column comment
}
```

### 8.3 `MySqlColumn` Reflection Helper

For source generator or runtime use, provide a helper to map MySQL types:

```csharp
public static class MySqlTypeMapping
{
    public static string GetMySqlType(Type type, MySqlColumnAttribute? attr = null)
    {
        if (type == typeof(int)) return attr?.Unsigned == true ? "INT UNSIGNED" : "INT";
        if (type == typeof(long)) return attr?.Unsigned == true ? "BIGINT UNSIGNED" : "BIGINT";
        if (type == typeof(string)) return "VARCHAR(255)";
        if (type == typeof(bool)) return "TINYINT(1)";
        if (type == typeof(DateTime)) return "DATETIME";
        if (type == typeof(DateOnly)) return "DATE";
        if (type == typeof(TimeSpan)) return "TIME";
        if (type == typeof(decimal)) return "DECIMAL(18,6)";
        if (type == typeof(double)) return "DOUBLE";
        if (type == typeof(float)) return "FLOAT";
        if (type == typeof(byte[])) return "BLOB";
        if (type == typeof(Guid)) return "CHAR(36)";  // Or BINARY(16)
        // etc.
        return "VARCHAR(255)";
    }
}
```

---

## 9. Phase M8: MySQL Connection & Execution

### 9.1 `DbClient<MySqlSqlDialectImpl>` Usage

The existing [`DbClient<TDialect>`](Drizzle4Dotnet/src/Core/DbClient.cs) is already generic. MySQL usage:

```csharp
using MySqlConnector;  // or MySql.Data

// Create connection
await using var conn = new MySqlConnection("connection_string");
await conn.OpenAsync();

// Use with MySQL dialect
var db = new DbClient<MySqlSqlDialectImpl>(conn);

// All existing query builders work
var users = await db
    .Select(UserSelect.Record)
    .From(UsersTable)
    .Where(Eq(UsersTable.IsActive, true))
    .ExecuteGetListAsync();
```

### 9.2 `LAST_INSERT_ID()` Handling

Since MySQL doesn't support `RETURNING`, we need a pattern for retrieving auto-generated keys:

```csharp
// After INSERT, get the auto-increment value
public static class MySqlExecutionExtensions
{
    public static async Task<long> ExecuteInsertGetIdAsync<TTable>(
        this DbClient<MySqlSqlDialectImpl> db,
        InsertQuery<TTable, MySqlSqlDialectImpl> query)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        // Execute the INSERT, then SELECT LAST_INSERT_ID()
        // This can be done in a single command with batching:
        // "INSERT INTO ... ; SELECT LAST_INSERT_ID();"
    }
}
```

**Alternative:** Use `DbCommand.ExecuteScalarAsync()` with the insert + select combined.

### 9.3 MySQL Batching

MySQL supports batched statements natively (separate by `;`):
```sql
INSERT INTO `users` (`name`) VALUES ('Alice');
INSERT INTO `users` (`name`) VALUES ('Bob');
SELECT LAST_INSERT_ID();
```

The existing `ExecuteAsync()` and `ExecuteGetListAsync()` methods in [`DbClient`](Drizzle4Dotnet/src/Core/DbClient.cs) can handle this, but care must be taken with `LAST_INSERT_ID()` (it only returns the first generated ID from a batch).

### 9.4 Transaction Support

MySQL's `START TRANSACTION` / `COMMIT` / `ROLLBACK` are standard. The existing [`BeginTransactionAsync()`](Drizzle4Dotnet/src/Core/DbClient.cs#L76-L80) / [`CommitAsync()`](Drizzle4Dotnet/src/Core/DbClient.cs#L82) / [`RollbackAsync()`](Drizzle4Dotnet/src/Core/DbClient.cs#L83) methods work without changes.

### 9.5 `SQL_CALC_FOUND_ROWS` / `FOUND_ROWS()` Pattern

MySQL has a unique pagination pattern using `SQL_CALC_FOUND_ROWS`:

```csharp
var q = _db
    .Select(UserSelect.Record)
    .From(users)
    .Where(Eq(UsersTable.IsActive, true))
    .Limit(20)
    .Offset(0)
    .CalcFoundRows();  // Adds SQL_CALC_FOUND_ROWS to SELECT

// Then:
var totalCount = await _db.Select(MySqlFunctions.FoundRows()).ExecuteScalarAsync<long>();
```

**Proposed API addition to [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs):**
```csharp
public SelectQuery<TReturn, TDialect> CalcFoundRows();
// Emits: SELECT SQL_CALC_FOUND_ROWS ...
```

Or use the more standard approach: `SELECT COUNT(*) FROM (original_query) AS count_query`.

---

## 10. Phase M9: Source Generator MySQL Support

### 10.1 Dialect Parameterization

**File:** [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs)

Currently hardcodes `PgSqlSqlDialectImpl`. For MySQL support:

**Option A (Recommended):** Add a `Dialect` property to `TableAttribute`:

```csharp
[Table("users", Dialect = typeof(MySqlSqlDialectImpl))]
public partial class UsersTable : IMySqlDbTable { ... }
```

**Option B:** Generate all tables for all dialects:
```csharp
[Table("users")]
// Generates both PgUsersTable and MySqlUsersTable
```

**Option C:** Let the user specify dialect globally via generator parameter in `.csproj`:
```xml
<PropertyGroup>
  <DrizzleDialect>MySql</DrizzleDialect>
</PropertyGroup>
```

**Recommended:** **Option A** (most flexible) + **Option C** (default when not specified).

### 10.2 `TableAttribute` Extension

```csharp
[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name, string schema = "") : Attribute
{
    public string Name { get; } = name;
    public string Schema { get; } = schema;
    public Type? Dialect { get; set; }  // NEW: typeof(MySqlSqlDialectImpl) etc.
}
```

### 10.3 Generator Logic Changes

In [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs):

1. Read `Dialect` from `TableAttribute` constructor args or named args
2. Determine the dialect namespace and type name
3. Generate appropriate table class with correct base types

```csharp
// Pseudo-code for generator
if (dialectTypeName == "MySqlSqlDialectImpl")
{
    // Generate IMySqlDbTable implementation
    // Use MySqlColumn instead of PgColumn
    // Use MySqlSqlDialectImpl throughout
}
else
{
    // Default: PgSqlSqlDialectImpl (backward compatible)
}
```

### 10.4 `DbSelectGenerator` Changes

**File:** [`DbSelectGenerator.cs`](SourceGenerators/SourceGenerators/DbSelectGenerator.cs)

Similar dialect parameterization needed — the `MapWithAttribute` references table columns which are now dialect-specific:

```csharp
[DbSelect(Dialect = typeof(MySqlSqlDialectImpl))]
public partial class UserSelect { ... }
```

---

## 11. Phase M10: Testing Strategy

### 11.1 SQL Generation Tests

Create test files mirroring the existing [`Test/Select/Base.cs`](Test/Select/Base.cs) but for MySQL output:

| Test File | What It Tests |
|-----------|---------------|
| [`Test/Select/MySql/SelectTests.cs`](Test/Select/MySql/SelectTests.cs) | SELECT ... FROM, WHERE, JOIN, GROUP BY, HAVING, ORDER BY, LIMIT/OFFSET |
| [`Test/Insert/MySql/InsertTests.cs`](Test/Insert/MySql/InsertTests.cs) | INSERT, INSERT ... SET, INSERT IGNORE, ON DUPLICATE KEY UPDATE, REPLACE |
| [`Test/Update/MySql/UpdateTests.cs`](Test/Update/MySql/UpdateTests.cs) | UPDATE, UPDATE with JOIN, UPDATE with LIMIT, UPDATE with ORDER BY |
| [`Test/Delete/MySql/DeleteTests.cs`](Test/Delete/MySql/DeleteTests.cs) | DELETE, DELETE with JOIN, DELETE with LIMIT |

### 11.2 MySQL-Specific SQL Generation Test Cases

| Test Case | Expected SQL |
|-----------|-------------|
| Basic SELECT | ``SELECT `id`, `name` FROM `users` `` |
| SELECT with WHERE | ``SELECT `id` FROM `users` WHERE `is_active` = @p0`` |
| SELECT with LIMIT/OFFSET | ``SELECT `id` FROM `users` LIMIT @p0 OFFSET @p1`` |
| INSERT with ON DUPLICATE KEY UPDATE | ``INSERT INTO `users` (`id`, `name`) VALUES (@p0, @p1) ON DUPLICATE KEY UPDATE `name` = VALUES(`name`)`` |
| UPDATE with JOIN | ``UPDATE `users` JOIN `departments` ON `users`.`dept_id` = `departments`.`id` SET `users`.`name` = @p0 WHERE `departments`.`name` = @p1`` |
| DELETE with JOIN | ``DELETE `users` FROM `users` JOIN `departments` ON `users`.`dept_id` = `departments`.`id` WHERE `departments`.`name` = @p0`` |
| REPLACE INTO | ``REPLACE INTO `users` (`id`, `name`) VALUES (@p0, @p1)`` |
| INSERT IGNORE | ``INSERT IGNORE INTO `users` (`id`) VALUES (@p0)`` |
| CTE (MySQL 8.0+) | ``WITH `active_users` AS (SELECT `id` FROM `users` WHERE `is_active` = @p0) SELECT `id` FROM `active_users``` |
| Window function | ``SELECT `name`, ROW_NUMBER() OVER (PARTITION BY `dept_id` ORDER BY `salary` DESC) AS `rn` FROM `employees``` |
| JSON_EXTRACT | ``SELECT JSON_EXTRACT(`data`, @p0) FROM `users` `` |
| REGEXP | ``SELECT `id` FROM `users` WHERE `name` REGEXP @p0`` |
| NULL-safe equal | ``SELECT `id` FROM `users` WHERE `name` <=> @p0`` |

### 11.3 Integration Tests

Set up a MySQL/MariaDB test database (via Docker):

```yaml
# docker-compose.yaml addition
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: root
      MYSQL_DATABASE: drizzle_test
    ports:
      - "3306:3306"
```

Integration test plan:
1. Create tables using generated SQL
2. Insert test data
3. Execute queries against real MySQL
4. Verify results match expected

---

## 12. Phase M11: NuGet Packaging

### 12.1 Package Structure

The existing project produces a single NuGet package. For MySQL support, we have options:

**Option A (Recommended):** Single package with MySQL support included — `Drizzle4Dotnet` package includes both PgSql and MySql namespaces.

**Option B:** Separate packages:
- `Drizzle4Dotnet` — Core library
- `Drizzle4Dotnet.PgSql` — PostgreSQL support
- `Drizzle4Dotnet.MySql` — MySQL support

**Recommendation:** **Option B** — cleaner separation, lighter dependencies.

```
Drizzle4Dotnet/                -> Drizzle4Dotnet NuGet (Core)
Drizzle4Dotnet.MySql/          -> Drizzle4Dotnet.MySql NuGet (NEW)
Drizzle4Dotnet.PgSql/          -> Drizzle4Dotnet.PgSql NuGet (Future)
```

### 12.2 Dependency Requirements

| Package | Core | MySql |
|---------|------|-------|
| `MySqlConnector` (optional) | - | ✅ Recommended |

The MySQL package would have a soft dependency on `MySqlConnector` — users can use any ADO.NET MySQL provider.

---

## 13. Appendix: MySQL SQL Syntax Reference

### 13.1 Identifiers & Quoting

```sql
-- Backtick quoting
SELECT `u`.`id` FROM `users` AS `u`;

-- Database-qualified table name
SELECT * FROM `mydb`.`users`;

-- Reserved words as identifiers (must be backtick-quoted)
SELECT * FROM `order`;  -- ORDER is reserved
```

### 13.2 SELECT Syntax

```sql
SELECT 
    [ALL | DISTINCT | DISTINCTROW]
    [SQL_CALC_FOUND_ROWS]
    select_expr [, select_expr] ...
    FROM table_references
    [WHERE where_condition]
    [GROUP BY {col_name | expr | position}]
    [HAVING where_condition]
    [ORDER BY {col_name | expr | position} [ASC | DESC], ...]
    [LIMIT {[offset,] row_count | row_count OFFSET offset}]
    [FOR UPDATE [OF table_references] [NOWAIT | SKIP LOCKED]]
    [FOR SHARE [OF table_references] [NOWAIT | SKIP LOCKED]]
```

### 13.3 INSERT Syntax

```sql
INSERT [LOW_PRIORITY | DELAYED | HIGH_PRIORITY] [IGNORE]
    [INTO] tbl_name
    [PARTITION (partition_list)]
    [(col_list)]
    {VALUES | VALUE} (value_list) [, (value_list)] ...
    [ON DUPLICATE KEY UPDATE assignment_list]

INSERT [LOW_PRIORITY | DELAYED | HIGH_PRIORITY] [IGNORE]
    [INTO] tbl_name
    SET assignment_list
    [ON DUPLICATE KEY UPDATE assignment_list]

INSERT [LOW_PRIORITY | HIGH_PRIORITY] [IGNORE]
    [INTO] tbl_name
    [(col_list)]
    SELECT ...
    [ON DUPLICATE KEY UPDATE assignment_list]
```

### 13.4 REPLACE Syntax

```sql
REPLACE [LOW_PRIORITY | DELAYED]
    [INTO] tbl_name
    [(col_list)]
    {VALUES | VALUE} (value_list) [, (value_list)] ...
```

### 13.5 UPDATE Syntax

```sql
UPDATE [LOW_PRIORITY] [IGNORE]
    table_references
    SET assignment_list
    [WHERE where_condition]
    [ORDER BY ...]
    [LIMIT row_count]
```

### 13.6 DELETE Syntax

```sql
DELETE [LOW_PRIORITY] [QUICK] [IGNORE]
    FROM tbl_name
    [PARTITION (partition_list)]
    [WHERE where_condition]
    [ORDER BY ...]
    [LIMIT row_count]

-- Multi-table DELETE
DELETE tbl_name[, tbl_name] ...
    FROM table_references
    [WHERE where_condition]

-- Or using JOIN syntax
DELETE tbl_name
    FROM tbl_name
    JOIN other_table ON condition
    [WHERE where_condition]
```

### 13.7 CTE Syntax (MySQL 8.0+)

```sql
WITH [RECURSIVE]
    cte_name AS (subquery)
    [, cte_name AS (subquery)] ...
SELECT ... FROM cte_name ...
```

### 13.8 Window Function Syntax (MySQL 8.0+)

```sql
function_name([args]) OVER (
    [PARTITION BY expr [, expr] ...]
    [ORDER BY expr [ASC | DESC] [, ...]]
    [frame_clause]
)
```

---

## 14. Appendix: Implementation Order & Dependencies

### 14.1 Suggested Sprint Plan

| Sprint | Phase | Features | Dependencies | Estimated Effort |
|--------|-------|----------|--------------|-----------------|
| **S1** | M1, M2 | `MySqlSqlDialectImpl`, `ISqlDialect` extension, `MySqlTable`, `MySqlColumn` | None | Small (2-3 days) |
| **S2** | M3 | `MySqlFunctions` (string, date, math, JSON, info) | S1 | Medium (3-4 days) |
| **S3** | M4 | `MySqlOperators` (REGEXP, null-safe equal), string concat strategy | S1 | Small (1-2 days) |
| **S4** | M5 | INSERT ... ON DUPLICATE KEY UPDATE, REPLACE, INSERT IGNORE, INSERT ... SET | S1 | Medium (3-4 days) |
| **S5** | M6 | UPDATE/DELETE with JOIN, LIMIT on UPDATE/DELETE | S1, S4 | Medium (3-4 days) |
| **S6** | M7 | MySQL column types, `MySqlColumnAttribute`, type mapping | S1, S2 | Small (2 days) |
| **S7** | M8 | `LAST_INSERT_ID()`, execution extensions, batching | S1, S5 | Small (2 days) |
| **S8** | M9 | Source generator dialect parameterization | S1, S7 | Medium (3-4 days) |
| **S9** | M10 | Test suite — SQL generation + integration | All | Large (5-7 days) |
| **S10** | M11 | NuGet packaging, documentation, samples | All | Small (2 days) |

### 14.2 Priority-Effort Matrix

| Phase | Features | Priority | Effort | Dependencies |
|-------|----------|----------|--------|--------------|
| **M1** | Dialect foundation | **Critical** | Small | None |
| **M2** | Types & interfaces | **Critical** | Small | M1 |
| **M3** | MySQL functions | High | Medium | M1 |
| **M4** | MySQL operators | High | Small | M1 |
| **M5** | MySQL INSERT upsert | **Critical** | Medium | M1 |
| **M6** | MySQL DML with JOIN | High | Medium | M1, M5 |
| **M7** | Column types | Medium | Small | M1, M3 |
| **M8** | Execution extensions | High | Small | M1, M5 |
| **M9** | Source generator changes | High | Medium | M1, M7 |
| **M10** | Testing | **Critical** | Large | All |
| **M11** | Packaging | Medium | Small | All |

### 14.3 Files to Create

| # | File | Purpose | Sprint |
|---|------|---------|--------|
| 1 | `Drizzle4Dotnet/src/Dialect/MySqlSqlDialectImpl.cs` | MySQL dialect implementation | S1 |
| 2 | `Drizzle4Dotnet/src/MySql/MySqlTable.cs` | MySQL table interfaces | S1 |
| 3 | `Drizzle4Dotnet/src/MySql/MySqlColumn.cs` | MySQL column types | S1 |
| 4 | `Drizzle4Dotnet/src/MySql/MySqlFunctions.cs` | MySQL-specific functions | S2 |
| 5 | `Drizzle4Dotnet/src/MySql/Nodes/IntervalMySqlNode.cs` | MySQL INTERVAL syntax | S2 |
| 6 | `Drizzle4Dotnet/src/MySql/MySqlOperators.cs` | MySQL-specific operators | S3 |
| 7 | `Drizzle4Dotnet/src/MySql/MySqlExecutionExtensions.cs` | LAST_INSERT_ID() etc. | S8 |
| 8 | `Test/Select/MySql/SelectTests.cs` | MySQL SELECT tests | S9 |
| 9 | `Test/Insert/MySql/InsertTests.cs` | MySQL INSERT tests | S9 |
| 10 | `Test/Update/MySql/UpdateTests.cs` | MySQL UPDATE tests | S9 |
| 11 | `Test/Delete/MySql/DeleteTests.cs` | MySQL DELETE tests | S9 |

### 14.4 Files to Modify

| # | File | Change | Sprint |
|---|------|--------|--------|
| 1 | `Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs` | Add new static abstract members for MySQL features | S1 |
| 2 | `Drizzle4Dotnet/src/Core/Shared/Sql.cs` | Add MySQL-compatible helper methods if needed | S2 |
| 3 | `Drizzle4Dotnet/src/Core/DbClient.cs` | Add `Replace()` method | S5 |
| 4 | `Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs` | Add `OnDuplicateKeyUpdate()`, `Ignore()`, `Set()` for MySQL syntax | S4 |
| 5 | `Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs` | Add `Join()`, `Limit()`, `OrderBy()` for MySQL syntax | S5 |
| 6 | `Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs` | Add `Join()`, `Limit()`, `OrderBy()` for MySQL syntax | S5 |
| 7 | `SourceGenerators/SourceGenerators/TableGenerator.cs` | Dialect parameterization | S8 |
| 8 | `SourceGenerators/SourceGenerators/DbSelectGenerator.cs` | Dialect parameterization | S8 |
| 9 | `SourceGenerators/SourceGenerators/Utils.cs` | MySQL type mappings | S8 |
| 10 | `Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs` | Add `Dialect` property to `TableAttribute` | S8 |
| 11 | `Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs` | Implement new `ISqlDialect` members (for compatibility) | S1 |

---

## Summary

This plan outlines the complete MySQL/MariaDB implementation for Drizzle4Dotnet. The key architectural insight is that the existing generic `DbClient<TDialect>` design already supports multiple dialects — the main work is:

1. **Dialect implementation** (identifier quoting, feature flags, SQL syntax variations)
2. **Type system** (MySQL-specific table/column types)
3. **Functions & operators** (MySQL-specific SQL functions)
4. **DML variations** (ON DUPLICATE KEY UPDATE, JOIN-based UPDATE/DELETE)
5. **Source generator parameterization** (dialect-aware code generation)

The total estimated effort is **~25-35 days** for a single developer, with the critical path being the dialect foundation (S1) and source generator changes (S8).
