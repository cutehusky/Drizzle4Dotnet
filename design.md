# Class Design Overview — Drizzle4Dotnet

## 1. Architecture Philosophy

Drizzle4Dotnet is a **C# ORM / query builder** inspired by [Drizzle ORM](https://orm.drizzle.team/), designed around **type-safe SQL generation** using C# generics. The architecture follows several key patterns:

- **Builder Pattern**: Query objects are built by chaining fluent methods (`.Where()`, `.Limit()`, `.OrderBy()`), and are executed via `await` (using `TaskAwaiter`).
- **Strategy Pattern via Static Abstract Members**: Each database dialect (PgSql, MySql, Mssql, Oracle) implements [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:5) using C# 11's `static abstract` interface members, allowing compile-time dispatch without runtime overhead.
- **Generic Virtual Table System**: Queries carry a `TVirtualTable` type parameter enabling subquery/CTE composition with full type safety.
- **Source Generation**: Table schemas with strongly-typed columns are generated via source generators, producing `DbColumn<T, TTable, TDialect>` static properties.

---

## 2. Core SQL Abstraction Layer

The foundation of the system is the SQL expression hierarchy in [`Drizzle4Dotnet/src/Core/Shared/ISql.cs`](Drizzle4Dotnet/src/Core/Shared/ISql.cs).

### 2.1 SQL Builder

```
ISqlBuilder                          # Interface: Append(), AddParameter(), Build()
  └── SqlBuilder<TDialect>           # Implementation using StringBuilder + Dictionary<name, value>
```

- [`ISqlBuilder`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:9): Defines `Append(string)`, `Append(char)`, `AddParameter(object?)`, and `Build()` which returns `(string sql, Dictionary<string, object?> parameters)`.
- [`SqlBuilder<TDialect>`](Drizzle4Dotnet/src/Core/Shared/ISql.cs:17): Concrete builder parameterized by dialect. Uses `TDialect.BuildParameterName()` for dialect-appropriate parameter naming.

### 2.2 SQL Expression Interfaces

```
IGenericSql                          # BuildSql(ISqlBuilder) — all SQL nodes implement this
  ├── ISql<TReturn>                  # Typed SQL producing TReturn
  │    └── ISql                      # Marker interface for untyped SQL
  └── IAliasedSql<T>                 # SQL with an alias, inherits ISql<T>
       ├── IColumn<T>                # Base column interface
       │    ├── DbColumn<T,TTable,TDialect>     # Database column (from table schema)
       │    └── VirtualColumn<T,TDialect>       # Virtual column (subquery/alias reference)
       └── AliasedSql<T>             # Wraps any ISql<T> + alias string
```

### 2.3 Raw SQL Types

```
RawSql                               # Untyped raw SQL with parameter substitution
RawSql<TReturn>                      # Typed raw SQL
RawSubqueryTableSql<TDialect>        # Raw SQL wrapped as a virtual table (subquery or CTE)
```

---

## 3. Table & Column Schema Layer

### 3.1 Table Type Hierarchy

```
IGenericTable<TDialect>              # BuildRefSql() — can be referenced in FROM/JOIN
  ├── ITable<TDialect>               # Has static abstract TableRefName
  │    ├── IDbTable<TDialect>        # Has static abstract TableName + SchemaName
  │    └── ITableAlias<TDialect>     # Has instance Alias property
  ├── IVirtualTable<TDialect>        # Subquery/CTE with static Create() factory + BuildSql()
  └── ICteTable<TDialect>            # CTE table with BuildSql()
```

- [`ITable<TDialect>`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:10): Uses `static abstract string TableRefName` to get the fully-qualified table reference string (e.g., `"public"."users"`).
- [`IDbTable<TDialect>`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:27): Adds `TableName` and `SchemaName` static properties.
- [`IVirtualTable<TDialect>`](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs:20): Represents subqueries and CTEs. Has a static factory `Create(IGenericSql, string, object)` used by source-generated tuples.

### 3.2 Column Architecture

```
IColumn<T> : IAliasedSql<T>          # Marker interface for typed columns
IColumnOfTable<TTable>               # Non-generic column reference (Identifier property)
  │
DbColumn<T, TTable, TDialect>        # Database column — uses TDialect for name quoting
VirtualColumn<T, TDialect>           # Column from subquery/alias reference
```

- [`DbColumn<T, TTable, TDialect>`](Drizzle4Dotnet/src/Core/Schema/Columns/DbColumn.cs:6) is the runtime representation of a generated column property. Its `BuildSql()` renders `"tablename"."columnname"` using the dialect's naming conventions.
- [`VirtualColumn<T, TDialect>`](Drizzle4Dotnet/src/Core/Schema/Columns/VirtualColumn.cs:5) represents columns from subqueries, CTEs, or aliased references. Its `BuildSql()` renders `"alias"."columnname"`.

### 3.3 Schema Attributes (Table-Level)

Defined in [`Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs):

| Attribute | Purpose |
|---|---|
| [`[Table(name, schema)]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:9) | Marks a class as a database table; specifies DB name and schema |
| [`[Alias(table, alias)]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:30) | Self-join alias declaration |
| [`[Virtual]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:43) | Marks a virtual table class |
| [`[ForeignKeyKeyConstraint]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:54) | FOREIGN KEY constraint (repeatable) |
| [`[UniqueConstraint]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:74) | UNIQUE constraint (repeatable) |
| [`[PrimaryKeyTableConstraint]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:88) | Composite PRIMARY KEY (repeatable) |
| [`[CheckTableConstraint]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:102) | CHECK constraint (repeatable) |
| [`[Index]`](Drizzle4Dotnet/src/Core/Schema/Tables/Attributes.cs:117) | Database index specification (repeatable) |

### 3.4 Column Attributes

Defined in [`Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs):

| Attribute | Purpose |
|---|---|
| [`[Column(name)]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:11) | Maps a property to a DB column |
| [`[PrimaryKey]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:58) | Marks column as primary key |
| [`[DefaultValue(value)]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:65) | Default value expression (raw SQL) |
| [`[AutoIncrement]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:75) | Auto-increment / identity |
| [`[NotNull]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:81) | Forces NOT NULL |
| [`[Nullable]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:88) | Forces nullable |
| [`[Check(expression)]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:94) | CHECK constraint expression |
| [`[Comment(comment)]`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:104) | Column comment |
| [`SqlTypeAttribute`](Drizzle4Dotnet/src/Core/Schema/Columns/Attributes.cs:120) | Abstract base for dialect-specific type overrides |

---

## 4. Query System

### 4.1 Query Base Classes

```
QueryBase<TDialect>                                 # Abstract base: Build(), BuildSql(), ValidateQuery(), CTE list
  ├── Query<TDialect>                               # Non-returning: ExecuteAsync(), Returning<>()
  │    ├── InsertQuery<TTable,TDialect,TSelf>       # INSERT INTO ... VALUES | SELECT | DEFAULT VALUES
  │    ├── UpdateQuery<TTable,TDialect,TSelf>       # UPDATE ... SET ... WHERE ...
  │    └── DeleteQuery<TTable,TDialect,TSelf>       # DELETE FROM ... WHERE ... (see Delete/)
  │
  └── Query<TReturn,TDialect,TVirtualTable>         # Returning: ExecuteGetListAsync(), AsSubQuery()
       ├── SelectQuery<TReturn,TDialect,TVirtualTable,TSelf>   # SELECT ... FROM ... JOIN ... WHERE ...
       └── CompoundQuery<TReturn,TDialect,TVirtualTable>       # UNION / INTERSECT / EXCEPT
```

- [`QueryBase<TDialect>`](Drizzle4Dotnet/src/Core/Query/QueryBase.cs:8): Provides `Build()` method that creates a `SqlBuilder<TDialect>`, calls `BuildSql()`, and returns `(sql, parameters)`. Contains the CTE list that all queries share.
- [`Query<TDialect>`](Drizzle4Dotnet/src/Core/Query/Query.cs:7): Non-returning query (INSERT/UPDATE/DELETE). Implements `IAwaitableQuery` (no-return-value await), and provides `Returning<>()` extension for DML statements that produce output.
- [`Query<TReturn, TDialect, TVirtualTable>`](Drizzle4Dotnet/src/Core/Query/Query.cs:25): Returning query (SELECT, compound). Implements `IAwaitableQuery<List<TReturn>>` and provides `AsSubQuery()` for nesting.

### 4.2 Query Clause Interfaces

Defined in [`Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs):

| Interface | Methods | Implemented By |
|---|---|---|
| [`ISupportWhere<TQuery>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:12) | `Where(IGenericSql)` / `Where(params IGenericSql[])` | Select, Update, Delete |
| [`ISupportOrderBy<TQuery>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:28) | `OrderBy(col, asc)` / `OrderBy(params (col, asc)[])` | Select, Compound |
| [`ISupportOffsetLimit<TQuery>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:41) | `Limit(int)` / `Offset(int)` | Select, Compound |
| [`ISupportCte<TQuery,TDialect>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:57) | `With(ICteTable[])` / `WithRecursive(ICteTable[])` | Select, Insert, Update, Delete, Compound |
| [`IJoin<TQuery,TDialect>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:73) | `InnerJoin`, `LeftJoin`, `RightJoin`, `CrossJoin` | Select |
| [`ISupportDistinct<TQuery>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:131) | `Distinct()` | Select |
| [`ISupportInsertValue<TQuery,TTable,TDialect>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:158) | `Value(IInsertRecord)` / `Values(...)` | Insert |
| [`ISupportUpdateSet<TQuery,TTable,TDialect>`](Drizzle4Dotnet/src/Core/Shared/QueryClauseInterfaces.cs:187) | `Set(column, value)` / `Set(IUpdateRecord)` | Update |

### 4.3 Returning Query Support

```
IReturning<TReturn, TDialect, TVirtualTable> : ISql<TReturn>
  └── ReturningQuery<TReturn, TDialect, TVirtualTable>   # Wraps a Query<TDialect> + SelectedColumns, appends RETURNING
```

- [`IReturning<T,D,VT>`](Drizzle4Dotnet/src/Core/Shared/IReturning.cs:7): Interface for any query that can return typed results. Provides `Mapper` (from `SelectedColumns.Mapper`) and `AsSubQuery()`.
- [`ReturningQuery<T,D,VT>`](Drizzle4Dotnet/src/Core/Query/ReturningQuery.cs:7): Concrete wrapper that takes a base `Query<TDialect>`, calls its `BuildSql()`, and appends ` RETURNING <columns>`.
- [`QueryReturningExtensions`](Drizzle4Dotnet/src/Core/Query/QueryReturningExtensions.cs:9): 1–16 column overloads of `Returning()` that create `TypedTupleSelectedColumns` and return correctly typed `ReturningQuery<Tuple>`.

### 4.4 Selected Columns Architecture

```
ISelectedColumns<TReturn,TDialect,TVirtualTable> : ISql    # BuildSql() + Mapper(DbDataReader)
  └── ITypedTupleSelectedColumns<TReturn,TDialect,TVirtualTable> : IGetFieldByName
       └── TypedTupleSelectedColumns<T1..T16, TDialect>     # Source-generated 1–16 column overloads
```

- The `Mapper` function reads from a `DbDataReader` and maps columns by ordinal to the return type.
- For value tuples, the mapper reads each column by position and creates `(T1, T2, ...)`.

### 4.5 Compound Query Set Operations

```
CompoundQuery<T,D,VT>                # Wraps left + right IReturning + operation string
  └── CompoundQueryExtensions        # .Union(), .UnionAll(), .Intersect(), .Except(), .IntersectAll(), .ExceptAll()
                                     # .AsRecursiveCte() for recursive CTE wrapping
```

### 4.6 Subquery & CTE Types

```
TypedTupleGeneratedSubqueryTable<T,D>                  # Subquery from typed tuple columns
  └── TypedTupleAnonymousGeneratedSubqueryTable<TShape,T,D>  # Subquery with shape function
TypedTupleGeneratedCteTable<T,D>                       # CTE from typed tuple columns
  └── TypedTupleAnonymousGeneratedCteTable<TShape,T,D>       # CTE with shape function
RecursiveCteTable<T,D>                                 # Wraps compound query as recursive CTE
```

---

## 5. Migration System

### 5.1 Migration Data Model

```
TableDefinition                                        # Complete table structure
  ├── TableName / SchemaName
  ├── IReadOnlyList<IColumnDefinition> Columns         # Column definitions
  ├── IReadOnlyList<TableConstraint> TableConstraints   # FK, UNIQUE, PK, CHECK
  └── IReadOnlyList<TableIndex> Indexes                # Database indexes

IColumnDefinition                                      # Non-generic column interface
  ├── ColumnDefinition                                 # Concrete implementation with typed ISqlDataType
  └── RawColumnDefinition                              # Internal helper used by snapshot deserialization
```

### 5.2 Constraint Type Hierarchy

```
TableConstraint (abstract)                             # BuildSql() + ConstraintName
  ├── RawTableConstraint                               # Raw SQL fallback
  ├── ForeignKeyConstraint                             # FOREIGN KEY (cols) REFERENCES tbl (fcols)
  ├── UniqueConstraint                                 # UNIQUE (cols)
  ├── PrimaryKeyTableConstraint                        # PRIMARY KEY (cols)
  └── CheckTableConstraint                             # CHECK (expr)

TableIndex                                             # Index with full properties (not part of hierarchy)
  ├── IndexName / SchemaName / TableName
  ├── Columns[] / IsUnique / IndexType / Where
  └── BuildSql() — generates CREATE INDEX
```

### 5.3 Snapshot / Diff / Migration Plan

```
SchemaSnapshot                  # JSON-serializable schema state
  ├── Serialize() / Deserialize()
  ├── FromTableDefinitions()    # Build from TableDefinition[]
  ├── CreateNew()               # All tables = Added
  ├── CreateRollback()          # All tables = Removed
  └── Compare(SchemaSnapshot)   # Diff two snapshots → SchemaDiff

SchemaDiff                      # List of TableChange
  └── ToMigrationPlan(name)     # Generate ordered DDL steps

TableChange                     # Per-table change descriptor
  ├── ChangeType: Added / Removed / Modified
  ├── ColumnChanges[] / AddedConstraints[] / RemovedConstraints[]
  └── AddedIndexes[] / RemovedIndexes[]

MigrationPlan                   # Ordered list of MigrationStep
  └── ToSql<TDialect>()         # Render full SQL script

MigrationStep                   # Single DDL step
  ├── StepType: CreateSchema|Database|Table|DropTable|AlterTable|CreateIndex|DropIndex
  └── ToSql<TDialect>()
```

### 5.4 Migration Query Builders

```
ISql (implemented by DDL queries)
  ├── CreateTableQuery(tableDef)      # CREATE TABLE (...)
  ├── DropTableQuery(name, schema)    # DROP TABLE [IF EXISTS]
  ├── AlterTableQuery(name, schema)   # ALTER TABLE ... ADD/DROP/MODIFY/ALTER
  ├── CreateIndexQuery(name,table)    # CREATE [UNIQUE] INDEX ... ON ... USING ...
  ├── DropIndexQuery(name)            # DROP INDEX [IF EXISTS]
  ├── CreateSchemaQuery(name)         # CREATE SCHEMA [IF NOT EXISTS]
  └── CreateDatabaseQuery(name)       # CREATE DATABASE [IF NOT EXISTS]
```

### 5.5 Schema Comparison

```
TableDefinitionComparer              # Static class
  ├── Compare(old, new)              # Single table diff → TableDiffResult
  ├── CompareSets(oldDict, newDict)  # Multi-table diff
  ├── ToSql<TDialect>(...)           # Convenience → SQL script
  └── GetColumnModifications()       # Detect type/nullability/default changes
```

### 5.6 ORM Schema Exporter

```
OrmSchemaExporter                   # Static class — reflection-based
  ├── GetTableDefinition<TTable,TDialect>()   # Extract from generated table type
  ├── GetTableDefinitions<TDialect>(types[])  # Bulk extraction
  ├── CreateSchemaSnapshot<TDialect>(name, types[])
  └── CreateTable<TTable,TDialect>()          # Convenience → CreateTableQuery
```

Internally uses reflection to:
1. Read `TableName`/`SchemaName` static properties
2. Enumerate `DbColumn<,,>` static properties
3. Map CLR types to dialect-specific SQL types via `TDialect.ClrToSqlTypeMap`
4. Extract table-level constraints/indexes from attributes

---

## 6. Database Client & Execution

### 6.1 Client Hierarchy

```
DbClient<TDialect> : IQueryExecutor<TDialect>, IAsyncDisposable
  ├── PgSqlDbClient
  ├── MySqlDbClient
  ├── MssqlDbClient
  └── OracleDbClient

DbClientWithTransaction<TInstance,TDialect> : DbClient<TDialect>
  └── BeginTransactionAsync() / CommitAsync() / RollbackAsync()
      / RunInTransactionAsync(Func<TInstance, Task>)
```

- [`DbClient<TDialect>`](Drizzle4Dotnet/src/Core/DbClient.cs:8): Provides `CreateCommandAsync()` which builds SQL via `SqlBuilder<TDialect>`, creates a `DbCommand`, and populates parameters. Implements `IQueryExecutor<TDialect>`.
- [`DbClientWithTransaction<TInstance,TDialect>`](Drizzle4Dotnet/src/Core/DbClient.cs:102): Adds transaction support via the abstract `CreateInstance()` factory method (CRTP pattern for returning the concrete type).

### 6.2 Query Execution

```
IQueryExecutor<TDialect>
  ├── ExecuteGetListAsync<T,TVT>(IReturning)    → List<T>
  ├── ExecuteAsync(IGenericSql)                  → void
  ├── ExecuteScalarAsync<T>(IGenericSql)         → T?
  └── ExecuteReaderAsync(IGenericSql)            → DbDataReader
```

---

## 7. Dialect System

### 7.1 Dialect Interface

[`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:5) uses C# 11 `static abstract` members:

| Category | Members |
|---|---|
| **Identifier/Naming** | `BuildIdentifier()`, `BuildTableName()`, `BuildColumnName()`, `BuildParameterName()` (string + int overloads) |
| **Limit/Offset** | `BuildLimitOffset()`, `BuildLimitOffsetForUpdateDelete()` |
| **Feature Flags** | `SupportsReturning`, `SupportsArrays`, `SupportsJson`, `SupportsWindowFunctions`, `SupportsCte`, `SupportsRecursiveCte`, `SupportsDeleteUsing`, `SupportsIsDistinctFrom`, `SupportsFilteredAggregates`, `SupportsFullOuterJoin`, `SupportsNaturalJoin`, `SupportsLateralJoin`, `SupportsApplyJoin` |
| **String Escaping** | `EscapeString()` |
| **Type Mapping** | `ClrToSqlTypeMap` — `Dictionary<Type, ISqlDataType>` |

### 7.2 Dialect Implementations

```
ISqlDialect (static abstract interface)
  ├── PgSqlSqlDialectImpl        # PostgreSQL: double-quote identifiers, @p0 params, all features
  ├── MySqlSqlDialectImpl        # MySQL: backtick quoting, @p0 params, limited features
  ├── MssqlSqlDialectImpl        # SQL Server: bracket quoting, @p0 params, some features
  └── OracleSqlDialectImpl       # Oracle: double-quote, :p0 params, some features
```

### 7.3 SQL Data Types

```
ISqlDataType                       # Sql property (string)
  ├── RawSqlDataType               # Raw string data type
  ├── PgSqlDataType (static)       # PostgreSQL types: Integer, BigInt, Text, Boolean, ...
  ├── MySqlDataType (static)       # MySQL types
  ├── MssqlDataType (static)       # SQL Server types
  └── OracleDataType (static)      # Oracle types
```

### 7.4 SQL Builder Utilities

```
SqlStatics                         # Static helper methods
  ├── BuildClause()                # Generic clause builder (WHERE, HAVING, etc.)
  ├── BuildSqlSetClause()          # UPDATE SET clause
  ├── BuildSetValues()             # SET value pairs (also for ON CONFLICT/DUPLICATE KEY)
  ├── BuildInsertColumnList()      # INSERT column list
  ├── BuildInsertRowValues()       # INSERT VALUES rows
  ├── BuildSqlJoins()              # JOIN clause rendering
  ├── BuildSqlCte()                # WITH / WITH RECURSIVE clause
  └── BuildSqlOrderBy()            # ORDER BY clause

SqlDialectDefaults                 # Default dialect behavior (LIMIT/OFFSET, escaping)
```

---

## 8. Operator System

### 8.1 Core Operator Architecture

```
Operators (static partial class)   # Extension methods + operator constants
  ├── Operators.Comparison.cs      # Eq, Neq, Gt, Lt, Gte, Lte, IsNull, IsNotNull
  ├── Operators.Logical.cs         # And, Or, Not
  ├── Operators.Arithmetic.cs      # Add, Sub, Mul, Div, Mod
  ├── Operators.String.cs          # Like, NotLike, Concat
  ├── Operators.Collection.cs      # In, NotIn, Between, NotBetween
  └── Operators.Range.cs           # Range operators

Functions (static partial class)   # SQL function wrappers
  ├── Functions.Aggregate.cs       # Count, Sum, Avg, Min, Max
  ├── Functions.String.cs          # Concat, Substring, Trim, Upper, Lower, Length
  ├── Functions.Numeric.cs         # Abs, Ceil, Floor, Round, Power, Sqrt
  ├── Functions.DateTime.cs        # Now, CurrentDate, Extract, DateTrunc, ToChar
  ├── Functions.Casting.cs         # Cast expression builder
  └── Functions.Conditional.cs     # Coalesce, NullIf, Case/When
```

### 8.2 SQL Expression Nodes

```
IGenericSql
  ├── BinaryNode                   # a Op b (e.g., col = value)
  ├── UnaryNode                    # Op a (e.g., NOT, IS NULL)
  ├── NnaryNode                    # Op(a, b, c) (e.g., IN, BETWEEN)
  ├── TrinaryNode                  # a Op1 b Op2 c (e.g., BETWEEN)
  ├── FunctionCallNode             # FuncName(args) + optional filter/order
  ├── CaseNode                     # CASE WHEN ... THEN ... ELSE ... END
  ├── CastNode                     # CAST(expr AS type)
  ├── FilteredAggregateNode        # AGG(...) FILTER (WHERE ...)
  └── SqlValueNode                 # Literal value wrapper
```

### 8.3 Dialect Operator Extensions

| Dialect | Extensions |
|---|---|
| **PgSql** | `PgOperators` (`@>`, `<@`, `?`, `?|`, `?&`, `->`, `->>`, `#>`, `#>>`, `@@`, `%%`, `^@`, `#-#`) |
| **MySql** | `MySqlOperators` (`<=>` null-safe equals), `MySqlOperators.Regex` (`REGEXP`, `NOT REGEXP`, `RLIKE`) |
| **Mssql** | `MssqlOperators` |
| **Oracle** | `OracleOperators` |

---

## 9. CLI Tool

### 9.1 Command Architecture

```
Program (entry point)              # Parses first arg → dispatches to Handle[Command]()
  ├── GenerateCommand              # Generates migration SQL + journal
  ├── SnapshotCommand              # Creates schema snapshot JSON
  ├── ApplyCommand                 # Applies pending migrations to DB
  ├── StatusCommand                # Shows migration status
  └── DebugCommand                 # Debug/inspection of schema
```

### 9.2 CLI Services

```
CliOptionParser                    # CLI argument parsing + option descriptions
ProjectBuilder                     # Builds .csproj → .dll via dotnet build
MigrationGenerator                 # Core migration generation logic
```

### 9.3 Generate Flow

1. Parse CLI options (provider, name, types, assembly/project path)
2. Build project or load assembly
3. Discover table types (via `--types` or auto-discovery scanning for `*Table` types)
4. Load existing snapshot JSON if available
5. Extract [`TableDefinition`](Drizzle4Dotnet/src/Core/Schema/Migration/TableDefinition.cs:232) via [`OrmSchemaExporter`](Drizzle4Dotnet/src/Core/Schema/Migration/OrmSchemaExporter.cs:14) reflection
6. Create [`SchemaSnapshot.FromTableDefinitions()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:44)
7. Compare with existing snapshot via [`SchemaSnapshot.Compare()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:118)
8. Generate [`MigrationPlan`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:731) via [`SchemaDiff.ToMigrationPlan()`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:524)
9. Write migration SQL file + migration journal JSON + new snapshot JSON

---

## 10. Key Design Patterns Summary

| Pattern | Usage |
|---|---|
| **Fluent Builder** | Query methods return `TSelf` for chaining |
| **CRTP (Curiously Recurring Template)** | `SelectQuery<T,D,VT,TSelf>` where `TSelf : SelectQuery<...>` enables fluent return types in subclasses |
| **Strategy (static abstract)** | [`ISqlDialect`](Drizzle4Dotnet/src/Core/Shared/ISqlDialect.cs:5) with C# 11 static abstract members for compile-time dispatch |
| **Virtual Table / Type-Safe Subquery** | `TVirtualTable` type parameter enables subquery composition with preserved column types |
| **Tuple-Based Column Selection** | 1–16 column overloads mapping to `ValueTuple<T1..T16>` |
| **Awaitable Query** | `GetAwaiter()` returns `TaskAwaiter<List<T>>` or `TaskAwaiter` for `await` support |
| **Snapshot Diffing** | [`SchemaSnapshot`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:10) → compare → [`SchemaDiff`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:515) → [`MigrationPlan`](Drizzle4Dotnet/src/Core/Schema/Migration/SchemaSnapshot.cs:731) |
| **Reflection-Based Schema Export** | [`OrmSchemaExporter`](Drizzle4Dotnet/src/Core/Schema/Migration/OrmSchemaExporter.cs:14) extracts schema from source-generated classes |
| **Static Utility Methods** | [`SqlStatics`](Drizzle4Dotnet/src/Core/Shared/SqlStatics.cs:9) for reusable SQL fragment building without runtime overhead |
