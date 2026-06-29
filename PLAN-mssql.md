# Drizzle4Dotnet — Microsoft SQL Server (MSSQL) Implementation Status & Plan

> **Status:** ✅ Implemented (Full MSSQL dialect)  
> **Last Updated:** 2026-06-29  
> **Target:** SQL Server 2016+ with Microsoft.Data.SqlClient

---

## 1. Implementation Status Overview

| Layer | Status | Details |
|-------|--------|---------|
| **Dialect** | ✅ Complete | [`MssqlSqlDialectImpl`](Drizzle4Dotnet/src/Mssql/MssqlSqlDialectImpl.cs:12) — `[bracket]` quoting, `OFFSET/FETCH` pagination |
| **Data Types** | ✅ Complete | [`MssqlDataType`](Drizzle4Dotnet/src/Mssql/Schema/MssqlDataType.cs) — Int, BigInt, SmallInt, TinyInt, Text, Boolean, Decimal, Float, DateTime2, UniqueIdentifier, VarBinary, NChar |
| **DbClient** | ✅ Complete | [`MssqlDbClient`](Drizzle4Dotnet/src/Mssql/MssqlDbClient.cs) — Microsoft.Data.SqlClient |
| **SELECT** | ✅ Complete | `MssqlSelectQuery` — TOP(n), TOP WITH TIES, TOP PERCENT, OFFSET/FETCH, CROSS/OUTER APPLY, FULL OUTER JOIN, table hints |
| **INSERT** | ✅ Complete | `MssqlInsertQuery` — OUTPUT INSERTED clause |
| **UPDATE** | ✅ Complete | `MssqlUpdateQuery` — OUTPUT, TOP |
| **DELETE** | ✅ Complete | `MssqlDeleteQuery` — OUTPUT, TOP |
| **MERGE** | ✅ Complete | `MssqlMergeQuery` — full MERGE with WHEN MATCHED/NOT MATCHED/BY SOURCE + OUTPUT |
| **Functions** | ✅ Complete | MSSQL-specific: GetDate, DateAdd, DateDiff, DatePart, Year/Month/Day, Format, EOMonth, JsonValue/Query/Modify, OpenJson, CharIndex, ConcatWs, StringAgg, StringAggWithinGroup, Trim/LTrim/RTrim, Len, Replace, Rand, Round, Ceiling, DbName, UserName, ObjectName |
| **Operators** | ✅ Complete | Core operators + MSSQL-specific |
| **Special Nodes** | ✅ Complete | `MssqlTopNode`, `MssqlTableHintNode`, `MssqlOutputNode`, `MssqlSequenceNode` |
| **Static Factories** | ✅ Complete | `NewId()`, `NewSequentialId()`, `CurrentTimestamp`, `GetAnsiNull()` |

## 2. Current File Structure

```
Drizzle4Dotnet/src/Mssql/
├── MssqlDbClient.cs                # Connection + factory methods
├── MssqlQueryBuilder.cs            # Offline query builder
├── MssqlQueryBuilderExtensions.cs  # 1–16 column Select/SelectDistinct extensions
├── MssqlSqlDialectImpl.cs          # Dialect: [bracket] quoting, OFFSET/FETCH, feature flags, type map
├── MssqlStatics.cs                 # NewId, NewSequentialId, CurrentTimestamp, GetAnsiNull
├── Schema/
│   ├── MssqlColumn.cs              # MssqlColumn type
│   ├── MssqlDataType.cs            # MssqlDataType static structs
│   └── MssqlTable.cs               # MssqlTable base class
├── Operators/
│   ├── MssqlFunctions.cs           # Entry point
│   ├── MssqlFunctions.DateTime.cs  # GetDate, SysDateTime, DateAdd, DateDiff, DatePart, Year, Month, Day, Format, EOMonth
│   ├── MssqlFunctions.Info.cs      # DbName, UserName, ObjectName
│   ├── MssqlFunctions.Json.cs      # JsonValue, JsonQuery, JsonModify, OpenJson
│   ├── MssqlFunctions.Numeric.cs   # Rand, Round, Ceiling
│   ├── MssqlFunctions.String.cs    # CharIndex, ConcatWs, StringAgg, StringAggWithinGroup, Trim, LTrim, RTrim, Len, Replace
│   └── MssqlOperators.cs           # MSSQL-specific operators
└── Query/
    ├── MssqlDeleteQuery.cs         # DELETE with TOP, OUTPUT
    ├── MssqlInsertQuery.cs         # INSERT with OUTPUT INSERTED
    ├── MssqlMergeQuery.cs          # MERGE with USING/ON/WHEN MATCHED/NOT MATCHED/BY SOURCE/OUTPUT
    ├── MssqlSelectQuery.cs         # SELECT with TOP, OFFSET/FETCH, APPLY, FULL JOIN, table hints
    └── MssqlUpdateQuery.cs         # UPDATE with TOP, OUTPUT
```

## 3. Complete Feature Matrix

### 3.1 SELECT Features — Status: ✅ All Implemented

| Feature | MSSQL Syntax | Implementation |
|---------|-------------|----------------|
| FROM | `FROM [table]` | ✅ Base |
| WHERE | `WHERE (cond)` | ✅ Base |
| GROUP BY / HAVING | `GROUP BY ... HAVING ...` | ✅ Base |
| ORDER BY | `ORDER BY col ASC/DESC` | ✅ Base (required for OFFSET/FETCH) |
| TOP(n) | `SELECT TOP (n) ...` | ✅ `Top(count)` |
| TOP WITH TIES | `SELECT TOP (n) WITH TIES ...` | ✅ `TopWithTies(count)` |
| TOP PERCENT | `SELECT TOP (n) PERCENT ...` | ✅ `TopPercent(count)` |
| OFFSET/FETCH | `OFFSET n ROWS FETCH NEXT m ROWS ONLY` | ✅ Via dialect `BuildLimitOffset()` |
| DISTINCT | `SELECT DISTINCT ...` | ✅ Base |
| INNER/LEFT/RIGHT JOIN | `JOIN ... ON ...` | ✅ Base |
| FULL OUTER JOIN | `FULL OUTER JOIN ... ON ...` | ✅ `FullJoin()` |
| CROSS APPLY | `CROSS APPLY (...)` | ✅ `CrossApply(table)` |
| OUTER APPLY | `OUTER APPLY (...)` | ✅ `OuterApply(table)` |
| Table Hints | `WITH (NOLOCK)` | ✅ `WithHint(MssqlTableHintNode)` |
| CTEs | `WITH cte AS (...) ...` | ✅ Base |
| Recursive CTEs | `WITH RECURSIVE ...` | ✅ Base |
| Subqueries | `(SELECT ...) AS alias` | ✅ Base |

### 3.2 INSERT Features — Status: ✅ All Implemented

| Feature | MSSQL Syntax | Implementation |
|---------|-------------|----------------|
| Single row | `INSERT INTO [t] (cols) VALUES (vals)` | ✅ Base |
| Multi row | `INSERT ... VALUES (...), (...)` | ✅ Base |
| INSERT ... SELECT | `INSERT ... SELECT ...` | ✅ Base |
| DEFAULT VALUES | `INSERT ... DEFAULT VALUES` | ✅ Base |
| OUTPUT INSERTED | `OUTPUT INSERTED.col` | ✅ `OutputInserted(columns)` |
| OUTPUT clause position | Between column list and VALUES | ✅ Correct T-SQL placement |

### 3.3 UPDATE Features — Status: ✅ All Implemented

| Feature | MSSQL Syntax | Implementation |
|---------|-------------|----------------|
| SET | `SET col = val` | ✅ Base |
| SET expression | `SET col = (expr)` | ✅ Base |
| FROM/JOIN in UPDATE | `UPDATE t SET ... FROM t JOIN ...` | ✅ MssqlUpdateQuery |
| WHERE | `WHERE (cond)` | ✅ Base |
| TOP(n) | `UPDATE TOP (n) ...` | ✅ MssqlUpdateQuery |
| OUTPUT | `OUTPUT INSERTED.col, DELETED.col` | ✅ MssqlUpdateQuery |
| CTE | `WITH cte AS (...) UPDATE ...` | ✅ Base |

### 3.4 DELETE Features — Status: ✅ All Implemented

| Feature | MSSQL Syntax | Implementation |
|---------|-------------|----------------|
| WHERE | `DELETE ... WHERE (cond)` | ✅ Base |
| FROM/JOIN in DELETE | `DELETE t FROM t JOIN ...` | ✅ MssqlDeleteQuery |
| TOP(n) | `DELETE TOP (n) ...` | ✅ MssqlDeleteQuery |
| OUTPUT | `OUTPUT DELETED.col` | ✅ MssqlDeleteQuery |
| CTE | `WITH cte AS (...) DELETE ...` | ✅ Base |

### 3.5 MERGE Features — Status: ✅ All Implemented

| Feature | MSSQL Syntax | Implementation |
|---------|-------------|----------------|
| USING source | `MERGE [t] USING source` | ✅ `Using(ISql)` |
| ON condition | `ON (condition)` | ✅ `On(IGenericSql)` |
| WHEN MATCHED THEN UPDATE | `WHEN MATCHED THEN UPDATE SET ...` | ✅ `WhenMatchedThenUpdate(Dict)` |
| WHEN NOT MATCHED THEN INSERT | `WHEN NOT MATCHED THEN INSERT ... VALUES ...` | ✅ `WhenNotMatchedThenInsert(cols, vals)` |
| WHEN NOT MATCHED BY SOURCE THEN DELETE | `WHEN NOT MATCHED BY SOURCE THEN DELETE` | ✅ `WhenNotMatchedBySourceThenDelete()` |
| OUTPUT INSERTED/DELETED | `OUTPUT INSERTED.col, DELETED.col` | ✅ `OutputInserted()`, `OutputDeleted()` |
| JOIN in USING | `MERGE ... USING source JOIN ...` | ✅ via IJoin |

### 3.6 MSSQL Functions — Status: ✅ All Implemented

| Category | Functions |
|----------|-----------|
| DateTime | `GetDate()`, `SysDateTime()`, `DateAdd()`, `DateDiff()`, `DatePart()`, `Year()`, `Month()`, `Day()`, `Format()`, `EOMonth()` |
| JSON | `JsonValue()`, `JsonQuery()`, `JsonModify()`, `OpenJson()` |
| String | `CharIndex()` (2-arg, 3-arg), `ConcatWs()`, `StringAgg()`, `StringAggWithinGroup()`, `Trim()`, `LTrim()`, `RTrim()`, `Len()`, `Replace()` |
| Numeric | `Rand()`, `Round()`, `Ceiling()` |
| Info | `DbName()`, `UserName()`, `ObjectName()` |

### 3.7 MSSQL Special Nodes

| Node | SQL Output |
|------|-----------|
| `MssqlTopNode(count)` | `TOP (@p0)` |
| `MssqlTopNode(count, withTies: true)` | `TOP (@p0) WITH TIES` |
| `MssqlTopNode(count, percent: true)` | `TOP (@p0) PERCENT` |
| `MssqlTableHintNode("NOLOCK")` | `WITH (NOLOCK)` |
| `MssqlOutputNode(columns)` | `OUTPUT INSERTED.[col1], DELETED.[col2]` |
| `MssqlSequenceNode(name)` | `NEXT VALUE FOR [sequence_name]` |

### 3.8 LIMIT/OFFSET Behavior

MSSQL uses `OFFSET/FETCH` which **requires ORDER BY**. The dialect and query handle this:

- If ORDER BY is present: `ORDER BY col OFFSET n ROWS FETCH NEXT m ROWS ONLY`
- If LIMIT/OFFSET without ORDER BY: Automatically adds `ORDER BY (SELECT 0)` as workaround
- For UPDATE/DELETE: `TOP(n)` is used instead of OFFSET/FETCH (via `BuildLimitOffsetForUpdateDelete`)

## 4. Remaining Work / Gaps

| Item | Priority | Notes |
|------|----------|-------|
| Query builder extensions (Returning) | Medium | MSSQL uses `OUTPUT` not `RETURNING` — need Mssql-specific `.Returning()` that maps to `OUTPUT INSERTED.*` |
| SELECT ... INTO | Low | `SELECT ... INTO [new_table] FROM ...` not yet exposed |
| `WITH TIES` in OFFSET/FETCH | Low | `OFFSET n ROWS FETCH NEXT m ROWS ONLY WITH TIES` |
| PIVOT/UNPIVOT | Low | Not yet supported; T-SQL specific |
| `FOR XML PATH` / `FOR JSON` | Low | Not yet supported |
| TRY_CAST / TRY_CONVERT | Low | T-SQL specific functions |
| IIF function | Low | T-SQL `IIF(cond, true, false)` = `CASE WHEN ...` |
