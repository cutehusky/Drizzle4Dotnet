# Drizzle4Dotnet — Oracle Database Implementation Status & Plan

> **Status:** ✅ Implemented (Full Oracle dialect)  
> **Last Updated:** 2026-06-29  
> **Target:** Oracle 12c+ with Oracle.ManagedDataAccess.Core

---

## 1. Implementation Status Overview

| Layer | Status | Details |
|-------|--------|---------|
| **Dialect** | ✅ Complete | [`OracleSqlDialectImpl`](Drizzle4Dotnet/src/Oracle/OracleSqlDialectImpl.cs:12) — double-quote identifiers, `:p0` colon-prefixed params, OFFSET/FETCH pagination |
| **Data Types** | ✅ Complete | [`OracleDataType`](Drizzle4Dotnet/src/Oracle/Schema/OracleDataType.cs) — Integer, BigInt, SmallInt, TinyInt, Text, Boolean, Decimal, BinaryFloat, BinaryDouble, Timestamp, Date, Time, Uuid, Blob, Char |
| **DbClient** | ✅ Complete | [`OracleDbClient`](Drizzle4Dotnet/src/Oracle/OracleDbClient.cs) — Oracle.ManagedDataAccess.Core |
| **SELECT** | ✅ Complete | OracleSelectQuery — FOR UPDATE, FOR UPDATE OF, NOWAIT, WAIT n, SKIP LOCKED |
| **INSERT** | ✅ Complete | OracleInsertQuery — RETURNING INTO, ON CONFLICT/DoUpdate/DoNothing (MERGE-based) |
| **UPDATE** | ✅ Complete | OracleUpdateQuery — RETURNING INTO |
| **DELETE** | ✅ Complete | OracleDeleteQuery — RETURNING INTO |
| **MERGE** | ✅ Complete | OracleMergeQuery — MERGE with USING, ON, WHEN MATCHED/NOT MATCHED, DELETE WHERE |
| **Functions** | ✅ Complete | DateTime (15+), JSON (6), Numeric (7), String (15+), Info (3), Analytic (9) |
| **Operators** | ✅ Complete | Core + Oracle-specific CONCAT operator |
| **Special Nodes** | ✅ Complete | `OracleReturningNode`, `OracleRowIdNode`, `OracleSequenceNode` |

## 2. Current File Structure

```
Drizzle4Dotnet/src/Oracle/
├── OracleDbClient.cs                # Connection + factory methods
├── OracleQueryBuilder.cs            # Offline query builder
├── OracleQueryBuilderExtensions.cs  # 1–16 column Select/SelectDistinct extensions
├── OracleSqlDialectImpl.cs          # Dialect: "quote" identifiers, :p0 params, OFFSET/FETCH, type map
├── OracleStatics.cs                 # Static factories
├── Schema/
│   ├── OracleColumn.cs              # OracleColumn type
│   ├── OracleDataType.cs            # OracleDataType static structs
│   └── OracleTable.cs               # OracleTable base class
├── Operators/
│   ├── OracleFunctions.cs           # Entry point
│   ├── OracleFunctions.Analytic.cs  # RatioToReport, CumeDist, PercentRank, PercentileCont/Disc, Median, StdDev, Variance
│   ├── OracleFunctions.DateTime.cs  # SysDate, CurrentDate, CurrentTimestamp, SysTimestamp, AddMonths, LastDay, MonthsBetween, NextDay, Round, Trunc, Extract, ToChar, ToDate, ToTimestamp, Interval
│   ├── OracleFunctions.Info.cs      # User, SysContext, Decode
│   ├── OracleFunctions.Json.cs      # JsonValue, JsonQuery, JsonExists, JsonArrayagg, JsonObjectagg, JsonTable
│   ├── OracleFunctions.Numeric.cs   # Round, Trunc, Mod, Remainder, Power, Sqrt
│   ├── OracleFunctions.String.cs    # Concat, ConcatWs, Instr, Length, LengthB, Substr, Trim, LTrim, RTrim, Translate, Replace, RegexpReplace, Listagg, ListaggWithinGroup
│   └── OracleOperators.cs           # CONCAT operator
└── Query/
    ├── OracleDeleteQuery.cs         # DELETE with RETURNING INTO
    ├── OracleInsertQuery.cs         # INSERT with RETURNING INTO, ON CONFLICT/MERGE-based upsert
    ├── OracleMergeQuery.cs          # MERGE with USING/ON/WHEN MATCHED/NOT MATCHED/DELETE WHERE
    ├── OracleSelectQuery.cs         # SELECT with FOR UPDATE, FOR UPDATE OF, NOWAIT, WAIT n, SKIP LOCKED
    └── OracleUpdateQuery.cs         # UPDATE with RETURNING INTO
```

## 3. Complete Feature Matrix

### 3.1 SELECT Features — Status: ✅ All Implemented

| Feature | Oracle Syntax | Implementation |
|---------|--------------|----------------|
| FROM | `FROM "table"` (DUAL for scalar) | ✅ Base |
| WHERE | `WHERE (cond)` | ✅ Base |
| GROUP BY / HAVING | `GROUP BY ... HAVING ...` | ✅ Base |
| ORDER BY | `ORDER BY col ASC/DESC` | ✅ Base |
| LIMIT / OFFSET | `OFFSET n ROWS FETCH NEXT m ROWS ONLY` (12c+) | ✅ Via dialect `BuildLimitOffset()` |
| DISTINCT | `SELECT DISTINCT ...` | ✅ Base |
| INNER/LEFT/RIGHT JOIN | `JOIN ... ON ...` | ✅ Base |
| FULL OUTER JOIN | `FULL OUTER JOIN ... ON ...` | ✅ Base |
| NATURAL JOIN | `NATURAL JOIN ...` | ✅ Base |
| FOR UPDATE | `SELECT ... FOR UPDATE` | ✅ `ForUpdate()` |
| FOR UPDATE OF | `SELECT ... FOR UPDATE OF "col"` | ✅ `ForUpdateOf(columns)` |
| NOWAIT | `FOR UPDATE NOWAIT` | ✅ `ForUpdateNoWait()` |
| WAIT n | `FOR UPDATE WAIT 5` | ✅ `ForUpdateWait(seconds)` |
| SKIP LOCKED | `FOR UPDATE SKIP LOCKED` | ✅ `ForUpdateSkipLocked()` |
| CTEs | `WITH ... AS (...) SELECT ...` | ✅ Base |
| Recursive CTEs | `WITH ... (col1, col2) AS (...)` | ✅ Base (11gR2+) |
| LATERAL join | `... JOIN LATERAL (...)` | ✅ Base (12c+ via core lateral support) |
| Subqueries | `(SELECT ...) alias` | ✅ Base |

### 3.2 INSERT Features — Status: ✅ All Implemented

| Feature | Oracle Syntax | Implementation |
|---------|--------------|----------------|
| Single row | `INSERT INTO "t" (cols) VALUES (vals)` | ✅ Base |
| Multi row | `INSERT ALL INTO ... VALUES ...` | ⚠️ Base multi-row uses standard VALUES |
| INSERT ... SELECT | `INSERT ... SELECT ...` | ✅ Base |
| DEFAULT VALUES | `INSERT ... DEFAULT VALUES` | ✅ Base |
| RETURNING INTO | `RETURNING col1, col2 INTO :out1, :out2` | ✅ `ReturningInto(columns)` |
| ON CONFLICT (MERGE-based) | Uses MERGE internally | ✅ `OnConflict()`, `DoNothing()`, `DoUpdate()` |

### 3.3 UPDATE / DELETE Features — Status: ✅ All Implemented

| Feature | Update | Delete |
|---------|--------|--------|
| WHERE | ✅ | ✅ |
| CTE | ✅ | ✅ |
| RETURNING INTO | ✅ `ReturningInto()` | ✅ `ReturningInto()` |

### 3.4 MERGE Features — Status: ✅ All Implemented

| Feature | Oracle Syntax | Implementation |
|---------|--------------|----------------|
| USING | `MERGE INTO "t" USING source` | ✅ `Using()` |
| ON | `ON (condition)` | ✅ `On()` |
| WHEN MATCHED THEN UPDATE | `WHEN MATCHED THEN UPDATE SET ...` | ✅ `WhenMatchedThenUpdate()` |
| WHEN NOT MATCHED THEN INSERT | `WHEN NOT MATCHED THEN INSERT ... VALUES ...` | ✅ `WhenNotMatchedThenInsert()` |
| DELETE WHERE (in WHEN MATCHED) | `WHEN MATCHED THEN UPDATE ... DELETE WHERE ...` | ✅ Oracle-specific `WhenMatchedThenDelete()` |
| JOIN in USING | Supported | ✅ via IJoin |

### 3.5 Oracle Functions — Status: ✅ All Implemented

| Category | Functions |
|----------|-----------|
| DateTime | `SysDate()`, `CurrentDate`, `CurrentTimestamp`, `SysTimestamp()`, `AddMonths(col, n)`, `LastDay(col)`, `MonthsBetween(col1, col2)`, `NextDay(col, day)`, `Round(col, fmt)`, `Trunc(col, fmt)`, `Extract(unit, col)`, `ToChar(col, fmt)`, `ToDate(str, fmt)`, `ToTimestamp(str, fmt)`, `Interval(num, unit)` |
| JSON (12c+) | `JsonValue(col, path)`, `JsonQuery(col, path)`, `JsonExists(col, path)`, `JsonArrayagg(expr)`, `JsonObjectagg(key, val)`, `JsonTable(col, path)` |
| String | `Concat(col1, col2)`, `ConcatWs(sep, vals...)`, `Instr(col, substr)`, `Length(col)`, `LengthB(col)`, `Substr(col, start, len)`, `Trim(col)`, `LTrim(col, chars)`, `RTrim(col, chars)`, `Translate(col, from, to)`, `Replace(col, old, new)`, `RegexpReplace(col, pat, rep)`, `Listagg(col, sep)`, `ListaggWithinGroup(col, sep, order)` |
| Numeric | `Round(col, decimals)`, `Trunc(col, decimals)`, `Mod(col, divisor)`, `Remainder(col, divisor)`, `Power(col, exp)`, `Sqrt(col)` |
| Analytic | `RatioToReport(col)`, `CumeDist()`, `PercentRank()`, `PercentileCont(p)`, `PercentileDisc(p)`, `Median(col)`, `StdDev(col) OVER (...)`, `Variance(col) OVER (...)` |
| Info | `User()`, `SysContext(namespace, param)`, `Decode(expr, search, result)` |

### 3.6 Oracle Special Nodes

| Node | SQL Output |
|------|-----------|
| `OracleReturningNode(columns)` | `RETURNING "col1", "col2" INTO :out1, :out2` |
| `OracleRowIdNode` | `ROWID` |
| `OracleSequenceNode(name)` | `"sequence_name".NEXTVAL` |

### 3.7 Oracle-Specific Syntax Notes

| Feature | Oracle Difference |
|---------|------------------|
| Parameter prefix | `:p0`, `:p1` (colon prefix) — different from `@p0` used by other dialects |
| DUAL table | Required for scalar queries: `SELECT SYSDATE FROM DUAL` |
| RETURNING clause | Uses `RETURNING ... INTO :bind_var` — different from PostgreSQL's row-returning RETURNING |
| Pagination | `OFFSET n ROWS FETCH NEXT m ROWS ONLY` (12c+); older versions use `ROW_NUMBER()` subquery |
| Sequence access | `"seq".NEXTVAL` — different from PostgreSQL's `NEXTVAL('seq')` or MSSQL's `NEXT VALUE FOR` |
| Empty string = NULL | Oracle treats `''` as NULL — affects query generation |
| No `LIMIT` on UPDATE/DELETE | Not supported; use `WHERE ROWNUM` or subquery |
| MERGE DELETE WHERE | Oracle-specific: `DELETE WHERE` inside `WHEN MATCHED THEN UPDATE` |

## 4. Remaining Work / Gaps

| Item | Priority | Notes |
|------|----------|-------|
| `INSERT ALL` (multi-table insert) | Low | Oracle-specific; rarely needed in ORM |
| `CONNECT BY` hierarchical queries | Low | Oracle alternative to recursive CTEs |
| `FLASHBACK` query clause | Low | `SELECT ... AS OF TIMESTAMP ...` |
| `SAMPLE` clause | Low | `SELECT ... SAMPLE (10)` |
| `MATCH_RECOGNIZE` | Low | Oracle 12c+ pattern matching |
| RETURNING result set emulation | Medium | Oracle's `RETURNING ... INTO` uses PL/SQL bind variables, not row sets. Need to handle this differently from PostgreSQL's row-returning RETURNING |
