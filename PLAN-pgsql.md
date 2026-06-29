# Drizzle4Dotnet — PostgreSQL Implementation Status & Plan

> **Status:** ✅ Implemented (Core + PgSql Dialect Complete)  
> **Last Updated:** 2026-06-29  
> **Target:** PostgreSQL 10+ with Npgsql

---

## 1. Implementation Status Overview

| Layer | Status | Details |
|-------|--------|---------|
| **Dialect** | ✅ Complete | [`PgSqlSqlDialectImpl`](Drizzle4Dotnet/src/PgSql/PgSqlSqlDialectImpl.cs:7) — double-quote identifiers, `@p0` params |
| **Data Types** | ✅ Complete | [`PgSqlDataType`](Drizzle4Dotnet/src/PgSql/Schema/PgSqlDataType.cs) — Integer, BigInt, Text, Boolean, Timestamp, UUID, etc. |
| **DbClient** | ✅ Complete | [`PgSqlDbClient`](Drizzle4Dotnet/src/PgSql/PgSqlDbClient.cs) — Npgsql connection |
| **Query Builders** | ✅ Complete | `PgSqlSelectQuery` / `PgSqlInsertQuery` / `PgSqlUpdateQuery` / `PgSqlDeleteQuery` |
| **Operators** | ✅ Complete | Core (39 ops) + PgSql-specific (20+ JSON/array/text search operators) |
| **Functions** | ✅ Complete | Core (35+) + PgSql-specific (30+ window/date/json/array/string functions) |
| **Window Functions** | ✅ Complete | RowNumber, Rank, DenseRank, Ntile, Lead, Lag, FirstValue, LastValue, NthValue + OVER builder |
| **Subqueries/CTEs** | ✅ Complete | Typed subqueries, recursive CTEs, compound queries |
| **Migration** | ✅ Complete | Schema snapshot, diff, migration plan generation |
| **CLI** | ✅ Complete | Generate, snapshot, apply, status, debug commands |

## 2. Current File Structure

```
Drizzle4Dotnet/src/PgSql/
├── PgSqlDbClient.cs              # Connection + factory methods
├── PgSqlQueryBuilder.cs           # Offline query builder
├── PgSqlQueryBuilderExtensions.cs # 1–16 column Select/SelectDistinct extensions
├── PgSqlSqlDialectImpl.cs         # Dialect: quoting, params, feature flags, type map
├── PgSqlStatics.cs                # Interval(), TimeZone(), Excluded() factories
├── Schema/
│   ├── PgSqlColumn.cs             # PgSqlColumn type
│   ├── PgSqlDataType.cs           # PgSqlDataType static structs
│   └── PgSqlTable.cs              # PgSqlTable base class
├── Operators/
│   ├── PgSqlFunctions.cs          # Entry point
│   ├── PgSqlFunctions.DateTime.cs # Extract, DateTrunc, DateAdd, DateDiff, AtTimeZone, Age
│   ├── PgSqlFunctions.Info.cs     # PgStatActivity functions
│   ├── PgSqlFunctions.Json.cs     # JsonExtract, JsonAgg, JsonBuildObject, ToJson, RowToJson
│   ├── PgSqlFunctions.Numeric.cs  # Random, etc.
│   ├── PgSqlFunctions.String.cs   # Position, ConcatWs
│   ├── PgSqlOperators.cs          # JSON ops, text search ops, array ops, IS DISTINCT FROM
│   └── Nodes/
│       ├── MssqlTopNode.cs        # (Shared — actually MSSQL, but file here)
│       ├── MssqlTableHintNode.cs  # (Shared)
│       └── MssqlOutputNode.cs     # (Shared)
└── Query/
    ├── PgSqlDeleteQuery.cs        # DELETE ... USING ... RETURNING
    ├── PgSqlInsertQuery.cs        # INSERT ... ON CONFLICT ... RETURNING
    ├── PgSqlMergeQuery.cs         # (Stub — PgSql uses ON CONFLICT, not MERGE)
    ├── PgSqlSelectQuery.cs        # DISTINCT ON, LATERAL joins, FOR UPDATE/SHARE/NOWAIT/SKIP LOCKED
    └── PgSqlUpdateQuery.cs        # UPDATE ... FROM ... RETURNING
```

## 3. Complete Feature Matrix

### 3.1 SELECT Features — Status: ✅ All Implemented

| Feature | Base | PgSql-Specific |
|---------|------|----------------|
| FROM | ✅ `From(table)` | — |
| WHERE | ✅ `Where(cond)` | — |
| GROUP BY / HAVING | ✅ `GroupBy()` / `Having()` | — |
| ORDER BY | ✅ `OrderBy(col, asc)` | — |
| LIMIT / OFFSET | ✅ `Limit()` / `Offset()` | — |
| DISTINCT | ✅ `Distinct()` | ✅ `DistinctOn(cols)` |
| INNER/LEFT/RIGHT JOIN | ✅ | — |
| CROSS JOIN | ✅ | — |
| FULL OUTER JOIN | — | ✅ `FullJoin()` (via IFullOuterJoin) |
| NATURAL JOIN | — | ✅ `NaturalJoin()` / `NaturalLeftJoin()` |
| LATERAL JOIN | — | ✅ `InnerLateralJoin()` / `LeftLateralJoin()` / `CrossLateralJoin()` |
| Row Locking | — | ✅ `ForUpdate()`, `ForNoKeyUpdate()`, `ForShare()`, `ForKeyShare()` |
| Lock Options | — | ✅ `.Nowait()`, `.SkipLocked()`, `.OfTable()` |
| CTEs | ✅ `With()` / `WithRecursive()` | — |
| Subqueries | ✅ `AsSubQuery(alias)` | — |
| Compound Queries | ✅ Union/Intersect/Except | ✅ `IntersectAll()` / `ExceptAll()` |

### 3.2 INSERT Features — Status: ✅ All Implemented

| Feature | Base | PgSql-Specific |
|---------|------|----------------|
| Single row | ✅ `.Value(record)` | — |
| Multi row | ✅ `.Values(records[])` | — |
| Default values | ✅ `.DefaultValues()` | — |
| INSERT ... SELECT | ✅ `.From(subquery)` | — |
| CTE support | ✅ `.With(cte)` | — |
| RETURNING | ✅ via `Returning()` | ✅ `.Returning()` on PgInsertQuery |
| ON CONFLICT DO NOTHING | — | ✅ `DoNothing()` |
| ON CONFLICT DO UPDATE | — | ✅ `DoUpdate()`, `SetOnConflict()`, `SetOnConflictExcluded()` |
| ON CONFLICT ON CONSTRAINT | — | ✅ `OnConflictOnConstraint(name)` |
| Conflict WHERE target | — | ✅ `WhereConflictTarget(cond)` |
| Conflict SET WHERE | — | ✅ `WhereOnConflictSet(cond)` |

### 3.3 UPDATE / DELETE Features

| Feature | Update | Delete |
|---------|--------|--------|
| WHERE | ✅ | ✅ |
| CTE | ✅ | ✅ |
| RETURNING | ✅ (via extension) | ✅ (via extension) |
| FROM / USING join | ✅ `From(table)` (PgSql) | ✅ `Using(table)` (PgSql) |

### 3.4 PgSql Operators — Status: ✅ All Implemented

| Category | Operators |
|----------|-----------|
| JSON | `@>` (contains), `<@` (contained by), `?` (key exists), `?\|` (any key), `?&` (all keys), `->` (path), `->>` (path text), `#>` (obj path), `#>>` (obj path text) |
| Text Search | `@@` (tsquery match with config), `@@` (plain query) |
| Trigram | `%` (similarity), `%%%` (word similarity) |
| Prefix/Distance | `^@` (prefix match), `<->` (distance) |
| Comparison | `IS DISTINCT FROM`, `IS NOT DISTINCT FROM` |
| Quantifiers | `ALL`, `ANY`, `SOME` (subquery) |

### 3.5 PgSql Functions — Status: ✅ All Implemented

| Category | Functions |
|----------|-----------|
| DateTime | `Extract()`, `DateTrunc()`, `DateAdd()`, `DateDiff()`, `AtTimeZone()`, `Age()` (1-arg, 2-arg) |
| JSON | `JsonExtract()`, `JsonExtractText()`, `JsonAgg()`, `JsonBuildObject()`, `ToJson()`, `RowToJson()`, `JsonArrayLength()` |
| Array | `ArrayAgg()`, `Unnest()`, `ArrayLength()`, `ArrayAny()`, `ArrayAll()` |
| String | `Position()`, `ConcatWs()` |
| Numeric | `Random()` |
| Other | `CastPg()` (:: syntax), `ConcatWs()` |
| Window | `RowNumber()`, `Rank()`, `DenseRank()`, `Ntile()`, `Lead()`, `Lag()`, `FirstValue()`, `LastValue()`, `NthValue()` |

## 4. Remaining Work / Gaps

| Item | Priority | Notes |
|------|----------|-------|
| `PgSqlMergeQuery` cleanup | Low | Current stub — PgSql uses `ON CONFLICT` not MERGE; consider removing |
| Some Nodes in wrong directory | Low | `MssqlTopNode` etc. exist under PgSql/Operators/Nodes/ but are MSSQL-specific |
| CLR type map completeness | Medium | Verify all CLR types map correctly to PgSql types |
| Bulk insert optimization | Low | Currently builds row-by-row VALUES; could use `COPY` for large datasets |
