# Drizzle4Dotnet — Microsoft SQL Server (MSSQL) Implementation Plan

> **Status:** Draft  
> **Last Updated:** 2026-06-28  
> **Target:** Full MSSQL (T-SQL) dialect support, paralleling existing PostgreSQL, MySQL, and SQLite implementations.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Phase M1: MSSQL Dialect (Foundation)](#2-phase-ms1-mssql-dialect-foundation)
3. [Phase M2: MSSQL Types & Interfaces](#3-phase-ms2-mssql-types--interfaces)
4. [Phase M3: MSSQL Functions](#4-phase-ms3-mssql-functions)
5. [Phase M4: MSSQL Operators & Nodes](#5-phase-ms4-mssql-operators--nodes)
6. [Phase M5: MSSQL DML — SELECT with TOP/OFFSET-FETCH](#6-phase-ms5-mssql-dml--select)
7. [Phase M6: MSSQL DML — INSERT with OUTPUT](#7-phase-ms6-mssql-dml--insert)
8. [Phase M7: MSSQL DML — UPDATE with FROM/JOIN](#8-phase-ms7-mssql-dml--update)
9. [Phase M8: MSSQL DML — DELETE with FROM/JOIN](#9-phase-ms8-mssql-dml--delete)
10. [Phase M9: MSSQL MERGE (UPSERT)](#10-phase-ms9-mssql-merge-upsert)
11. [Phase M10: MSSQL Schema / Column Types](#11-phase-ms10-mssql-schema--column-types)
12. [Phase M11: MSSQL Connection & Execution](#12-phase-ms11-mssql-connection--execution)
13. [Phase M12: Source Generator MSSQL Support](#13-phase-ms12-source-generator-mssql-support)
14. [Phase M13: Testing Strategy](#14-phase-ms13-testing-strategy)
15. [Phase M14: NuGet Packaging & Demo](#15-phase-ms14-nuget-packaging--demo)
16. [Appendix: MSSQL SQL Syntax Reference](#16-appendix-mssql-sql-syntax-reference)
17. [Appendix: Implementation Order & Dependencies](#17-appendix-implementation-order--dependencies)

---

## 1. Architecture Overview

### 1.1 MSSQL vs PostgreSQL/MySQL — Key Differences

| Aspect | PostgreSQL | MySQL | MSSQL (T-SQL) |
|--------|------------|-------|----------------|
| **Identifier quoting** | `"double quotes"` | `` `backticks` `` | `[square brackets]` (standard: `"quotes"` with `QUOTED_IDENTIFIER ON`) |
| **Parameter prefix** | `@p0`, `@p1` | `@p0`, `@p1` | `@p0`, `@p1` (same convention ✅) |
| **Schema support** | `schema.table` | `db.table` | `[schema].[table]` (default: `[dbo].[table]`) |
| **LIMIT / TOP / OFFSET** | `LIMIT ? OFFSET ?` | `LIMIT ?, ?` | `SELECT TOP ?` / `OFFSET ? ROWS FETCH NEXT ? ROWS ONLY` |
| **RETURNING clause** | ✅ `RETURNING` | ❌ Not supported | ✅ **`OUTPUT` clause** (different syntax, same capability) |
| **UPSERT** | `ON CONFLICT [...] DO UPDATE/NOTHING` | `ON DUPLICATE KEY UPDATE` / `REPLACE` | **`MERGE`** statement |
| **INSERT DEFAULT VALUES** | ✅ Supported | `() VALUES ()` | ✅ Supported |
| **UPDATE with JOIN** | `UPDATE t SET ... FROM other WHERE ...` | `UPDATE t JOIN other ON ... SET ...` | `UPDATE t SET ... FROM t JOIN other ON ... WHERE ...` |
| **DELETE with JOIN** | `DELETE FROM t USING other WHERE ...` | `DELETE t FROM t JOIN other ON ... WHERE ...` | `DELETE t FROM t JOIN other ON ... WHERE ...` |
| **CTEs (WITH)** | ✅ Supported | ✅ MySQL 8.0+ | ✅ Supported |
| **Recursive CTEs** | ✅ Supported | ✅ MySQL 8.0+ | ✅ Supported |
| **Window functions** | ✅ Supported | ⚠️ MySQL 8.0+ | ✅ Supported |
| **JSON functions** | Native JSON + `->`/`->>` | `JSON_EXTRACT()` | ⚠️ `JSON_VALUE()`, `JSON_QUERY()`, `OPENJSON()` (2016+) |
| **Array types** | ✅ Native array type | ❌ Not supported | ❌ Not supported |
| **`IS DISTINCT FROM`** | ✅ Supported | ❌ Not supported | ❌ Not supported (use `EXCEPT`/`INTERSECT`) |
| **`FILTER (WHERE ...)`** | ✅ Supported | ❌ Not supported | ❌ Not supported |
| **FULL OUTER JOIN** | ✅ Supported | ❌ Not supported | ✅ Supported |
| **NATURAL JOIN** | ✅ Supported | ✅ Supported | ❌ **Not supported** |
| **LATERAL JOIN** | ✅ `LATERAL` | ✅ `LATERAL` (8.0.14+) | ✅ **`CROSS APPLY` / `OUTER APPLY`** |
| **String concat** | `\|\|` operator | `CONCAT()` function | **`+` operator** (or `CONCAT()`) |
| **Date functions** | `NOW()`, `DATE_TRUNC()` | `NOW()`, `DATE_FORMAT()` | **`GETDATE()`, `DATEADD()`, `DATEDIFF()`** |
| **Last inserted ID** | `RETURNING` | `LAST_INSERT_ID()` | **`SCOPE_IDENTITY()`** / `OUTPUT` |
| **LIMIT on UPDATE/DELETE** | ❌ Not supported | ✅ Supported | ⚠️ Via `TOP()` only |
| **Table hints** | — | — | ✅ `WITH (NOLOCK)`, `WITH (TABLOCK)`, etc. |
| **`TOP` WITH TIES** | `FETCH FIRST ? ROWS WITH TIES` | ❌ | ✅ `SELECT TOP ? WITH TIES` |
| **`OFFSET` without `ORDER BY`** | ❌ Not allowed | ❌ Not allowed | ❌ **Not allowed in MSSQL** |
| **Identity columns** | `GENERATED AS IDENTITY` / `SERIAL` | `AUTO_INCREMENT` | **`IDENTITY(1,1)`** |
| **Sequences** | ✅ `CREATE SEQUENCE` | ❌ Not supported | ✅ `CREATE SEQUENCE` |
| **`EXCEPT` / `INTERSECT`** | ✅ Supported | ❌ Not supported | ✅ Supported |
| **`OUTPUT INSERTED`/`DELETED`** | `RETURNING` | ❌ | ✅ `OUTPUT INSERTED.column` |

### 1.2 Target File Structure

```
Drizzle4Dotnet/
├── src/
│   ├── Core/                               # Existing — minimal changes needed
│   │   ├── Shared/
│   │   │   ├── ISqlDialect.cs              # Minor additions for OUTPUT, TOP, APPLY support
│   │   │   └── SqlDialectDefaults.cs        # May need new default methods
│   │   └── ...
│   ├── Dialect/
│   │   ├── PgSqlSqlDialectImpl.cs          # Existing
│   │   ├── MySqlSqlDialectImpl.cs          # Existing
│   │   ├── SqliteSqlDialectImpl.cs         # Existing
│   │   └── MssqlSqlDialectImpl.cs          # NEW — MSSQL dialect
│   └── Mssql/                               # NEW — MSSQL-specific namespace
│       ├── MssqlDbClient.cs                 # DbClient + factory methods
│       ├── MssqlQueryBuilder.cs             # Offline query builder
│       ├── MssqlQueryBuilderExtensions.cs   # Auto-generated convenience overloads
│       ├── MssqlTable.cs                    # Table/alias interfaces
│       ├── MssqlColumn.cs                   # Column types
│       ├── MssqlStatics.cs                  # Static helpers (SCOPE_IDENTITY, etc.)
│       ├── MssqlFunctions.cs               # MSSQL-specific functions
│       ├── MssqlFunctions.DateTime.cs      # GETDATE(), DATEADD(), DATEDIFF(), etc.
│       ├── MssqlFunctions.String.cs        # CHARINDEX(), LEN(), SUBSTRING(), etc.
│       ├── MssqlFunctions.Numeric.cs       # RAND(), ABS(), ROUND(), etc.
│       ├── MssqlFunctions.Json.cs          # JSON_VALUE(), JSON_QUERY(), OPENJSON()
│       ├── MssqlFunctions.Info.cs          # SCOPE_IDENTITY(), @@ROWCOUNT, etc.
│       ├── MssqlOperators.cs               # MSSQL-specific operators (+ concat, etc.)
│       ├── Nodes/
│       │   ├── MssqlTopNode.cs             # TOP(n) expression wrapper
│       │   ├── MssqlOutputNode.cs          # OUTPUT INSERTED/DELETED clause
│       │   ├── MssqlTableHintNode.cs       # WITH (NOLOCK), WITH (TABLOCK), etc.
│       │   └── MssqlSequenceNode.cs        # NEXT VALUE FOR sequence
│       └── Query/
│           ├── MssqlSelectQuery.cs          # SELECT with TOP, OFFSET-FETCH, APPLY
│           ├── MssqlInsertQuery.cs          # INSERT with OUTPUT, identity insert
│           ├── MssqlUpdateQuery.cs          # UPDATE with FROM/JOIN, OUTPUT
│           ├── MssqlDeleteQuery.cs          # DELETE with FROM/JOIN, OUTPUT
│           └── MssqlMergeQuery.cs           # MERGE (upsert)

Test/
├── Select/
│   └── MssqlSelectTests.cs                 # NEW — MSSQL SELECT tests
├── Insert/
│   └── MssqlInsertTests.cs                 # NEW — MSSQL INSERT tests
├── Update/
│   └── MssqlUpdateTests.cs                 # NEW — MSSQL UPDATE tests
├── Delete/
│   └── MssqlDeleteTests.cs                 # NEW — MSSQL DELETE tests
├── Merge/
│   └── MssqlMergeTests.cs                  # NEW — MSSQL MERGE tests
└── Migration/
    └── MssqlMigrationTests.cs              # NEW — MSSQL DDL/migration tests

SharedDemo/
└── Mssql/
    ├── Schema.cs                            # MSSQL-specific schema definitions
    ├── DbDto.cs                             # MSSQL DTOs
    ├── schema.sql                           # DDL for demo setup
    └── data.sql                             # Sample data
```

### 1.3 MSSQL ADO.NET Provider

The ORM uses `System.Data.Common.DbConnection` / `DbCommand`, so any ADO.NET-compatible MSSQL provider works:

| Provider | NuGet Package | Notes |
|----------|--------------|-------|
| **Microsoft.Data.SqlClient** | `Microsoft.Data.SqlClient` | ✅ **Recommended.** Modern, cross-platform, actively maintained. Uses `@named` parameters. |
| System.Data.SqlClient | `System.Data.SqlClient` | Legacy, Windows-only. Use Microsoft.Data.SqlClient instead. |

**Recommendation:** Use **Microsoft.Data.SqlClient** — it's the modern, cross-platform provider from Microsoft. Uses `@name` parameter prefix (same convention as Npgsql), so the existing `SqlBuilder` parameter naming strategy works unchanged.

NuGet dependency to add to [`Drizzle4Dotnet.csproj`](Drizzle4Dotnet/Drizzle4Dotnet.csproj):
```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.0.0" />
```

---

## 2. Phase MS1: MSSQL Dialect (Foundation)

### 2.1 Create `MssqlSqlDialectImpl.cs`

**File:** [`Drizzle4Dotnet/src/Dialect/MssqlSqlDialectImpl.cs`](Drizzle4Dotnet/src/Dialect/MssqlSqlDialectImpl.cs) — **New file**

Implements [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) with T-SQL syntax patterns.

```csharp
namespace Drizzle4Dotnet.Dialect;

public class MssqlSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================
    
    // MSSQL uses square brackets: [identifier]
    public static string BuildIdentifier(string identifier)
        => $"[{identifier}]";

    // [schema].[table] or just [table] (default schema is dbo)
    public static string BuildTableName(string schemaName, string tableName)
        => string.IsNullOrEmpty(schemaName) 
            ? $"[{tableName}]" 
            : $"[{schemaName}].[{tableName}]";

    // [table].[column]
    public static string BuildColumnName(string refName, string columnName)
        => $"[{refName}].[{columnName}]";

    // Parameters: @paramName (same as Npgsql)
    public static string BuildParameterName(string parameterName)
        => $"@{parameterName}";
        
    public static string BuildParameterName(int parameterIndex)
        => $"@p{parameterIndex}";
    
    // ======================================================================
    // Limit / Offset — MSSQL uses OFFSET/FETCH or TOP
    // ======================================================================
    
    // Standard MSSQL: ORDER BY ... OFFSET ? ROWS FETCH NEXT ? ROWS ONLY
    // Note: MSSQL REQUIRES ORDER BY when using OFFSET/FETCH
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
    {
        // OFFSET/FETCH is appended after ORDER BY in the query builder
        // This method is called at the end of BuildSql() in SelectQuery
        // We need to handle the case where there's no ORDER BY but OFFSET/FETCH is used
        // MSSQL requires ORDER BY with OFFSET/FETCH, so we may need to add ORDER BY (SELECT NULL)
        if (limit.HasValue && offset.HasValue)
        {
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
            sqlBuilder.Append(" ROWS FETCH NEXT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
            sqlBuilder.Append(" ROWS ONLY");
        }
        else if (limit.HasValue)
        {
            sqlBuilder.Append(" OFFSET 0 ROWS FETCH NEXT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
            sqlBuilder.Append(" ROWS ONLY");
        }
        else if (offset.HasValue)
        {
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
            sqlBuilder.Append(" ROWS");
        }
    }
    
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => throw new NotSupportedException(
            "MSSQL does not support LIMIT/OFFSET for UPDATE/DELETE statements. " +
            "Use TOP() in the SELECT clause instead.");
    
    // ======================================================================
    // Feature Flags
    // ======================================================================
    
    public static bool SupportsReturning => true;       // Via OUTPUT clause
    public static bool SupportsArrays => false;          // Not supported
    public static bool SupportsJson => true;             // SQL Server 2016+ (JSON_VALUE, etc.)
    public static bool SupportsWindowFunctions => true;  // SQL Server 2005+
    public static bool SupportsCte => true;              // SQL Server 2005+
    public static bool SupportsRecursiveCte => true;     // SQL Server 2005+
    public static bool SupportsDeleteUsing => false;     // Uses JOIN syntax instead
    public static bool SupportsIsDistinctFrom => false;  // Not supported
    public static bool SupportsFilteredAggregates => false; // Not supported
    public static bool SupportsFullOuterJoin => true;    // Supported
    public static bool SupportsNaturalJoin => false;     // NOT supported
    public static bool SupportsLateralJoin => false;     // Uses APPLY instead
    public static bool SupportsApplyJoin => true;        // CROSS/OUTER APPLY supported
    public static bool UseLimitPairMode => false;
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    // MSSQL uses '' escaping like standard SQL
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
}
```

### 2.2 Dialect Interface Extensions Needed for MSSQL

The current [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) needs additional members to support MSSQL-specific features. These should be added with default implementations where possible, or the existing patterns should be used:

| New Member | Type | Purpose | MSSQL Behavior |
|------------|------|---------|----------------|
| `SupportsApplyJoin` | `bool` | Whether CROSS/OUTER APPLY is supported | `true` |
| `BuildTopClause` | `void(ISqlBuilder, int?)` | Build TOP(n) clause | `TOP (@p0)` |
| `BuildOutputClause` | `void(ISqlBuilder, ...)` | Build OUTPUT clause | `OUTPUT INSERTED.col1, INSERTED.col2` |

**Strategy:** Rather than adding many new members to [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) (which would require updating all existing dialect implementations), the MSSQL query builders should handle dialect-specific features inline, following the pattern used by MySQL for JOIN in UPDATE/DELETE and by PostgreSQL for ON CONFLICT.

### 2.3 Register the Dialect

- No changes needed to [`SqlDialectDefaults`](Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs) for initial MSSQL support.
- Add `SupportsApplyJoin` to [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) with existing dialects returning appropriate values.

---

## 3. Phase MS2: MSSQL Types & Interfaces

### 3.1 Create [`MssqlTable.cs`](Drizzle4Dotnet/src/Mssql/MssqlTable.cs)

```csharp
namespace Drizzle4Dotnet.Mssql;

public interface IMssqlGenericTable : IGenericTable<MssqlSqlDialectImpl> { }
public interface IMssqlTable : ITable<MssqlSqlDialectImpl> { }
public interface IMssqlCteTable : ICteTable<MssqlSqlDialectImpl> { }
public interface IMssqlVirtualTable : IVirtualTable<MssqlSqlDialectImpl> { }
public interface IMssqlDbTable : IDbTable<MssqlSqlDialectImpl> { }
public interface IMssqlTableAlias : ITableAlias<MssqlSqlDialectImpl> { }
```

### 3.2 Create [`MssqlColumn.cs`](Drizzle4Dotnet/src/Mssql/MssqlColumn.cs)

```csharp
namespace Drizzle4Dotnet.Mssql;

public class MssqlColumn<T, TTable> : DbColumn<T, TTable, MssqlSqlDialectImpl>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    public MssqlColumn(string columnName) : base(columnName) { }
}

public class MssqlVirtualColumn<T> : VirtualColumn<T, MssqlSqlDialectImpl>
{
    public MssqlVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
```

### 3.3 Generated Table Pattern (MSSQL)

When a table class is generated via [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs), the MSSQL variant would look like:

```csharp
// Generated by source generator with [Table("users", Dialect = typeof(MssqlSqlDialectImpl))]
public partial class UsersTable : IMssqlDbTable
{
    public const string TableRefName = "Users";
    public const string TableName = "users";
    public const string SchemaName = "dbo";  // MSSQL default schema

    public static class ColumnNames
    {
        public const string Id = "id";
        public const string Name = "name";
    }

    public static readonly MssqlColumn<int, UsersTable> Id = new("id");
    public static readonly MssqlColumn<string, UsersTable> Name = new("name");

    public void BuildSql(ISqlBuilder sqlBuilder) => 
        sqlBuilder.Append(MssqlSqlDialectImpl.BuildTableName(SchemaName, TableName));
    public void BuildRefSql(ISqlBuilder sqlBuilder) => BuildSql(sqlBuilder);
}
```

---

## 4. Phase MS3: MSSQL Functions

### 4.1 Create [`MssqlFunctions.cs`](Drizzle4Dotnet/src/Mssql/MssqlFunctions.cs)

Entry point partial class:

```csharp
namespace Drizzle4Dotnet.Mssql;

public static partial class MssqlFunctions
{
}
```

### 4.2 Create [`MssqlFunctions.DateTime.cs`](Drizzle4Dotnet/src/Mssql/MssqlFunctions.DateTime.cs)

MSSQL uses different date/time function names and syntax from PostgreSQL/MySQL:

| Function | MSSQL | PostgreSQL Equivalent |
|----------|-------|----------------------|
| Current date/time | `GETDATE()` / `SYSDATETIME()` | `NOW()` |
| UTC time | `GETUTCDATE()` | `NOW() AT TIME ZONE 'UTC'` |
| Current date only | `CAST(GETDATE() AS DATE)` | `CURRENT_DATE` |
| Date add | `DATEADD(unit, amount, date)` | `date + INTERVAL '1 day'` |
| Date diff | `DATEDIFF(unit, start, end)` | `EXTRACT(EPOCH FROM ...)` |
| Date part | `DATEPART(unit, date)` | `EXTRACT(unit FROM date)` |
| Date name | `DATENAME(unit, date)` | `to_char(date, 'Day')` |
| Year/Month/Day | `YEAR(date)`, `MONTH(date)`, `DAY(date)` | `EXTRACT(YEAR FROM date)` |
| Format | `FORMAT(date, format)` (slow) | `to_char(date, format)` |
| EOMONTH | `EOMONTH(date)` | `date_trunc('month', date) + INTERVAL '1 month - 1 day'` |
| ISDATE | `ISDATE(value)` | — |

**Implementation:**

```csharp
public static partial class MssqlFunctions
{
    public static class DateTime
    {
        // GETDATE() — current date and time
        public static FunctionCallNode<DateTime> GetDate()
            => new FunctionCallNode<DateTime>("GETDATE");
        
        public static FunctionCallNode<DateTime> SysDateTime()
            => new FunctionCallNode<DateTime>("SYSDATETIME");
        
        public static FunctionCallNode<DateTime> GetUtcDate()
            => new FunctionCallNode<DateTime>("GETUTCDATE");
        
        // DATEADD(unit, amount, date)
        public static FunctionCallNode<DateTime> DateAdd(IGenericSql unit, IGenericSql amount, ISql<DateTime> date)
            => new FunctionCallNode<DateTime>("DATEADD", unit, amount, date);
        
        // DATEDIFF(unit, start, end)
        public static FunctionCallNode<long> DateDiff(IGenericSql unit, ISql<DateTime> start, ISql<DateTime> end)
            => new FunctionCallNode<long>("DATEDIFF", unit, start, end);
        
        // DATEPART(unit, date)
        public static FunctionCallNode<int> DatePart(IGenericSql unit, ISql<DateTime> date)
            => new FunctionCallNode<int>("DATEPART", unit, date);
        
        // YEAR(date), MONTH(date), DAY(date)
        public static UnaryNode<DateTime, int> Year(ISql<DateTime> date)
            => new(date, "YEAR", true);
        public static UnaryNode<DateTime, int> Month(ISql<DateTime> date)
            => new(date, "MONTH", true);
        public static UnaryNode<DateTime, int> Day(ISql<DateTime> date)
            => new(date, "DAY", true);
        
        // EOMONTH(date)
        public static UnaryNode<DateTime, DateTime> EoMonth(ISql<DateTime> date)
            => new(date, "EOMONTH", true);
        
        // FORMAT(date, format) — Note: slow, use only when necessary
        public static FunctionCallNode<string> Format(ISql<DateTime> date, string format)
            => new FunctionCallNode<string>("FORMAT", date, new SqlValueNode<string>(format));
    }
}
```

### 4.3 Create [`MssqlFunctions.String.cs`](Drizzle4Dotnet/src/Mssql/MssqlFunctions.String.cs)

MSSQL string functions differ significantly:

| Function | MSSQL | PostgreSQL |
|----------|-------|------------|
| String length | `LEN(str)` | `LENGTH(str)` or `CHAR_LENGTH(str)` |
| Find position | `CHARINDEX(substr, str)` | `POSITION(substr IN str)` |
| Substring | `SUBSTRING(str, start, length)` | `SUBSTRING(str FROM start FOR length)` |
| Left/Right | `LEFT(str, n)`, `RIGHT(str, n)` | `LEFT(str, n)`, `RIGHT(str, n)` |
| Concatenate | `CONCAT(str1, str2, ...)` | `str1 \|\| str2` |
| Concatenate with separator | `CONCAT_WS(sep, str1, str2, ...)` | `CONCAT_WS(sep, str1, str2)` |
| Replace | `REPLACE(str, from, to)` | `REPLACE(str, from, to)` |
| Trim | `TRIM(str)`, `LTRIM(str)`, `RTRIM(str)` | Same |
| Upper/Lower | `UPPER(str)`, `LOWER(str)` | Same |
| Reverse | `REVERSE(str)` | `REVERSE(str)` |
| Replicate | `REPLICATE(str, n)` | `REPEAT(str, n)` |
| Space | `SPACE(n)` | `REPEAT(' ', n)` |
| String split | `STRING_SPLIT(str, delimiter)` | `regexp_split_to_table(str, pattern)` |
| Soundex | `SOUNDEX(str)` | `SOUNDEX(str)` (extension) |
| Difference | `DIFFERENCE(str1, str2)` (SOUNDEX diff) | — |
| ASCII / CHAR | `ASCII(str)`, `CHAR(n)` | `ASCII(str)`, `CHR(n)` |
| NCHAR / UNICODE | `NCHAR(n)`, `UNICODE(str)` | `CHR(n)`, `ASCII(str)` |
| PATINDEX | `PATINDEX(pattern, str)` | — |
| QUOTENAME | `QUOTENAME(str)` | `QUOTE_IDENT(str)` |

### 4.4 Create [`MssqlFunctions.Numeric.cs`](Drizzle4Dotnet/src/Mssql/MssqlFunctions.Numeric.cs)

| Function | MSSQL | Notes |
|----------|-------|-------|
| `ABS(value)` | `ABS(value)` | Same as standard |
| `CEILING(value)` | `CEILING(value)` | MSSQL uses CEILING, not CEIL |
| `FLOOR(value)` | `FLOOR(value)` | Same |
| `ROUND(value, decimals)` | `ROUND(value, decimals)` | Same |
| `RAND()` | `RAND()` | Returns float 0-1 |
| `RAND(seed)` | `RAND(seed)` | Seeded random |
| `SIGN(value)` | `SIGN(value)` | Same |
| `SQRT(value)` | `SQRT(value)` | Same |
| `POWER(value, exp)` | `POWER(value, exp)` | Same |
| `SQUARE(value)` | `SQUARE(value)` | MSSQL-specific |
| `LOG(value)` | `LOG(value)` | Natural log |
| `LOG10(value)` | `LOG10(value)` | Base-10 log |
| `EXP(value)` | `EXP(value)` | Same |
| `PI()` | `PI()` | π constant |
| `RANDOM()` | `RAND()` | (Use RAND() instead) |
| `TRUNCATE` | — | Use `ROUND(value, 0, 1)` — the third parameter truncates |

### 4.5 Create [`MssqlFunctions.Json.cs`](Drizzle4Dotnet/src/Mssql/MssqlFunctions.Json.cs)

MSSQL 2016+ has JSON support but uses different functions:

| Function | MSSQL | Notes |
|----------|-------|-------|
| `JSON_VALUE(expr, path)` | Extract scalar value | Path uses `'$.key'` or `'lax $.key'` |
| `JSON_QUERY(expr, path)` | Extract object/array | Returns JSON fragment |
| `JSON_MODIFY(expr, path, value)` | Modify JSON | Update JSON property |
| `OPENJSON(jsonString)` | Table-valued function | Parses JSON to rows/columns |
| `ISJSON(expr)` | Check valid JSON | Returns 1/0 |
| `JSON_PATH_EXISTS(expr, path)` | Check path exists | SQL Server 2022+ |

### 4.6 Create [`MssqlFunctions.Info.cs`](Drizzle4Dotnet/src/Mssql/MssqlFunctions.Info.cs)

```csharp
public static partial class MssqlFunctions
{
    public static class Info
    {
        /// <summary>Returns the last inserted identity value. Equivalent to LAST_INSERT_ID().</summary>
        public static ISql<long> ScopeIdentity() => new RawSql<long>("SELECT SCOPE_IDENTITY()");
        
        /// <summary>Returns @@IDENTITY (last identity, any scope).</summary>
        public static ISql<long> Identity() => new RawSql<long>("SELECT @@IDENTITY");
        
        /// <summary>Returns @@ROWCOUNT (rows affected by last statement).</summary>
        public static ISql<int> RowCount() => new RawSql<int>("SELECT @@ROWCOUNT");
        
        /// <summary>Returns @@VERSION (SQL Server version info).</summary>
        public static ISql<string> Version() => new RawSql<string>("SELECT @@VERSION");
        
        /// <summary>Returns @@SERVERNAME.</summary>
        public static ISql<string> ServerName() => new RawSql<string>("SELECT @@SERVERNAME");
        
        /// <summary>Returns @@DBTS (current database timestamp).</summary>
        public static ISql<byte[]> DbTs() => new RawSql<byte[]>("SELECT @@DBTS");
        
        /// <summary>Returns DB_NAME().</summary>
        public static ISql<string> DbName() => new RawSql<string>("SELECT DB_NAME()");
    }
}
```

### 4.7 Create [`MssqlStatics.cs`](Drizzle4Dotnet/src/Mssql/MssqlStatics.cs)

Static helper methods mirroring [`PgSqlStatics`](Drizzle4Dotnet/src/Dialect/PgSql/PgSqlStatics.cs) pattern.

```csharp
public static class MssqlStatics
{
    /// <summary>Creates a TOP(n) expression for SELECT queries.</summary>
    public static MssqlTopNode Top(int count) => new(count);
    
    /// <summary>Creates a TOP(n) WITH TIES expression.</summary>
    public static MssqlTopNode TopWithTies(int count) => new(count, withTies: true);
    
    /// <summary>Creates an OUTPUT INSERTED column reference.</summary>
    public static MssqlOutputNode OutputInserted(IAliasedSql column)
        => new(column.Identifier, "INSERTED");
    
    /// <summary>Creates an OUTPUT DELETED column reference.</summary>
    public static MssqlOutputNode OutputDeleted(IAliasedSql column)
        => new(column.Identifier, "DELETED");
    
    /// <summary>Creates a table hint: WITH (NOLOCK)</summary>
    public static MssqlTableHintNode NoLock()
        => new("NOLOCK");
    
    /// <summary>Creates a table hint with arbitrary hint text.</summary>
    public static MssqlTableHintNode TableHint(string hint)
        => new(hint);
}
```

---

## 5. Phase MS4: MSSQL Operators & Nodes

### 5.1 Create [`MssqlOperators.cs`](Drizzle4Dotnet/src/Mssql/MssqlOperators.cs)

MSSQL shares many common operators with standard SQL. Key differences:

| Operator | MSSQL | Notes |
|----------|-------|-------|
| String concat | `+` (plus) | Different from PostgreSQL's `\|\|` |
| Assignment | `SET @var = value` | Different syntax |
| String comparison | `=` (case-insensitive by default) | Depends on collation |
| `LIKE` | `LIKE` | Same as standard |
| `NOT LIKE` | `NOT LIKE` | Same |
| `IN` | `IN` | Same |
| `BETWEEN` | `BETWEEN` | Same |
| `EXISTS` | `EXISTS` | Same |
| `IS NULL` | `IS NULL` | Same |
| `IS NOT NULL` | `IS NOT NULL` | Same |

Most standard operators (comparison, arithmetic, logical, string) are already in [`Core/Shared/Operators/`](Drizzle4Dotnet/src/Core/Shared/Operators/) and work with MSSQL unchanged. The string concatenation operator `+` is available as `SqlOperators.Plus` or can be used via `MssqlFunctions.Concat()`.

No MSSQL-specific operator nodes are needed for basic operations — the existing core operators suffice. However, the `+` operator for concatenation in MSSQL differs from `||` in PostgreSQL. The core string concatenation operators should be reviewed for dialect-awareness.

### 5.2 Create [`Nodes/MssqlTopNode.cs`](Drizzle4Dotnet/src/Mssql/Nodes/MssqlTopNode.cs)

```csharp
public class MssqlTopNode : IOperator<int>
{
    private readonly int _count;
    private readonly bool _withTies;
    private readonly bool _percent;

    public MssqlTopNode(int count, bool withTies = false, bool percent = false)
    {
        _count = count;
        _withTies = withTies;
        _percent = percent;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("TOP (");
        sqlBuilder.Append(sqlBuilder.AddParameter(_count));
        sqlBuilder.Append(')');
        if (_percent) sqlBuilder.Append(" PERCENT");
        if (_withTies) sqlBuilder.Append(" WITH TIES");
        sqlBuilder.Append(' ');
    }
}
```

### 5.3 Create [`Nodes/MssqlOutputNode.cs`](Drizzle4Dotnet/src/Mssql/Nodes/MssqlOutputNode.cs)

```csharp
/// <summary>
/// Represents an OUTPUT clause column reference: INSERTED.column or DELETED.column
/// </summary>
public class MssqlOutputNode : IGenericSql
{
    private readonly string _columnName;
    private readonly string _table; // "INSERTED" or "DELETED"

    public MssqlOutputNode(string columnName, string table)
    {
        _columnName = columnName;
        _table = table;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_table).Append('.');
        sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_columnName));
    }
}
```

### 5.4 Create [`Nodes/MssqlTableHintNode.cs`](Drizzle4Dotnet/src/Mssql/Nodes/MssqlTableHintNode.cs)

```csharp
/// <summary>
/// Represents a table hint in MSSQL: WITH (NOLOCK), WITH (TABLOCK), etc.
/// </summary>
public class MssqlTableHintNode
{
    private readonly string _hint;

    public MssqlTableHintNode(string hint)
    {
        _hint = hint;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(" WITH (").Append(_hint).Append(')');
    }
}
```

### 5.5 Create [`Nodes/MssqlSequenceNode.cs`](Drizzle4Dotnet/src/Mssql/Nodes/MssqlSequenceNode.cs)

```csharp
/// <summary>
/// Represents NEXT VALUE FOR [sequence_name].
/// MSSQL syntax: NEXT VALUE FOR [dbo].[my_sequence]
/// </summary>
public class MssqlSequenceNode : IOperator<long>
{
    private readonly string _sequenceName;
    private readonly string? _schemaName;

    public MssqlSequenceNode(string sequenceName, string? schemaName = "dbo")
    {
        _sequenceName = sequenceName;
        _schemaName = schemaName;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("NEXT VALUE FOR ");
        sqlBuilder.Append(MssqlSqlDialectImpl.BuildTableName(_schemaName, _sequenceName));
    }
}
```

---

## 6. Phase MS5: MSSQL DML — SELECT

### 6.1 Key MSSQL SELECT Differences

| Feature | MSSQL Syntax | Notes |
|---------|-------------|-------|
| **Limit rows** | `SELECT TOP (n) ...` | Use before select list |
| **Limit with ties** | `SELECT TOP (n) WITH TIES ...` | Must have ORDER BY |
| **Pagination** | `OFFSET n ROWS FETCH NEXT m ROWS ONLY` | Requires ORDER BY |
| **Pagination without ORDER BY** | `ORDER BY (SELECT 0) OFFSET n ROWS FETCH NEXT m ROWS ONLY` | Workaround |
| **CROSS APPLY** | `CROSS APPLY` | Like LATERAL JOIN |
| **OUTER APPLY** | `OUTER APPLY` | Like LEFT LATERAL JOIN |
| **Table hints** | `WITH (NOLOCK)` | After table reference |
| **DISTINCT** | `SELECT DISTINCT` | Same as standard |
| **FOR XML** | `FOR XML PATH, RAW, AUTO` | XML output |
| **FOR JSON** | `FOR JSON PATH, AUTO` | JSON output (2016+) |
| **OPTION clause** | `OPTION (MAXDOP 1, RECOMPILE)` | Query hints |

### 6.2 Create [`MssqlSelectQuery.cs`](Drizzle4Dotnet/src/Mssql/Query/MssqlSelectQuery.cs)

```csharp
public class MssqlSelectQuery<TReturn, TVirtualTable> 
    : SelectQuery<TReturn, MssqlSqlDialectImpl, TVirtualTable, MssqlSelectQuery<TReturn, TVirtualTable>>,
      IApplyJoin<MssqlSelectQuery<TReturn, TVirtualTable>, MssqlSqlDialectImpl>,
      IFullOuterJoin<MssqlSelectQuery<TReturn, TVirtualTable>, MssqlSqlDialectImpl>
    where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
{
    private MssqlTopNode? _topNode;
    private readonly List<MssqlTableHintNode> _tableHints = new();

    public MssqlSelectQuery(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<MssqlSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ====== TOP ======
    public MssqlSelectQuery<TReturn, TVirtualTable> Top(int count)
    {
        _topNode = new MssqlTopNode(count);
        return this;
    }

    public MssqlSelectQuery<TReturn, TVirtualTable> TopWithTies(int count)
    {
        _topNode = new MssqlTopNode(count, withTies: true);
        return this;
    }

    // ====== CROSS / OUTER APPLY (MSSQL alternative to LATERAL JOIN) ======
    public MssqlSelectQuery<TReturn, TVirtualTable> CrossApply(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS APPLY");

    public MssqlSelectQuery<TReturn, TVirtualTable> OuterApply(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "OUTER APPLY");

    // ====== FULL OUTER JOIN ======
    public MssqlSelectQuery<TReturn, TVirtualTable> FullJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "FULL");

    // ====== Table Hints ======
    public MssqlSelectQuery<TReturn, TVirtualTable> WithHint(MssqlTableHintNode hint)
    {
        _tableHints.Add(hint);
        return this;
    }

    // ====== Override BuildSql to inject TOP ======
    protected override void BuildSqlSelect(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("SELECT ");
        if (_topNode != null)
        {
            _topNode.BuildSql(sqlBuilder);
        }
        else if (IsDistinct)
        {
            sqlBuilder.Append("DISTINCT ");
        }
    }

    // ====== Override BuildSqlLimitOffset for OFFSET/FETCH ======
    // MSSQL requires OFFSET/FETCH to come after ORDER BY, not as a suffix.
    // We need to override how ORDER BY and LIMIT/OFFSET are built together.
    // (This may require changes to the base SelectQuery class or a custom approach.)
    
    protected override void BuildSqlLock(ISqlBuilder sqlBuilder)
    {
        // MSSQL does not have FOR UPDATE in the same way as PostgreSQL/MySQL.
        // Table hints (WITH (NOLOCK), etc.) are used instead.
        // Lock hints are typically applied per-table, handled separately.
    }
}
```

### 6.3 Important: OFFSET/FETCH Handling

MSSQL's `OFFSET ... FETCH NEXT ... ROWS ONLY` must come **after** `ORDER BY` in the SQL statement. The current base [`SelectQuery`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs) likely calls `BuildSqlOrderBy` then `BuildSqlLimitOffset` separately. 

For MSSQL:
1. If TOP is used, no OFFSET/FETCH is needed (TOP is simpler).
2. If OFFSET/FETCH is used, it must be integrated with ORDER BY.
3. If neither TOP nor OFFSET/FETCH is used, standard behavior applies.

**Design Decision:** The MSSQL `MssqlSelectQuery` should override the ORDER BY and LIMIT/OFFSET building to emit:
```sql
ORDER BY col1 ASC, col2 DESC OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY
```

This may require:
- A custom approach in `MssqlSelectQuery.BuildSql()` that builds ORDER BY and OFFSET/FETCH together
- Or a modification to the base class to support dialect-specific ORDER BY/LIMIT integration

**Recommended approach:** Override `BuildSql()` in `MssqlSelectQuery` to handle the ORDER BY + OFFSET/FETCH combination, delegating to base for other clauses.

### 6.4 FOR JSON Support (Optional, Future)

```csharp
public MssqlSelectQuery<TReturn, TVirtualTable> ForJsonPath()
{
    _forJsonMode = "JSON PATH";
    return this;
}

public MssqlSelectQuery<TReturn, TVirtualTable> ForJsonAuto()
{
    _forJsonMode = "JSON AUTO";
    return this;
}
```

---

## 7. Phase MS6: MSSQL DML — INSERT

### 7.1 Key MSSQL INSERT Differences

| Feature | MSSQL Syntax | Notes |
|---------|-------------|-------|
| **Standard INSERT** | `INSERT INTO [table] (cols) VALUES (vals)` | Same as standard |
| **OUTPUT INSERTED** | `OUTPUT INSERTED.col1, INSERTED.col2` | Returns inserted values |
| **OUTPUT DELETED** | `OUTPUT DELETED.col1` | Returns values before update/delete |
| **OUTPUT INTO** | `OUTPUT INSERTED.col INTO @tableVar` | Captures output to table variable |
| **Multiple rows** | `VALUES (...), (...), (...)` | Same as standard |
| **INSERT ... SELECT** | `INSERT INTO [table] SELECT ...` | Same as standard |
| **INSERT ... EXEC** | `INSERT INTO [table] EXEC proc` | MSSQL-specific |
| **IDENTITY_INSERT** | `SET IDENTITY_INSERT [table] ON` | Required for explicit identity values |
| **DEFAULT VALUES** | `INSERT INTO [table] DEFAULT VALUES` | ✅ Supported |

### 7.2 Create [`MssqlInsertQuery.cs`](Drizzle4Dotnet/src/Mssql/Query/MssqlInsertQuery.cs)

```csharp
public class MssqlInsertQuery<TTable> : InsertQuery<TTable, MssqlSqlDialectImpl, MssqlInsertQuery<TTable>>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private readonly List<string> _outputColumns = new();
    private bool _outputIntoTableVar;

    public MssqlInsertQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ====== OUTPUT clause (MSSQL's equivalent of RETURNING) ======
    
    /// <summary>
    /// Adds OUTPUT INSERTED.column to capture inserted values.
    /// MSSQL syntax: INSERT INTO [table] (cols) OUTPUT INSERTED.col VALUES (...)
    /// </summary>
    public MssqlInsertQuery<TTable> OutputInserted(params IColumnOfTable<TTable>[] columns)
    {
        foreach (var col in columns)
            _outputColumns.Add(col.Identifier);
        return this;
    }

    // ====== BuildSql ======
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        base.BuildSql(sqlBuilder);
        // OUTPUT clause is built inside the INSERT statement
    }

    protected override void BuildInsertKeywords(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INSERT INTO ");
    }

    // Override to inject OUTPUT before VALUES
    protected override void BuildInsertValues(ISqlBuilder sqlBuilder, List<Dictionary<string, object?>> newValues, List<string> allColumns)
    {
        BuildOutputClause(sqlBuilder);
        base.BuildInsertValues(sqlBuilder, newValues, allColumns);
    }

    private void BuildOutputClause(ISqlBuilder sqlBuilder)
    {
        if (_outputColumns.Count == 0) return;
        
        sqlBuilder.Append(" OUTPUT ");
        for (int i = 0; i < _outputColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append("INSERTED.");
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_outputColumns[i]));
        }
        sqlBuilder.Append(' ');
    }
}
```

**Design Note:** The OUTPUT clause must be placed after the column list but before the VALUES clause:
```sql
INSERT INTO [users] ([name], [email])
OUTPUT INSERTED.[id], INSERTED.[name]
VALUES (@p0, @p1)
```

This differs from PostgreSQL's `RETURNING` which goes at the end. The `InsertQuery` base class should provide a virtual hook `BuildInsertValues()` or similar that MSSQL can override to inject the OUTPUT clause.

---

## 8. Phase MS7: MSSQL DML — UPDATE

### 8.1 Key MSSQL UPDATE Differences

| Feature | MSSQL Syntax | Notes |
|---------|-------------|-------|
| **Standard UPDATE** | `UPDATE [table] SET col = val WHERE ...` | Same as standard |
| **UPDATE with JOIN** | `UPDATE t SET ... FROM t JOIN other ON ... WHERE ...` | Uses FROM clause (similar to PostgreSQL) |
| **UPDATE with TOP** | `UPDATE TOP (n) [table] SET ...` | Limit rows to update |
| **OUTPUT clause** | `OUTPUT INSERTED.col, DELETED.col` | Returns old/new values |
| **UPDATE with CTE** | `WITH cte AS (...) UPDATE cte SET ...` | Same as standard |

### 8.2 Create [`MssqlUpdateQuery.cs`](Drizzle4Dotnet/src/Mssql/Query/MssqlUpdateQuery.cs)

```csharp
public class MssqlUpdateQuery<TTable> : UpdateQuery<TTable, MssqlSqlDialectImpl, MssqlUpdateQuery<TTable>>,
    IJoin<MssqlUpdateQuery<TTable>, MssqlSqlDialectImpl>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MssqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private int? _topCount;
    private readonly List<string> _outputColumns = new();
    private bool _outputInserted; // true = OUTPUT INSERTED.*, false = OUTPUT DELETED.*

    public MssqlUpdateQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ====== UPDATE with JOIN (MSSQL syntax: FROM clause) ======
    
    private MssqlUpdateQuery<TTable> JoinInternal(
        IGenericTable<MssqlSqlDialectImpl> table,
        IGenericSql? on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public MssqlUpdateQuery<TTable> InnerJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");
    public MssqlUpdateQuery<TTable> LeftJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");
    public MssqlUpdateQuery<TTable> RightJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");
    public MssqlUpdateQuery<TTable> CrossJoin(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS");

    // ====== TOP on UPDATE ======
    public MssqlUpdateQuery<TTable> Top(int count)
    {
        _topCount = count;
        return this;
    }

    // ====== OUTPUT clause ======
    public MssqlUpdateQuery<TTable> OutputInserted(params string[] columns)
    {
        _outputInserted = true;
        _outputColumns.AddRange(columns);
        return this;
    }

    public MssqlUpdateQuery<TTable> OutputDeleted(params string[] columns)
    {
        _outputInserted = false;
        _outputColumns.AddRange(columns);
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("UPDATE");
        if (_topCount.HasValue)
        {
            sqlBuilder.Append(" TOP (");
            sqlBuilder.Append(sqlBuilder.AddParameter(_topCount.Value));
            sqlBuilder.Append(')');
        }
        sqlBuilder.Append(' ');
        Table.BuildRefSql(sqlBuilder);

        // OUTPUT clause (after table, before SET)
        BuildOutputClause(sqlBuilder);

        // SET clause
        SqlStatics.BuildSqlSetClause<MssqlSqlDialectImpl>(sqlBuilder, SetValues);

        // FROM clause for JOINs
        if (_joins.Count > 0)
        {
            sqlBuilder.Append(" FROM ");
            Table.BuildRefSql(sqlBuilder);
            SqlStatics.BuildSqlJoins<MssqlSqlDialectImpl>(sqlBuilder, _joins);
        }

        // WHERE clause
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }

    private void BuildOutputClause(ISqlBuilder sqlBuilder)
    {
        if (_outputColumns.Count == 0) return;
        
        sqlBuilder.Append(" OUTPUT ");
        string prefix = _outputInserted ? "INSERTED" : "DELETED";
        for (int i = 0; i < _outputColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(prefix).Append('.');
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_outputColumns[i]));
        }
    }
}
```

**MSSQL UPDATE with OUTPUT syntax:**
```sql
UPDATE [users]
SET [name] = @p0
OUTPUT INSERTED.[id], INSERTED.[name]
WHERE [id] = @p1
```

**MSSQL UPDATE with JOIN syntax:**
```sql
UPDATE u
SET u.[salary] = @p0
OUTPUT INSERTED.[id]
FROM [users] u
INNER JOIN [departments] d ON u.[department_id] = d.[id]
WHERE d.[name] = @p1
```

---

## 9. Phase MS8: MSSQL DML — DELETE

### 9.1 Key MSSQL DELETE Differences

| Feature | MSSQL Syntax | Notes |
|---------|-------------|-------|
| **Standard DELETE** | `DELETE FROM [table] WHERE ...` | Same as standard |
| **DELETE with JOIN** | `DELETE t FROM t JOIN other ON ... WHERE ...` | Same as MySQL syntax |
| **DELETE with TOP** | `DELETE TOP (n) FROM [table]` | Limit rows to delete |
| **OUTPUT clause** | `OUTPUT DELETED.col` | Returns deleted values |
| **TRUNCATE** | `TRUNCATE TABLE [table]` | Faster than DELETE (no logging) |

### 9.2 Create [`MssqlDeleteQuery.cs`](Drizzle4Dotnet/src/Mssql/Query/MssqlDeleteQuery.cs)

```csharp
public class MssqlDeleteQuery<TTable> : DeleteQuery<TTable, MssqlSqlDialectImpl, MssqlDeleteQuery<TTable>>,
    IJoin<MssqlDeleteQuery<TTable>, MssqlSqlDialectImpl>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MssqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private int? _topCount;
    private readonly List<string> _outputColumns = new();

    public MssqlDeleteQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ====== DELETE with JOIN ======
    
    private MssqlDeleteQuery<TTable> JoinInternal(
        IGenericTable<MssqlSqlDialectImpl> table,
        IGenericSql? on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public MssqlDeleteQuery<TTable> InnerJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");
    public MssqlDeleteQuery<TTable> LeftJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");
    public MssqlDeleteQuery<TTable> CrossJoin(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS");

    // ====== TOP on DELETE ======
    public MssqlDeleteQuery<TTable> Top(int count)
    {
        _topCount = count;
        return this;
    }

    // ====== OUTPUT clause ======
    public MssqlDeleteQuery<TTable> OutputDeleted(params string[] columns)
    {
        _outputColumns.AddRange(columns);
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        if (_joins.Count > 0)
        {
            // DELETE with JOIN: DELETE u FROM [users] u JOIN ... WHERE ...
            sqlBuilder.Append("DELETE");
            if (_topCount.HasValue)
            {
                sqlBuilder.Append(" TOP (");
                sqlBuilder.Append(sqlBuilder.AddParameter(_topCount.Value));
                sqlBuilder.Append(')');
            }
            sqlBuilder.Append(' ');
            Table.BuildRefSql(sqlBuilder);
            BuildOutputClause(sqlBuilder);
            sqlBuilder.Append(" FROM ");
            Table.BuildRefSql(sqlBuilder);
            SqlStatics.BuildSqlJoins<MssqlSqlDialectImpl>(sqlBuilder, _joins);
        }
        else
        {
            // Standard DELETE
            sqlBuilder.Append("DELETE");
            if (_topCount.HasValue)
            {
                sqlBuilder.Append(" TOP (");
                sqlBuilder.Append(sqlBuilder.AddParameter(_topCount.Value));
                sqlBuilder.Append(')');
            }
            sqlBuilder.Append(" FROM ");
            Table.BuildRefSql(sqlBuilder);
            BuildOutputClause(sqlBuilder);
        }

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }

    private void BuildOutputClause(ISqlBuilder sqlBuilder)
    {
        if (_outputColumns.Count == 0) return;
        
        sqlBuilder.Append(" OUTPUT ");
        for (int i = 0; i < _outputColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append("DELETED.");
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_outputColumns[i]));
        }
    }
}
```

---

## 10. Phase MS9: MSSQL MERGE (UPSERT)

### 10.1 MERGE Statement Overview

MSSQL's `MERGE` is the primary upsert mechanism (equivalent to PostgreSQL's `ON CONFLICT` or MySQL's `ON DUPLICATE KEY UPDATE`).

```sql
MERGE [target_table] AS target
USING [source_table | SELECT ...] AS source
ON (target.id = source.id)
WHEN MATCHED THEN
    UPDATE SET target.name = source.name
WHEN NOT MATCHED THEN
    INSERT (id, name) VALUES (source.id, source.name)
WHEN NOT MATCHED BY SOURCE THEN
    DELETE;
```

### 10.2 Create [`MssqlMergeQuery.cs`](Drizzle4Dotnet/src/Mssql/Query/MssqlMergeQuery.cs)

```csharp
/// <summary>
/// MSSQL MERGE (upsert) query builder.
/// MERGE combines INSERT, UPDATE, and DELETE into a single statement.
/// Supported actions: WHEN MATCHED THEN UPDATE, WHEN NOT MATCHED THEN INSERT,
/// WHEN NOT MATCHED BY SOURCE THEN DELETE/UPDATE.
/// </summary>
public class MssqlMergeQuery<TTable> : ReturningQuery<MssqlMergeQuery<TTable>, TTable, MssqlSqlDialectImpl>,
    IJoin<MssqlMergeQuery<TTable>, MssqlSqlDialectImpl>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private IGenericSql? _source;
    private IGenericSql? _onCondition;
    private IGenericSql? _whenMatchedUpdate;
    private IGenericSql? _whenNotMatchedInsert;
    private IGenericSql? _whenNotMatchedBySourceDelete;
    private IGenericSql? _whenNotMatchedBySourceUpdate;
    private readonly List<(IGenericTable<MssqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private readonly List<string> _outputColumns = new();

    public MssqlMergeQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    /// <summary>Sets the source table/subquery for MERGE.</summary>
    public MssqlMergeQuery<TTable> Using(IGenericSql source)
    {
        _source = source;
        return this;
    }

    /// <summary>Sets the ON condition for matching.</summary>
    public MssqlMergeQuery<TTable> On(IGenericSql condition)
    {
        _onCondition = condition;
        return this;
    }

    /// <summary>WHEN MATCHED THEN UPDATE SET ...</summary>
    public MssqlMergeQuery<TTable> WhenMatchedThenUpdate(Dictionary<string, object?> setValues)
    {
        _whenMatchedUpdate = new RawSql(/* build SET clause */);
        return this;
    }

    /// <summary>WHEN NOT MATCHED THEN INSERT (cols) VALUES (vals)</summary>
    public MssqlMergeQuery<TTable> WhenNotMatchedThenInsert(List<string> columns, List<object?> values)
    {
        _whenNotMatchedInsert = new RawSql(/* build INSERT clause */);
        return this;
    }

    /// <summary>WHEN NOT MATCHED BY SOURCE THEN DELETE</summary>
    public MssqlMergeQuery<TTable> WhenNotMatchedBySourceThenDelete()
    {
        _whenNotMatchedBySourceDelete = new RawSql("DELETE");
        return this;
    }

    // ====== OUTPUT clause ======
    public MssqlMergeQuery<TTable> OutputInserted(params string[] columns) { ... }
    public MssqlMergeQuery<TTable> OutputDeleted(params string[] columns) { ... }
    public MssqlMergeQuery<TTable> OutputAction() { ... } // OUTPUT $action

    // ====== JOIN support ======
    // ... (same pattern as other MSSQL query builders)

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("MERGE ");
        Table.BuildRefSql(sqlBuilder);
        sqlBuilder.Append(" AS target");

        // USING clause
        sqlBuilder.Append(" USING ");
        _source?.BuildSql(sqlBuilder);
        sqlBuilder.Append(" AS source");

        // ON clause
        sqlBuilder.Append(" ON (");
        _onCondition?.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');

        // WHEN MATCHED THEN UPDATE
        if (_whenMatchedUpdate != null)
        {
            sqlBuilder.Append(" WHEN MATCHED THEN UPDATE SET ");
            _whenMatchedUpdate.BuildSql(sqlBuilder);
        }

        // WHEN NOT MATCHED THEN INSERT
        if (_whenNotMatchedInsert != null)
        {
            sqlBuilder.Append(" WHEN NOT MATCHED THEN INSERT ");
            _whenNotMatchedInsert.BuildSql(sqlBuilder);
        }

        // WHEN NOT MATCHED BY SOURCE THEN DELETE
        if (_whenNotMatchedBySourceDelete != null)
        {
            sqlBuilder.Append(" WHEN NOT MATCHED BY SOURCE THEN DELETE");
        }

        // OUTPUT clause
        BuildOutputClause(sqlBuilder);
    }

    protected override void ValidateQuery()
    {
        if (_source == null)
            throw new InvalidOperationException("MERGE requires a source. Call Using().");
        if (_onCondition == null)
            throw new InvalidOperationException("MERGE requires an ON condition. Call On().");
    }
}
```

### 10.3 MERGE Design Considerations

- **Complexity:** MERGE is complex and has known edge cases (e.g., `MERGE` with `OUTPUT` can be tricky with triggers)
- **Alternative:** When MERGE is overkill, use separate INSERT + UPDATE with `@@ROWCOUNT` check
- **Recommendation:** Start with a simple MERGE implementation supporting basic WHEN MATCHED / WHEN NOT MATCHED, then add advanced features (WHEN NOT MATCHED BY SOURCE, OUTPUT INTO, etc.)
- **Add `Merge()` method** to [`MssqlDbClient.cs`](Drizzle4Dotnet/src/Mssql/MssqlDbClient.cs):
  ```csharp
  public MssqlMergeQuery<TTable> Merge<TTable>(TTable table) where TTable : ITable<MssqlSqlDialectImpl>
      => new MssqlMergeQuery<TTable>(table, this);
  ```

---

## 11. Phase MS10: MSSQL Schema / Column Types

### 11.1 MSSQL Data Type Mapping

| .NET Type | MSSQL Type | Notes |
|-----------|-----------|-------|
| `int` | `INT` | |
| `long` | `BIGINT` | |
| `short` | `SMALLINT` | |
| `byte` | `TINYINT` | |
| `bool` | `BIT` | |
| `string` | `NVARCHAR(MAX)` / `NVARCHAR(n)` | Unicode by default |
| `char` | `NCHAR(1)` | |
| `decimal` / `double` / `float` | `DECIMAL(p,s)` / `FLOAT` / `REAL` | |
| `DateTime` | `DATETIME2` / `DATETIME` | `DATETIME2` recommended |
| `DateTimeOffset` | `DATETIMEOFFSET` | |
| `TimeSpan` | `TIME` | |
| `Guid` | `UNIQUEIDENTIFIER` | |
| `byte[]` | `VARBINARY(MAX)` / `VARBINARY(n)` | |
| `object` | `SQL_VARIANT` | |

### 11.2 Key DDL Differences

| Feature | MSSQL | PostgreSQL |
|---------|-------|------------|
| **Auto-increment** | `IDENTITY(1,1)` | `GENERATED AS IDENTITY` / `SERIAL` |
| **Default values** | `DEFAULT 'value'` | `DEFAULT 'value'` |
| **NOT NULL** | `NOT NULL` | Same |
| **Primary key** | `PRIMARY KEY` | Same |
| **Foreign key** | `REFERENCES [table](col)` | Same |
| **Unique constraint** | `UNIQUE` / `UNIQUE (col1, col2)` | Same |
| **Index** | `CREATE INDEX ... ON [table](col)` | Same |
| **Sequences** | `CREATE SEQUENCE` | `CREATE SEQUENCE` |
| **Temporal tables** | `SYSTEM_VERSIONING = ON` | — |

The existing [`MigrationSchemaGenerator.cs`](SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs) and [`MigrationManager.cs`](Drizzle4Dotnet/src/Core/Schema/Migration/MigrationManager.cs) need MSSQL-specific DDL generation for:

- `IDENTITY(1,1)` instead of `SERIAL`/`GENERATED AS IDENTITY`
- `NVARCHAR(MAX)` instead of `TEXT`
- `DATETIME2` instead of `TIMESTAMP`/`TIMESTAMPTZ`
- `BIT` instead of `BOOLEAN`

### 11.3 Schema/Column Definitions for MSSQL

Create [`SharedDemo/Mssql/Schema.cs`](SharedDemo/Mssql/Schema.cs) and [`SharedDemo/Mssql/DbDto.cs`](SharedDemo/Mssql/DbDto.cs) following the patterns in [`SharedDemo/PgSql/`](SharedDemo/PgSql/).

---

## 12. Phase MS11: MSSQL Connection & Execution

### 12.1 Create [`MssqlDbClient.cs`](Drizzle4Dotnet/src/Mssql/MssqlDbClient.cs)

Following the pattern of [`PgSqlDbClient.cs`](Drizzle4Dotnet/src/Dialect/PgSql/PgSqlDbClient.cs):

```csharp
public class MssqlDbClient : DbClientWithTransaction<MssqlDbClient, MssqlSqlDialectImpl>
{
    public MssqlDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public MssqlSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) 
        where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
        => new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, this);

    public MssqlSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) 
        where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
        => new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, this).Distinct();

    public MssqlInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlInsertQuery<TTable>(table, this);

    public MssqlUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlUpdateQuery<TTable>(table, this);

    public MssqlDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlDeleteQuery<TTable>(table, this);

    public MssqlMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlMergeQuery<TTable>(table, this);

    protected override MssqlDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
        => new MssqlDbClient(conn, transaction);
}
```

### 12.2 Create [`MssqlQueryBuilder.cs`](Drizzle4Dotnet/src/Mssql/MssqlQueryBuilder.cs)

Following the pattern of [`PgSqlQueryBuilder.cs`](Drizzle4Dotnet/src/Dialect/PgSql/PgSqlQueryBuilder.cs):

```csharp
public class MssqlQueryBuilder
{
    private static readonly IQueryExecutor<MssqlSqlDialectImpl>? _nullExecutor = null;

    public MssqlSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) 
        where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
        => new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!);

    public MssqlSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) 
        where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
        => new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!).Distinct();

    public MssqlInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlInsertQuery<TTable>(table, _nullExecutor!);

    public MssqlUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlUpdateQuery<TTable>(table, _nullExecutor!);

    public MssqlDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlDeleteQuery<TTable>(table, _nullExecutor!);

    public MssqlMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
        => new MssqlMergeQuery<TTable>(table, _nullExecutor!);
}
```

### 12.3 Create [`MssqlQueryBuilderExtensions.cs`](Drizzle4Dotnet/src/Mssql/MssqlQueryBuilderExtensions.cs)

Auto-generated convenience overloads following the pattern of [`SqliteQueryBuilderExtensions.cs`](Drizzle4Dotnet/src/Dialect/Sqlite/SqliteQueryBuilderExtensions.cs):

```csharp
public static class MssqlQueryBuilderExtensions
{
    public static MssqlSelectQuery<T1, TypedTupleGeneratedSubqueryTable<T1, MssqlSqlDialectImpl>> 
        Select<T1>(this MssqlQueryBuilder builder, IAliasedSql<T1> col1)
        => builder.Select(new TypedTupleSelectedColumns<T1, MssqlSqlDialectImpl>(col1));

    public static MssqlSelectQuery<T1, TypedTupleGeneratedSubqueryTable<T1, MssqlSqlDialectImpl>> 
        SelectDistinct<T1>(this MssqlQueryBuilder builder, IAliasedSql<T1> col1)
        => builder.SelectDistinct(new TypedTupleSelectedColumns<T1, MssqlSqlDialectImpl>(col1));

    // ... up to 8 column overloads ...
}
```

### 12.4 Add NuGet Dependencies

Add to [`Drizzle4Dotnet.csproj`](Drizzle4Dotnet/Drizzle4Dotnet.csproj):
```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.0.0" />
```

---

## 13. Phase MS12: Source Generator MSSQL Support

### 13.1 Update [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs)

The source generator currently generates tables hardcoded to specific dialects. Add support for `MssqlSqlDialectImpl`:

- Accept a `Dialect` parameter in the `[Table]` attribute: `[Table("users", Dialect = typeof(MssqlSqlDialectImpl))]`
- Generate `MssqlColumn<T, TTable>` instances instead of `PgColumn<T, TTable>`
- Generate `IMssqlDbTable` interface implementation

### 13.2 Update [`MigrationSchemaGenerator.cs`](SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs)

Add MSSQL-specific DDL generation:

- `IDENTITY(1,1)` for auto-increment columns
- `NVARCHAR(MAX)` for string columns
- `DATETIME2` for DateTime columns
- `BIT` for boolean columns
- `[dbo].[table]` naming convention (schema-qualified)

### 13.3 Update [`DbSelectGenerator.cs`](SourceGenerators/SourceGenerators/DbSelectGenerator.cs)

Add support for generating MSSQL-specific select DTOs and mapper functions.

---

## 14. Phase MS13: Testing Strategy

### 14.1 Test Categories

| Test Type | File | Description |
|-----------|------|-------------|
| **SELECT** | [`Test/Select/MssqlSelectTests.cs`](Test/Select/MssqlSelectTests.cs) | SELECT with TOP, DISTINCT, OFFSET/FETCH, APPLY, FULL OUTER JOIN, table hints, window functions |
| **INSERT** | [`Test/Insert/MssqlInsertTests.cs`](Test/Insert/MssqlInsertTests.cs) | INSERT with OUTPUT, INSERT ... SELECT, DEFAULT VALUES, multiple rows |
| **UPDATE** | [`Test/Update/MssqlUpdateTests.cs`](Test/Update/MssqlUpdateTests.cs) | UPDATE with JOIN, TOP, OUTPUT, CTE |
| **DELETE** | [`Test/Delete/MssqlDeleteTests.cs`](Test/Delete/MssqlDeleteTests.cs) | DELETE with JOIN, TOP, OUTPUT, CTE |
| **MERGE** | [`Test/Merge/MssqlMergeTests.cs`](Test/Merge/MssqlMergeTests.cs) | MERGE with matched/not matched, OUTPUT |
| **Migration** | [`Test/Migration/MssqlMigrationTests.cs`](Test/Migration/MssqlMigrationTests.cs) | DDL generation, schema comparison, migration SQL |
| **Functions** | (inline in the files above) | DateTime, String, Numeric, JSON functions |
| **Integration** | (separate) | End-to-end with real SQL Server (Docker) |

### 14.2 Test Approach

Following the pattern of existing tests (e.g., [`Test/Select/PgSqlSelectTests.cs`](Test/Select/PgSqlSelectTests.cs)):

1. **Offline testing:** Use `MssqlQueryBuilder` to build queries without a database connection
2. **SQL comparison:** Build SQL and compare expected output strings
3. **Parameter assertion:** Verify parameter names and values
4. **Exception testing:** Verify validation logic (e.g., OFFSET without ORDER BY)

**Example test structure:**

```csharp
[TestFixture]
public class MssqlSelectTests
{
    private MssqlQueryBuilder _db = null!;

    [SetUp]
    public void Setup()
    {
        _db = new MssqlQueryBuilder();
    }

    [Test]
    public void Select_WithTop_GeneratesCorrectSql()
    {
        var query = _db.Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .Top(10);

        var (sql, parameters) = QueryTestHelper.Build(query);

        Assert.That(sql, Does.Contain("SELECT TOP (@p0)"));
        Assert.That(parameters["@p0"], Is.EqualTo(10));
    }

    [Test]
    public void Select_WithOffsetFetch_GeneratesCorrectSql()
    {
        var query = _db.Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .OrderBy(UsersTable.Id)
            .Limit(10)
            .Offset(20);

        var (sql, parameters) = QueryTestHelper.Build(query);

        Assert.That(sql, Does.Contain("OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY"));
        Assert.That(parameters["@p0"], Is.EqualTo(20));
        Assert.That(parameters["@p1"], Is.EqualTo(10));
    }

    [Test]
    public void Select_WithCrossApply_GeneratesCorrectSql()
    {
        var query = _db.Select(UsersTable.Id, UsersTable.Name)
            .From(users)
            .CrossApply(someTable);

        var (sql, _) = QueryTestHelper.Build(query);

        Assert.That(sql, Does.Contain("CROSS APPLY"));
    }

    [Test]
    public void Insert_WithOutput_GeneratesCorrectSql()
    {
        var query = _db.Insert(UsersTable)
            .Values(new { Name = "Test", Email = "test@test.com" })
            .OutputInserted(UsersTable.Id);

        var (sql, _) = QueryTestHelper.Build(query);

        Assert.That(sql, Does.Contain("OUTPUT INSERTED.[id]"));
    }
}
```

### 14.3 Docker-based Integration Testing

For integration tests with a real SQL Server, provide a docker-compose setup:

```yaml
# docker-compose.mssql.yml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourStrong!Password"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
```

---

## 15. Phase MS14: NuGet Packaging & Demo

### 15.1 Demo Project

Create a [`Demo1/Program.cs`](Demo1/Program.cs) variant or separate demo showing MSSQL usage:

```csharp
using var db = new MssqlDbClient(connection);

// SELECT with TOP
var users = await db.Select(UsersTable.Id, UsersTable.Name)
    .From(users)
    .Top(10)
    .ExecuteGetListAsync();

// INSERT with OUTPUT
var newId = await db.Insert(UsersTable)
    .Values(new { Name = "John", Email = "john@test.com" })
    .OutputInserted(UsersTable.Id)
    .ExecuteAsync();

// UPDATE with JOIN
await db.Update(UsersTable)
    .Set(UsersTable.Salary, 50000)
    .InnerJoin(DepartmentsTable, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
    .Where(Eq(DepartmentsTable.Name, "Engineering"))
    .ExecuteAsync();

// DELETE with OUTPUT
await db.Delete(UsersTable)
    .OutputDeleted(UsersTable.Id, UsersTable.Name)
    .Where(Eq(UsersTable.IsActive, false))
    .ExecuteAsync();

// MERGE (upsert)
await db.Merge(UsersTable)
    .Using(new RawSql("SELECT @p0 AS id, @p1 AS name"))
    .On(Eq(UsersTable.Id, Sql.Value(1)))
    .WhenMatchedThenUpdate(new() { [UsersTable.Name.Identifier] = "Updated" })
    .WhenNotMatchedThenInsert(new() { "id", "name" }, new() { 1, "New" })
    .ExecuteAsync();
```

### 15.2 Shared Demo Schema

Create [`SharedDemo/Mssql/Schema.cs`](SharedDemo/Mssql/Schema.cs), [`SharedDemo/Mssql/DbDto.cs`](SharedDemo/Mssql/DbDto.cs), [`SharedDemo/Mssql/schema.sql`](SharedDemo/Mssql/schema.sql), and [`SharedDemo/Mssql/data.sql`](SharedDemo/Mssql/data.sql) following existing patterns.

### 15.3 NuGet Packaging

When packaging, include MSSQL support as an optional dependency:

```xml
<!-- In Drizzle4Dotnet.nuspec or similar -->
<group>
  <dependency id="Microsoft.Data.SqlClient" version="6.0.0" />
</group>
```

---

## 16. Appendix: MSSQL SQL Syntax Reference

### 16.1 Identifier Quoting

```sql
-- Square bracket quoting (default)
SELECT [id], [name] FROM [dbo].[users]

-- With QUOTED_IDENTIFIER ON, double quotes also work
SET QUOTED_IDENTIFIER ON
SELECT "id", "name" FROM "dbo"."users"
```

### 16.2 SELECT with TOP

```sql
-- TOP n
SELECT TOP (10) [id], [name] FROM [users]

-- TOP n PERCENT
SELECT TOP (10) PERCENT [id], [name] FROM [users]

-- TOP WITH TIES (requires ORDER BY)
SELECT TOP (10) WITH TIES [name], [salary] 
FROM [employees] 
ORDER BY [salary] DESC

-- OFFSET / FETCH (requires ORDER BY)
SELECT [id], [name] 
FROM [users] 
ORDER BY [id] 
OFFSET 20 ROWS FETCH NEXT 10 ROWS ONLY
```

### 16.3 INSERT with OUTPUT

```sql
-- INSERT with OUTPUT INSERTED
INSERT INTO [users] ([name], [email])
OUTPUT INSERTED.[id]
VALUES (@p0, @p1)

-- INSERT ... SELECT with OUTPUT
INSERT INTO [users] ([name], [email])
OUTPUT INSERTED.[id]
SELECT [name], [email] FROM [temp_users] WHERE [is_active] = 1
```

### 16.4 UPDATE with FROM/JOIN

```sql
-- UPDATE with JOIN via FROM clause
UPDATE u
SET u.[salary] = @p0
OUTPUT INSERTED.[id]
FROM [users] u
INNER JOIN [departments] d ON u.[department_id] = d.[id]
WHERE d.[name] = @p1

-- UPDATE with TOP
UPDATE TOP (10) [users]
SET [is_active] = 0
WHERE [last_login] < @p0
```

### 16.5 DELETE with FROM/JOIN

```sql
-- DELETE with JOIN
DELETE u
OUTPUT DELETED.[id]
FROM [users] u
INNER JOIN [departments] d ON u.[department_id] = d.[id]
WHERE d.[name] = @p0

-- DELETE with TOP
DELETE TOP (100) FROM [logs]
WHERE [created_at] < @p0
```

### 16.6 MERGE

```sql
MERGE [users] AS target
USING (SELECT @p0 AS id, @p1 AS name) AS source
ON (target.[id] = source.[id])
WHEN MATCHED THEN
    UPDATE SET target.[name] = source.[name]
WHEN NOT MATCHED THEN
    INSERT ([id], [name]) VALUES (source.[id], source.[name])
OUTPUT INSERTED.[id], $action;
```

### 16.7 CROSS/OUTER APPLY

```sql
SELECT u.[id], u.[name], recent_orders.[total]
FROM [users] u
CROSS APPLY (
    SELECT TOP (3) [total]
    FROM [orders]
    WHERE [user_id] = u.[id]
    ORDER BY [created_at] DESC
) recent_orders
```

### 16.8 Table Hints

```sql
SELECT [id], [name] FROM [users] WITH (NOLOCK)
UPDATE [users] WITH (TABLOCK) SET [name] = @p0
DELETE FROM [logs] WITH (READUNCOMMITTED)
```

### 16.9 Common T-SQL Functions

```sql
-- Date/Time
SELECT GETDATE(), SYSDATETIME(), GETUTCDATE()
SELECT DATEADD(DAY, 7, GETDATE())
SELECT DATEDIFF(DAY, '2024-01-01', GETDATE())
SELECT DATEPART(YEAR, GETDATE())
SELECT YEAR(GETDATE()), MONTH(GETDATE()), DAY(GETDATE())
SELECT EOMONTH(GETDATE())

-- String
SELECT LEN('Hello'), CHARINDEX('l', 'Hello')
SELECT SUBSTRING('Hello', 2, 3)
SELECT CONCAT('Hello', ' ', 'World')
SELECT REPLACE('Hello', 'l', 'x')
SELECT TRIM(' Hello '), LTRIM(' Hello '), RTRIM(' Hello ')

-- System
SELECT SCOPE_IDENTITY(), @@ROWCOUNT, @@IDENTITY
SELECT DB_NAME(), @@SERVERNAME, @@VERSION
```

---

## 17. Appendix: Implementation Order & Dependencies

### 17.1 Implementation Phases (Recommended Order)

| Phase | Description | Dependencies | Estimated Effort |
|-------|-------------|-------------|-----------------|
| **MS1** | `MssqlSqlDialectImpl` (dialect foundation) | None | Small |
| **MS2** | `MssqlTable`, `MssqlColumn` (types & interfaces) | MS1 | Small |
| **MS3a** | `MssqlFunctions.DateTime`, `MssqlFunctions.String` | MS1 | Medium |
| **MS3b** | `MssqlFunctions.Numeric`, `MssqlFunctions.Json`, `MssqlFunctions.Info` | MS1 | Medium |
| **MS4a** | `MssqlTopNode`, `MssqlOutputNode` (SQL nodes) | MS1 | Small |
| **MS4b** | `MssqlTableHintNode`, `MssqlSequenceNode` | MS1 | Small |
| **MS5** | `MssqlSelectQuery` (SELECT with TOP, OFFSET/FETCH, APPLY) | MS1, MS2, MS4a | Large |
| **MS6** | `MssqlInsertQuery` (INSERT with OUTPUT) | MS1, MS2, MS4a | Medium |
| **MS7** | `MssqlUpdateQuery` (UPDATE with FROM/JOIN, OUTPUT) | MS1, MS2, MS4a | Medium |
| **MS8** | `MssqlDeleteQuery` (DELETE with FROM/JOIN, OUTPUT) | MS1, MS2, MS4a | Medium |
| **MS9** | `MssqlMergeQuery` (MERGE upsert) | MS1, MS2, MS4a | Large |
| **MS10** | `MssqlDbClient`, `MssqlQueryBuilder`, extensions | MS5-MS9 | Medium |
| **MS11** | Schema/column types, migration DDL | MS10 | Medium |
| **MS12** | Source generator updates | MS10 | Medium |
| **MS13** | Tests (unit + integration) | MS5-MS11 | Large |
| **MS14** | Demo project, packaging, documentation | MS5-MS13 | Small |

### 17.2 Critical Path

The minimum viable MSSQL implementation follows this critical path:

```
MS1 → MS2 → MS4a → MS5 (SELECT) → MS6 (INSERT) → MS7 (UPDATE) → MS8 (DELETE) → MS10 (DbClient)
```

This enables basic CRUD operations with MSSQL syntax. MERGE (MS9) and advanced features can be added in subsequent iterations.

### 17.3 Core Changes Required

The following core infrastructure files may need modifications:

| File | Change Required | Reason |
|------|----------------|--------|
| [`ISqlDialect.cs`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) | Add `SupportsApplyJoin` property | MSSQL uses CROSS/OUTER APPLY instead of LATERAL |
| [`SqlDialectDefaults.cs`](Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs) | Add `SupportsApplyJoin` default | Default to `false` |
| [`SelectQuery.cs`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs) | Review ORDER BY + LIMIT/OFFSET integration hook | MSSQL's OFFSET/FETCH must come after ORDER BY |
| [`InsertQuery.cs`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) | Add virtual hook for OUTPUT clause injection | MSSQL's OUTPUT clause goes before VALUES |
| [`ISelectedColumns.cs`](Drizzle4Dotnet/src/Core/Shared/ISelectedColumns.cs) | Review for APPLY compatibility | CROSS/OUTER APPLY returns table expressions |
| [`QueryClauseInterfaces.cs`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs) | Add `IApplyJoin<TQuery, TDialect>` interface | For CROSS/OUTER APPLY support |

### 17.4 New Files Summary

| # | File Path | Type | Description |
|---|-----------|------|-------------|
| 1 | `Drizzle4Dotnet/src/Dialect/MssqlSqlDialectImpl.cs` | New | MSSQL dialect implementation |
| 2 | `Drizzle4Dotnet/src/Mssql/MssqlDbClient.cs` | New | MSSQL database client |
| 3 | `Drizzle4Dotnet/src/Mssql/MssqlQueryBuilder.cs` | New | Offline query builder |
| 4 | `Drizzle4Dotnet/src/Mssql/MssqlQueryBuilderExtensions.cs` | New | Convenience overloads |
| 5 | `Drizzle4Dotnet/src/Mssql/MssqlTable.cs` | New | Table interfaces |
| 6 | `Drizzle4Dotnet/src/Mssql/MssqlColumn.cs` | New | Column types |
| 7 | `Drizzle4Dotnet/src/Mssql/MssqlStatics.cs` | New | Static helpers |
| 8 | `Drizzle4Dotnet/src/Mssql/MssqlFunctions.cs` | New | Functions entry point |
| 9 | `Drizzle4Dotnet/src/Mssql/MssqlFunctions.DateTime.cs` | New | Date/time functions |
| 10 | `Drizzle4Dotnet/src/Mssql/MssqlFunctions.String.cs` | New | String functions |
| 11 | `Drizzle4Dotnet/src/Mssql/MssqlFunctions.Numeric.cs` | New | Numeric functions |
| 12 | `Drizzle4Dotnet/src/Mssql/MssqlFunctions.Json.cs` | New | JSON functions |
| 13 | `Drizzle4Dotnet/src/Mssql/MssqlFunctions.Info.cs` | New | Info functions |
| 14 | `Drizzle4Dotnet/src/Mssql/MssqlOperators.cs` | New | MSSQL operators |
| 15 | `Drizzle4Dotnet/src/Mssql/Nodes/MssqlTopNode.cs` | New | TOP(n) node |
| 16 | `Drizzle4Dotnet/src/Mssql/Nodes/MssqlOutputNode.cs` | New | OUTPUT clause node |
| 17 | `Drizzle4Dotnet/src/Mssql/Nodes/MssqlTableHintNode.cs` | New | Table hint node |
| 18 | `Drizzle4Dotnet/src/Mssql/Nodes/MssqlSequenceNode.cs` | New | Sequence node |
| 19 | `Drizzle4Dotnet/src/Mssql/Query/MssqlSelectQuery.cs` | New | SELECT query builder |
| 20 | `Drizzle4Dotnet/src/Mssql/Query/MssqlInsertQuery.cs` | New | INSERT query builder |
| 21 | `Drizzle4Dotnet/src/Mssql/Query/MssqlUpdateQuery.cs` | New | UPDATE query builder |
| 22 | `Drizzle4Dotnet/src/Mssql/Query/MssqlDeleteQuery.cs` | New | DELETE query builder |
| 23 | `Drizzle4Dotnet/src/Mssql/Query/MssqlMergeQuery.cs` | New | MERGE query builder |
| 24 | `SharedDemo/Mssql/Schema.cs` | New | Demo schema |
| 25 | `SharedDemo/Mssql/DbDto.cs` | New | Demo DTOs |
| 26 | `SharedDemo/Mssql/schema.sql` | New | Demo DDL |
| 27 | `SharedDemo/Mssql/data.sql` | New | Demo data |
| 28 | `Test/Select/MssqlSelectTests.cs` | New | SELECT tests |
| 29 | `Test/Insert/MssqlInsertTests.cs` | New | INSERT tests |
| 30 | `Test/Update/MssqlUpdateTests.cs` | New | UPDATE tests |
| 31 | `Test/Delete/MssqlDeleteTests.cs` | New | DELETE tests |
| 32 | `Test/Merge/MssqlMergeTests.cs` | New | MERGE tests |
| 33 | `Test/Migration/MssqlMigrationTests.cs` | New | Migration tests |

### 17.5 Modified Files Summary

| # | File Path | Change |
|---|-----------|--------|
| 1 | `Drizzle4Dotnet/Drizzle4Dotnet.csproj` | Add `Microsoft.Data.SqlClient` package reference |
| 2 | `Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs` | Add `SupportsApplyJoin` property |
| 3 | `Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs` | Add `SupportsApplyJoin => false` default |
| 4 | `Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs` | Add `IApplyJoin<TQuery, TDialect>` interface |
| 5 | `Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs` | Review ORDER BY/OFFSET integration hook |
| 6 | `Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs` | Add virtual hook for OUTPUT clause |
| 7 | `SourceGenerators/SourceGenerators/TableGenerator.cs` | Add MSSQL dialect support |
| 8 | `SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs` | Add MSSQL DDL generation |
| 9 | `SourceGenerators/SourceGenerators/DbSelectGenerator.cs` | Add MSSQL select generation |

---

> **Next Steps:** Begin implementation with Phase **MS1** (MssqlSqlDialectImpl), then proceed through the critical path. Each phase builds on the previous, following the established patterns in the existing PostgreSQL, MySQL, and SQLite implementations.
