# Drizzle4Dotnet — SQLite Implementation Status & Plan

> **Status:** 🚧 Not Yet Implemented (Planned)  
> **Last Updated:** 2026-06-29  
> **Target:** SQLite 3.35+ with Microsoft.Data.Sqlite

---

## 1. Current Status

SQLite does **NOT** yet have a dialect implementation in the main library. There is no `Drizzle4Dotnet/src/Sqlite/` directory. However, `SharedDemo/Sqlite/` exists with schema definitions, and source generators have produced SQLite-specific generated output (e.g., `SharedDemo_Sqlite_ProjectSelect_generated.g.cs`, `SharedDemo_Sqlite_UserFullSelect_generated.g.cs`).

### 1.1 What Exists

| Component | Status | Location |
|-----------|--------|----------|
| Shared demo schemas | ✅ Partial | `SharedDemo/Sqlite/Schema.cs`, `SharedDemo/Sqlite/DbDto.cs` |
| Source generator output | ✅ Generated | `.g.cs` files with SQLite-specific table/select types |
| Dialect implementation | ❌ Missing | No `SqliteSqlDialectImpl` |
| Data types | ❌ Missing | No `SqliteDataType` |
| DbClient | ❌ Missing | No `SqliteDbClient` |
| Query builders | ❌ Missing | No query types |
| Functions | ❌ Missing | No `SqliteFunctions` |
| Operators | ❌ Missing | No `SqliteOperators` |
| Tests | ❌ Missing | No test files |
| CLI support | ❌ Missing | CLI option parser doesn't list SQLite |

### 1.2 SQLite vs Other Dialects — Key Differences

| Aspect | PostgreSQL | MySQL | MSSQL | **SQLite** |
|--------|------------|-------|-------|------------|
| **Identifier quoting** | `"double quotes"` | `` `backticks` `` | `[square brackets]` | `"double quotes"` |
| **Parameter prefix** | `@p0`, `@p1` | `@p0`, `@p1` | `@p0`, `@p1` | `@p0`, `@p1` (Microsoft.Data.Sqlite) |
| **Schema support** | `schema.table` | `db.table` | `[schema].[table]` | ❌ No schema — single file |
| **LIMIT / OFFSET** | `LIMIT ? OFFSET ?` | `LIMIT ?, ?` | `OFFSET ? FETCH NEXT ?` | `LIMIT ? OFFSET ?` (standard) |
| **RETURNING clause** | ✅ (8.2+) | ❌ | ✅ OUTPUT | ✅ (3.35+) |
| **UPSERT** | `ON CONFLICT DO UPDATE` | `ON DUPLICATE KEY` | `MERGE` | `ON CONFLICT DO UPDATE` (3.24+) |
| **INSERT DEFAULT VALUES** | ✅ | ❌ | ✅ | ❌ (use `DEFAULT` per-column) |
| **UPDATE/DELETE with JOIN** | `FROM` / `USING` | `JOIN` syntax | `FROM t JOIN` | ❌ (use correlated subqueries) |
| **CTEs** | ✅ | ✅ (8.0+) | ✅ | ✅ (3.8.3+) |
| **Recursive CTEs** | ✅ | ✅ (8.0+) | ✅ | ✅ (3.8.3+) |
| **Window functions** | ✅ | ⚠️ (8.0+) | ✅ | ✅ (3.25+) |
| **JSON functions** | Native + `->`/`->>` | `JSON_EXTRACT()` | `JSON_VALUE()` | ✅ `json_extract()` (3.38+) |
| **FULL OUTER JOIN** | ✅ | ❌ | ✅ | ⚠️ (3.39+) |
| **NATURAL JOIN** | ✅ | ✅ | ❌ | ✅ |
| **LATERAL JOIN** | ✅ | ✅ (8.0.14+) | ✅ APPLY | ❌ |
| **`NOW()`** | `NOW()` | `NOW()` | `GETDATE()` | `datetime('now')` |
| **String concat** | `\|\|` | `CONCAT()` | `+` | `\|\|` |
| **Auto-increment** | `GENERATED AS IDENTITY` | `AUTO_INCREMENT` | `IDENTITY(1,1)` | `INTEGER PRIMARY KEY` (auto) |
| **Last inserted ID** | `RETURNING` | `LAST_INSERT_ID()` | `SCOPE_IDENTITY()` | `last_insert_rowid()` |
| **ROWID** | — | — | — | Implicit `rowid` on all tables |
| **DDL transactions** | Not always | Not always | Not always | ✅ DDL is transactional |
| **ALTER TABLE** | Full | Full | Full | ⚠️ Limited (ADD COLUMN only) |

## 2. Implementation Plan

### Phase S1: SQLite Dialect (Foundation)

**Files to create:**
```
Drizzle4Dotnet/src/Sqlite/
├── SqliteSqlDialectImpl.cs    # ISqlDialect implementation
```

**Key behaviors:**
- Identifier quoting: `"double quotes"` (like PostgreSQL)
- Parameter prefix: `@p0`, `@p1` (Microsoft.Data.Sqlite compatible)
- No schema support — `BuildTableName()` ignores schema
- `BuildLimitOffset()`: Standard `LIMIT @p0 OFFSET @p1` (same as `SqlDialectDefaults`)
- No `BuildLimitOffsetForUpdateDelete()` — SQLite doesn't support LIMIT in UPDATE/DELETE directly

**Feature flags:**
```csharp
SupportsReturning = true        // SQLite 3.35+
SupportsArrays = false
SupportsJson = true             // 3.38+ built-in
SupportsWindowFunctions = true  // 3.25+
SupportsCte = true              // 3.8.3+
SupportsRecursiveCte = true     // 3.8.3+
SupportsDeleteUsing = false     // No JOIN in DELETE
SupportsIsDistinctFrom = false
SupportsFilteredAggregates = false
SupportsFullOuterJoin = true    // 3.39+
SupportsNaturalJoin = true
SupportsLateralJoin = false
SupportsApplyJoin = false
```

### Phase S2: SQLite Data Types & Interfaces

**Files to create:**
```
├── Schema/
│   ├── SqliteColumn.cs         # SqliteColumn type
│   ├── SqliteDataType.cs       # SqliteDataType static structs
│   └── SqliteTable.cs          # SqliteTable base class
```

**CLR-to-SQLite type mapping:**
```csharp
typeof(int) → SqliteDataType.Integer
typeof(long) → SqliteDataType.Integer  (INTEGER = 64-bit in SQLite)
typeof(short) → SqliteDataType.Integer
typeof(string) → SqliteDataType.Text
typeof(bool) → SqliteDataType.Integer  (0 or 1)
typeof(decimal) → SqliteDataType.Real
typeof(float) → SqliteDataType.Real
typeof(double) → SqliteDataType.Real
typeof(DateTime) → SqliteDataType.Text (ISO 8601 text)
typeof(byte[]) → SqliteDataType.Blob
typeof(Guid) → SqliteDataType.Text     (or BLOB)
```

### Phase S3: SQLite Query Types

```
├── Query/
│   ├── SqliteSelectQuery.cs    # SELECT with DISTINCT, LIMIT/OFFSET, RETURNING
│   ├── SqliteInsertQuery.cs    # INSERT with ON CONFLICT, RETURNING
│   ├── SqliteUpdateQuery.cs    # UPDATE with RETURNING
│   └── SqliteDeleteQuery.cs    # DELETE with RETURNING
```

SQLite has relatively fewer dialect-specific features:
- **SELECT**: Standard features only; no FOR UPDATE, no DISTINCT ON
- **INSERT**: `ON CONFLICT DO UPDATE/NOTHING` (like PostgreSQL, but `EXCLUDED` uses `EXCLUDED` like PgSql)
- **UPDATE**: No JOIN support — must use correlated subqueries
- **DELETE**: No JOIN support — must use correlated subqueries
- **RETURNING**: Supported on all DML (3.35+), similar to PostgreSQL

### Phase S4: SQLite Functions

```
├── Operators/
│   ├── SqliteFunctions.cs              # Entry point
│   ├── SqliteFunctions.DateTime.cs     # datetime(), date(), time(), strftime(), julianday()
│   ├── SqliteFunctions.String.cs       # instr(), substr(), replace(), trim(), ltrim(), rtrim(), length(), upper(), lower()
│   ├── SqliteFunctions.Numeric.cs      # random(), abs(), round(), total_changes()
│   ├── SqliteFunctions.Json.cs         # json_extract(), json_object(), json_array(), json_set(), json_type()
│   ├── SqliteFunctions.Info.cs         # last_insert_rowid(), sqlite_version(), changes(), total_changes()
│   └── SqliteOperators.cs              # REGEXP (if loaded), MATCH (FTS)
```

**Key SQLite-specific functions:**
| Function | SQL |
|----------|-----|
| `SqliteFunctions.DateTime(expr)` | `datetime(expr)` |
| `SqliteFunctions.Date(expr)` | `date(expr)` |
| `SqliteFunctions.Time(expr)` | `time(expr)` |
| `SqliteFunctions.Strftime(fmt, expr)` | `strftime(format, expr)` |
| `SqliteFunctions.JulianDay(expr)` | `julianday(expr)` |
| `SqliteFunctions.LastInsertRowId()` | `last_insert_rowid()` |
| `SqliteFunctions.Instr(str, substr)` | `instr(str, substr)` |
| `SqliteFunctions.TotalChanges()` | `total_changes()` |

### Phase S5: Connection & Execution

```
└── SqliteDbClient.cs           # DbClient + factory methods
```

- Uses `Microsoft.Data.Sqlite` NuGet package
- `SqliteDbClient` extends `DbClient<SqliteSqlDialectImpl>`
- `SqliteQueryBuilder` with offline query building

### Phase S6: Source Generator Support

- Add SQLite dialect info to source generator's `DialectInfo` mapping
- Generate `SqliteTable`, `SqliteColumn`, `SqliteQueryBuilderExtensions` equivalents

### Phase S7: Migration / DDL Layer

SQLite has limited DDL support:
- **ALTER TABLE**: Only `ADD COLUMN` — no `DROP COLUMN`, no `ALTER COLUMN`
- Migration strategy: For complex changes, must recreate the table (CREATE new → INSERT SELECT → DROP old → RENAME)
- **Column types**: SQLite uses type affinity (TEXT, NUMERIC, INTEGER, REAL, BLOB) — not strict typing
- The `TableDefinitionComparer` and `SchemaSnapshot` system works, but DDL queries need SQLite-specific handling

### Phase S8: Testing

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

## 3. Priority Task Breakdown

| Task | Priority | Estimated Effort | Dependencies |
|------|----------|------------------|--------------|
| S1: SqliteSqlDialectImpl | 🔴 High | 1 day | None |
| S2: Data types + schema | 🔴 High | 1 day | S1 |
| S5: SqliteDbClient | 🔴 High | 1 day | S1, S2 |
| S3: Query types (SELECT/INSERT/UPDATE/DELETE) | 🟡 Medium | 2 days | S1 |
| S4: Functions | 🟡 Medium | 2 days | S1 |
| S6: Source generator support | 🟡 Medium | 1 day | S1, S2 |
| S7: Migration DDL | 🟠 Low | 2 days | S1, S2, S3 |
| S8: Tests | 🟠 Low | 2 days | All above |
| CLI integration | 🟠 Low | 0.5 day | S5 |

## 4. Key Design Decisions

1. **Use `Microsoft.Data.Sqlite`** — It's the official .NET SQLite provider, follows `DbProviderFactory` pattern, and uses `@named` parameters like other providers.

2. **No schema support** — SQLite stores all tables in a single file with no schema hierarchy. `BuildTableName()` should return just the table name.

3. **RETURNING support** — SQLite 3.35+ supports `RETURNING` clause on INSERT/UPDATE/DELETE, similar to PostgreSQL. This should be used instead of `last_insert_rowid()` for modern SQLite.

4. **ON CONFLICT** — SQLite's `ON CONFLICT DO UPDATE/NOTHING` is similar to PostgreSQL, using `EXCLUDED` pseudo-table. Reuse PgSql-like upsert logic.

5. **DDL limitations** — SQLite's `ALTER TABLE` only supports `ADD COLUMN`. The migration system needs a fallback: CREATE NEW TABLE → COPY DATA → DROP OLD → RENAME.

6. **Type affinity** — SQLite doesn't enforce strict column types. All types map to INTEGER, REAL, TEXT, BLOB, or NUMERIC. This simplifies the data type system.
