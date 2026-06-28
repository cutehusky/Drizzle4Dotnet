# Drizzle4Dotnet — Oracle Database Implementation Plan

> **Status:** Draft  
> **Last Updated:** 2026-06-28  
> **Target:** Full Oracle (PL/SQL) dialect support, paralleling existing PostgreSQL, MySQL, SQLite, and MSSQL implementations.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Phase O1: Oracle Dialect (Foundation)](#2-phase-o1-oracle-dialect-foundation)
3. [Phase O2: Oracle Types & Interfaces](#3-phase-o2-oracle-types--interfaces)
4. [Phase O3: Oracle Functions](#4-phase-o3-oracle-functions)
5. [Phase O4: Oracle Operators & Nodes](#5-phase-o4-oracle-operators--nodes)
6. [Phase O5: Oracle DML — SELECT with Pagination](#6-phase-o5-oracle-dml--select)
7. [Phase O6: Oracle DML — INSERT with RETURNING](#7-phase-o6-oracle-dml--insert)
8. [Phase O7: Oracle DML — UPDATE](#8-phase-o7-oracle-dml--update)
9. [Phase O8: Oracle DML — DELETE](#9-phase-o8-oracle-dml--delete)
10. [Phase O9: Oracle MERGE (UPSERT)](#10-phase-o9-oracle-merge-upsert)
11. [Phase O10: Oracle Schema / Column Types](#11-phase-o10-oracle-schema--column-types)
12. [Phase O11: Oracle Connection & Execution](#12-phase-o11-oracle-connection--execution)
13. [Phase O12: Source Generator Oracle Support](#13-phase-o12-source-generator-oracle-support)
14. [Phase O13: Testing Strategy](#14-phase-o13-testing-strategy)
15. [Phase O14: NuGet Packaging & Demo](#15-phase-o14-nuget-packaging--demo)
16. [Appendix: Oracle SQL Syntax Reference](#16-appendix-oracle-sql-syntax-reference)
17. [Appendix: Implementation Order & Dependencies](#17-appendix-implementation-order--dependencies)

---

## 1. Architecture Overview

### 1.1 Oracle vs Other Dialects — Key Differences

| Aspect | PostgreSQL | MSSQL | **Oracle (PL/SQL)** |
|--------|------------|-------|----------------------|
| **Identifier quoting** | `"double quotes"` | `[square brackets]` | `"double quotes"` (uppercase unquoted) |
| **Parameter prefix** | `@p0`, `@p1` | `@p0`, `@p1` | **`:p0`, `:p1`** (colon prefix) |
| **Schema support** | `schema.table` | `[schema].[table]` | `"schema"."table"` (default: current user) |
| **Pagination** | `LIMIT ? OFFSET ?` | `OFFSET ? FETCH NEXT ?` | **`OFFSET ? ROWS FETCH NEXT ? ROWS ONLY`** (12c+) / `ROW_NUMBER()` (older) |
| **RETURNING clause** | ✅ `RETURNING ... INTO ...` | ✅ `OUTPUT INSERTED/DELETED` | ✅ **`RETURNING ... INTO ...`** (for DML; uses PL/SQL bind variables) |
| **RETURNING result set** | ✅ Returns rows directly | ✅ `OUTPUT` returns rows | ⚠️ `RETURNING ... BULK COLLECT INTO` (PL/SQL); no direct rowset |
| **UPSERT / MERGE** | `ON CONFLICT DO UPDATE/NOTHING` | `MERGE` | ✅ **`MERGE`** (Oracle 9i+) |
| **INSERT ALL** | ❌ Not supported | ❌ Not supported | ✅ **`INSERT ALL`** (multi-table insert) |
| **Dual table** | ❌ Not needed | ❌ Not needed | ✅ **`FROM DUAL`** required for scalar SELECT |
| **Sequence** | `NEXTVAL` / `CURRVAL` on `SEQUENCE` | `NEXT VALUE FOR` | ✅ **`sequence.NEXTVAL` / `sequence.CURRVAL`** |
| **CTEs (WITH)** | ✅ Supported | ✅ Supported | ✅ Supported (Oracle 9i+) |
| **Recursive CTEs** | ✅ Supported | ✅ Supported | ✅ `CONNECT BY` / Recursive CTE (11gR2+) |
| **Window functions** | ✅ Supported | ✅ Supported | ✅ Supported (Oracle 9i+) |
| **JSON functions** | Native JSON + `->`/`->>` | `JSON_VALUE()`, `JSON_QUERY()` | ✅ `JSON_VALUE()`, `JSON_QUERY()`, `JSON_TABLE()` (12c+) |
| **Array types** | ✅ Native array | ❌ Not supported | ❌ Not supported (VARRAYs exist but rarely used in ORM) |
| **`IS DISTINCT FROM`** | ✅ Supported | ❌ Not supported | ❌ Not supported (use `DECODE` or `NVL2`) |
| **`FILTER (WHERE ...)`** | ✅ Supported | ❌ Not supported | ❌ Not supported (use `CASE WHEN`) |
| **FULL OUTER JOIN** | ✅ Supported | ✅ Supported | ✅ Supported |
| **NATURAL JOIN** | ✅ Supported | ❌ Not supported | ✅ Supported |
| **LATERAL / APPLY** | ✅ `LATERAL` | ✅ `CROSS/OUTER APPLY` | ✅ **`LATERAL`** (12c+) |
| **String concat** | `\|\|` operator | `+` / `CONCAT()` | ✅ **`\|\|` operator** / `CONCAT()` |
| **Date functions** | `NOW()`, `DATE_TRUNC()` | `GETDATE()`, `DATEADD()` | ✅ **`SYSDATE`**, `SYSTIMESTAMP`, `TRUNC()`, `ADD_MONTHS()` |
| **Identity / Auto-increment** | `GENERATED AS IDENTITY` / `SERIAL` | `IDENTITY(1,1)` | ✅ **`GENERATED AS IDENTITY`** (12c+) / **SEQUENCE + TRIGGER** (legacy) |
| **LIMIT on UPDATE/DELETE** | ❌ Not supported | ⚠️ Via `TOP()` only | ❌ **Not supported** |
| **`ROW_NUMBER()` pagination** | ❌ Not needed | ❌ Not needed | ✅ **Required for pre-12c** (`ROW_NUMBER() OVER(...)`) |
| **`ROWNUM` pseudo-column** | ❌ Not supported | ❌ Not supported | ✅ **`ROWNUM`** (pre-12c row limit) |
| **`CONNECT BY`** | ❌ Not supported | ✅ (recursive CTE) | ✅ **`CONNECT BY`** hierarchical queries |
| **`PIVOT` / `UNPIVOT`** | `crosstab()` | `PIVOT` | ✅ `PIVOT` / `UNPIVOT` (11g+) |
| **`MATCH_RECOGNIZE`** | ❌ Not supported | ❌ Not supported | ✅ `MATCH_RECOGNIZE` (12c+) |
| **Flashback queries** | ❌ Not supported | ❌ `SYSTEM_TIME` | ✅ `AS OF TIMESTAMP` / `AS OF SCN` |
| **`ROWID`** | `ctid` | `%%physloc%%` | ✅ `ROWID` (physical row identifier) |
| **`DECODE`** | ❌ Not supported | ❌ Not supported | ✅ `DECODE(expr, search, result, ...)` |
| **`NVL` / `NVL2`** | `COALESCE` | `ISNULL` / `COALESCE` | ✅ `NVL(expr, default)` / `NVL2(expr, notnull, null)` |
| **`MERGE` syntax** | `MERGE INTO ... USING ... ON ...` | `MERGE ... USING ... ON ...` | ✅ **Same basic structure** (`MERGE INTO` required) |
| **`DEFAULT VALUES`** | ✅ Supported | ✅ Supported | ❌ **Not supported** (use explicit column list) |
| **Flashback / AS OF** | ❌ Not supported | ❌ Not supported | ✅ `SELECT ... AS OF TIMESTAMP(...)` |
| **`WITH FUNCTION`** | ❌ Not supported | ❌ Not supported | ✅ PL/SQL function in WITH clause (12c+) |
| **`SAMPLE` clause** | `TABLESAMPLE` | `TABLESAMPLE` | ✅ `SAMPLE(percentage)` |

### 1.2 Oracle RETURNING Strategy

Oracle's `RETURNING ... INTO` clause is fundamentally different from PostgreSQL's `RETURNING` and MSSQL's `OUTPUT`:

| Dialect | Mechanism | Returns Rowset? | Usage |
|---------|-----------|----------------|-------|
| **PostgreSQL** | `RETURNING *` | ✅ Yes — appended to statement | `INSERT INTO t VALUES(...) RETURNING id` |
| **MSSQL** | `OUTPUT INSERTED.*` | ✅ Yes — appended to statement | `INSERT INTO t OUTPUT INSERTED.id VALUES(...)` |
| **Oracle** | `RETURNING ... INTO` | ❌ No — uses **OUT parameters** | `INSERT INTO t VALUES(...) RETURNING id INTO :outParam` |

**Strategy for Oracle:**

1. **Simple scalar returns** (e.g., last inserted ID): Use `RETURNING ... INTO` with output parameters via `OracleCommand` — handled in the `OracleDbClient` executor layer.
2. **Full row returning**: For SELECT-like returning (e.g., `INSERT ... RETURNING *`), use a two-step approach:
   - Execute the DML, then immediately query the affected rows using `ROWID` or primary key.
   - Or use PL/SQL blocks with `BULK COLLECT INTO`.
3. **`RETURNING` via PL/SQL block**: Wrap the DML in a `BEGIN ... END;` block with `RETURNING ... BULK COLLECT INTO` for multi-row returns.

The Oracle dialect should expose a `Returning()` method on INSERT/UPDATE/DELETE queries that captures the `RETURNING ... INTO` clause and handles parameter binding at the executor level, similar to how MSSQL uses `OUTPUT`.

### 1.3 Oracle .NET Provider

| Provider | NuGet Package | Notes |
|----------|--------------|-------|
| **Oracle.ManagedDataAccess.Core** | `Oracle.ManagedDataAccess.Core` | ✅ **Recommended.** Modern, cross-platform, actively maintained by Oracle. Uses `:name` parameter prefix. |
| Oracle.DataAccess (ODP.NET unmanaged) | `Oracle.DataAccess` | Legacy, Windows-only. Avoid. |

**Recommendation:** Use **Oracle.ManagedDataAccess.Core** — the cross-platform managed provider from Oracle.

**Parameter naming:** Oracle uses `:name` prefix (e.g., `:p0`, `:p1`). The existing `SqlBuilder` parameter naming strategy needs to be updated for the `OracleSqlDialectImpl` to use colon prefix.

NuGet dependency to add to [`Drizzle4Dotnet.csproj`](Drizzle4Dotnet/Drizzle4Dotnet.csproj):
```xml
<PackageReference Include="Oracle.ManagedDataAccess.Core" Version="23.7.0" />
```

### 1.4 Target File Structure

```
Drizzle4Dotnet/
├── src/
│   ├── Core/
│   │   ├── Shared/
│   │   │   ├── ISqlDialect.cs              # Minor additions for Oracle-specific features
│   │   │   └── SqlDialectDefaults.cs        # May need new default methods
│   │   └── ...
│   ├── Dialect/
│   │   ├── PgSqlSqlDialectImpl.cs          # Existing
│   │   ├── MySqlSqlDialectImpl.cs          # Existing
│   │   ├── SqliteSqlDialectImpl.cs         # Existing
│   │   ├── MssqlSqlDialectImpl.cs          # Existing
│   │   └── OracleSqlDialectImpl.cs         # NEW — Oracle dialect
│   └── Oracle/                              # NEW — Oracle-specific namespace
│       ├── OracleDbClient.cs                # DbClient + factory methods
│       ├── OracleQueryBuilder.cs            # Offline query builder
│       ├── OracleQueryBuilderExtensions.cs  # Convenience overloads
│       ├── OracleTable.cs                   # Table/alias interfaces
│       ├── OracleColumn.cs                  # Column types
│       ├── OracleStatics.cs                 # Static helpers (SEQUENCE, DUAL, etc.)
│       ├── OracleFunctions.cs              # Oracle-specific functions
│       ├── OracleFunctions.DateTime.cs     # SYSDATE, SYSTIMESTAMP, ADD_MONTHS(), TRUNC(), etc.
│       ├── OracleFunctions.String.cs       # INSTR(), SUBSTR(), LENGTH(), CONCAT(), etc.
│       ├── OracleFunctions.Numeric.cs      # ROUND(), TRUNC(), MOD(), POWER(), etc.
│       ├── OracleFunctions.Json.cs         # JSON_VALUE(), JSON_QUERY(), JSON_TABLE()
│       ├── OracleFunctions.Info.cs         # USER, UID, ROWID, etc.
│       ├── OracleFunctions.Analytic.cs     # Oracle analytic functions (RANK, DENSE_RANK, LAG, LEAD, etc.)
│       ├── OracleOperators.cs              # Oracle-specific operators (|| concat, etc.)
│       ├── OracleOperators.Comparison.cs   # IS NULL, IS OF TYPE, etc.
│       ├── Nodes/
│       │   ├── OracleSequenceNode.cs       # sequence.NEXTVAL / sequence.CURRVAL
│       │   ├── OracleRowIdNode.cs          # ROWID pseudo-column
│       │   ├── OracleSampleNode.cs         # SAMPLE(percentage) clause
│       │   ├── OracleFlashbackNode.cs      # AS OF TIMESTAMP / AS OF SCN
│       │   ├── OracleHierarchicalNode.cs   # CONNECT BY, START WITH
│       │   ├── OraclePivotNode.cs          # PIVOT / UNPIVOT
│       │   └── OracleReturningNode.cs      # RETURNING ... INTO clause node
│       └── Query/
│           ├── OracleSelectQuery.cs         # SELECT with OFFSET/FETCH, ROWNUM, SAMPLE, CONNECT BY, PIVOT
│           ├── OracleInsertQuery.cs         # INSERT with RETURNING, INSERT ALL
│           ├── OracleUpdateQuery.cs         # UPDATE with RETURNING
│           ├── OracleDeleteQuery.cs         # DELETE with RETURNING
│           └── OracleMergeQuery.cs          # MERGE (upsert)

Test/
├── Select/
│   └── OracleSelectTests.cs                # NEW — Oracle SELECT tests
├── Insert/
│   └── OracleInsertTests.cs                # NEW — Oracle INSERT tests
├── Update/
│   └── OracleUpdateTests.cs                # NEW — Oracle UPDATE tests
├── Delete/
│   └── OracleDeleteTests.cs                # NEW — Oracle DELETE tests
├── Merge/
│   └── OracleMergeTests.cs                 # NEW — Oracle MERGE tests
└── Migration/
    └── OracleMigrationTests.cs             # NEW — Oracle DDL/migration tests

SharedDemo/
└── Oracle/
    ├── Schema.cs                            # Oracle-specific schema definitions
    ├── DbDto.cs                             # Oracle DTOs
    ├── schema.sql                           # DDL for demo setup
    └── data.sql                             # Sample data
```

---

## 2. Phase O1: Oracle Dialect (Foundation)

### 2.1 Create `OracleSqlDialectImpl.cs`

**File:** [`Drizzle4Dotnet/src/Dialect/OracleSqlDialectImpl.cs`](Drizzle4Dotnet/src/Dialect/OracleSqlDialectImpl.cs) — **New file**

Implements [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) with Oracle PL/SQL syntax patterns.

```csharp
namespace Drizzle4Dotnet.Dialect;

public class OracleSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================
    
    // Oracle uses double quotes: "identifier"
    // Oracle folds unquoted identifiers to UPPERCASE
    public static string BuildIdentifier(string identifier)
        => $"\"{identifier}\"";

    // "schema"."table" or just "table" (default schema is current user/schema)
    public static string BuildTableName(string schemaName, string tableName)
        => string.IsNullOrEmpty(schemaName) 
            ? $"\"{tableName}\"" 
            : $"\"{schemaName}\".\"{tableName}\"";

    // "table"."column"
    public static string BuildColumnName(string refName, string columnName)
        => $"\"{refName}\".\"{columnName}\"";

    // Parameters: :paramName (Oracle uses colon prefix)
    // Oracle.ManagedDataAccess.Core requires :named parameters
    public static string BuildParameterName(string parameterName)
        => $":{parameterName}";
        
    public static string BuildParameterName(int parameterIndex)
        => $":p{parameterIndex}";
    
    // ======================================================================
    // Limit / Offset — Oracle 12c+ uses OFFSET/FETCH
    // ======================================================================
    
    // Oracle 12c+: ORDER BY ... OFFSET ? ROWS FETCH NEXT ? ROWS ONLY
    // Pre-12c fallback: ROWNUM (handled in SelectQuery override)
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
    {
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
            "Oracle does not support LIMIT/OFFSET for UPDATE/DELETE statements.");
    
    // ======================================================================
    // Feature Flags
    // ======================================================================
    
    public static bool SupportsReturning => true;       // Via RETURNING ... INTO (different from PG)
    public static bool SupportsArrays => false;          // Not supported (VARRAYs are different)
    public static bool SupportsJson => true;             // Oracle 12c+ (JSON_VALUE, JSON_QUERY, JSON_TABLE)
    public static bool SupportsWindowFunctions => true;  // Oracle 9i+
    public static bool SupportsCte => true;              // Oracle 9i+
    public static bool SupportsRecursiveCte => true;     // Oracle 11gR2+
    public static bool SupportsDeleteUsing => false;     // Not supported (use subquery in WHERE)
    public static bool SupportsIsDistinctFrom => false;  // Not supported (use DECODE/NVL2)
    public static bool SupportsFilteredAggregates => false; // Not supported (use CASE WHEN)
    public static bool SupportsFullOuterJoin => true;    // Supported
    public static bool SupportsNaturalJoin => true;      // Supported
    public static bool SupportsLateralJoin => true;      // Oracle 12c+ (LATERAL)
    public static bool SupportsApplyJoin => false;       // Oracle uses LATERAL, not APPLY
    public static bool UseLimitPairMode => false;
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    // Oracle uses '' escaping like standard SQL
    // Oracle also supports alternative quoting: q'[text]'
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
}
```

### 2.2 Dialect Interface Considerations

The current [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs) already has the necessary abstraction points. Oracle-specific features like `CONNECT BY`, `PIVOT`, flashback queries, and `SAMPLE` should be handled in the query builder subclasses rather than adding new members to the interface, following the pattern used by MySQL for JOIN in UPDATE/DELETE and by MSSQL for TOP/OUTPUT.

**Potential additions to `ISqlDialect`:**
- `SupportsConnectBy` (`bool`) — Oracle `CONNECT BY` hierarchical queries
- `SupportsPivot` (`bool`) — Oracle `PIVOT`/`UNPIVOT`
- `SupportsFlashbackQuery` (`bool`) — Oracle `AS OF TIMESTAMP`/`SCN`
- `SupportsSample` (`bool`) — Oracle `SAMPLE` clause
- `SupportsMerge` (`bool`) — Oracle `MERGE` (9i+)
- `SupportsInsertAll` (`bool`) — Oracle `INSERT ALL`

However, to minimize changes to existing dialects, these can be added with `=> false` defaults in [`SqlDialectDefaults`](Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs) with only Oracle overriding them to `true`.

### 2.3 Register the Dialect

- Add new feature flag defaults to [`SqlDialectDefaults`](Drizzle4Dotnet/src/Core/Shared/SqlDialectDefaults.cs)
- Add new feature flags to [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs)
- Update existing dialect implementations to provide appropriate values for new flags

---

## 3. Phase O2: Oracle Types & Interfaces

### 3.1 Create [`OracleTable.cs`](Drizzle4Dotnet/src/Oracle/OracleTable.cs)

```csharp
namespace Drizzle4Dotnet.Oracle;

public interface IOracleGenericTable : IGenericTable<OracleSqlDialectImpl> { }
public interface IOracleTable : ITable<OracleSqlDialectImpl> { }
public interface IOracleCteTable : ICteTable<OracleSqlDialectImpl> { }
public interface IOracleVirtualTable : IVirtualTable<OracleSqlDialectImpl> { }
public interface IOracleDbTable : IDbTable<OracleSqlDialectImpl> { }
public interface IOracleTableAlias : ITableAlias<OracleSqlDialectImpl> { }
```

### 3.2 Create [`OracleColumn.cs`](Drizzle4Dotnet/src/Oracle/OracleColumn.cs)

```csharp
namespace Drizzle4Dotnet.Oracle;

public class OracleColumn<T, TTable> : DbColumn<T, TTable, OracleSqlDialectImpl>
    where TTable : ITable<OracleSqlDialectImpl>
{
    public OracleColumn(string columnName) : base(columnName) { }
}

public class OracleVirtualColumn<T> : VirtualColumn<T, OracleSqlDialectImpl>
{
    public OracleVirtualColumn(string alias, string columnName) 
        : base(alias, columnName) { }
}
```

---

## 4. Phase O3: Oracle Functions

### 4.1 Create [`OracleFunctions.cs`](Drizzle4Dotnet/src/Oracle/OracleFunctions.cs)

Entry point class:
```csharp
namespace Drizzle4Dotnet.Oracle;

public static partial class OracleFunctions
{
    // Aggregates (shared with core)
    // DateTime, String, Numeric, Json, Analytic in partial files
}
```

### 4.2 `OracleFunctions.DateTime.cs`

| Function | Oracle SQL | Notes |
|----------|-----------|-------|
| `CurrentDate()` | `CURRENT_DATE` | Current date in session time zone |
| `CurrentTimestamp()` | `CURRENT_TIMESTAMP` | Current timestamp in session time zone |
| `Sysdate()` | `SYSDATE` | Current date/time (database server) |
| `Systimestamp()` | `SYSTIMESTAMP` | Current timestamp with time zone |
| `AddMonths(date, n)` | `ADD_MONTHS(date, n)` | Add months to date |
| `MonthsBetween(d1, d2)` | `MONTHS_BETWEEN(d1, d2)` | Months between two dates |
| `Trunc(date, fmt)` | `TRUNC(date, fmt)` | Truncate date |
| `Extract(YEAR/MONTH/DAY FROM date)` | `EXTRACT(... FROM date)` | Extract date parts |
| `ToDate(str, fmt)` | `TO_DATE(str, fmt)` | String to date |
| `ToChar(date, fmt)` | `TO_CHAR(date, fmt)` | Date to string |
| `NextDay(date, dayOfWeek)` | `NEXT_DAY(date, 'MONDAY')` | Next occurrence of day |
| `LastDay(date)` | `LAST_DAY(date)` | Last day of month |
| `NewTime(date, tz1, tz2)` | `NEW_TIME(date, 'AST', 'PST')` | Time zone conversion |
| `NumToDsInterval(n, unit)` | `NUMTODSINTERVAL(n, 'DAY')` | Number to day-second interval |
| `NumToYmInterval(n, unit)` | `NUMTOYMINTERVAL(n, 'MONTH')` | Number to year-month interval |
| `FromTz(timestamp, tz)` | `FROM_TZ(timestamp, 'UTC')` | Timestamp with time zone |
| `AtTimeZone(timestamp, tz)` | `timestamp AT TIME ZONE 'UTC'` | Time zone conversion |

### 4.3 `OracleFunctions.String.cs`

| Function | Oracle SQL | Notes |
|----------|-----------|-------|
| `Length(str)` | `LENGTH(str)` | String length |
| `Substr(str, pos, len)` | `SUBSTR(str, pos, len)` | Substring (1-based) |
| `Instr(str, substr, pos, occ)` | `INSTR(str, substr, pos, occ)` | Find substring position |
| `Concat(str1, str2)` | `CONCAT(str1, str2)` or `\|\|` | Concatenation |
| `Upper(str)` | `UPPER(str)` | Uppercase |
| `Lower(str)` | `LOWER(str)` | Lowercase |
| `InitCap(str)` | `INITCAP(str)` | Capitalize first letter of each word |
| `Trim(str)` | `TRIM(str)` | Trim whitespace |
| `LTrim(str, chars)` | `LTRIM(str, chars)` | Left trim |
| `RTrim(str, chars)` | `RTRIM(str, chars)` | Right trim |
| `LPad(str, len, pad)` | `LPAD(str, len, pad)` | Left pad |
| `RPad(str, len, pad)` | `RPAD(str, len, pad)` | Right pad |
| `Replace(str, from, to)` | `REPLACE(str, from, to)` | Replace substring |
| `Translate(str, from, to)` | `TRANSLATE(str, from, to)` | Character-level translation |
| `Decode(expr, search, result, ...)` | `DECODE(expr, search, result, ..., default)` | Oracle-specific conditional |
| `Nvl(expr, default)` | `NVL(expr, default)` | Null replacement |
| `Nvl2(expr, notnull, nullval)` | `NVL2(expr, notnull, nullval)` | Null-aware conditional |
| `RegexpLike(str, pattern)` | `REGEXP_LIKE(str, pattern)` | Regex matching |
| `RegexpSubstr(str, pattern)` | `REGEXP_SUBSTR(str, pattern)` | Regex substring |
| `RegexpReplace(str, pattern, repl)` | `REGEXP_REPLACE(str, pattern, repl)` | Regex replace |
| `RegexpInstr(str, pattern)` | `REGEXP_INSTR(str, pattern)` | Regex position |
| `Listagg(expr, delimiter)` | `LISTAGG(expr, delimiter)` | String aggregation (11g+) |

### 4.4 `OracleFunctions.Numeric.cs`

| Function | Oracle SQL | Notes |
|----------|-----------|-------|
| `Abs(n)` | `ABS(n)` | Absolute value |
| `Ceil(n)` | `CEIL(n)` | Ceiling |
| `Floor(n)` | `FLOOR(n)` | Floor |
| `Round(n, d)` | `ROUND(n, d)` | Round |
| `Trunc(n, d)` | `TRUNC(n, d)` | Truncate |
| `Mod(a, b)` | `MOD(a, b)` | Modulo |
| `Power(a, b)` | `POWER(a, b)` | Power |
| `Sqrt(n)` | `SQRT(n)` | Square root |
| `Exp(n)` | `EXP(n)` | Exponential |
| `Ln(n)` | `LN(n)` | Natural log |
| `Log(base, n)` | `LOG(base, n)` | Logarithm |
| `Sin(n)` | `SIN(n)` | Sine |
| `Cos(n)` | `COS(n)` | Cosine |
| `Tan(n)` | `TAN(n)` | Tangent |
| `Sign(n)` | `SIGN(n)` | Sign (-1, 0, 1) |
| `Greatest(v1, v2, ...)` | `GREATEST(v1, v2, ...)` | Greatest value |
| `Least(v1, v2, ...)` | `LEAST(v1, v2, ...)` | Least value |
| `WidthBucket(expr, min, max, buckets)` | `WIDTH_BUCKET(expr, min, max, buckets)` | Histogram buckets |
| `BinToNum(bits)` | `BIN_TO_NUM(bits)` | Binary to number |

### 4.5 `OracleFunctions.Json.cs`

| Function | Oracle SQL | Notes |
|----------|-----------|-------|
| `JsonValue(expr, path)` | `JSON_VALUE(expr, '$.path')` | Extract scalar JSON value |
| `JsonQuery(expr, path)` | `JSON_QUERY(expr, '$.path')` | Extract JSON object/array |
| `JsonExists(expr, path)` | `JSON_EXISTS(expr, '$.path')` | Check JSON path existence |
| `JsonTable(expr, path COLUMNS ...)` | `JSON_TABLE(expr, '$.path' COLUMNS (...))` | JSON to relational |
| `JsonObject(key VALUE val, ...)` | `JSON_OBJECT(key VALUE val)` | Create JSON object |
| `JsonArray(val1, val2, ...)` | `JSON_ARRAY(val1, val2)` | Create JSON array |
| `JsonArrayagg(expr)` | `JSON_ARRAYAGG(expr)` | Aggregate to JSON array |
| `JsonObjectagg(key VALUE val)` | `JSON_OBJECTAGG(key VALUE val)` | Aggregate to JSON object |
| `JsonDataguide(expr)` | `JSON_DATAGUIDE(expr)` | JSON schema discovery |
| `JsonMergepatch(target, patch)` | `JSON_MERGEPATCH(target, patch)` | JSON merge (18c+) |
| `JsonTransform(expr, path, value)` | `JSON_TRANSFORM(expr, SET '$.path' = value)` | JSON transformation (19c+) |

### 4.6 `OracleFunctions.Analytic.cs`

| Function | Oracle SQL | Notes |
|----------|-----------|-------|
| `RowNumber()` | `ROW_NUMBER() OVER (...)` | Row number |
| `Rank()` | `RANK() OVER (...)` | Rank with gaps |
| `DenseRank()` | `DENSE_RANK() OVER (...)` | Rank without gaps |
| `Lag(expr, offset, default)` | `LAG(expr, offset, default) OVER (...)` | Previous row value |
| `Lead(expr, offset, default)` | `LEAD(expr, offset, default) OVER (...)` | Next row value |
| `FirstValue(expr)` | `FIRST_VALUE(expr) OVER (...)` | First value in window |
| `LastValue(expr)` | `LAST_VALUE(expr) OVER (...)` | Last value in window |
| `NthValue(expr, n)` | `NTH_VALUE(expr, n) OVER (...)` | Nth value in window |
| `NTile(n)` | `NTILE(n) OVER (...)` | Bucket rows into n tiles |
| `CumeDist()` | `CUME_DIST() OVER (...)` | Cumulative distribution |
| `PercentRank()` | `PERCENT_RANK() OVER (...)` | Percent rank |
| `PercentileCont(p)` | `PERCENTILE_CONT(p) WITHIN GROUP (ORDER BY ...)` | Continuous percentile |
| `PercentileDisc(p)` | `PERCENTILE_DISC(p) WITHIN GROUP (ORDER BY ...)` | Discrete percentile |
| `Median(expr)` | `MEDIAN(expr) OVER (...)` | Median |
| `RatioToReport(expr)` | `RATIO_TO_REPORT(expr) OVER (...)` | Ratio of value to total |
| `Stddev(expr)` | `STDDEV(expr) OVER (...)` | Standard deviation |
| `Variance(expr)` | `VARIANCE(expr) OVER (...)` | Variance |

### 4.7 `OracleFunctions.Info.cs`

| Function | Oracle SQL | Notes |
|----------|-----------|-------|
| `User()` | `USER` | Current database user |
| `Uid()` | `UID` | User ID |
| `RowId()` | `ROWID` | Row physical identifier |
| `RowNum()` | `ROWNUM` | Row number (before ordering) |
| `CurrentSchema()` | `SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')` | Current schema |
| `DbName()` | `SYS_CONTEXT('USERENV', 'DB_NAME')` | Database name |
| `InstanceName()` | `SYS_CONTEXT('USERENV', 'INSTANCE_NAME')` | Instance name |
| `SessionId()` | `SYS_CONTEXT('USERENV', 'SID')` | Session ID |
| `ClientInfo()` | `SYS_CONTEXT('USERENV', 'CLIENT_INFO')` | Client info |
| `Host()` | `SYS_CONTEXT('USERENV', 'HOST')` | Client host |
| `IpAddress()` | `SYS_CONTEXT('USERENV', 'IP_ADDRESS')` | Client IP |
| `ServerHost()` | `SYS_CONTEXT('USERENV', 'SERVER_HOST')` | Server host |
| `NlsLanguage()` | `SYS_CONTEXT('USERENV', 'NLS_LANGUAGE')` | NLS language |
| `NlsTerritory()` | `SYS_CONTEXT('USERENV', 'NLS_TERRITORY')` | NLS territory |

---

## 5. Phase O4: Oracle Operators & Nodes

### 5.1 Create [`OracleOperators.cs`](Drizzle4Dotnet/src/Oracle/OracleOperators.cs)

```csharp
namespace Drizzle4Dotnet.Oracle;

public static class OracleOperators
{
    // ======================================================================
    // String Concatenation: Oracle uses || operator
    // ======================================================================
    
    /// <summary>String concatenation: a || b</summary>
    public static ConcatOperator Concat(IGenericSql a, IGenericSql b) => new(a, b);
    
    // ======================================================================
    // Equality: Oracle uses IS NULL for null-safe comparison
    // ======================================================================
    
    /// <summary>Null-safe equality (IS NOT DISTINCT FROM equivalent via DECODE)</summary>
    public static IGenericSql IsNotDistinctFrom(IGenericSql a, IGenericSql b)
        => new RawSql($"DECODE({a}, {b}, 1, 0) = 1");
    
    /// <summary>Null-safe inequality</summary>
    public static IGenericSql IsDistinctFrom(IGenericSql a, IGenericSql b)
        => new RawSql($"DECODE({a}, {b}, 1, 0) = 0");
    
    // ======================================================================
    // Conditional
    // ======================================================================
    
    /// <summary>DECODE(expr, search, result, ..., default)</summary>
    public static DecodeNode Decode(IGenericSql expr, params (IGenericSql search, IGenericSql result)[] pairs, IGenericSql? defaultVal = null)
        => new(expr, pairs, defaultVal);
    
    /// <summary>NVL(expr, default) — return default if expr is null</summary>
    public static NvlNode Nvl(IGenericSql expr, IGenericSql defaultVal) => new(expr, defaultVal);
    
    /// <summary>NVL2(expr, notnull, nullval) — return notnull if expr is not null, else nullval</summary>
    public static Nvl2Node Nvl2(IGenericSql expr, IGenericSql notnull, IGenericSql nullval) => new(expr, notnull, nullval);
}
```

### 5.2 Create Nodes

#### `OracleSequenceNode.cs` — Sequence reference
```
Renders: "schema"."sequence".NEXTVAL or "schema"."sequence".CURRVAL
```

#### `OracleRowIdNode.cs` — ROWID pseudo-column
```
Renders: ROWID or table.ROWID
```

#### `OracleSampleNode.cs` — SAMPLE clause
```
Renders: SAMPLE (percentage) or SAMPLE BLOCK (percentage)
```

#### `OracleFlashbackNode.cs` — Flashback queries
```
Renders: AS OF TIMESTAMP (timestamp) or AS OF SCN (scn)
VERSIONS BETWEEN ... (for flashback version query)
```

#### `OracleHierarchicalNode.cs` — Hierarchical queries
```
Renders: CONNECT BY [NOCYCLE] condition
         START WITH condition
         ORDER SIBLINGS BY ...
```

#### `OraclePivotNode.cs` — PIVOT/UNPIVOT
```
Renders: PIVOT (agg_func(column) FOR pivot_column IN (value1 AS alias1, ...))
         UNPIVOT (value_column FOR name_column IN (col1 AS 'val1', ...))
```

#### `OracleReturningNode.cs` — RETURNING ... INTO clause
```
Renders: RETURNING col1, col2 INTO :outP0, :outP1
Used in INSERT/UPDATE/DELETE statements with Oracle's RETURNING clause.
The output parameters are bound at execution time in OracleDbClient.
```

---

## 6. Phase O5: Oracle DML — SELECT

### 6.1 Create [`OracleSelectQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleSelectQuery.cs)

**File:** [`Drizzle4Dotnet/src/Oracle/Query/OracleSelectQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleSelectQuery.cs) — **New file**

```csharp
public class OracleSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, OracleSqlDialectImpl, TVirtualTable, OracleSelectQuery<TReturn, TVirtualTable>>,
    IFullOuterJoin<OracleSelectQuery<TReturn, TVirtualTable>, OracleSqlDialectImpl>,
    INaturalJoin<OracleSelectQuery<TReturn, TVirtualTable>, OracleSqlDialectImpl>
    where TVirtualTable : IVirtualTable<OracleSqlDialectImpl>
```

**Oracle-specific features to implement:**

1. **`FROM DUAL`**: When no `FROM` table is specified, automatically append `FROM DUAL` (Oracle requires this for scalar SELECTs).

2. **Pagination (Oracle 12c+):** Use `OFFSET ? ROWS FETCH NEXT ? ROWS ONLY` — already handled by the dialect's `BuildLimitOffset`.

3. **`SAMPLE` clause:** Add `Sample(double percentage)` and `SampleBlock(double percentage)` methods.

4. **Flashback queries:** Add `AsOfTimestamp(DateTime timestamp)` and `AsOfScn(long scn)` methods.

5. **Hierarchical queries (CONNECT BY):** Add `ConnectBy(IGenericSql condition)`, `StartWith(IGenericSql condition)`, `OrderSiblingsBy(...)` methods.

6. **`PIVOT`/`UNPIVOT`:** Add `Pivot(...)` and `Unpivot(...)` methods.

7. **`FOR UPDATE`** with optional `WAIT`/`NOWAIT`:
   - Oracle syntax: `FOR UPDATE [OF table.col] [WAIT n | NOWAIT | SKIP LOCKED]`
   - Add `ForUpdate()`, `ForUpdateNoWait()`, `ForUpdateWait(int seconds)`, `ForUpdateSkipLocked()`

8. **`WITH FUNCTION`** (Oracle 12c+): Allow inline PL/SQL function definitions in WITH clause.

9. **`LATERAL` join support** (Oracle 12c+):
   - Inner/Left/Cross lateral via `LATERAL(...)` subquery in `FROM` clause.
   - Implement via `ILateralJoin<TQuery, TDialect>` interface.

**BuildSql override pattern:**
```csharp
public override void BuildSql(ISqlBuilder sqlBuilder)
{
    ValidateQuery();

    // WITH FUNCTION (Oracle 12c+)
    BuildWithFunction(sqlBuilder);
    
    // WITH clause
    SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

    sqlBuilder.Append("SELECT ");
    if (IsDistinct) sqlBuilder.Append("DISTINCT ");
    // SAMPLE clause (after SELECT, before column list)
    _sampleNode?.BuildSql(sqlBuilder);
    SelectedColumns.BuildSql(sqlBuilder);

    // FROM — handle DUAL fallback
    if (FromTable != null)
    {
        sqlBuilder.Append(" FROM ");
        FromTable.BuildRefSql(sqlBuilder);
        // Flashback
        _flashbackNode?.BuildSql(sqlBuilder);
    }
    else if (!HasExplicitFrom())
    {
        sqlBuilder.Append(" FROM DUAL");
    }

    // JOINs
    SqlStatics.BuildSqlJoins(sqlBuilder, Joins);

    // WHERE
    SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

    // CONNECT BY (Oracle hierarchical)
    BuildConnectBy(sqlBuilder);

    // GROUP BY
    SqlStatics.BuildClause(sqlBuilder, " GROUP BY ", ", ", GroupBys);

    // HAVING
    SqlStatics.BuildClause(sqlBuilder, " HAVING ", " AND ", Havings, wrapInParentheses: true);

    // PIVOT/UNPIVOT
    _pivotNode?.BuildSql(sqlBuilder);

    // ORDER BY
    if (OrderBys.Count > 0)
    {
        SqlStatics.BuildSqlOrderBy(sqlBuilder, OrderBys);
    }

    // OFFSET/FETCH (Oracle 12c+)
    OracleSqlDialectImpl.BuildLimitOffset(sqlBuilder, LimitValue, OffsetValue);

    // FOR UPDATE
    BuildForUpdate(sqlBuilder);
}
```

---

## 7. Phase O6: Oracle DML — INSERT

### 7.1 Create [`OracleInsertQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleInsertQuery.cs)

**File:** [`Drizzle4Dotnet/src/Oracle/Query/OracleInsertQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleInsertQuery.cs) — **New file**

```csharp
public class OracleInsertQuery<TTable> : InsertQuery<TTable, OracleSqlDialectImpl, OracleInsertQuery<TTable>>
    where TTable : ITable<OracleSqlDialectImpl>
```

**Oracle-specific features:**

1. **`RETURNING ... INTO` clause** — Oracle's mechanism for returning values from DML:
   ```
   INSERT INTO table (cols) VALUES (vals) RETURNING col1, col2 INTO :outP0, :outP1
   ```
   Unlike PostgreSQL's `RETURNING` which returns a result set, Oracle's `RETURNING ... INTO` binds output parameters. The query builder needs to:
   - Track which columns to return
   - Build the `RETURNING ... INTO` clause with output parameter placeholders
   - The executor (`OracleDbClient`) handles binding output parameters and retrieving values

2. **`INSERT ALL`** — Oracle's multi-table insert:
   ```
   INSERT ALL
     INTO table1 (cols) VALUES (vals)
     INTO table2 (cols) VALUES (vals)
   SELECT * FROM DUAL;
   ```
   - `InsertAll()` method
   - Supports conditional inserts (`WHEN condition THEN`)

3. **No `DEFAULT VALUES` support** — Oracle does not support `INSERT ... DEFAULT VALUES`. Override `BuildDefaultValues` to throw `NotSupportedException` or generate `INSERT INTO table (pk_col) VALUES (DEFAULT)`.

4. **Sequence-based identity** — Support for `sequence.NEXTVAL` as default value:
   ```csharp
   // In OracleStatics:
   public static IGenericSql NextVal(string sequenceName, string? schemaName = null)
       => new OracleSequenceNode(sequenceName, schemaName, isNextVal: true);
   ```

**BuildSql override pattern:**
```csharp
public override void BuildSql(ISqlBuilder sqlBuilder)
{
    ValidateQuery();

    SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

    if (_isInsertAll)
    {
        BuildInsertAll(sqlBuilder);
        return;
    }

    BuildInsertKeywords(sqlBuilder);
    Table.BuildRefSql(sqlBuilder);

    if (UseDefaultValues)
    {
        throw new NotSupportedException(
            "Oracle does not support INSERT ... DEFAULT VALUES. " +
            "Use explicit column values with DEFAULT keyword instead.");
    }
    else if (FromQuery != null)
    {
        sqlBuilder.Append(' ');
        FromQuery.BuildSql(sqlBuilder);
        BuildReturningClause(sqlBuilder);
    }
    else if (ValuesToInsert.Count > 0)
    {
        var allColumns = ValuesToInsert.SelectMany(d => d.Keys).Distinct().ToList();
        SqlStatics.BuildInsertColumnList<OracleSqlDialectImpl>(sqlBuilder, allColumns);
        SqlStatics.BuildInsertRowValues(sqlBuilder, ValuesToInsert, allColumns);
        BuildReturningClause(sqlBuilder);
    }
}
```

---

## 8. Phase O7: Oracle DML — UPDATE

### 8.1 Create [`OracleUpdateQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleUpdateQuery.cs)

**File:** [`Drizzle4Dotnet/src/Oracle/Query/OracleUpdateQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleUpdateQuery.cs) — **New file**

```csharp
public class OracleUpdateQuery<TTable> : UpdateQuery<TTable, OracleSqlDialectImpl, OracleUpdateQuery<TTable>>
    where TTable : ITable<OracleSqlDialectImpl>
```

**Oracle-specific features:**

1. **`RETURNING ... INTO` clause** for UPDATE:
   ```
   UPDATE table SET col = val WHERE condition RETURNING col1 INTO :outP0
   ```
   Supports both `RETURNING ... INTO` (single row) and `RETURNING ... BULK COLLECT INTO` (multiple rows).

2. **Correlated subquery updates** (no `FROM`/`JOIN` in Oracle UPDATE — use subqueries):
   ```
   UPDATE table t SET col = (SELECT val FROM other WHERE other.id = t.id) WHERE EXISTS (...)
   ```
   Oracle does NOT support `UPDATE ... FROM` or `UPDATE ... JOIN` like MSSQL/MySQL. Instead, use subqueries in SET clauses.

3. **`FOR UPDATE` subquery** — Can be used in WHERE clause subqueries.

**BuildSql override pattern:**
```csharp
public override void BuildSql(ISqlBuilder sqlBuilder)
{
    ValidateQuery();

    SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

    sqlBuilder.Append("UPDATE ");
    Table.BuildRefSql(sqlBuilder);

    // SET clause
    SqlStatics.BuildSqlSetClause<OracleSqlDialectImpl>(sqlBuilder, SetValues);

    // WHERE
    SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

    // RETURNING ... INTO
    BuildReturningClause(sqlBuilder);
}
```

---

## 9. Phase O8: Oracle DML — DELETE

### 9.1 Create [`OracleDeleteQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleDeleteQuery.cs)

**File:** [`Drizzle4Dotnet/src/Oracle/Query/OracleDeleteQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleDeleteQuery.cs) — **New file**

```csharp
public class OracleDeleteQuery<TTable> : DeleteQuery<TTable, OracleSqlDialectImpl, OracleDeleteQuery<TTable>>
    where TTable : ITable<OracleSqlDialectImpl>
```

**Oracle-specific features:**

1. **`RETURNING ... INTO` clause** for DELETE:
   ```
   DELETE FROM table WHERE condition RETURNING col1 INTO :outP0
   ```

2. **No `DELETE ... USING`** — Oracle does not support `DELETE ... USING`. Use subqueries in WHERE:
   ```
   DELETE FROM table WHERE col IN (SELECT col FROM other WHERE condition)
   ```
   Override the `SupportsDeleteUsing` flag to `false` (already set in dialect).

**BuildSql override pattern:**
```csharp
public override void BuildSql(ISqlBuilder sqlBuilder)
{
    ValidateQuery();

    SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

    sqlBuilder.Append("DELETE FROM ");
    Table.BuildRefSql(sqlBuilder);

    // WHERE
    SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);

    // RETURNING ... INTO
    BuildReturningClause(sqlBuilder);
}
```

---

## 10. Phase O9: Oracle MERGE (UPSERT)

### 10.1 Create [`OracleMergeQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleMergeQuery.cs)

**File:** [`Drizzle4Dotnet/src/Oracle/Query/OracleMergeQuery.cs`](Drizzle4Dotnet/src/Oracle/Query/OracleMergeQuery.cs) — **New file**

Oracle MERGE syntax (9i+):
```sql
MERGE INTO target_table t
USING source_table s
ON (t.id = s.id)
WHEN MATCHED THEN UPDATE SET t.name = s.name
WHEN NOT MATCHED THEN INSERT (id, name) VALUES (s.id, s.name)
```

**Oracle-specific features:**

1. **`MERGE INTO`** — Oracle requires the `INTO` keyword (unlike MSSQL which makes it optional).

2. **`DELETE` in `WHEN MATCHED`**: Oracle allows `DELETE WHERE` clause within `WHEN MATCHED`:
   ```sql
   WHEN MATCHED THEN UPDATE SET ... DELETE WHERE condition
   ```

3. **`RETURNING ... INTO` with MERGE**: Oracle supports returning values from MERGE.

4. **Multiple `WHEN MATCHED`/`WHEN NOT MATCHED` clauses**: Oracle supports conditional clauses with `AND`:
   ```sql
   WHEN MATCHED AND (t.status = 'ACTIVE') THEN UPDATE ...
   ```

---

## 11. Phase O10: Oracle Schema / Column Types

### 11.1 Create [`OracleColumn.cs`](Drizzle4Dotnet/src/Oracle/Schema/OracleColumn.cs)

**Oracle-specific column types:**

| Oracle Type | .NET Type | Mapping |
|-------------|-----------|---------|
| `NUMBER(p,s)` | `int`, `long`, `decimal`, `double`, `float` | Precision/scale based |
| `VARCHAR2(n)` | `string` | Variable length |
| `NVARCHAR2(n)` | `string` | Unicode variable length |
| `CHAR(n)` | `string` | Fixed length |
| `NCHAR(n)` | `string` | Unicode fixed length |
| `CLOB` | `string` | Character large object |
| `NCLOB` | `string` | Unicode CLOB |
| `BLOB` | `byte[]` | Binary large object |
| `RAW(n)` | `byte[]` | Fixed-length binary |
| `DATE` | `DateTime` | Date + time |
| `TIMESTAMP` | `DateTime` | Timestamp |
| `TIMESTAMP WITH TIME ZONE` | `DateTimeOffset` | Timestamp with TZ |
| `TIMESTAMP WITH LOCAL TZ` | `DateTime` | Timestamp with local TZ |
| `INTERVAL YEAR TO MONTH` | `int` (months) | Date interval |
| `INTERVAL DAY TO SECOND` | `TimeSpan` | Time interval |
| `BINARY_FLOAT` | `float` | 32-bit float |
| `BINARY_DOUBLE` | `double` | 64-bit float |
| `ROWID` | `string` | Physical row ID |
| `UROWID` | `string` | Universal row ID |
| `XMLTYPE` | `string`/`XDocument` | XML data |
| `SDO_GEOMETRY` | (Oracle Spatial) | Spatial data |
| `BOOLEAN` (23c+) | `bool` | Boolean (new in Oracle 23c) |

### 11.2 Column Attributes

```csharp
// Oracle-specific column attributes
public class OracleColumnAttribute : Attribute
{
    public string? ColumnName { get; set; }
    public string? DataType { get; set; }       // e.g., "VARCHAR2(100)"
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public int? Length { get; set; }
    public bool? IsVirtual { get; set; }        // GENERATED ALWAYS AS (...)
    public string? VirtualExpression { get; set; }
    public string? DefaultExpression { get; set; } // DEFAULT value or expression
    public string? SequenceName { get; set; }   // For sequence-based identity
}

// Oracle-specific table attributes
public class OracleTableAttribute : Attribute
{
    public string? TableName { get; set; }
    public string? SchemaName { get; set; }
    public string? Tablespace { get; set; }
}
```

### 11.3 Migration Support

Oracle DDL differences from standard SQL:

| Feature | PostgreSQL | Oracle |
|---------|------------|--------|
| **Auto-increment** | `SERIAL` / `GENERATED AS IDENTITY` | `GENERATED AS IDENTITY` (12c+) / Sequence + Trigger |
| **Primary key** | `PRIMARY KEY` | `PRIMARY KEY` |
| **Foreign key** | `REFERENCES table(col)` | `REFERENCES table(col)` |
| **Unique constraint** | `UNIQUE` | `UNIQUE` |
| **Not null** | `NOT NULL` | `NOT NULL` |
| **Default** | `DEFAULT value` | `DEFAULT value` |
| **Check** | `CHECK (condition)` | `CHECK (condition)` |
| **Index** | `CREATE INDEX ... ON ...` | `CREATE INDEX ... ON ...` |
| **Sequence** | `CREATE SEQUENCE` | `CREATE SEQUENCE` |
| **Comment** | `COMMENT ON COLUMN table.col IS 'text'` | `COMMENT ON COLUMN "table"."col" IS 'text'` |
| **Alter column type** | `ALTER TABLE t ALTER COLUMN c TYPE newtype` | `ALTER TABLE t MODIFY (c newtype)` |
| **Rename column** | `ALTER TABLE t RENAME COLUMN c TO new` | `ALTER TABLE t RENAME COLUMN c TO new` |
| **Drop column** | `ALTER TABLE t DROP COLUMN c` | `ALTER TABLE t DROP (c)` |
| **Add column** | `ALTER TABLE t ADD COLUMN c type` | `ALTER TABLE t ADD (c type)` |

---

## 12. Phase O11: Oracle Connection & Execution

### 12.1 Create [`OracleDbClient.cs`](Drizzle4Dotnet/src/Oracle/OracleDbClient.cs)

**File:** [`Drizzle4Dotnet/src/Oracle/OracleDbClient.cs`](Drizzle4Dotnet/src/Oracle/OracleDbClient.cs) — **New file**

```csharp
public class OracleDbClient : DbClientWithTransaction<OracleDbClient, OracleSqlDialectImpl>
{
    public OracleDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public OracleSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, OracleSqlDialectImpl, TVirtualTable> selectedColumns)
        where TVirtualTable : IVirtualTable<OracleSqlDialectImpl> { ... }

    public OracleSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, OracleSqlDialectImpl, TVirtualTable> selectedColumns)
        where TVirtualTable : IVirtualTable<OracleSqlDialectImpl> { ... }

    public OracleInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl> { ... }

    public OracleUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl> { ... }

    public OracleDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl> { ... }

    public OracleMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl> { ... }

    protected override OracleDbClient CreateInstance(DbConnection conn, DbTransaction? transaction) { ... }
}
```

### 12.2 Create [`OracleQueryBuilder.cs`](Drizzle4Dotnet/src/Oracle/OracleQueryBuilder.cs)

```csharp
public class OracleQueryBuilder
{
    // SQL-only builder — no executor needed
    public OracleSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(...) { ... }
    public OracleInsertQuery<TTable> Insert<TTable>(TTable table) { ... }
    public OracleUpdateQuery<TTable> Update<TTable>(TTable table) { ... }
    public OracleDeleteQuery<TTable> Delete<TTable>(TTable table) { ... }
    public OracleMergeQuery<TTable> Merge<TTable>(TTable table) { ... }
}
```

### 12.3 Create [`OracleQueryBuilderExtensions.cs`](Drizzle4Dotnet/src/Oracle/OracleQueryBuilderExtensions.cs)

Convenience overloads for `Select`/`SelectDistinct` with 1-8 typed columns, following the pattern of existing dialect extension files.

### 12.4 RETURNING Executor Support

The [`OracleDbClient`](Drizzle4Dotnet/src/Oracle/OracleDbClient.cs) needs special handling for `RETURNING ... INTO`:

1. Override `ExecuteAsync` to detect `RETURNING ... INTO` clauses in the SQL.
2. When a `RETURNING` clause is present:
   - Parse the output parameter placeholders (`:outP0`, `:outP1`, etc.)
   - Add `OracleParameter` objects with `Direction = ParameterDirection.Output` for each
   - Execute the command
   - Read the output parameter values
   - Return them to the caller
3. For `BULK COLLECT INTO` (multi-row returning), use `OracleDataAdapter` or array binding.

**Implementation approach:**
```csharp
// In OracleDbClient:
protected override async Task<DbDataReader> ExecuteReaderAsync(IGenericSql query)
{
    // For queries with RETURNING INTO, wrap in PL/SQL block
    // For regular queries, use standard execution
}

// Helper to build PL/SQL block for RETURNING:
// BEGIN
//   INSERT INTO table (cols) VALUES (vals) RETURNING col1 INTO :outP0;
//   :outP0 := :outP0;  -- no-op to make output binding work
// END;
```

---

## 13. Phase O12: Source Generator Oracle Support

### 13.1 Update [`TableGenerator.cs`](SourceGenerators/SourceGenerators/TableGenerator.cs)

- Add Oracle dialect support: generate `IOracleTable` implementations.
- Generate `OracleColumn<T, TTable>` typed properties.

### 13.2 Update [`DbSelectGenerator.cs`](SourceGenerators/SourceGenerators/DbSelectGenerator.cs)

- Generate `OracleSelectQuery` return types for Oracle dialect.
- Generate `IOracleVirtualTable` references.

### 13.3 Update [`MigrationSchemaGenerator.cs`](SourceGenerators/SourceGenerators/MigrationSchemaGenerator.cs)

- Generate Oracle-specific DDL:
  - `GENERATED AS IDENTITY` for identity columns
  - Oracle-compatible column types (`VARCHAR2`, `NUMBER`, etc.)
  - Oracle-compatible constraints
  - Sequence creation for legacy identity patterns

---

## 14. Phase O13: Testing Strategy

### 14.1 Unit Tests (SQL Output Verification)

Following the pattern of existing dialect tests, create tests that verify the generated SQL output for each query type.

#### `Test/Select/OracleSelectTests.cs`
- Basic SELECT with column list
- SELECT with FROM (including DUAL fallback)
- SELECT with WHERE conditions
- SELECT with ORDER BY
- Pagination with OFFSET/FETCH
- SAMPLE clause
- Flashback query (AS OF TIMESTAMP)
- Hierarchical query (CONNECT BY, START WITH)
- PIVOT/UNPIVOT
- FOR UPDATE with NOWAIT/WAIT/SKIP LOCKED
- JOINs (INNER, LEFT, RIGHT, FULL, CROSS, NATURAL, LATERAL)
- CTE (WITH clause)
- Recursive CTE
- Subquery in FROM
- WITH FUNCTION (Oracle 12c+)

#### `Test/Insert/OracleInsertTests.cs`
- Basic INSERT with VALUES
- INSERT with column list
- INSERT ... SELECT (INSERT from subquery)
- INSERT with RETURNING ... INTO
- INSERT ALL (multi-table insert)
- INSERT with sequence.NEXTVAL
- INSERT with DEFAULT keyword for columns
- INSERT without DEFAULT VALUES (verify throws)

#### `Test/Update/OracleUpdateTests.cs`
- Basic UPDATE with SET
- UPDATE with WHERE
- UPDATE with correlated subquery SET
- UPDATE with RETURNING ... INTO
- UPDATE with subquery in WHERE

#### `Test/Delete/OracleDeleteTests.cs`
- Basic DELETE with WHERE
- DELETE with subquery
- DELETE with RETURNING ... INTO
- DELETE all rows

#### `Test/Merge/OracleMergeTests.cs`
- Basic MERGE with WHEN MATCHED and WHEN NOT MATCHED
- MERGE with DELETE in WHEN MATCHED
- MERGE with RETURNING ... INTO
- MERGE with conditional clauses (WHEN MATCHED AND ...)

#### `Test/Migration/OracleMigrationTests.cs`
- CREATE TABLE with Oracle types
- ALTER TABLE ADD/MODIFY/DROP column
- CREATE SEQUENCE
- Identity column DDL
- Comment generation

### 14.2 Integration Tests (Requires Oracle Database)

- Docker Compose configuration for Oracle XE (21c or 23c)
- Connection test with OracleDbClient
- Full CRUD operations
- MERGE upsert
- Transaction support
- RETURNING clause with output parameter binding

---

## 15. Phase O14: NuGet Packaging & Demo

### 15.1 NuGet Dependencies

Add to [`Drizzle4Dotnet.csproj`](Drizzle4Dotnet/Drizzle4Dotnet.csproj):
```xml
<ItemGroup>
  <PackageReference Include="Oracle.ManagedDataAccess.Core" Version="23.7.0" />
</ItemGroup>
```

### 15.2 Oracle-Specific Demo Application

Create [`SharedDemo/Oracle/Schema.cs`](SharedDemo/Oracle/Schema.cs):
```csharp
// Oracle-specific table definitions using source generators
[OracleTable(Schema = "APP")]
public partial class UsersTable : IOracleDbTable
{
    [OracleColumn(DataType = "NUMBER(10)", SequenceName = "USERS_SEQ")]
    public OracleColumn<int, UsersTable> Id { get; } = new("ID");
    
    [OracleColumn(DataType = "VARCHAR2(100)")]
    public OracleColumn<string, UsersTable> Name { get; } = new("NAME");
    
    [OracleColumn(DataType = "VARCHAR2(255)")]
    public OracleColumn<string, UsersTable> Email { get; } = new("EMAIL");
}
```

Create [`SharedDemo/Oracle/DbDto.cs`](SharedDemo/Oracle/DbDto.cs):
```csharp
[DbSelect(UserSchema.Id, UserSchema.Name, UserSchema.Email)]
public partial record UserDto(int Id, string Name, string Email);
```

Create [`Demo1/Program.cs`](Demo1/Program.cs) example usage:
```csharp
using var db = new OracleDbClient(connection);

// Basic SELECT
var users = await db.Select(UserDto.Select)
    .From(UsersTable.As("u"))
    .Where(UsersTable.Name.Like("A%"))
    .OrderBy(UsersTable.Name)
    .Limit(10)
    .ExecuteGetListAsync();

// INSERT with sequence
await db.Insert(UsersTable)
    .Value(new { Name = "Alice", Email = "alice@example.com" })
    .ExecuteAsync();

// MERGE upsert
await db.Merge(UsersTable)
    .Using(RawSql.Create("SELECT 1 AS id, 'Bob' AS name, 'bob@example.com' AS email FROM DUAL"))
    .On(UsersTable.Id.Equals(RawSql.Create("source.id")))
    .WhenMatchedThenUpdate(new() { [UsersTable.Name.Identifier] = RawSql.Create("source.name") })
    .WhenNotMatchedThenInsert(
        new() { UsersTable.Name.Identifier, UsersTable.Email.Identifier },
        new() { RawSql.Create("source.name"), RawSql.Create("source.email") })
    .ExecuteAsync();
```

### 15.3 Docker Compose for Oracle

Add to [`docker-compose.yaml`](docker-compose.yaml):
```yaml
oracle:
  image: gvenzl/oracle-free:23-slim
  container_name: drizzle-oracle
  environment:
    ORACLE_PASSWORD: password
    APP_USER: drizzle
    APP_USER_PASSWORD: password
  ports:
    - "1521:1521"
  volumes:
    - oracle_data:/opt/oracle/oradata
```

---

## 16. Appendix: Oracle SQL Syntax Reference

### 16.1 SELECT Syntax

```sql
[WITH 
  [FUNCTION func_name RETURN type IS ... END;]
  cte_name AS (subquery) [, ...]
]
SELECT [DISTINCT] [SAMPLE (p)] column_list
FROM table_name [AS OF TIMESTAMP (expr) | AS OF SCN (expr)]
  [LATERAL (subquery)]
  [JOIN ...]
[WHERE condition]
[START WITH condition]
[CONNECT BY [NOCYCLE] condition]
[GROUP BY expr]
[HAVING condition]
[PIVOT | UNPIVOT ...]
[ORDER BY expr [ASC | DESC]]
[OFFSET n ROWS] [FETCH NEXT n ROWS ONLY]
[FOR UPDATE [OF table.col] [WAIT n | NOWAIT | SKIP LOCKED]]
```

### 16.2 INSERT Syntax

```sql
-- Standard INSERT
INSERT INTO table (col1, col2) VALUES (val1, val2)
RETURNING col1 INTO :outParam

-- INSERT ... SELECT
INSERT INTO table (col1, col2) SELECT col1, col2 FROM source WHERE condition

-- INSERT ALL
INSERT ALL
  [WHEN condition THEN]
  INTO table1 (col1, col2) VALUES (val1, val2)
  [WHEN condition THEN]
  INTO table2 (col1, col2) VALUES (val1, val2)
  [ELSE]
  INTO table3 (col1, col2) VALUES (val1, val2)
SELECT ... FROM DUAL
```

### 16.3 UPDATE Syntax

```sql
UPDATE table SET col1 = val1, col2 = val2
WHERE condition
RETURNING col1 INTO :outParam
```

### 16.4 DELETE Syntax

```sql
DELETE FROM table WHERE condition
RETURNING col1 INTO :outParam
```

### 16.5 MERGE Syntax

```sql
MERGE INTO target t
USING source s
ON (t.key = s.key)
WHEN MATCHED THEN
  UPDATE SET t.col = s.col
  [DELETE WHERE condition]
WHEN NOT MATCHED THEN
  INSERT (col1, col2) VALUES (s.col1, s.col2)
```

### 16.6 PIVOT/UNPIVOT Syntax

```sql
-- PIVOT
SELECT * FROM (
  SELECT category, amount FROM sales
)
PIVOT (
  SUM(amount) FOR category IN ('Electronics' AS electronics, 'Clothing' AS clothing)
)

-- UNPIVOT
SELECT * FROM (
  SELECT id, q1_sales, q2_sales FROM quarterly_sales
)
UNPIVOT (
  sales FOR quarter IN (q1_sales AS 'Q1', q2_sales AS 'Q2')
)
```

### 16.7 Hierarchical Query Syntax

```sql
SELECT ... FROM table
START WITH condition
CONNECT BY [NOCYCLE] PRIOR child_col = parent_col
[ORDER SIBLINGS BY col]
```

### 16.8 Flashback Query Syntax

```sql
-- As of timestamp
SELECT * FROM table AS OF TIMESTAMP (SYSTIMESTAMP - INTERVAL '1' HOUR)

-- As of SCN
SELECT * FROM table AS OF SCN 1234567

-- Flashback version query
SELECT * FROM table
  VERSIONS BETWEEN TIMESTAMP (SYSTIMESTAMP - INTERVAL '1' HOUR) AND SYSTIMESTAMP
```

### 16.9 Common Table Expressions (CTE)

```sql
-- Standard CTE
WITH cte AS (
  SELECT * FROM table WHERE condition
)
SELECT * FROM cte

-- Recursive CTE (11gR2+)
WITH rec_cte (col1, col2) AS (
  -- Anchor member
  SELECT col1, col2 FROM table WHERE condition
  UNION ALL
  -- Recursive member
  SELECT r.col1, t.col2 FROM table t
  JOIN rec_cte r ON t.parent_id = r.id
)
SELECT * FROM rec_cte

-- WITH FUNCTION (12c+)
WITH
  FUNCTION double_it(n NUMBER) RETURN NUMBER IS
  BEGIN RETURN n * 2; END;
SELECT double_it(column) FROM table
```

### 16.10 SELECT with `WITH FUNCTION` (Oracle 12c+)

```sql
WITH
  FUNCTION get_department_name(dept_id NUMBER) RETURN VARCHAR2 IS
    name departments.department_name%TYPE;
  BEGIN
    SELECT department_name INTO name FROM departments WHERE department_id = dept_id;
    RETURN name;
  END;
SELECT employee_id, get_department_name(department_id) AS dept_name
FROM employees
```

---

## 17. Appendix: Implementation Order & Dependencies

### 17.1 Dependency Graph

```
Phase O1 (Dialect Foundation)
  ├── Phase O2 (Types & Interfaces)
  │     ├── Phase O3 (Functions)
  │     ├── Phase O4 (Operators & Nodes)
  │     └── Phase O5 (SELECT Query) ────────────────────────┐
  │              ├── Phase O6 (INSERT Query) ───────────────┐│
  │              ├── Phase O7 (UPDATE Query) ───────────────┤│
  │              ├── Phase O8 (DELETE Query) ───────────────┤│
  │              └── Phase O9 (MERGE Query) ────────────────┤│
  └── Phase O10 (Schema / Column Types) ────────────────────┘│
                    ├── Phase O11 (Connection & Execution) ──┘
                    ├── Phase O12 (Source Generators)
                    ├── Phase O13 (Testing)
                    └── Phase O14 (Packaging & Demo)
```

### 17.2 Recommended Implementation Order

| Step | Phase | Description | Est. Effort |
|------|-------|-------------|-------------|
| 1 | O1 | Create `OracleSqlDialectImpl.cs` | 1 day |
| 2 | O2 | Create `OracleTable.cs`, `OracleColumn.cs` | 0.5 day |
| 3 | O4 | Create core operators and nodes (`OracleSequenceNode`, `OracleRowIdNode`, `OracleReturningNode`) | 2 days |
| 4 | O3 | Create Oracle functions (DateTime, String, Numeric, Json, Analytic, Info) | 3 days |
| 5 | O5 | Create `OracleSelectQuery` with pagination, DUAL, SAMPLE, flashback, CONNECT BY, PIVOT, FOR UPDATE | 4 days |
| 6 | O6 | Create `OracleInsertQuery` with RETURNING, INSERT ALL | 2 days |
| 7 | O7 | Create `OracleUpdateQuery` with RETURNING, subquery SET | 1 day |
| 8 | O8 | Create `OracleDeleteQuery` with RETURNING | 1 day |
| 9 | O9 | Create `OracleMergeQuery` | 2 days |
| 10 | O10 | Create Oracle schema types, column type mapping, migration DDL support | 3 days |
| 11 | O11 | Create `OracleDbClient`, `OracleQueryBuilder`, RETURNING executor support | 3 days |
| 12 | O12 | Update source generators for Oracle | 2 days |
| 13 | O13 | Write unit tests for all query types | 4 days |
| 14 | O14 | Create demo app, update Docker Compose, update NuGet packaging | 1 day |

**Total estimated effort:** ~29.5 days

### 17.3 Risk Factors

1. **RETURNING clause complexity**: Oracle's `RETURNING ... INTO` uses output parameters rather than returning a rowset. This requires special handling in the executor layer, which is more complex than PostgreSQL or MSSQL approaches.

2. **Parameter naming**: Oracle uses `:` prefix instead of `@`. The `SqlBuilder.AddParameter` method uses the dialect's `BuildParameterName`, so this should work correctly as long as the ADO.NET provider supports `:` prefix parameters. Oracle.ManagedDataAccess.Core does, but this should be verified early.

3. **`FROM DUAL` requirement**: Oracle requires `FROM DUAL` for any SELECT that doesn't reference a table. The `OracleSelectQuery.BuildSql` must detect this case and add `FROM DUAL` automatically.

4. **`DEFAULT VALUES` not supported**: Oracle doesn't support `INSERT ... DEFAULT VALUES`. The `OracleInsertQuery` must handle this gracefully, either by throwing a clear error or by generating `INSERT INTO table (pk_col) VALUES (DEFAULT)`.

5. **No `UPDATE ... FROM`/`DELETE ... USING`**: Oracle doesn't support these constructs. Queries that need joins in UPDATE/DELETE must use correlated subqueries instead.

6. **Pre-12c compatibility**: If supporting Oracle versions prior to 12c, `OFFSET`/`FETCH` is not available. A fallback using `ROW_NUMBER()` in a subquery would be needed. For initial implementation, targeting Oracle 12c+ is recommended.

7. **Oracle 23c new features**: Oracle 23c introduces `IF [NOT] EXISTS`, schema-level privileges, `BOOLEAN` type, and other improvements. These can be added as enhancements after the initial implementation.

---

> **End of Plan**
