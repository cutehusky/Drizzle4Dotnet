# Drizzle4Dotnet — Class Design & Relationships

> This document details the class hierarchy, interface contracts, inheritance chains, and relationships between all major types in Drizzle4Dotnet.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Core Type Hierarchy](#2-core-type-hierarchy)
3. [Query Builder Class Hierarchy](#3-query-builder-class-hierarchy)
4. [Interface Contracts](#4-interface-contracts)
5. [Schema / Table / Column Class Design](#5-schema--table--column-class-design)
6. [SQL Expression Nodes (AST)](#6-sql-expression-nodes-ast)
7. [Operator & Function Classes](#7-operator--function-classes)
8. [Dialect System](#8-dialect-system)
9. [Migration / DDL Class Design](#9-migration--ddl-class-design)
10. [Source Generator Classes](#10-source-generator-classes)
11. [Execution / DbClient Class Design](#11-execution--dbclient-class-design)
12. [Window Function Classes](#12-window-function-classes)
13. [Utility & Helper Classes](#13-utility--helper-classes)

---

## 1. Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                        DbClient<TDialect>                           │
│  Abstract base: connection mgmt, query factory, async execution     │
└──────┬────────────────────────────────────┬─────────────────────────┘
       │                                    │
       ▼                                    ▼
┌─────────────────────┐          ┌──────────────────────────┐
│  Query<TDialect>    │          │  Query<TReturn, TDialect>│
│  DML base (no return)│          │  SELECT base (has return)│
└──────┬──────────────┘          └──────────┬───────────────┘
       │                                    │
       ▼                                    ▼
┌─────────────────────┐          ┌──────────────────────────┐
│  InsertQuery        │          │  SelectQuery              │
│  UpdateQuery        │          │  (CRTP pattern)           │
│  DeleteQuery        │          └──────────┬───────────────┘
│  (CRTP pattern)     │                     │
└──────┬──────────────┘                     ▼
       │                          ┌──────────────────────────┐
       ▼                          │  PgSelectQuery           │
┌─────────────────────┐          │  MySqlSelectQuery         │
│  PgInsertQuery      │          │  (dialect-specific)       │
│  PgUpdateQuery      │          └──────────────────────────┘
│  PgDeleteQuery      │
│  MySqlInsertQuery   │
│  MySqlUpdateQuery   │
│  MySqlDeleteQuery   │
│  MySqlReplaceQuery  │
└─────────────────────┘
```

---

## 2. Core Type Hierarchy

### 2.1 Query Base Classes

```
QueryBase<TDialect>                         (abstract)
├── IGenericSql                             (interface: BuildSql)
│
├── Fields:
│   ├── DbClient<TDialect> DbClient        (reference to parent client)
│   ├── List<ICteTable<TDialect>> CteTables (registered CTEs)
│   └── bool Recursive                     (whether CTE is recursive)
│
├── Methods:
│   ├── BuildSql(ISqlBuilder)              (abstract — override in subclasses)
│   ├── Build() → (string, Dictionary)     (compiles to SQL + params)
│   ├── BuildSqlCte(ISqlBuilder)           (renders WITH / WITH RECURSIVE prefix)
│   └── AppendClause(...)                  (utility for rendering clauses)
│
├── Query<TDialect>                        (DML queries — no return type)
│   ├── ISql                              (interface)
│   ├── GetAwaiter() → TaskAwaiter        (awaitable — calls ExecuteAsync)
│   ├── Returning<TReturn>(selectedColumns) → ReturningQuery
│   └── Returning<TReturn, TVirtualTable>(selectedColumns) → ReturningQuery
│
├── Query<TReturn, TDialect>              (SELECT queries — has return type)
│   ├── IReturning<TReturn, TDialect>     (interface)
│   ├── Fields: SelectedColumns
│   ├── GetAwaiter() → TaskAwaiter<List<TReturn>>
│   ├── AsSubQuery(alias) → RawSubqueryTableSql
│   └── AsSubQuery<T>(alias, selector) → TypedTupleAnonymousGeneratedSubqueryTable
│
└── Query<TReturn, TDialect, TVirtualTable>  (SELECT with virtual table)
    ├── IReturning<TReturn, TDialect, TVirtualTable>
    ├── AsSubQuery(alias) → TVirtualTable
    └── AsSubQuery<T>(alias, selector) → TypedTupleAnonymousGeneratedSubqueryTable
```

### 2.2 CRTP (Curiously Recurring Template Pattern)

The project uses CRTP extensively to enable method-chaining that returns the concrete subclass type:

```
│   Pattern:  class X<T> where T : X<T>
│   Purpose:  Fluent methods in base class return `T` (the concrete subtype)
│
│   Examples:
│   - InsertQuery<TTable, TDialect, TSelf> where TSelf : InsertQuery<...>
│   - UpdateQuery<TTable, TDialect, TSelf> where TSelf : UpdateQuery<...>
│   - DeleteQuery<TTable, TDialect, TSelf> where TSelf : DeleteQuery<...>
│
│   Concrete subclasses:
│   - PgInsertQuery<TTable> : InsertQuery<TTable, PgSqlDialect, PgInsertQuery<TTable>>
│   - MySqlInsertQuery<TTable> : InsertQuery<TTable, MySqlDialect, MySqlInsertQuery<TTable>>
```

```
For SelectQuery, a dual CRTP pattern is used:

SelectQuery<TReturn, TDialect, TSelf>
  └── TSelf : SelectQuery<TReturn, TDialect, TSelf>
      └── e.g., PgSelectQuery<TReturn> : SelectQuery<TReturn, PgSqlDialect, PgSelectQuery<TReturn>>

SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>
  └── TSelf : SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>
      └── e.g., PgSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, PgSqlDialect, TVirtualTable, PgSelectQuery<TReturn, TVirtualTable>>
```

---

## 3. Query Builder Class Hierarchy

### 3.1 SelectQuery Chain

```
SelectQuery<TReturn, TDialect, TSelf>                     (abstract CRTP)
├── Inherits: Query<TReturn, TDialect>
├── Fields:
│   ├── FromTable: IGenericTable<TDialect>?
│   ├── Joins: List<(IGenericTable<TDialect>, string, IGenericSql?)>
│   ├── Wheres: List<IGenericSql>
│   ├── OrderBys: List<(IGenericSql, bool)>
│   ├── _limit: int?
│   ├── _offset: int?
│   ├── _distinct: bool
│   ├── _groupBys: List<IGenericSql>
│   ├── _havings: List<IGenericSql>
│   └── _intoTable: string?
│
├── Methods:
│   ├── From(table) → TSelf
│   ├── Where(conditions) → TSelf
│   ├── GroupBy(columns) → TSelf
│   ├── Having(conditions) → TSelf
│   ├── OrderBy(col, asc) → TSelf
│   ├── Limit(n) → TSelf
│   ├── Offset(n) → TSelf
│   ├── Distinct() → TSelf
│   ├── Into(tableName) → TSelf
│   ├── InnerJoin/LeftJoin/RightJoin/FullJoin/CrossJoin → TSelf
│   ├── With(cte) / WithRecursive(ctes) → TSelf
│   ├── BuildSql(sqlBuilder)                  (override — SELECT ... FROM ... WHERE ...)
│   ├── BuildSqlJoins(sqlBuilder)             (virtual — hook for dialects)
│   ├── BuildSqlOrderBy(sqlBuilder)           (virtual — hook for dialects)
│   └── BuildSqlLock(sqlBuilder)              (virtual — hook for dialects)
│
├── Dialect Subclasses:
│   ├── PgSelectQuery<TReturn>
│   │   ├── Extends: SelectQuery<TReturn, PgSqlDialectImpl, PgSelectQuery<TReturn>>
│   │   ├── Adds: LATERAL joins, 4 lock types (ForUpdate/ForShare/ForNoKeyUpdate/ForKeyShare)
│   │   └── Multi-lock clause support (List<PgLockSpec>)
│   │
│   ├── PgSelectQuery<TReturn, TVirtualTable>
│   │   └── Same features + virtual table support
│   │
│   ├── MySqlSelectQuery<TReturn>
│   │   └── Adds: ForUpdate/ForShare (simple string-based)
│   │
│   └── MySqlSelectQuery<TReturn, TVirtualTable>
│       └── Same features + virtual table support
│
└── SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>  (abstract CRTP)
    └── Extends: Query<TReturn, TDialect, TVirtualTable>
        └── Same structure as non-virtual variant
```

### 3.2 InsertQuery Chain

```
InsertQuery<TTable, TDialect, TSelf>                      (abstract CRTP)
├── Inherits: Query<TDialect>
├── Fields:
│   ├── Table: TTable
│   ├── NewValues: List<Dictionary<string, object?>>
│   ├── _useDefaultValues: bool
│   └── _fromQuery: IGenericSql?
│
├── Methods:
│   ├── Value(record) → TSelf
│   ├── Values(records[]) → TSelf
│   ├── Value(dictionary) → TSelf
│   ├── Values(dictionaries[]) → TSelf
│   ├── From(subquery) → TSelf
│   ├── DefaultValues() → TSelf
│   ├── With(cte) → TSelf
│   └── BuildSql(sqlBuilder)                  (override)
│
├── Dialect Subclasses:
│   ├── PgInsertQuery<TTable>
│   │   ├── Extends: InsertQuery<TTable, PgSqlDialectImpl, PgInsertQuery<TTable>>
│   │   ├── Adds: OnConflict(columns), OnConflictOnConstraint(constraint)
│   │   ├── Adds: DoNothing(), DoUpdate()
│   │   ├── Adds: SetOnConflict(column, value), SetOnConflictExcluded(column)
│   │   ├── Adds: WhereConflictTarget(condition), WhereOnConflictSet(condition)
│   │   └── Uses: PgConflictHelper (shared static helper)
│   │
│   ├── MySqlInsertQuery<TTable>
│   │   ├── Extends: InsertQuery<TTable, MySqlDialectImpl, MySqlInsertQuery<TTable>>
│   │   ├── Adds: OnDuplicateKeyUpdate(columns), OnDuplicateKeyUpdateAll()
│   │   ├── Adds: Ignore() → INSERT IGNORE
│   │   └── Adds: Set(column, value) → INSERT ... SET syntax
│   │
│   └── MySqlReplaceQuery<TTable>
│       ├── Extends: InsertQuery<TTable, MySqlDialectImpl, MySqlReplaceQuery<TTable>>
│       └── Overrides: BuildSql → REPLACE INTO syntax
│
└── PgConflictHelper (internal static)
    └── BuildOnConflictSql(...) — renders ON CONFLICT clause
```

### 3.3 UpdateQuery Chain

```
UpdateQuery<TTable, TDialect, TSelf>                      (abstract CRTP)
├── Inherits: Query<TDialect>
├── Fields:
│   ├── Table: TTable
│   ├── SetValues: Dictionary<string, object?>
│   └── Wheres: List<IGenericSql>
│
├── Methods:
│   ├── Set(column, value) → TSelf
│   ├── Set(column, ISql<T>) → TSelf
│   ├── Set(record) → TSelf
│   ├── Set(dictionary) → TSelf
│   ├── Where(conditions) → TSelf
│   ├── With(cte) → TSelf
│   ├── BuildSql(sqlBuilder)             (override)
│   └── BuildSqlSet(builder, values)     (protected — renders SET clause)
│
├── Dialect Subclasses:
│   ├── PgUpdateQuery<TTable>
│   │   ├── Extends: UpdateQuery<TTable, PgSqlDialectImpl, PgUpdateQuery<TTable>>
│   │   └── Adds: From(table, condition) → UPDATE ... FROM join
│   │
│   └── MySqlUpdateQuery<TTable>
│       ├── Extends: UpdateQuery<TTable, MySqlDialectImpl, MySqlUpdateQuery<TTable>>
│       ├── Adds: InnerJoin/LeftJoin/RightJoin/CrossJoin → UPDATE ... JOIN
│       ├── Adds: Limit(n), OrderBy(col)
│       └── Overrides: BuildSql — renders JOIN syntax
```

### 3.4 DeleteQuery Chain

```
DeleteQuery<TTable, TDialect, TSelf>                      (abstract CRTP)
├── Inherits: Query<TDialect>
├── Fields:
│   ├── Table: TTable
│   └── Wheres: List<IGenericSql>
│
├── Methods:
│   ├── Where(conditions) → TSelf
│   ├── With(cte) → TSelf
│   └── BuildSql(sqlBuilder)             (override)
│
├── Dialect Subclasses:
│   ├── PgDeleteQuery<TTable>
│   │   ├── Extends: DeleteQuery<TTable, PgSqlDialectImpl, PgDeleteQuery<TTable>>
│   │   └── Adds: Using(table, condition) → DELETE ... USING join
│   │
│   └── MySqlDeleteQuery<TTable>
│       ├── Extends: DeleteQuery<TTable, MySqlDialectImpl, MySqlDeleteQuery<TTable>>
│       ├── Adds: InnerJoin/LeftJoin/RightJoin/CrossJoin → DELETE ... JOIN
│       ├── Adds: Limit(n), OrderBy(col)
│       └── Overrides: BuildSql — renders DELETE t1 FROM t1 JOIN ...
```

### 3.5 CompoundQuery Chain

```
CompoundQuery<TReturn, TDialect>
├── Inherits: QueryBase<TDialect>
├── Implements: IReturning<TReturn, TDialect>
├── Fields:
│   ├── _left: IReturning<TReturn, TDialect>
│   ├── _right: IReturning<TReturn, TDialect>
│   ├── _operation: string ("UNION" | "UNION ALL" | "INTERSECT" | "EXCEPT")
│   └── SelectedColumns: ISelectedColumns<TReturn, TDialect>
│
└── BuildSql: (left) OPERATION (right)

CompoundQuery<TReturn, TDialect, TVirtualTable>
├── Inherits: QueryBase<TDialect>
├── Implements: IReturning<TReturn, TDialect, TVirtualTable>
├── AsSubQuery(alias) → TVirtualTable
└── Same structure as above

CompoundQueryExtensions                                   (static)
├── Union(left, right) → CompoundQuery
├── UnionAll(left, right) → CompoundQuery
├── Intersect(left, right) → CompoundQuery
├── Except(left, right) → CompoundQuery
├── AsRecursiveCte(compound, alias) → RecursiveCteTable
└── All have TVirtualTable variants
```

### 3.6 ReturningQuery Chain

```
ReturningQuery<TReturn, TDialect>
├── Inherits: QueryBase<TDialect>
├── Implements: IReturning<TReturn, TDialect>
├── Wraps: Query<TDialect> + ISelectedColumns<TReturn, TDialect>
└── Adds: .AsSubQuery(alias) support

ReturningQuery<TReturn, TDialect, TVirtualTable>
├── Inherits: QueryBase<TDialect>
├── Implements: IReturning<TReturn, TDialect, TVirtualTable>
└── Adds: .AsSubQuery(alias) → TVirtualTable

ReturningExtensions                                       (static)
├── 16 overloads of Returning() — 1 column through 16 columns
└── Each creates TypedTupleSelectedColumns for the specified arity
```

---

## 4. Interface Contracts

### 4.1 Core Expression Interfaces

```
IGenericSql                         (fundamental — all SQL expressions)
└── void BuildSql(ISqlBuilder sqlBuilder)

ISql<TReturn>                       (typed SQL expression)
└── extends: IGenericSql

ISql                                (non-typed SQL expression)
└── extends: IGenericSql

IAliasedSql<T>                      (aliased SQL expression — e.g., "col AS alias")
├── extends: ISql<T>
└── string Identifier { get; }      (the alias name)

IOperator<T>                        (operator result)
├── extends: ISql<T>
└── marker interface

IFunctionCallNode<T>                (function call — e.g., COUNT(col))
├── extends: IOperator<T>
├── string FunctionName { get; }
└── IGenericSql[] Arguments { get; }
```

### 4.2 Query Result Interfaces

```
IReturning<TReturn, TDialect>       (query with typed result)
├── extends: ISql<TReturn>
├── ISelectedColumns<TReturn, TDialect> SelectedColumns { get; }
└── Func<DbDataReader, TReturn> Mapper { get; }   (default impl from SelectedColumns)

IReturning<TReturn, TDialect, TVirtualTable>
├── extends: ISql<TReturn>
├── same properties
└── TVirtualTable AsSubQuery(string alias) { get; }

ISelectedColumns<TReturn, TDialect>
├── extends: ISql
└── TReturn Mapper(DbDataReader r)

ISelectedColumns<TReturn, TDialect, TVirtualTable>
├── extends: ISql
└── TReturn Mapper(DbDataReader r)

ITypedTupleSelectedColumns<TReturn, TDialect, TVirtualTable>
├── extends: ISelectedColumns<TReturn, TDialect, TVirtualTable>
├── extends: IGetFieldByName
└── ITypedTupleSelectedColumns As(string alias)     (rebind alias)

ISelection<TModel, TRecord, TDialect>   (source-generator interface)
├── static abstract ISelectedColumns<TRecord, TDialect> Record { get; }
└── static abstract ISelectedColumns<TModel, TDialect> Mapping { get; }

IGetFieldByName                         (dynamic field access on subquery)
├── IAliasedSql<T> Field<T>(string columnName)
└── IAliasedSql<T> Field<T>(IAliasedSql<T> column)
```

### 4.3 Builder Interface

```
ISqlBuilder
├── string AddParameter(object? value)     (registers param, returns @pN name)
├── ISqlBuilder Append(string sql)
└── ISqlBuilder Append(char sql)

SqlBuilder<TDialect> : ISqlBuilder        (mutable struct implementation)
├── StringBuilder _sb
├── Dictionary<string, object?> _parameters
└── (string, Dictionary<string, object?>) Build()     (finalize SQL + params)
```

### 4.4 Table / Column Interfaces

```
IGenericTable<TDialect>                 (any table-like in FROM clause)
├── void BuildSql(ISqlBuilder)          (full definition — subquery or alias)
└── void BuildRefSql(ISqlBuilder)       (reference only — name or alias)

ITable<TDialect> : IGenericTable<TDialect>
└── static abstract string TableRefName { get; }

ICteTable<TDialect> : IGenericTable<TDialect>
└── marker — CTE-specific

IVirtualTable<TDialect> : IGenericTable<TDialect>
└── static abstract IVirtualTable<TDialect> Create(baseQuery, alias, selectedColumns)

IDbTable<TDialect> : ITable<TDialect>
├── static abstract string TableName { get; }
└── static abstract string SchemaName { get; }

ITableAlias<TDialect> : ITable<TDialect>
└── static abstract string Alias { get; }

IColumn<T>                              (column descriptor)
└── string Identifier { get; }

IColumnOfTable<TTable>                  (column scoped to a table)
├── extends: IColumn<T>
└── string Identifier { get; }

IColumnOfDialect<T, TDialect>           (column scoped to a dialect)
├── extends: ISql<T>
└── string Identifier { get; }
```

---

## 5. Schema / Table / Column Class Design

### 5.1 Column Types

```
DbColumn<T, TTable, TDialect>               (strongly-typed column)
├── Implements: IColumnOfTable<TTable>, IColumnOfDialect<T, TDialect>
├── Fields:
│   ├── static string Identifier            (column name, e.g., "Id")
│   ├── static string TableRefName          (table name, e.g., "Users")
│   └── string IColumn.Identifier => Identifier
│
├── Usage: generated by TableGenerator
│   └── class UsersTable.Columns {
│           public static readonly DbColumn<int, UsersTable, PgSqlDialect> Id = new("Id");
│       }
│
└── Operators via extension methods:
    ├── .Eq(value) → BinaryNode
    ├── .IsNull() → UnaryNode
    ├── .Like(pattern) → BinaryNode
    └── (all other Operators extensions)

VirtualColumn<T, TDialect>                  (column in a subquery/virtual table)
├── Implements: IAliasedSql<T>
├── Constructor(tableAlias, columnName)
└── BuildSql → "alias"."column"

MySqlColumn<T, TTable>                      (MySQL column)
└── Same structure as DbColumn but for MySqlDialect

PgColumn<T, TTable>                         (PG column)
└── Same structure as DbColumn but for PgSqlDialect
```

### 5.2 Table Types

```
PgTable<TTable, TSchema, TColumns>          (base for PG generated tables)
├── Implements: IDbTable<PgSqlDialectImpl>
├── static TableName, SchemaName, TableRefName
├── BuildSql → "schema"."table"
└── BuildRefSql → "schema"."table"

MySqlTable<TTable, TSchema, TColumns>       (base for MySQL generated tables)
├── Implements: IDbTable<MySqlDialectImpl>
├── static TableName, SchemaName, TableRefName
├── BuildSql → `schema`.`table`
└── BuildRefSql → `schema`.`table`

RawSubqueryTableSql<TDialect>               (raw SQL subquery table)
├── Implements: IVirtualTable<TDialect>, ICteTable<TDialect>, IGetFieldByName
├── Fields: _sql (RawSql), _alias, _isCte
├── BuildSql → (subquery) AS alias  OR  alias AS (subquery) for CTE
├── BuildRefSql → alias (CTE) or full BuildSql
├── AsCte() → converts to CTE version
└── Field<T>(name) → VirtualColumn

TypedTupleGeneratedSubqueryTable<TReturn, TDialect>
├── Implements: IVirtualTable<TDialect>, IGetFieldByName
├── Selected → SelectedColumns.As(AliasName)   (rebinds columns to alias)
├── AsCte() → TypedTupleGeneratedCteTable
└── Field<T>(name/column) → VirtualColumn

TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, TDialect>
├── Extends: TypedTupleGeneratedSubqueryTable<TReturn, TDialect>
├── Adds: TShape Shape { get; }           (custom shape from lambda)
└── AsCte() → TypedTupleAnonymousGeneratedCteTable

TypedTupleGeneratedCteTable<TReturn, TDialect>
├── Extends: TypedTupleGeneratedSubqueryTable<TReturn, TDialect>
├── Implements: ICteTable<TDialect>
├── Overrides: BuildSql → alias AS (query)
└── Overrides: BuildRefSql → alias

TypedTupleAnonymousGeneratedCteTable<TShape, TReturn, TDialect>
├── Extends: TypedTupleAnonymousGeneratedSubqueryTable<...>
├── Implements: ICteTable<TDialect>
└── CTE-specific BuildSql/BuildRefSql

RawSqlTable<TDialect>                       (raw SQL table reference)
├── Implements: IGenericTable<TDialect>
├── Constructor: tableName, schemaName, alias?
└── BuildSql/BuildRefSql → [schema.]table [AS alias]

RecursiveCteTable<TReturn, TDialect>        (recursive CTE)
├── Implements: ICteTable<TDialect>, IGetFieldByName
├── Constructor: alias, compoundQuery, selectedColumns
├── BuildSql → alias AS (anchor UNION ALL recursive)
└── Field<T>(name) → VirtualColumn
```

### 5.3 Attribute System

```
[Table(name, schema)]                       → class-level
├── string Name
├── string Schema
├── Type? Dialect                           (optional — defaults to PgSql)
└── string[]? Constraints                   (table-level SQL constraints)

[Alias(table, alias)]                       → class-level
├── Type Table                              (source table type)
├── string Alias
└── Type? Dialect

[Virtual]                                   → class-level (marker)

[Column(name)]                              → property-level
├── string Name
├── string? DataType                        (optional override)
├── string? DefaultValue
├── bool AutoIncrement
├── bool NotNull
├── bool Nullable
├── string? Check
├── string? Comment
└── bool PrimaryKey

[PrimaryKey]                                → property-level (marker)

[DbSelect]                                  → class-level (source generator)

[MapWith(typeof(ModelClass))]               → class-level
[MapWithAlias(typeof(ModelClass))]          → class-level
```

---

## 6. SQL Expression Nodes (AST)

### 6.1 Node Hierarchy

```
IGenericSql
├── BinaryNode<T1, T2, TResult>            "expr OP expr"
│   └── IOperator<TResult>
│       └── c1, c2, operator, wrapInParens
│
├── BinaryNode<TResult>                    "expr OP expr" (same type)
│   └── IOperator<TResult>
│
├── UnaryNode<T, TResult>                  "OP expr" or "expr OP"
│   └── IOperator<TResult>
│       └── operand, operator, isPrefix
│
├── NnaryNode<T, TResult>                  "expr OP expr OP expr..."
│   └── IOperator<TResult>
│       └── conditions[], separator
│
├── TrinaryNode<T, TResult>                "expr OP1 expr OP2 expr"
│   └── IOperator<TResult>
│       └── c1, c2, c3, operator1, operator2
│
├── FunctionCallNode<TReturn>              "FUNC(args)"
│   ├── IFunctionCallNode<TReturn>
│   ├── IOperator<TReturn>
│   ├── FunctionName, Arguments[]
│   └── Variants: 0-arg, 1-arg, 2-arg, ..., multi-arg
│
├── CaseNode<T>                            "CASE WHEN ... THEN ... ELSE ... END"
│   └── IOperator<T>
│       └── WhenBuilder: .When(condition, result).Else(default)
│
├── CastNode<T>                            "CAST(expr AS type)" or "expr::type"
│   └── IOperator<T>
│       └── expression, targetType, usePostgresSyntax
│
├── FilteredAggregateNode<T>               "AGG(...) FILTER (WHERE ...)"
│   └── IOperator<T>
│       └── aggregate, filter
│
├── SqlValueNode<T>                        parameterized value
│   └── IOperator<T>
│       └── Value
│
├── SqlNullNode<T>                         "NULL"
│   └── IOperator<T>
│
├── SqlDefaultNode                         "DEFAULT"
│   └── IGenericSql
│
├── RawSql / RawSql<T>                     raw SQL fragment
│   └── ISql / ISql<T>
│       └── sql, parameters (for substitution)
│
├── AliasedSql<T>                          "(expr) AS alias"
│   └── IAliasedSql<T>
│       └── sql, alias
│
├── BinarySqlListValueNode<T, TResult>     "col IN (val1, val2, ...)"
│   └── IOperator<TResult>
│       └── column, values[], operator
│
└── NnaryAnyNode<T, TResult, TDialect>     "col IN (param1, param2, ...)"
    └── IOperator<TResult>
        └── column, nodes[], operator
```

### 6.2 PostgreSQL-Specific Nodes

```
PgWindowFunctionNode<TReturn>              "FUNC(args) OVER (...)"
└── IOperator<TReturn>
    └── FunctionName, Arguments[], PgOverNode

PgOverNode                                 "OVER (PARTITION BY ... ORDER BY ... frame)"
└── IGenericSql
    └── PartitionBy?, OrderBy?, Frame?

PgOverBuilder                              (fluent builder)
├── PartitionBy(columns) → PgOverBuilder
├── OrderBy(columns) → PgOverBuilder
├── RowsBetween(start, end) → PgOverBuilder
├── RangeBetween(start, end) → PgOverBuilder
├── GroupsBetween(start, end) → PgOverBuilder
└── Build() → PgOverNode
    └── implicit operator → PgOverNode

PgWindowFrame                              "ROWS | RANGE | GROUPS BETWEEN ... AND ..."
├── RowsBetween(start, end) → PgWindowFrame
├── RangeBetween(start, end) → PgWindowFrame
├── GroupsBetween(start, end) → PgWindowFrame
└── BuildSql → "ROWS BETWEEN ... AND ..."

PgWindowFrameBoundary                      (struct)
├── UnboundedPreceding
├── Preceding(n)
├── CurrentRow
├── UnboundedFollowing
└── Following(n)

PgPartitionByNode                          (internal — renders PARTITION BY cols)
PgOrderByOverNode                          (internal — renders ORDER BY in OVER)

PgIntervalNode                             "INTERVAL 'amount unit'"
├── Constructor: amount, unit
└── BuildSql → "INTERVAL '1 day'"

PgTimeZoneNode                             "'timezone'"
├── Constructor: timezone
└── BuildSql → "'UTC'"

PgExcludedNode<T>                          "EXCLUDED.column"
├── Constructor: columnIdentifier
└── BuildSql → "EXCLUDED."column""

PositionNode                               "POSITION(substring IN column)"
├── Constructor: substring, column
└── BuildSql → "POSITION(@p0 IN "col")"

MySqlIntervalNode                          "INTERVAL 1 DAY" (no quotes)
├── Constructor: amount, unit
└── BuildSql → "INTERVAL 1 DAY"

MySqlPositionNode                          "POSITION(substring IN column)"
└── MySQL-specific syntax
```

---

## 7. Operator & Function Classes

### 7.1 Static Operator Classes

```
Operators (static)                         [Core/Shared/Operators/Operators.cs]
├── Extension methods on IColumnOfDialect<T, TDialect>
├── Static methods on ISql<T>
│
├── Comparison: Eq, Ne, Lt, Gt, Ltq, Gtq      (value + column overloads)
├── Logical: And, Or, Xor                      (binary + params overloads)
├── Not
├── Like, NotLike, Contains, StartsWith, EndsWith
├── In, NotIn                                   (list, subquery, params overloads)
├── IsNull, IsNotNull
├── Between, NotBetween                         (value + column overloads)
├── Exists
├── Add, Sub, Mul, Div, Mod                     (value + column overloads)
└── Concat

PgOperators (static)                       [PgSql/PgOperators.cs]
├── IsDistinctFrom, IsNotDistinctFrom           (value + column overloads)
└── All, Any, Some                              (subquery quantifiers)
```

### 7.2 Static Function Classes

```
Functions (static)                         [Core/Shared/Operators/Functions.cs]
├── Aggregate: Count, CountDistinct, Sum, Avg, Min, Max
├── Aggregate: StdDev, Variance, VarSample, VarPop, StdDevSample, StdDevPop
├── String: Upper, Lower, Trim, LTrim, RTrim, Length, Substring, Replace
├── Numeric: Abs, Ceil, Floor, Round (1-arg + 2-arg), Power, Sqrt, Sign
├── DateTime: Now, CurrentTimestamp, CurrentDate
├── Conditional: Coalesce, NullIf, IIf
└── Casting: Cast, CastToString, CastToInt, CastToLong, CastToDouble, CastToDateTime

PgFunctions (static)                       [PgSql/PgFunctions.cs]
├── String: Position
├── DateTime: Extract, DateTrunc, DateAdd, DateDiff, AtTimeZone, Age
├── JSON: JsonExtract, JsonExtractText, JsonAgg, JsonBuildObject, JsonArrayLength
├── JSON: ToJson, RowToJson
├── Array: ArrayAgg, Unnest, ArrayLength, ArrayAny, ArrayAll
├── Other: Random, CastPg, ConcatWs
└── Window: RowNumber, Rank, DenseRank, Ntile, Lead, Lag, FirstValue, LastValue, NthValue
```

---

## 8. Dialect System

### 8.1 Interface & Implementations

```
ISqlDialect                                 (interface with static abstract members)
│
├── PgSqlSqlDialectImpl                     (PostgreSQL implementation)
│   ├── Identifiers → "double quotes"
│   ├── SupportsReturning → true
│   ├── SupportsArrays → true
│   ├── SupportsJson → true
│   ├── SupportsWindowFunctions → true
│   ├── SupportsCte → true
│   ├── SupportsRecursiveCte → true
│   ├── SupportsDeleteUsing → true
│   ├── SupportsIsDistinctFrom → true
│   ├── SupportsFilteredAggregates → true
│   └── BuildLimitOffset → delegates to SqlDialectDefaults
│
├── MySqlSqlDialectImpl                     (MySQL implementation)
│   ├── Identifiers → `backticks`
│   ├── SupportsReturning → false
│   ├── SupportsArrays → false
│   ├── SupportsJson → true
│   ├── SupportsWindowFunctions → true
│   ├── SupportsCte → true
│   ├── SupportsRecursiveCte → true
│   ├── SupportsDeleteUsing → false
│   ├── SupportsIsDistinctFrom → false
│   ├── SupportsFilteredAggregates → false
│   └── BuildOnDuplicateKeyUpdate → delegates to SqlDialectDefaults
│
└── SqlDialectDefaults                      (static — default implementations)
    ├── BuildLimitOffset(limit, offset) → "LIMIT x OFFSET y"
    ├── BuildReturning() → ""
    ├── BuildOnDuplicateKeyUpdate(columns) → "ON DUPLICATE KEY UPDATE col=VALUES(col)"
    └── EscapeString(value) → replace ' with ''
```

### 8.2 DbClient Dialect Specialization

```
DbClient<TDialect>                          (abstract — parameterized by dialect)
│
├── PgSqlDbClient                           (concrete — Npgsql)
│   ├── Inherits: DbClientWithTransaction<PgSqlDbClient, PgSqlSqlDialectImpl>
│   ├── Select<TReturn>(columns) → PgSelectQuery<TReturn>
│   ├── Insert<TTable>(table) → PgInsertQuery<TTable>
│   ├── Update<TTable>(table) → PgUpdateQuery<TTable>
│   └── Delete<TTable>(table) → PgDeleteQuery<TTable>
│
└── MySqlDbClient                           (concrete — MySqlConnector)
    ├── Inherits: DbClientWithTransaction<MySqlDbClient, MySqlSqlDialectImpl>
    ├── Select<TReturn>(columns) → MySqlSelectQuery<TReturn>
    ├── Insert<TTable>(table) → MySqlInsertQuery<TTable>
    ├── Update<TTable>(table) → MySqlUpdateQuery<TTable>
    ├── Delete<TTable>(table) → MySqlDeleteQuery<TTable>
    └── Replace<TTable>(table) → MySqlReplaceQuery<TTable>
```

---

## 9. Migration / DDL Class Design

### 9.1 DDL Query Classes

```
CreateTableQuery                            "CREATE TABLE"
├── ISql
├── Fields: TableDefinition, _ifNotExists, _temporary
├── IfNotExists() → self
├── Temporary() → self
└── BuildSql → "CREATE [TEMPORARY] TABLE [IF NOT EXISTS] schema.table (col1 type, ...)"

DropTableQuery                              "DROP TABLE"
├── ISql
├── Fields: tableName, schemaName, _ifExists, _cascade
├── IfExists() → self
├── Cascade() → self
└── BuildSql → "DROP TABLE [IF EXISTS] schema.table [CASCADE]"

AlterTableQuery                             "ALTER TABLE"
├── ISql
├── Fields: tableName, schemaName, List<AlterTableAction>
├── AddColumn(column), DropColumn(name)
├── AlterColumnType(name, type, using?)
├── SetNotNull(name), DropNotNull(name)
├── SetDefault(name, value), DropDefault(name)
├── RenameColumn(old, new)
├── AddConstraint(sql), DropConstraint(name)
└── BuildSql → "ALTER TABLE t ACTION1; ALTER TABLE t ACTION2; ..."

CreateIndexQuery                            "CREATE INDEX"
├── ISql
├── Fields: indexName, tableName, schemaName, columns, _unique, _ifNotExists, _indexType, _where
├── On(columns), Unique(), IfNotExists(), Using(type), Where(condition)
└── BuildSql → "CREATE [UNIQUE] INDEX [IF NOT EXISTS] name ON table [USING type] (cols) [WHERE condition]"

DropIndexQuery                              "DROP INDEX"
├── ISql
├── Fields: indexName, tableName?, _ifExists, _cascade
├── IfExists(), Cascade()
└── BuildSql → "DROP INDEX [IF EXISTS] [table.]name [CASCADE]"

AlterTableAction                            (internal — action descriptor)
├── AlterTableActionType enum
├── ColumnDefinition? Column
├── string? ColumnName, NewDataType, NewDefault, NewName, ConstraintSql
└── string? UsingExpression
```

### 9.2 Schema Definition Classes

```
TableDefinition                             (table metadata for DDL)
├── string TableName, SchemaName
├── List<ColumnDefinition> Columns
└── List<string> TableConstraints

ColumnDefinition                            (column metadata for DDL)
├── string Name, DataType
├── bool IsNullable, IsPrimaryKey, IsAutoIncrement
├── string? DefaultValue, CheckExpression
└── ColumnDefinition(Name, DataType) — constructor
```

### 9.3 Snapshot & Diff Classes

```
SchemaSnapshot                              (serializable schema state)
├── string Name
├── List<SnapshotTable> Tables
├── Serialize() → JSON string
├── Deserialize(json) → SchemaSnapshot
├── FromTableDefinitions(name, tables) → SchemaSnapshot
└── Compare(other) → SchemaDiff

SnapshotTable                               (serializable table)
├── string SchemaName, TableName
└── List<SnapshotColumn> Columns

SnapshotColumn                              (serializable column)
├── string Name, DataType
├── bool IsNullable, IsPrimaryKey, IsAutoIncrement
├── string? DefaultValue, CheckExpression
└── Properties: all mutable for JSON deserialization

SchemaDiff                                  (diff result)
├── IReadOnlyList<TableChange> TableChanges
├── bool HasChanges
└── ToMigrationPlan(name) → MigrationPlan

TableChange                                 (table-level change)
├── TableChangeType: Added, Removed, Modified
├── string TableName, SchemaName
├── List<ColumnChange>? ColumnChanges
├── TableDefinition? OldTable, NewTable
└── Constructor: (type, name, schema, columns, old, new)

ColumnChange                                (column-level change)
├── ColumnChangeType: Added, Removed, TypeChanged, NullabilityChanged, DefaultChanged
├── string ColumnName
├── ColumnDefinition? OldDefinition, NewDefinition
└── Constructor: (type, name, old, new)
```

### 9.4 Migration Management Classes

```
MigrationPlan                               (ordered migration steps)
├── string Name
├── IReadOnlyList<MigrationStep> Steps
└── ToSql<TDialect>() → full SQL script

MigrationStep                               (single DDL action)
├── MigrationStepType: CreateTable, DropTable, AlterTable, CreateIndex, DropIndex
├── string Description
├── ISql Sql
└── ToSql<TDialect>() → SQL string

MigrationManager<TDialect>                  (applies migrations)
├── DbConnection _connection
├── const MigrationTableName = "__Migrations"
├── EnsureMigrationTableAsync()
├── GetAppliedMigrationsAsync() → List<AppliedMigration>
├── ApplyMigrationAsync(plan)
├── ApplyMigrationsAsync(plans)
└── CreateMigrationFromDiff(name, target, current?) → MigrationPlan

AppliedMigration                            (record of applied migration)
├── string Name
├── DateTime AppliedAt
└── string Checksum

OrmSchemaExporter                           (exports ORM schema to snapshot)
├── static methods
└── Export(tableTypes) → SchemaSnapshot

TableDefinitionComparer                     (compares table definitions)
├── static methods
├── AreEqual(t1, t2) → bool
└── FindDifferences(t1, t2) → List<ColumnChange>
```

---

## 10. Source Generator Classes

### 10.1 Generator Classes

```
TableGenerator                              (Roslyn incremental generator)
├── Implements: IIncrementalGenerator
├── Input: [Table] / [Column] / [PrimaryKey] attributes
├── Output: Strongly-typed Table class with Columns, ColumnNames nested
├── Generates:
│   ├── Table class (implements IDbTable<TDialect>)
│   ├── Columns nested class with static DbColumn fields
│   └── ColumnNames nested class with static string fields
│
├── Processing:
│   ├── Detects dialect (PgSql vs MySql) from TableAttribute.Dialect
│   ├── For PgSql → inherits PgTable<...>
│   └── For MySql → inherits MySqlTable<...>

DbSelectGenerator                           (Roslyn incremental generator)
├── Implements: IIncrementalGenerator
├── Input: [DbSelect] / [MapWith] / [MapWithAlias] attributes
├── Output: ISelection<TModel, TRecord, TDialect> implementation
├── Generates:
│   ├── Record struct (with DbDataReader mapper)
│   └── Model class (with mapping from Record)
│
├── Processing: creates TypedTupleSelectedColumns or RecordSelectedColumns

MigrationSchemaGenerator                    (Roslyn incremental generator)
├── Implements: IIncrementalGenerator
├── Input: [Table] classes across the project
├── Output: Schema snapshot + migration code
└── Generates: migration classes and snapshots
```

### 10.2 Generator Utility

```
Utils                                       (source generator utilities)
├── String extensions
├── Symbol resolution helpers
└── Code generation helpers
```

---

## 11. Execution / DbClient Class Design

### 11.1 DbClient Hierarchy

```
DbClient<TDialect>                          (abstract — core execution)
├── Implements: IAsyncDisposable
├── Type parameter: TDialect : ISqlDialect
│
├── Fields:
│   ├── DbConnection _conn
│   └── DbTransaction? _transaction
│
├── Constructor: DbClient(connection, transaction?)
│
├── Static DDL Factory Methods:
│   ├── protected CreateTable(table) → CreateTableQuery
│   ├── protected DropTable(tableName, schema) → DropTableQuery
│   ├── protected AlterTable(tableName, schema) → AlterTableQuery
│   ├── protected CreateIndex(name, table, schema) → CreateIndexQuery
│   └── protected DropIndex(name, table?) → DropIndexQuery
│
├── Static Schema Methods:
│   ├── protected GetTableDefinition<TTable>() → TableDefinition
│   └── protected ClrTypeToSqlType(type) → string
│
├── Async Execution Methods:
│   ├── ExecuteGetListAsync<T>(IReturning) → List<T>
│   ├── ExecuteGetListAsync<T, TVirtualTable>(IReturning) → List<T>
│   └── ExecuteAsync(ISql) → void (non-query)
│
└── Dispose / IAsyncDisposable

DbClientWithTransaction<TInstance, TDialect>  (abstract — adds transactions)
├── Inherits: DbClient<TDialect>
├── Type parameter: TInstance (CRTP — concrete client type)
│
├── Methods:
│   ├── abstract CreateInstance(conn, tx) → TInstance
│   ├── BeginTransactionAsync() → TInstance   (wraps conn in tx)
│   ├── CommitAsync()
│   ├── RollbackAsync()
│   └── RunInTransactionAsync(action)          (auto commit/rollback)
│
├── PgSqlDbClient                             (concrete PostgreSQL)
│   ├── Inherits: DbClientWithTransaction<PgSqlDbClient, PgSqlSqlDialectImpl>
│   ├── Factory: Select<TReturn>, Insert<TTable>, Update<TTable>, Delete<TTable>
│   ├── Maps CLR types to PostgreSQL types
│   └── Uses Npgsql
│
└── MySqlDbClient                             (concrete MySQL)
    ├── Inherits: DbClientWithTransaction<MySqlDbClient, MySqlSqlDialectImpl>
    ├── Factory: Select<TReturn>, Insert<TTable>, Update<TTable>, Delete<TTable>, Replace<TTable>
    ├── Maps CLR types to MySQL types
    └── Uses MySqlConnector
```

### 11.2 Execution Flow

```
User Code:
    _db.Select(UserSelect.Record)
       .From(UsersTable)
       .Where(UsersTable.IsActive.Eq(true))
       .Build()

Internal Flow:
    1. SelectQuery.BuildSql(sqlBuilder)
       └─→ BuildSqlCte()                     // WITH clause
       └─→ "SELECT " + SelectedColumns.BuildSql()
       └─→ "FROM " + FromTable.BuildRefSql()
       └─→ BuildSqlJoins()
       └─→ "WHERE " + Wheres
       └─→ "GROUP BY " + GroupBys
       └─→ "HAVING " + Havings
       └─→ BuildSqlOrderBy()
       └─→ BuildLimitOffset()
       └─→ BuildSqlLock()

    2. sqlBuilder.Build()
       └─→ returns (string sql, Dictionary<string, object?> params)

    3. DbClient.ExecuteGetListAsync()
       └─→ DbCommand.CommandText = sql
       └─→ foreach param → DbCommand.Parameters.Add()
       └─→ DbDataReader → query.Mapper(reader) → List<T>
```

---

## 12. Window Function Classes

### 12.1 Core Window Classes

```
PgWindowFunctionNode<TReturn>               "FUNC(args) OVER (...)"
├── IOperator<TReturn>
├── Fields: FunctionName, Arguments[], PgOverNode
└── BuildSql → "FUNC(arg1, arg2) OVER (...)"

PgWindowFunctionExtensions (static)
└── Over<T>(this IFunctionCallNode<T>, PgOverNode) → PgWindowFunctionNode<T>

PgOverNode                                  "OVER (PARTITION BY ... ORDER BY ... frame)"
├── IGenericSql
├── Fields: PartitionBy?, OrderBy?, Frame?
└── BuildSql → "OVER (PARTITION BY col ORDER BY col ROWS BETWEEN ...)"

PgOverBuilder                               (fluent)
├── PartitionBy(IGenericSql[]) → PgOverBuilder
├── OrderBy((col, asc)[]) → PgOverBuilder
├── OrderBy(col, asc) → PgOverBuilder
├── RowsBetween(start, end) → PgOverBuilder
├── RangeBetween(start, end) → PgOverBuilder
├── GroupsBetween(start, end) → PgOverBuilder
├── Build() → PgOverNode
└── implicit operator PgOverNode → PgOverNode

PgOver (static entry point)
└── Create() → PgOverBuilder

PgPartitionByNode (internal)
├── Fields: IGenericSql[] _columns
└── BuildSql → "col1, col2, col3"

PgOrderByOverNode (internal)
├── Fields: (IGenericSql column, bool asc)[] _columns
└── BuildSql → "col1 ASC, col2 DESC"

PgWindowFrame                               "ROWS | RANGE | GROUPS BETWEEN ... AND ..."
├── Fields: FrameType (Rows|Range|Groups), Start, End (PgWindowFrameBoundary)
├── Static: RowsBetween(start, end), RangeBetween(start, end), GroupsBetween(start, end)
└── BuildSql → "ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW"

PgWindowFrameBoundary                       (struct)
├── Fields: BoundaryType enum, int? Offset
├── Static factories:
│   ├── UnboundedPreceding
│   ├── Preceding(n)
│   ├── CurrentRow
│   ├── UnboundedFollowing
│   └── Following(n)
└── BuildSql → "UNBOUNDED PRECEDING" | "3 PRECEDING" | "CURRENT ROW" | ...
```

---

## 13. Utility & Helper Classes

### 13.1 Static Utility Classes

```
Sql (static)
├── Value<T>(T) → SqlValueNode<T>                    (parameterized value)
├── Raw(string) → RawSql                             (raw SQL fragment)
├── Raw<T>(string) → RawSql<T>                       (typed raw SQL)
├── Literal(string) → RawSql                         (unsafe literal — no param)
├── Literal<T>(string) → RawSql<T>                   (typed unsafe literal)
├── Null<T>() → SqlNullNode<T>                       (NULL literal)
└── Default() → SqlDefaultNode                       (DEFAULT keyword)

SqlExtensions (static)
├── AsSubQuery(this RawSql, alias) → RawSubqueryTableSql<TDialect>
└── As<T>(this ISql<T>, alias) → AliasedSql<T>

PgSqlStatics (static)
├── Interval(int amount, string unit) → PgIntervalNode
├── Interval(double amount, string unit) → PgIntervalNode
├── TimeZone(string tz) → PgTimeZoneNode
└── Excluded<T>(column) → PgExcludedNode<T>

SqlDialectDefaults (static)
├── BuildLimitOffset(limit?, offset?) → string
├── BuildReturning() → string
├── BuildOnDuplicateKeyUpdate(columns) → string
├── EscapeString(value) → string
└── Feature flag defaults (static bool properties)

Utils (static)
└── General-purpose utility methods
```

### 13.2 Data Interfaces

```
IWriteRecord<TTable, TDialect>              (write record — for insert/update)
├── void Writer(Dictionary<string, object?> values)
└── Populates column→value dictionary

IInsertRecord<TTable, TDialect>             (insert record)
└── extends IWriteRecord

IUpdateRecord<TTable, TDialect>             (update record)
└── extends IWriteRecord
```

### 13.3 Typed Tuple Selected Columns

```
TypedTupleSelectedColumns — N overloads (1 to 16 columns)
├── Each implements ITypedTupleSelectedColumns<TReturn, TDialect, TVirtualTable>
├── Stores IAliasedSql<T1>..IAliasedSql<TN>
├── BuildSql → renders column list
├── Mapper → creates ValueTuple<T1..TN> from DbDataReader
└── As(alias) → rebinds virtual column aliases
```

### 13.4 PgConflictHelper

```
PgConflictHelper (internal static)
└── BuildOnConflictSql(sqlBuilder, targetColumns, targetString,
                       isConstraint, action, updates,
                       targetWhere, setWhere)
    ├── "ON CONFLICT (col1, col2)"
    ├── "ON CONFLICT ON CONSTRAINT name"
    ├── "WHERE condition" (on target)
    ├── "DO NOTHING" | "DO UPDATE SET"
    ├── "col = value, col2 = EXCLUDED.col2"
    └── "WHERE condition" (on SET)
```

---

## Relationship Summary (Package/Namespace Dependencies)

```
Drizzle4Dotnet.Core
├── Core.Shared          ← fundamental interfaces, ISql, ISqlBuilder, Sql
├── Core.Shared.Operators ← Operators, Functions, IOperator
├── Core.Shared.Operators.Nodes ← AST nodes (Binary, Unary, FunctionCall, etc.)
├── Core.Query           ← QueryBase, Query, CompoundQuery, ReturningExtensions
├── Core.Query.Select    ← SelectQuery
├── Core.Query.Insert    ← InsertQuery, IInsertRecord
├── Core.Query.Update    ← UpdateQuery, IUpdateRecord
├── Core.Query.Delete    ← DeleteQuery
├── Core.Schema.Tables   ← ITable, IGenericTable, IVirtualTable, ICteTable, attributes
├── Core.Schema.Columns  ← IColumn, DbColumn, VirtualColumn, attributes
├── Core.Schema.Migration ← DDL queries, SchemaSnapshot, MigrationManager
└── Core.DbClient        ← DbClient, DbClientWithTransaction

Drizzle4Dotnet.Dialect
└── PgSqlSqlDialectImpl, MySqlSqlDialectImpl  ← implement ISqlDialect

Drizzle4Dotnet.PgSql
├── Depends on: Core, Dialect
├── PgInsertQuery, PgSelectQuery, PgUpdateQuery, PgDeleteQuery
├── PgFunctions, PgOperators, PgSqlStatics
├── PgColumn, PgTable
├── PgSqlDbClient
└── Nodes: PgOverNode, PgWindowFunctionNode, PgWindowFrame, etc.

Drizzle4Dotnet.MySql
├── Depends on: Core, Dialect
├── MySqlInsertQuery, MySqlSelectQuery, MySqlUpdateQuery, MySqlDeleteQuery
├── MySqlReplaceQuery
├── MySqlFunctions, MySqlOperators
├── MySqlColumn, MySqlTable
├── MySqlDbClient
└── Nodes: MySqlIntervalNode, MySqlPositionNode

SourceGenerators
├── Depends on: Microsoft.CodeAnalysis
├── TableGenerator      ← reads [Table], [Column] attributes → generates Table classes
├── DbSelectGenerator   ← reads [DbSelect] → generates ISelection types
└── MigrationSchemaGenerator ← reads [Table] → generates migration code

Test/
└── Tests for all query types across PgSql and MySql dialects

SharedDemo/
└── Shared schema and DTO definitions for PgSql and MySql
```
