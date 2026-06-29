# Drizzle4Dotnet — Unit Test Plan & Report

> **Status:** Live Document  
> **Last Updated:** 2026-06-29  
> **Test Framework:** NUnit  
> **Test Pattern:** SQL output comparison via `query.Build()` → string matching / visual inspection

---

## 1. Testing Approach

### 1.1 Test Strategy

The testing strategy is **offline SQL generation testing** — tests build queries using the `QueryBuilder` classes (no database connection needed) and validate the generated SQL string and parameters:

```csharp
var query = _db.Select(col1, col2).From(table).Where(condition);
var (sql, parameters) = query.Build();

// Assert SQL string matches expected
Assert.That(sql, Is.EqualTo("SELECT \"col1\", \"col2\" FROM \"table\" WHERE (\"col\" = @p0)"));

// Assert parameters
Assert.That(parameters["@p0"], Is.EqualTo(expectedValue));
```

### 1.2 Test Infrastructure

| Component | Purpose |
|-----------|---------|
| `PgSqlQueryBuilder` | Offline query builder for PostgreSQL — no DB connection |
| `MySqlQueryBuilder` | Offline query builder for MySQL — no DB connection |
| `MssqlQueryBuilder` | Offline query builder for MSSQL — no DB connection |
| `OracleQueryBuilder` | Offline query builder for Oracle — no DB connection |
| SharedDemo schemas | Reusable table/column definitions for all dialects |
| NUnit `[TestCase]` | Data-driven tests for operator combinations |

### 1.3 Current Test Coverage Summary

| Area | Files | Tests | Status |
|------|-------|-------|--------|
| **PgSql SELECT** | [`PgSqlSelectTests.cs`](Test/Select/PgSqlSelectTests.cs) | ~200+ | ✅ Implemented |
| **PgSql Compound** | [`PgSqlCompoundQueryTests.cs`](Test/Select/PgSqlCompoundQueryTests.cs) | ~20+ | ✅ Implemented |
| **MySql SELECT** | [`MySqlSelectTests.cs`](Test/Select/MySqlSelectTests.cs) | ~150+ | ✅ Implemented |
| **MySql Compound** | [`MySqlCompoundQueryTests.cs`](Test/Select/MySqlCompoundQueryTests.cs) | ~20+ | ✅ Implemented |
| **PgSql INSERT** | [`PgSqlInsertTests.cs`](Test/Insert/PgSqlInsertTests.cs) | ~50+ | ✅ Implemented |
| **MySql INSERT** | [`MySqlInsertTests.cs`](Test/Insert/MySqlInsertTests.cs) | ~40+ | ✅ Implemented |
| **PgSql UPDATE** | [`PgSqlUpdateTests.cs`](Test/Update/PgSqlUpdateTests.cs) | ~30+ | ✅ Implemented |
| **MySql UPDATE** | [`MySqlUpdateTests.cs`](Test/Update/MySqlUpdateTests.cs) | ~25+ | ✅ Implemented |
| **PgSql DELETE** | [`PgSqlDeleteTests.cs`](Test/Delete/PgSqlDeleteTests.cs) | ~25+ | ✅ Implemented |
| **MySql DELETE** | [`MySqlDeleteTests.cs`](Test/Delete/MySqlDeleteTests.cs) | ~20+ | ✅ Implemented |
| **PgSql MERGE** | [`PgSqlMergeTests.cs`](Test/Merge/PgSqlMergeTests.cs) | ~30+ | ✅ Implemented |
| **PgSql Migration** | [`PgSqlMigrationTests.cs`](Test/Migration/PgSqlMigrationTests.cs) | ~200+ | ✅ Implemented |
| **MSSQL tests** | ❌ Missing | 0 | ❌ Not started |
| **Oracle tests** | ❌ Missing | 0 | ❌ Not started |
| **SQLite tests** | ❌ Missing | 0 | ❌ Not started |

---

## 2. Existing Test Coverage Detail

### 2.1 PgSql SELECT Tests ([`Test/Select/PgSqlSelectTests.cs`](Test/Select/PgSqlSelectTests.cs))

| Category | Test Cases | Status |
|----------|-----------|--------|
| **Basic SELECT** | Basic, WithWhere, WithJoin, WithMultipleJoins, WithLeftJoin, WithRightJoin, WithFullJoin, WithCrossJoin, WithNaturalJoin, WithNaturalLeftJoin | ✅ |
| **SELECT with WHERE** | WhereEq, WhereNeq, WhereLt, WhereGt, WhereLte, WhereGte, WhereAnd, WhereOr, WhereAndOr, WhereNot, WhereIn, WhereNotIn, WhereBetween, WhereNotBetween, WhereLike, WhereNotLike, WhereIsNull, WhereIsNotNull, WhereExists, WhereContains, WhereStartsWith, WhereEndsWith, WhereInSubquery, WhereCompositeAnd | ✅ |
| **SELECT with DISTINCT** | DistinctBasic, DistinctOnBasic, DistinctOnMulti | ✅ |
| **SELECT with Aggregates** | Count, Sum, Avg, Min, Max, CountDistinct, CountWithWhere, SumWithGroupBy, AvgWithHaving, MultipleAggregates | ✅ |
| **SELECT with GROUP BY/HAVING** | GroupByBasic, GroupByMulti, HavingBasic, HavingWithAggregate, GroupByHaving, GroupByHavingOrderBy | ✅ |
| **SELECT with ORDER BY** | OrderByAsc, OrderByDesc, OrderByMulti, OrderByExpression | ✅ |
| **SELECT with LIMIT/OFFSET** | LimitOnly, OffsetOnly, LimitOffset, LimitOffsetLarge | ✅ |
| **SELECT with LATERAL JOIN** | InnerLateral, LeftLateral, CrossLateral | ✅ |
| **SELECT with FOR UPDATE** | ForUpdate, ForNoKeyUpdate, ForShare, ForKeyShare, ForUpdateNowait, ForUpdateSkipLocked, ForUpdateOfTable, MultipleLockClauses | ✅ |
| **SELECT with CTE** | WithCte, WithMultipleCtes, WithRecursiveCte | ✅ |
| **SELECT with Subquery** | SubqueryInFrom, SubqueryInJoin, SubqueryWithAlias, SubqueryShapeMapping | ✅ |
| **SELECT with Functions** | Upper, Lower, Trim, Length, Substring, Concat, Coalesce, NullIf, Cast, Case, Now, Extract, DateTrunc | ✅ |
| **SELECT with Window Functions** | RowNumber, Rank, DenseRank, Ntile, Lead, Lag, FirstValue, LastValue, NthValue, PartitionBy, OrderBy, RowsBetween | ✅ |
| **SELECT with Set Operations** | Union, UnionAll, Intersect, Except, IntersectAll, ExceptAll, CompoundWithLimit, CompoundWithOrderBy | ✅ |
| **SELECT with Typed Tuples** | Select1Col, Select2Cols, Select3Cols, Select4Cols, Select5Cols, Select6Cols, Select7Cols, Select8Cols | ✅ |
| **SELECT with Generated Types** | SelectResultAll, SelectModelAll, SelectRecord, SelectMapping | ✅ |

### 2.2 PgSql Compound Query Tests ([`Test/Select/PgSqlCompoundQueryTests.cs`](Test/Select/PgSqlCompoundQueryTests.cs))

| Test Case | Description |
|-----------|-------------|
| Compound_Union | Two SELECTs combined with UNION |
| Compound_UnionAll | Two SELECTs combined with UNION ALL |
| Compound_Intersect | Two SELECTs combined with INTERSECT |
| Compound_Except | Two SELECTs combined with EXCEPT |
| Compound_WithLimit | Compound query with LIMIT |
| Compound_WithOrderBy | Compound query with ORDER BY |
| Compound_AsRecursiveCte | Compound query wrapped as recursive CTE |
| Compound_AsSubQuery | Compound query used as subquery |

### 2.3 MySql SELECT Tests ([`Test/Select/MySqlSelectTests.cs`](Test/Select/MySqlSelectTests.cs))

Parallel to PgSql SELECT tests but with MySQL-specific syntax verification:
- Backtick quoting: `` `table`.`column` ``
- `LIMIT n OFFSET m` / pair mode `LIMIT n, m`
- No FULL OUTER JOIN, no NATURAL JOIN tests
- No DISTINCT ON tests
- No FOR UPDATE/SHARE tests (separate file)
- No LATERAL join tests

### 2.4 INSERT Tests

#### PgSql INSERT ([`Test/Insert/PgSqlInsertTests.cs`](Test/Insert/PgSqlInsertTests.cs))

| Category | Test Cases | Status |
|----------|-----------|--------|
| Basic INSERT | InsertSingleRow, InsertMultipleRows, InsertDefaultValues, InsertFromSelect | ✅ |
| INSERT with RETURNING | InsertReturningSingle, InsertReturningMultiple, InsertReturningAll | ✅ |
| ON CONFLICT | InsertOnConflictDoNothing, InsertOnConflictDoUpdate, InsertOnConflictSetExcluded, InsertOnConflictOnConstraint, InsertOnConflictWhereTarget, InsertOnConflictWhereSet | ✅ |
| CTE with INSERT | InsertWithCte | ✅ |

#### MySql INSERT ([`Test/Insert/MySqlInsertTests.cs`](Test/Insert/MySqlInsertTests.cs))

| Category | Test Cases | Status |
|----------|-----------|--------|
| Basic INSERT | InsertSingleRow, InsertMultipleRows, InsertFromSelect | ✅ |
| ON DUPLICATE KEY | InsertOnDuplicateKeyUpdate, InsertOnDuplicateKeyUpdateAll, InsertOnDuplicateKeyUpdateMultiple | ✅ |
| INSERT IGNORE | InsertIgnore | ✅ |
| INSERT SET syntax | InsertSet | ✅ |
| REPLACE | ReplaceBasic, ReplaceMultipleRows | ✅ |
| CTE with INSERT | InsertWithCte | ✅ |

### 2.5 UPDATE Tests

#### PgSql UPDATE ([`Test/Update/PgSqlUpdateTests.cs`](Test/Update/PgSqlUpdateTests.cs))

| Category | Test Cases |
|----------|-----------|
| Basic UPDATE | UpdateBasic, UpdateMultipleSet, UpdateWithExpression |
| UPDATE with WHERE | UpdateWhereBasic, UpdateWhereComposite |
| UPDATE with FROM | UpdateFromJoin, UpdateFromMultipleJoins |
| UPDATE with RETURNING | UpdateReturningSingle, UpdateReturningMultiple |
| UPDATE with CTE | UpdateWithCte |

#### MySql UPDATE ([`Test/Update/MySqlUpdateTests.cs`](Test/Update/MySqlUpdateTests.cs))

| Category | Test Cases |
|----------|-----------|
| Basic UPDATE | UpdateBasic, UpdateMultipleSet |
| UPDATE with JOIN | UpdateWithInnerJoin, UpdateWithLeftJoin |
| UPDATE with WHERE | UpdateWhere |
| UPDATE with LIMIT/ORDER BY | UpdateLimit, UpdateOrderBy |
| CTE with UPDATE | UpdateWithCte |

### 2.6 DELETE Tests

#### PgSql DELETE ([`Test/Delete/PgSqlDeleteTests.cs`](Test/Delete/PgSqlDeleteTests.cs))

| Category | Test Cases |
|----------|-----------|
| Basic DELETE | DeleteBasic, DeleteWhere |
| DELETE with USING | DeleteUsingJoin |
| DELETE with RETURNING | DeleteReturning |
| DELETE with CTE | DeleteWithCte |

#### MySql DELETE ([`Test/Delete/MySqlDeleteTests.cs`](Test/Delete/MySqlDeleteTests.cs))

| Category | Test Cases |
|----------|-----------|
| Basic DELETE | DeleteBasic, DeleteWhere |
| DELETE with JOIN | DeleteWithInnerJoin, DeleteWithLeftJoin |
| DELETE with LIMIT/ORDER BY | DeleteLimit, DeleteOrderBy |
| DELETE with CTE | DeleteWithCte |

### 2.7 MERGE Tests ([`Test/Merge/PgSqlMergeTests.cs`](Test/Merge/PgSqlMergeTests.cs))

| Category | Test Cases |
|----------|-----------|
| Basic MERGE | MergeBasic |
| MERGE with WHEN MATCHED | MergeWhenMatchedUpdate, MergeWhenMatchedUpdateMultiple |
| MERGE with WHEN NOT MATCHED | MergeWhenNotMatchedInsert, MergeWhenNotMatchedInsertMultiple |
| MERGE with BY SOURCE | MergeWhenNotMatchedBySourceDelete |
| MERGE with OUTPUT | MergeOutputInserted, MergeOutputDeleted, MergeOutputBoth |
| MERGE with JOIN | MergeWithJoin |

### 2.8 Migration Tests ([`Test/Migration/PgSqlMigrationTests.cs`](Test/Migration/PgSqlMigrationTests.cs))

| Category | Test Cases |
|----------|-----------|
| TableDefinition | TableDefBasic, TableDefWithColumns, TableDefWithConstraints, TableDefWithIndexes |
| ColumnDefinition | ColumnBasic, ColumnWithDefault, ColumnWithCheck, ColumnWithComment, ColumnPrimaryKey, ColumnAutoIncrement |
| TableConstraint | ForeignKeyConstraint, UniqueConstraint, PrimaryKeyConstraint, CheckConstraint, RawConstraint |
| TableIndex | IndexBasic, IndexUnique, IndexWithType, IndexWithWhere |
| SchemaSnapshot | SnapshotSerialize, SnapshotDeserialize, SnapshotRoundTrip, SnapshotFromTableDefs |
| SchemaDiff | DiffAddedTable, DiffRemovedTable, DiffModifiedTable, DiffAddedColumn, DiffRemovedColumn, DiffTypeChanged, DiffNullabilityChanged, DiffDefaultChanged |
| MigrationPlan | PlanBasicAdded, PlanBasicRemoved, PlanModifiedColumns, PlanFullMigration |
| DDL Queries | CreateTableBasic, CreateTableWithAllFeatures, DropTableBasic, DropTableIfExists, CreateIndexBasic, CreateIndexUnique, DropIndexBasic, AlterTableAddColumn, AlterTableDropColumn, AlterTableAlterType, AlterTableSetNotNull, AlterTableDropDefault, AlterTableAddConstraint, AlterTableDropConstraint |
| OrmSchemaExporter | ExportBasicTable, ExportTableWithAllColumns, ExportTableWithConstraints, ExportTableWithIndexes, CreateSnapshotFromTypes |
| TableDefinitionComparer | CompareSameTable, CompareAddedColumn, CompareRemovedColumn, CompareTypeChanged, CompareMultipleChanges, CompareSetsBasic |

---

## 3. Test Coverage Gap Analysis

### 3.1 Missing Dialect Tests

| Dialect | SELECT | INSERT | UPDATE | DELETE | MERGE | Migration | Compound |
|---------|--------|--------|--------|--------|-------|-----------|----------|
| **PgSql** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **MySql** | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ | ✅ |
| **MSSQL** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **Oracle** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| **SQLite** | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |

### 3.2 Missing Feature Tests (Across All Dialects)

| Feature | PgSql | MySql | MSSQL | Oracle | Priority |
|---------|-------|-------|-------|--------|----------|
| Core Operators (all 39) | ✅ Partial | ❌ | ❌ | ❌ | 🔴 High |
| Core Functions (all 35) | ✅ Partial | ❌ | ❌ | ❌ | 🔴 High |
| Type Cast expressions | ✅ | ❌ | ❌ | ❌ | 🟡 Medium |
| CASE expressions | ✅ | ❌ | ❌ | ❌ | 🟡 Medium |
| Filtered aggregates | ✅ | N/A | N/A | N/A | 🟢 Low |
| Window functions | ✅ | ❌ | ❌ | ❌ | 🟡 Medium |
| SELECT DISTINCT ON (PgSql) | ✅ | N/A | N/A | N/A | ✅ |
| SELECT FOR UPDATE variants | ✅ | ✅ | N/A | ❌ | 🟡 Medium |
| LATERAL joins | ✅ | ❌ | N/A | ❌ | 🟢 Low |
| CROSS/OUTER APPLY | N/A | N/A | ❌ | N/A | 🟡 Medium |
| Table hints | N/A | N/A | ❌ | N/A | 🟢 Low |
| OUTPUT clause | N/A | N/A | ❌ | N/A | 🟡 Medium |
| RETURNING INTO (Oracle) | N/A | N/A | N/A | ❌ | 🟡 Medium |
| Sequence / NEXTVAL | ❌ | ❌ | ❌ | ❌ | 🟢 Low |
| Full MERGE + variants | ✅ | ❌ | ❌ | ❌ | 🟡 Medium |

### 3.3 Missing Component Tests

| Component | Test Coverage | Priority | Notes |
|-----------|--------------|----------|-------|
| **SqlBuilder** | ❌ | 🔴 High | Unit test `Append()`, `AddParameter()`, `Build()` independently |
| **ISqlDialect implementations** (naming) | ❌ | 🔴 High | Test identifier quoting per dialect |
| **AliasedSql / VirtualColumn** | ❌ | 🟡 Medium | Test `BuildSql()` output |
| **RawSql / RawSql<T>** | ❌ | 🟡 Medium | Test parameter substitution |
| **TypedTupleSelectedColumns** (Mapper) | ❌ | 🟡 Medium | Test DbDataReader → tuple mapping |
| **CaseNode** | ❌ | 🟡 Medium | Test CASE WHEN ... THEN ... ELSE ... END |
| **CastNode** | ❌ | 🟢 Low | Test CAST/:: syntax |
| **FunctionCallNode** | ❌ | 🟡 Medium | Test function building with args, DISTINCT, FILTER |
| **BinaryNode / UnaryNode / NnaryNode / TrinaryNode** | ❌ | 🟡 Medium | Test each node type independently |
| **SqlValueNode** | ❌ | 🟢 Low | Test parameter creation |
| **SqlStatics** (all methods) | ❌ | 🔴 High | Test `BuildClause()`, `BuildSqlSetClause()`, `BuildSqlJoins()`, `BuildSqlCte()`, `BuildSqlOrderBy()` |
| **SqlDialectDefaults** | ❌ | 🟡 Medium | Test `BuildLimitOffset()`, `EscapeString()` |
| **IInsertRecord / IUpdateRecord** Writers | ❌ | 🟡 Medium | Test Writer() dictionary population |
| **Optional<T> pattern** | ❌ | 🟢 Low | Test HasValue/Value behavior |

---

## 4. New Test Implementation Plan

### Phase T1: Core Component Tests (🔴 High Priority)

```
Test/Core/
├── SqlBuilderTests.cs           # ISqlBuilder / SqlBuilder<TDialect> unit tests
├── SqlStaticsTests.cs           # All SqlStatics helper methods
├── SqlDialectTests.cs           # SqlDialectDefaults + per-dialect naming
├── RawSqlTests.cs               # RawSql, RawSql<T>, parameter substitution
├── AliasTests.cs                # AliasedSql<T>, VirtualColumn<T,TDialect>
├── ExpressionNodeTests.cs       # BinaryNode, UnaryNode, NnaryNode, TrinaryNode
├── FunctionCallNodeTests.cs     # FunctionCallNode with args, DISTINCT, FILTER, ORDER BY
├── CaseNodeTests.cs             # CASE WHEN ... THEN ... ELSE ... END
├── CastNodeTests.cs             # CAST / :: syntax
└── TypedTupleColumnTests.cs     # TypedTupleSelectedColumns Mapper + Field lookup
```

#### T1.1 SqlBuilderTests

```csharp
[Test]
public void Append_String_Accumulates()
{
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    builder.Append("SELECT ").Append("*").Append(" FROM users");
    var (sql, _) = builder.Build();
    Assert.That(sql, Is.EqualTo("SELECT * FROM users"));
}

[Test]
public void AddParameter_ReturnsParameterName()
{
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    var name = builder.AddParameter(42);
    Assert.That(name, Is.EqualTo("@p0"));
    var (_, parameters) = builder.Build();
    Assert.That(parameters["@p0"], Is.EqualTo(42));
}

[Test]
public void AddParameter_Multiple_IncrementsIndex()
{
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    builder.AddParameter("a");
    builder.AddParameter("b");
    var (_, parameters) = builder.Build();
    Assert.That(parameters.Keys, Is.EquivalentTo(new[] { "@p0", "@p1" }));
}
```

#### T1.2 SqlStaticsTests

```csharp
[Test]
public void BuildClause_WithItems_AddsHeaderAndSeparator()
{
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    var conditions = new IGenericSql[] { new RawSql("a = 1"), new RawSql("b = 2") };
    SqlStatics.BuildClause(builder, " WHERE ", " AND ", conditions, wrapInParentheses: true);
    var (sql, _) = builder.Build();
    Assert.That(sql, Is.EqualTo(" WHERE (a = 1) AND (b = 2)"));
}

[Test]
public void BuildClause_EmptyItems_NoOutput()
{
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    SqlStatics.BuildClause(builder, " WHERE ", " AND ", Array.Empty<IGenericSql>());
    var (sql, _) = builder.Build();
    Assert.That(sql, Is.EqualTo(""));
}
```

### Phase T2: MSSQL Test Suite (🔴 High Priority)

```
Test/
├── Select/
│   └── MssqlSelectTests.cs        # TOP, OFFSET/FETCH, APPLY, table hints, FULL JOIN
├── Insert/
│   └── MssqlInsertTests.cs        # OUTPUT INSERTED
├── Update/
│   └── MssqlUpdateTests.cs        # UPDATE with TOP, OUTPUT, FROM/JOIN
├── Delete/
│   └── MssqlDeleteTests.cs        # DELETE with TOP, OUTPUT, FROM/JOIN
├── Merge/
│   └── MssqlMergeTests.cs         # Full MERGE coverage
└── Migration/
    └── MssqlMigrationTests.cs     # MSSQL DDL syntax tests
```

**MssqlSelectTests key test cases:**
```csharp
[Test]
public void Select_Top() { /* SELECT TOP (10) ... */ }

[Test]
public void Select_TopWithTies() { /* SELECT TOP (10) WITH TIES ... */ }

[Test]
public void Select_TopPercent() { /* SELECT TOP (10) PERCENT ... */ }

[Test]
public void Select_OffsetFetch() { /* ... OFFSET @p0 ROWS FETCH NEXT @p1 ROWS ONLY */ }

[Test]
public void Select_CrossApply() { /* ... CROSS APPLY (...) */ }

[Test]
public void Select_OuterApply() { /* ... OUTER APPLY (...) */ }

[Test]
public void Select_WithTableHint() { /* ... FROM [table] WITH (NOLOCK) */ }

[Test]
public void Select_FullJoin() { /* ... FULL OUTER JOIN ... */ }

[Test]
public void Select_OrderByRequiredForOffset() { /* ORDER BY (SELECT 0) workaround */ }
```

**MssqlInsertTests key test cases:**
```csharp
[Test]
public void Insert_OutputInserted() { /* OUTPUT INSERTED.[col1], INSERTED.[col2] */ }

[Test]
public void Insert_OutputInsertedBetweenColumnsAndValues() { /* ... (cols) OUTPUT ... VALUES ... */ }
```

**MssqlMergeTests key test cases:**
```csharp
[Test]
public void Merge_Basic() { /* MERGE [target] USING source ON ... */ }

[Test]
public void Merge_WhenMatchedUpdate() { /* WHEN MATCHED THEN UPDATE SET ... */ }

[Test]
public void Merge_WhenNotMatchedInsert() { /* WHEN NOT MATCHED THEN INSERT ... */ }

[Test]
public void Merge_WhenNotMatchedBySourceDelete() { /* WHEN NOT MATCHED BY SOURCE THEN DELETE */ }

[Test]
public void Merge_OutputInserted() { /* OUTPUT INSERTED.[id] */ }

[Test]
public void Merge_OutputDeleted() { /* OUTPUT DELETED.[id] */ }
```

### Phase T3: Oracle Test Suite (🔴 High Priority)

```
Test/
├── Select/
│   └── OracleSelectTests.cs       # FOR UPDATE, OFFSET/FETCH, DUAL
├── Insert/
│   └── OracleInsertTests.cs       # RETURNING INTO, MERGE-based upsert
├── Update/
│   └── OracleUpdateTests.cs       # RETURNING INTO
├── Delete/
│   └── OracleDeleteTests.cs       # RETURNING INTO
├── Merge/
│   └── OracleMergeTests.cs        # MERGE with DELETE WHERE
└── Migration/
    └── OracleMigrationTests.cs    # Oracle DDL syntax tests
```

**OracleSelectTests key test cases:**
```csharp
[Test]
public void Select_ForUpdate() { /* SELECT ... FOR UPDATE */ }

[Test]
public void Select_ForUpdateOf() { /* SELECT ... FOR UPDATE OF "table"."col" */ }

[Test]
public void Select_ForUpdateNoWait() { /* FOR UPDATE NOWAIT */ }

[Test]
public void Select_ForUpdateWait() { /* FOR UPDATE WAIT 5 */ }

[Test]
public void Select_ForUpdateSkipLocked() { /* FOR UPDATE SKIP LOCKED */ }

[Test]
public void Select_LimitOffset() { /* OFFSET :p0 ROWS FETCH NEXT :p1 ROWS ONLY */ }
```

### Phase T4: Operator Test Suite (🟡 Medium Priority)

```
Test/Operators/
├── ComparisonOperatorTests.cs     # Eq, Ne, Lt, Gt, Ltq, Gtq (value + column overloads)
├── LogicalOperatorTests.cs        # And, Or, Xor, Not (binary + N-ary)
├── StringOperatorTests.cs         # Like, NotLike, Contains, StartsWith, EndsWith
├── CollectionOperatorTests.cs     # In, NotIn (values + subquery), IsNull, IsNotNull, Exists
├── RangeOperatorTests.cs          # Between, NotBetween
├── ArithmeticOperatorTests.cs     # Add, Sub, Mul, Div, Mod, Neg
├── PgSqlOperatorTests.cs          # JSON ops, text search, IS DISTINCT FROM, quantifiers
├── MySqlOperatorTests.cs          # NullSafeEq, RegexMatch, NotRegexMatch, Rlike
└── OracleOperatorTests.cs         # CONCAT operator
```

**Example operator test patterns:**
```csharp
[Test]
public void Eq_Value_GeneratesCorrectSql()
{
    var expr = UsersTable.Id.Eq(42);
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    expr.BuildSql(builder);
    var (sql, parameters) = builder.Build();
    Assert.That(sql, Is.EqualTo("\"Users\".\"Id\" = @p0"));
    Assert.That(parameters["@p0"], Is.EqualTo(42));
}

[Test]
public void Eq_Column_GeneratesCorrectSql()
{
    var expr = Operators.Eq(UsersTable.Id, DepartmentsTable.Id);
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    expr.BuildSql(builder);
    var (sql, _) = builder.Build();
    Assert.That(sql, Is.EqualTo("\"Users\".\"Id\" = \"Departments\".\"Id\""));
}

[TestCase(1, 10, "BETWEEN")]
[TestCase(1, null, ">=")]
public void Between_Variations(int low, int? high, string expectedOp)
{
    // Test Between/NotBetween with both bounds, missing low, missing high
}
```

### Phase T5: Function Test Suite (🟡 Medium Priority)

```
Test/Functions/
├── AggregateFunctionTests.cs      # Count, Sum, Avg, Min, Max, StdDev, Variance + DISTINCT + FILTER
├── StringFunctionTests.cs         # Upper, Lower, Trim, Length, Substring, Replace, Concat
├── NumericFunctionTests.cs        # Abs, Ceil, Floor, Round, Power, Sqrt, Sign
├── DateTimeFunctionTests.cs       # Now, CurrentTimestamp, CurrentDate
├── ConditionalFunctionTests.cs    # Coalesce, NullIf, Case
├── CastingFunctionTests.cs        # Cast, CastToString, CastToInt, CastToLong, CastToDouble, CastToDateTime
├── PgSqlFunctionTests.cs          # Position, Extract, DateTrunc, DateAdd, DateDiff, JsonExtract, ArrayAgg, etc.
├── MySqlFunctionTests.cs          # DateAdd, DateFormat, JsonExtract, GroupConcat, FindInSet, etc.
├── MssqlFunctionTests.cs          # DateAdd, DateDiff, CharIndex, StringAgg, JsonValue, etc.
└── OracleFunctionTests.cs         # AddMonths, LastDay, Listagg, RatioToReport, Decode, etc.
```

**Example function test:**
```csharp
[Test]
public void Count_WithFilter_GeneratesFilteredAggregate()
{
    var expr = Functions.Count(UsersTable.Id).Filter(UsersTable.IsActive.Eq(true));
    var builder = new SqlBuilder<PgSqlSqlDialectImpl>();
    expr.BuildSql(builder);
    var (sql, _) = builder.Build();
    Assert.That(sql, Is.EqualTo("COUNT(\"Users\".\"Id\") FILTER (WHERE (\"Users\".\"IsActive\" = @p0))"));
}
```

### Phase T6: Migration / DDL Test Suite (🟡 Medium Priority)

```
Test/Migration/
├── PgSqlMigrationTests.cs         # ✅ Existing (expand)
├── MySqlMigrationTests.cs         # ❌ New — MySQL DDL syntax tests
├── MssqlMigrationTests.cs         # ❌ New — MSSQL DDL syntax tests
├── OracleMigrationTests.cs        # ❌ New — Oracle DDL syntax tests
└── DDLQueryBuilderTests.cs        # ❌ New — CreateTableQuery, AlterTableQuery, etc. in isolation
```

**Missing DDL tests to add:**
```csharp
[Test]
public void CreateTable_WithIfNotExists() { /* CREATE TABLE IF NOT EXISTS ... */ }

[Test]
public void CreateTable_Temporary() { /* CREATE TEMPORARY TABLE ... */ }

[Test]
public void AlterTable_RenameColumn() { /* ALTER TABLE ... RENAME COLUMN ... */ }

[Test]
public void DropTable_Cascade() { /* DROP TABLE ... CASCADE */ }

[Test]
public void CreateIndex_WithType() { /* CREATE INDEX ... USING GIN (...) */ }

[Test]
public void CreateIndex_WithWhere() { /* CREATE INDEX ... ON ... WHERE ... */ }
```

### Phase T7: Generated Code Tests (🟡 Medium Priority)

```
Test/Generated/
├── TableGeneratorOutputTests.cs   # Verify generated .g.cs files compile correctly
├── DbSelectGeneratorOutputTests.cs # Verify TypedTupleSelectedColumns + DbSelect output
├── InsertRecordWriterTests.cs     # Test Writer() for InsertModel + InsertRecord
├── UpdateRecordWriterTests.cs     # Test Writer() for UpdateModel + UpdateRecord (Optional<T>)
└── SubqueryTableFieldTests.cs     # Test Field<T>() lookup on generated subquery tables
```

### Phase T8: End-to-End SQL Tests (🟠 Low Priority)

```
Test/Integration/
├── PgSqlIntegrationTests.cs       # Full query → DB execution (requires PG instance)
├── MySqlIntegrationTests.cs       # Full query → DB execution (requires MySQL instance)
├── MssqlIntegrationTests.cs       # Full query → DB execution (requires SQL Server)
└── OracleIntegrationTests.cs      # Full query → DB execution (requires Oracle)
```

---

## 5. Test Implementation Priority Matrix

| Phase | Area | Tests | Effort | Priority | Dependencies |
|-------|------|-------|--------|----------|--------------|
| **T1** | Core Components | 50+ | 3 days | 🔴 High | None |
| **T2** | MSSQL Dialect | 200+ | 5 days | 🔴 High | MSSQL implementation (exists) |
| **T3** | Oracle Dialect | 200+ | 5 days | 🔴 High | Oracle implementation (exists) |
| **T4** | Operators (all dialects) | 150+ | 4 days | 🟡 Medium | T1 |
| **T5** | Functions (all dialects) | 150+ | 4 days | 🟡 Medium | T1 |
| **T6** | Migration DDL (all dialects) | 100+ | 3 days | 🟡 Medium | T1 |
| **T7** | Generated Code | 30+ | 2 days | 🟡 Medium | Source generators stable |
| **T8** | Integration/E2E | 50+ | 5 days | 🟠 Low | All above + DB instances |
| **Total** | | **~930+** | **~31 days** | | |

## 6. Recommended Test File Structure

```
Test/
├── Core/
│   ├── SqlBuilderTests.cs
│   ├── SqlStaticsTests.cs
│   ├── SqlDialectTests.cs
│   ├── RawSqlTests.cs
│   ├── AliasTests.cs
│   ├── ExpressionNodeTests.cs
│   ├── FunctionCallNodeTests.cs
│   ├── CaseNodeTests.cs
│   ├── CastNodeTests.cs
│   └── TypedTupleColumnTests.cs
├── Select/
│   ├── PgSqlSelectTests.cs          # ✅ Existing
│   ├── PgSqlCompoundQueryTests.cs   # ✅ Existing
│   ├── MySqlSelectTests.cs          # ✅ Existing
│   ├── MySqlCompoundQueryTests.cs   # ✅ Existing
│   ├── MssqlSelectTests.cs          # ❌ New
│   └── OracleSelectTests.cs         # ❌ New
├── Insert/
│   ├── PgSqlInsertTests.cs          # ✅ Existing
│   ├── MySqlInsertTests.cs          # ✅ Existing
│   ├── MssqlInsertTests.cs          # ❌ New
│   └── OracleInsertTests.cs         # ❌ New
├── Update/
│   ├── PgSqlUpdateTests.cs          # ✅ Existing
│   ├── MySqlUpdateTests.cs          # ✅ Existing
│   ├── MssqlUpdateTests.cs          # ❌ New
│   └── OracleUpdateTests.cs         # ❌ New
├── Delete/
│   ├── PgSqlDeleteTests.cs          # ✅ Existing
│   ├── MySqlDeleteTests.cs          # ✅ Existing
│   ├── MssqlDeleteTests.cs          # ❌ New
│   └── OracleDeleteTests.cs         # ❌ New
├── Merge/
│   ├── PgSqlMergeTests.cs           # ✅ Existing
│   ├── MssqlMergeTests.cs           # ❌ New
│   └── OracleMergeTests.cs          # ❌ New
├── Operators/
│   ├── ComparisonOperatorTests.cs   # ❌ New
│   ├── LogicalOperatorTests.cs      # ❌ New
│   ├── StringOperatorTests.cs       # ❌ New
│   ├── CollectionOperatorTests.cs   # ❌ New
│   ├── RangeOperatorTests.cs        # ❌ New
│   ├── ArithmeticOperatorTests.cs   # ❌ New
│   ├── PgSqlOperatorTests.cs        # ❌ New
│   ├── MySqlOperatorTests.cs        # ❌ New
│   └── OracleOperatorTests.cs       # ❌ New
├── Functions/
│   ├── AggregateFunctionTests.cs    # ❌ New
│   ├── StringFunctionTests.cs       # ❌ New
│   ├── NumericFunctionTests.cs      # ❌ New
│   ├── DateTimeFunctionTests.cs     # ❌ New
│   ├── ConditionalFunctionTests.cs  # ❌ New
│   ├── CastingFunctionTests.cs      # ❌ New
│   ├── PgSqlFunctionTests.cs        # ❌ New
│   ├── MySqlFunctionTests.cs        # ❌ New
│   ├── MssqlFunctionTests.cs        # ❌ New
│   └── OracleFunctionTests.cs       # ❌ New
├── Migration/
│   ├── PgSqlMigrationTests.cs       # ✅ Existing
│   ├── DDLQueryBuilderTests.cs      # ❌ New
├── Generated/
│   ├── TableGeneratorOutputTests.cs # ❌ New
│   ├── InsertRecordWriterTests.cs   # ❌ New
│   ├── UpdateRecordWriterTests.cs   # ❌ New
│   └── SubqueryTableFieldTests.cs   # ❌ New
└── Integration/
    ├── PgSqlIntegrationTests.cs     # ❌ New
    ├── MySqlIntegrationTests.cs     # ❌ New
    ├── MssqlIntegrationTests.cs     # ❌ New
    └── OracleIntegrationTests.cs    # ❌ New
```

## 7. Testing Standards & Conventions

### 7.1 Naming Convention

```
{Dialect}_{Feature}_{Scenario}_{ExpectedResult}
```

Examples:
- `PgSqlSelect_WithWhere_GeneratesCorrectSql`
- `MssqlInsert_OutputInserted_ColumnListCorrect`
- `OracleMerge_WhenMatchedDeleteWhere_AddsDeleteClause`
- `Core_SqlBuilder_AddParameter_ReturnsIncrementedName`

### 7.2 Test Structure (AAA Pattern)

```csharp
[Test]
public void PgSqlSelect_WithWhere_GeneratesCorrectSql()
{
    // Arrange
    var db = new PgSqlQueryBuilder();
    var users = new UsersTable();

    // Act
    var query = db.Select(users.Id, users.Name)
        .From(users)
        .Where(users.Id.Eq(1));
    var (sql, parameters) = query.Build();

    // Assert
    Assert.Multiple(() =>
    {
        Assert.That(sql, Does.Contain("SELECT"));
        Assert.That(sql, Does.Contain("WHERE"));
        Assert.That(parameters["@p0"], Is.EqualTo(1));
    });
}
```

### 7.3 What to Assert

| Aspect | What to Check | Example |
|--------|---------------|---------|
| **SQL Structure** | Keywords present, correct order | `SELECT`, `FROM`, `WHERE` in right positions |
| **Identifier Quoting** | Dialect-correct quoting | `"table"`, `` `table` ``, `[table]` |
| **Parameter Names** | Correct prefix | `@p0`, `:p0` |
| **Parameter Values** | Values match expected | `parameters["@p0"] == 42` |
| **SQL Correctness** | Full string match for simple cases | `Assert.That(sql, Is.EqualTo(expected))` |
| **Edge Cases** | Empty lists, null values, default values | `.Where()`, null parameters, `DEFAULT VALUES` |

### 7.4 What NOT to Assert

- Do NOT connect to a real database (unit tests are offline)
- Do NOT test DbDataReader behavior (mock the reader in dedicated tests)
- Do NOT test source generator output directly (test the generated assembly compiles)
