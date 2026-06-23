# Drizzle4Dotnet — Source vs Test Coverage Analysis

I've completed a thorough review of the entire codebase. Below is the comprehensive coverage analysis organized by category.

---

## 1. QUERY TYPES COVERAGE

### SELECT Query
| Feature | Source | PgSql Test | MySql Test |
|---------|--------|:----------:|:----------:|
| Basic SELECT | [`SelectQuery.cs`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs) | ✅ `Select_Basic` | ✅ `Select_Basic` |
| WHERE clause | ✅ | ✅ `Select_WithWhere` | ✅ `Select_WithWhere` |
| JOIN (Inner/Left/Right/Full/Cross) | ✅ | ✅ `Select_WithJoin` | ✅ `Select_WithJoin` |
| Self-Join / Alias | ✅ | ✅ `Select_SelfJoin_Alias` | ✅ |
| Many-to-Many | ✅ | ✅ `Select_ManyToMany` | ❌ **MISSING** |
| ORDER BY | ✅ | ✅ `Select_WithOrderBy` | ❌ **MISSING** |
| LIMIT / OFFSET | ✅ | ✅ `Select_WithLimitOffset` | ❌ **MISSING** |
| DISTINCT | ✅ | ✅ `Select_WithDistinct` | ❌ **MISSING** |
| GROUP BY / HAVING | ✅ | ✅ `Select_WithGroupBy` | ❌ **MISSING** |
| Subquery in FROM (Derived Table) | ✅ | ✅ `Select_FromSubquery_DerivedTable` | ❌ **MISSING** |
| Subquery in SELECT list | ✅ | ✅ `Select_SubqueryInSelectList` | ❌ **MISSING** |
| Subquery in WHERE (IN) | ✅ | ✅ `Select_WithSubquery` | ❌ **MISSING** |
| EXISTS subquery | ✅ | ✅ `Select_WithExists` | ❌ **MISSING** |
| Nested subqueries | ✅ | ✅ `Select_WithNestedSubqueries` | ❌ **MISSING** |
| Aggregated subquery | ✅ | ✅ `Select_WithAggregatedSubquery` | ❌ **MISSING** |
| CTEs (WITH) | ✅ | ✅ `Select_WithCTE` | ✅ `Select_WithMultipleCTEs` |
| Multiple CTEs | ✅ | ✅ `Select_MultipleCTEs` | ✅ |
| WITH RECURSIVE | ✅ | ❌ **MISSING** (commented out in `Base.cs`) | ❌ **MISSING** |
| SELECT INTO | ✅ | ❌ **MISSING** | ❌ **MISSING** |
| FOR UPDATE / FOR SHARE | ✅ | ❌ **MISSING** | ✅ `Select_WithForUpdate/Share` |
| FOR NO KEY UPDATE / FOR KEY SHARE | ✅ (`PgSelectQuery.cs`) | ❌ **MISSING** | N/A |
| LATERAL JOINs | ✅ (`PgSelectQuery.cs`) | ❌ **MISSING** | N/A |
| Raw SQL fragments | ✅ (`Sql.cs`) | ✅ | ✅ `Select_WithRawSQL` |
| XOR operator | ✅ (`Operators.cs`) | ❌ **MISSING** | ✅ `Select_WithXorOperator` |

### INSERT Query
| Feature | Source | PgSql Test | MySql Test |
|---------|--------|:----------:|:----------:|
| Basic INSERT | [`InsertQuery.cs`](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs) | ✅ `Insert_Basic` | ✅ `Insert_Basic` |
| Multiple rows | ✅ | ✅ `Insert_MultipleValues` | ✅ `Insert_MultipleValues` |
| Dictionary values | ✅ | ✅ `Insert_WithDictionary` | ✅ `Insert_WithDictionary` |
| INSERT ... SELECT | ✅ | ✅ `Insert_SelectFrom` | ✅ `Insert_SelectFrom` |
| INSERT DEFAULT VALUES | ✅ | ✅ `Insert_DefaultValues` | ❌ **MISSING** |
| ON CONFLICT DO NOTHING | ✅ (`PgInsertQuery.cs`) | ✅ `Insert_OnConflictDoNothing` | N/A |
| ON CONFLICT DO UPDATE | ✅ (`PgInsertQuery.cs`) | ✅ `Insert_OnConflictDoUpdate` | N/A |
| ON CONFLICT ON CONSTRAINT | ✅ (`PgInsertQuery.cs`) | ✅ `Insert_OnConflictOnConstraint` | N/A |
| ON DUPLICATE KEY UPDATE | ✅ (`MySqlInsertQuery.cs`) | N/A | ✅ `Insert_OnDuplicateKeyUpdate` |
| ON DUPLICATE KEY UPDATE ALL | ✅ (`MySqlInsertQuery.cs`) | N/A | ✅ `Insert_OnDuplicateKeyUpdateAll` |
| INSERT IGNORE | ✅ (`MySqlInsertQuery.cs`) | N/A | ✅ `Insert_Ignore` |
| INSERT ... SET syntax | ✅ (`MySqlInsertQuery.cs`) | N/A | ✅ `Insert_SetSyntax` |
| REPLACE INTO | ✅ (`MySqlReplaceQuery.cs`) | N/A | ✅ `Replace_Basic` |
| REPLACE multiple rows | ✅ (`MySqlReplaceQuery.cs`) | N/A | ✅ `Replace_MultipleRows` |
| INSERT with CTE (WITH) | ✅ | ❌ **MISSING** | ❌ **MISSING** |
| RETURNING (PgSql via base) | ✅ (`ReturningQuery.cs`) | ❌ **MISSING** | N/A |

### UPDATE Query
| Feature | Source | PgSql Test | MySql Test |
|---------|--------|:----------:|:----------:|
| Basic UPDATE | [`UpdateQuery.cs`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs) | ✅ `Update_Basic` | ✅ `Update_Basic` |
| Multiple WHERE conditions | ✅ | ✅ `Update_MultipleConditions` | ✅ `Update_MultipleConditions` |
| Expression value (e.g., SET col = col + 1) | ✅ | ✅ `Update_WithExpressionValue` | ✅ `Update_WithExpressionValue` |
| Set from record | ✅ | ✅ `Update_WithSetRecord` | ❌ **MISSING** |
| UPDATE ... FROM (PgSql) | ✅ (`PgUpdateQuery.cs`) | ✅ `Update_WithFrom` | N/A |
| Multi-table FROM | ✅ (`PgUpdateQuery.cs`) | ✅ `Update_WithMultipleFromTables` | N/A |
| UPDATE with JOIN (MySql) | ✅ (`MySqlUpdateQuery.cs`) | N/A | ✅ `Update_WithJoin` |
| UPDATE with LEFT JOIN | ✅ (`MySqlUpdateQuery.cs`) | N/A | ✅ `Update_WithLeftJoin` |
| UPDATE with RIGHT JOIN | ✅ (`MySqlUpdateQuery.cs`) | N/A | ✅ `Update_WithRightJoin` |
| UPDATE with CROSS JOIN | ✅ (`MySqlUpdateQuery.cs`) | N/A | ✅ `Update_WithCrossJoin` |
| UPDATE with ORDER BY + LIMIT | ✅ (`MySqlUpdateQuery.cs`) | N/A | ✅ `Update_WithOrderByAndLimit` |
| UPDATE with CTE (WITH) | ✅ | ❌ **MISSING** | ❌ **MISSING** |
| RETURNING (PgSql via base) | ✅ (`ReturningQuery.cs`) | ❌ **MISSING** | N/A |

### DELETE Query
| Feature | Source | PgSql Test | MySql Test |
|---------|--------|:----------:|:----------:|
| Basic DELETE | [`DeleteQuery.cs`](Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs) | ✅ `Delete_Basic` | ✅ `Delete_Basic` |
| DELETE all | ✅ | ✅ `Delete_All` | ✅ `Delete_All` |
| Multiple WHERE conditions | ✅ | ✅ `Delete_MultipleConditions` | ✅ `Delete_MultipleConditions` |
| Complex WHERE | ✅ | ✅ `Delete_WithComplexWhere` | ✅ `Delete_WithComplexWhere` |
| DELETE ... USING (PgSql) | ✅ (`PgDeleteQuery.cs`) | ✅ `Delete_WithUsing` | N/A |
| Multiple USING | ✅ (`PgDeleteQuery.cs`) | ✅ `Delete_WithMultipleUsing` | N/A |
| DELETE with JOIN (MySql) | ✅ (`MySqlDeleteQuery.cs`) | N/A | ✅ `Delete_WithJoin` |
| DELETE with LEFT JOIN | ✅ (`MySqlDeleteQuery.cs`) | N/A | ✅ `Delete_WithLeftJoin` |
| DELETE with RIGHT JOIN | ✅ (`MySqlDeleteQuery.cs`) | N/A | ✅ `Delete_WithRightJoin` |
| DELETE with CROSS JOIN | ✅ (`MySqlDeleteQuery.cs`) | N/A | ✅ `Delete_WithCrossJoin` |
| DELETE with ORDER BY + LIMIT | ✅ (`MySqlDeleteQuery.cs`) | N/A | ✅ `Delete_WithOrderByAndLimit` |
| DELETE with CTE (WITH) | ✅ | ❌ **MISSING** | ❌ **MISSING** |
| RETURNING (PgSql via base) | ✅ (`ReturningQuery.cs`) | ❌ **MISSING** | N/A |

### Compound Queries
| Feature | Source | PgSql Test | MySql Test |
|---------|--------|:----------:|:----------:|
| UNION | [`CompoundQuery.cs`](Drizzle4Dotnet/src/Core/Query/CompoundQuery.cs) | ✅ `Union_Basic` | ✅ `Union_Basic` |
| UNION ALL | ✅ | ✅ `UnionAll_Basic` | ✅ `UnionAll_Basic` |
| INTERSECT | ✅ | ✅ `Intersect_Basic` | ✅ `Intersect_Basic` |
| EXCEPT | ✅ | ✅ `Except_Basic` | ✅ `Except_Basic` |
| Compound with subquery | ✅ | ✅ `UnionAll_WithSubquery` | ✅ `UnionAll_WithSubquery` |

---

## 2. STANDARD SQL FUNCTIONS COVERAGE

### Aggregate Functions — [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs)
| Function | Source | PgSql Test | MySql Test |
|----------|--------|:----------:|:----------:|
| Count() | ✅ | ✅ `Select_WithAggregateFunctions` | ✅ `Select_WithAggregateFunctions` (in `Base.cs`) |
| CountDistinct() | ✅ | ✅ | ✅ |
| Sum() | ✅ | ✅ | ✅ |
| Avg() | ✅ | ✅ | ✅ |
| Min() | ✅ | ✅ | ✅ |
| Max() | ✅ | ✅ | ✅ |
| StdDev() | ✅ | ✅ | ❌ **MISSING** |
| Variance() | ✅ | ✅ | ❌ **MISSING** |
| VarSample() | ✅ | ✅ | ❌ **MISSING** |
| VarPop() | ✅ | ✅ | ❌ **MISSING** |
| StdDevSample() | ✅ | ✅ | ❌ **MISSING** |
| StdDevPop() | ✅ | ✅ | ❌ **MISSING** |

### String Functions
| Function | Source | PgSql Test | MySql Test |
|----------|--------|:----------:|:----------:|
| Upper() | ✅ | ✅ | ✅ |
| Lower() | ✅ | ✅ | ✅ |
| Trim() | ✅ | ✅ | ✅ |
| LTrim() | ✅ | ✅ | ✅ |
| RTrim() | ✅ | ✅ | ✅ |
| Length() | ✅ | ✅ | ✅ |
| Substring() | ✅ | ✅ | ✅ |
| Replace() | ✅ | ✅ | ✅ |
| Concat(||) | ✅ (`Operators.cs`) | ✅ `Select_ConcatOperator` | ❌ **MISSING** |

### Numeric/Math Functions
| Function | Source | PgSql Test | MySql Test |
|----------|--------|:----------:|:----------:|
| Abs() | ✅ | ✅ | ✅ |
| Ceil() | ✅ | ✅ | ✅ |
| Floor() | ✅ | ✅ | ✅ |
| Round() | ✅ | ✅ | ✅ |
| Round(col, decimals) | ✅ | ✅ | ✅ |
| Power() | ✅ | ✅ | ✅ |
| Sqrt() | ✅ | ✅ | ✅ |
| Sign() | ✅ | ✅ | ✅ |

### Date/Time Functions
| Function | Source | PgSql Test | MySql Test |
|----------|--------|:----------:|:----------:|
| Now() | ✅ | ✅ | ✅ |
| CurrentTimestamp() | ✅ | ✅ | ✅ |
| CurrentDate() | ✅ | ✅ | ✅ |

### Conditional Functions
| Function | Source | PgSql Test | MySql Test |
|----------|--------|:----------:|:----------:|
| Coalesce() | ✅ | ✅ `Select_ConditionalFunctions` | ✅ `Select_ConditionalFunctions` |
| NullIf() | ✅ | ✅ | ✅ |
| IIf() | ✅ | ❌ **MISSING** | ❌ **MISSING** |
| Case/When/Then/Else | ✅ (`CaseNode.cs`) | ✅ `Select_WithCaseExpression` | ❌ **MISSING** |
| Case multi-when | ✅ | ✅ `Select_WithCaseMultiWhen` | ❌ **MISSING** |

### Casting
| Feature | Source | PgSql Test | MySql Test |
|---------|--------|:----------:|:----------:|
| Cast() | ✅ (`CastNode.cs`) | ✅ `Select_CastFunctions` | ✅ `Select_CastFunctions` |
| CastToString/Int/Long/Double/DateTime | ✅ | ✅ | ✅ |

---

## 3. PGSQL-SPECIFIC FUNCTIONS — [`PgFunctions.cs`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs)

| Function | Source | Test |
|----------|--------|:----:|
| Position() | ✅ | ✅ `Select_PgPosition` |
| ConcatWs() | ✅ | ✅ `Select_PgConcatWs` |
| Extract() | ✅ | ✅ `Select_PgExtract` |
| DateTrunc() | ✅ | ✅ `Select_PgDateTrunc` |
| DateAdd() | ✅ | ✅ `Select_PgDateAdd` |
| DateDiff() | ✅ | ✅ `Select_PgDateDiff` |
| AtTimeZone() | ✅ | ✅ `Select_PgAtTimeZone` |
| Age() | ✅ | ✅ `Select_PgAge` |
| Age(t1, t2) | ✅ | ❌ **MISSING** (commented out) |
| Interval literal | ✅ | ✅ `Select_PgIntervalLiteral` |
| JsonExtract (->) | ✅ | ✅ `Select_PgJsonExtract` |
| JsonExtractText (->>) | ✅ | ✅ `Select_PgJsonExtractText` |
| JsonAgg() | ✅ | ✅ `Select_PgJsonAgg` |
| JsonBuildObject() | ✅ | ✅ `Select_PgJsonBuildObject` |
| JsonArrayLength() | ✅ | ✅ `Select_PgJsonArrayLength` |
| ToJson() | ✅ | ✅ `Select_PgToJson` |
| RowToJson() | ✅ | ❌ **MISSING** (commented out) |
| ArrayAgg() | ✅ | ✅ `Select_PgArrayAgg` |
| Unnest() | ✅ | ✅ `Select_PgUnnest` |
| ArrayLength() | ✅ | ✅ `Select_PgArrayLength` |
| ArrayAny() | ✅ | ❌ **MISSING** |
| ArrayAll() | ✅ | ❌ **MISSING** |
| Random() | ✅ | ✅ `Select_PgRandom` |
| CastPg (::) | ✅ | ✅ `Select_PgCastPg` |

---

## 4. PGSQL WINDOW FUNCTIONS — [`PgFunctions.cs`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs#L163-L229)

| Function | Source | Test |
|----------|--------|:----:|
| RowNumber() | ✅ | ✅ `Select_Window_RowNumber` |
| Rank() | ✅ | ✅ `Select_Window_Rank` |
| DenseRank() | ✅ | ✅ `Select_Window_DenseRank` |
| Ntile() | ✅ | ✅ `Select_Window_Ntile` |
| Lead() | ✅ | ✅ `Select_Window_Lead` |
| Lead with default | ✅ | ✅ `Select_Window_LeadWithDefault` |
| Lag() | ✅ | ✅ `Select_Window_Lag` |
| Lag with offset | ✅ | ✅ |
| FirstValue() | ✅ | ❌ **MISSING** |
| LastValue() | ✅ | ❌ **MISSING** |
| NthValue() | ✅ | ❌ **MISSING** |
| PgOverBuilder.PartitionBy | ✅ | ✅ |
| PgOverBuilder.OrderBy | ✅ | ✅ |
| PgOverBuilder.RowsBetween | ✅ | ✅ |
| PgOverBuilder.RangeBetween | ✅ | ✅ |
| PgOverBuilder.GroupsBetween | ✅ | ❌ **MISSING** |

---

## 5. MYSQL-SPECIFIC FUNCTIONS — [`MySqlFunctions.cs`](Drizzle4Dotnet/src/MySql/MySqlFunctions.cs)

| Function | Source | Test |
|----------|--------|:----:|
| Concat() | ✅ | ✅ `Select_MySqlConcat` |
| ConcatWs() | ✅ | ✅ `Select_MySqlConcatWs` |
| CharLength() | ✅ | ✅ `Select_MySqlCharLength` |
| Locate() | ✅ | ✅ `Select_MySqlLocate` |
| SubstringIndex() | ✅ | ✅ `Select_MySqlSubstringIndex` |
| Position() | ✅ | ✅ `Select_MySqlPosition` |
| DateAdd() | ✅ | ✅ `Select_MySqlDateAdd` |
| DateSub() | ✅ | ✅ `Select_MySqlDateSub` |
| DateFormat() | ✅ | ✅ `Select_MySqlDateFormat` |
| UnixTimestamp() | ✅ | ✅ `Select_MySqlUnixTimestamp` |
| FromUnixTime() | ✅ | ✅ `Select_MySqlFromUnixTime` |
| StrToDate() | ✅ | ✅ `Select_MySqlStrToDate` |
| Rand() | ✅ | ✅ `Select_MySqlRand` |
| Truncate() | ✅ | ✅ `Select_MySqlTruncate` |
| BitCount() | ✅ | ✅ `Select_MySqlBitCount` |
| Crc32() | ✅ | ✅ `Select_MySqlCrc32` |
| CurDate() | ✅ | ✅ `Select_MySqlCurDate` |
| CurTime() | ✅ | ✅ `Select_MySqlCurTime` |
| LastInsertId() | ✅ | ✅ `Select_MySqlLastInsertId` |
| Database() | ✅ | ✅ `Select_MySqlDatabase` |
| User() | ✅ | ✅ `Select_MySqlUser` |
| Version() | ✅ | ✅ `Select_MySqlVersion` |
| JsonExtract() | ✅ | ✅ `Select_MySqlJsonExtract` |
| JsonExtractText() | ✅ | ✅ `Select_MySqlJsonExtractText` |
| JsonArrow (->) | ✅ | ✅ `Select_MySqlJsonArrow` |
| JsonArrowText (->>) | ✅ | ✅ `Select_MySqlJsonArrowText` |
| JsonArrayAgg() | ✅ | ✅ `Select_MySqlJsonArrayAgg` |
| JsonObjectAgg() | ✅ | ✅ `Select_MySqlJsonObjectAgg` |
| JsonArray() | ✅ | ✅ `Select_MySqlJsonArray` |
| JsonObject() | ✅ | ✅ `Select_MySqlJsonObject` |
| JsonContains() | ✅ | ✅ `Select_MySqlJsonContains` |
| JsonLength() | ✅ | ✅ `Select_MySqlJsonLength` |
| JsonKeys() | ✅ | ✅ `Select_MySqlJsonKeys` |

---

## 6. OPERATOR COVERAGE

### Standard Operators — [`Operators.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs)
| Operator | Source | PgSql Test | MySql Test |
|----------|--------|:----------:|:----------:|
| Eq, Lt, Gt, Ltq, Gtq, Ne | ✅ | ✅ `Select_ComparisonOperators` | ❌ **MISSING** individually |
| And, Or | ✅ | ✅ `Select_LogicalOperators` | ✅ |
| Xor | ✅ | ✅ | ✅ `Select_WithXorOperator` |
| Not | ✅ | ❌ **MISSING** | ❌ **MISSING** |
| Like, NotLike | ✅ | ✅ `Select_StringOperators` | ❌ **MISSING** |
| Contains, StartsWith, EndsWith | ✅ | ✅ | ❌ **MISSING** |
| IsNull, IsNotNull | ✅ | ✅ `Select_NullCheck` | ❌ **MISSING** |
| In (value list) | ✅ | ✅ `Select_InValuesList` | ✅ `Select_WithInValuesList` |
| In (subquery) | ✅ | ✅ `Select_InOperator` | ❌ **MISSING** |
| NotIn (value list) | ✅ | ✅ `Select_NotInOperator` | ❌ **MISSING** |
| Between | ✅ | ✅ `Select_BetweenOperator` | ❌ **MISSING** |
| NotBetween | ✅ | ✅ `Select_NotBetweenOperator` | ❌ **MISSING** |
| Add, Sub, Mul, Div, Mod | ✅ | ✅ `Select_ArithmeticOperators` | ❌ **MISSING** |
| Concat (\|\|) | ✅ | ✅ `Select_ConcatOperator` | ❌ **MISSING** |
| Exists | ✅ | ✅ `Select_WithExists` | ❌ **MISSING** |

### PostgreSQL Operators — [`PgOperators.cs`](Drizzle4Dotnet/src/PgSql/PgOperators.cs)
| Operator | Source | Test |
|----------|--------|:----:|
| IsDistinctFrom | ✅ | ✅ `Select_IsDistinctFrom` |
| IsNotDistinctFrom | ✅ | ✅ `Select_IsNotDistinctFrom` |
| IsDistinctFrom (column) | ✅ | ✅ `Select_IsDistinctFromColumn` |
| All (subquery quantifier) | ✅ | ✅ `Select_AllSubquery` |
| Any (subquery quantifier) | ✅ | ✅ `Select_AnySubquery` |
| Some (subquery quantifier) | ✅ | ✅ `Select_SomeSubquery` |

### MySQL Operators — [`MySqlOperators.cs`](Drizzle4Dotnet/src/MySql/MySqlOperators.cs)
| Operator | Source | Test |
|----------|--------|:----:|
| NullSafeEqual (<=>) | ✅ | ❌ **MISSING** |
| Regexp | ✅ | ❌ **MISSING** |
| NotRegexp | ✅ | ❌ **MISSING** |

---

## 7. SUMMARY OF GAPS

### 🔴 Critical Gaps (implemented but untested)

| Area | Items Missing Tests |
|------|-------------------|
| **PgSql MySql SELECT** | ORDER BY, LIMIT/OFFSET, DISTINCT, GROUP BY/HAVING, subqueries (IN, EXISTS, nested), self-join, aggregate functions detailed, comparison operators, string operators, IsNull/IsNotNull, Between, arithmetic, In/NotIn subquery |
| **MySQL Operators** | `NullSafeEqual`, `Regexp`, `NotRegexp` — entire [`MySqlOperators.cs`](Drizzle4Dotnet/src/MySql/MySqlOperators.cs) is untested |
| **PgSql MySQL Operators** | Standard operators Eq/Lt/Gt/Ltq/Gtq/Ne tested in PgSql but NOT individually in MySql |
| **PgSql LATERAL JOINs** | `InnerLateralJoin`, `LeftLateralJoin`, `CrossLateralJoin` in [`PgSelectQuery.cs`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs) — untested |
| **PgSql Extended Lock Types** | `ForNoKeyUpdate`, `ForKeyShare` in [`PgSelectQuery.cs`](Drizzle4Dotnet/src/PgSql/PgSelectQuery.cs) — untested |
| **RETURNING clause** | Implemented in [`ReturningQuery.cs`](Drizzle4Dotnet/src/Core/Query/ReturningQuery.cs) — no tests for either PgSql or MySql |
| **CTEs on INSERT/UPDATE/DELETE** | `With()` method exists on all query types — no tests for non-SELECT CTEs |
| **SELECT INTO** | `_intoTable` field in [`SelectQuery.cs`](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs:69) — untested |
| **FirstValue/LastValue/NthValue** | Window functions in [`PgFunctions.cs`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs:215-228) — untested |
| **ArrayAny/ArrayAll** | PgSql array operators in [`PgFunctions.cs`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs:128-133) — untested |
| **PgSql GroupsBetween** | Window frame in [`PgWindowFrame.cs`](Drizzle4Dotnet/src/PgSql/Nodes/PgWindowFrame.cs:69) — untested |
| **RowToJson** | In [`PgFunctions.cs`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs:101) — commented out in tests |
| **Age(t1, t2)** | Two-date overload in [`PgFunctions.cs`](Drizzle4Dotnet/src/PgSql/PgFunctions.cs:64) — commented out in tests |
| **MySql INSERT DefaultValues** | Not applicable for MySQL, but core InsertQuery supports it |
| **MySql UPDATE SetRecord** | `Set(IUpdateRecord)` method in [`UpdateQuery.cs`](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs:38) — tested in PgSql, not in MySql |

### 🟡 Minor Gaps
| Feature | Notes |
|---------|-------|
| `IIf()` function | Defined in [`Functions.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Functions.cs:255) — untested in both dialects |
| `SELECT ... WITH RECURSIVE` | Implemented but test is commented out in [`Base.cs`](Test/Select/Base.cs:575) |
| `Not()` operator | Defined in [`Operators.cs`](Drizzle4Dotnet/src/Core/Shared/Operators/Operators.cs:149) — untested in both dialects |
| `PgSqlStatics.TimeZone()` | In [`PgSqlStatics.cs`](Drizzle4Dotnet/src/PgSql/PgSqlStatics.cs) — no direct test |

---

## 8. OVERALL ASSESSMENT

| Category | Coverage Estimate |
|----------|:-----------------:|
| **PgSql SELECT features** | ~90% tested |
| **PgSql INSERT features** | ~90% tested |
| **PgSql UPDATE features** | ~80% tested (missing RETURNING, CTE) |
| **PgSql DELETE features** | ~85% tested (missing RETURNING, CTE) |
| **PgSql Compound Queries** | 100% tested |
| **PgSql Functions** | ~90% tested (missing ArrayAny, ArrayAll, RowToJson, GroupBetween) |
| **PgSql Operators** | 100% tested |
| **PgSql Window Functions** | ~70% tested (missing FirstValue, LastValue, NthValue) |
| **MySql SELECT features** | ~60% tested (many gaps in subqueries, ORDER BY, GROUP BY, LIMIT) |
| **MySql INSERT features** | ~95% tested |
| **MySql UPDATE features** | ~90% tested (missing SetRecord) |
| **MySql DELETE features** | 100% tested |
| **MySql Compound Queries** | 100% tested |
| **MySql Functions** | ~95% tested |
| **MySql Operators** | **0% tested** (NullSafeEqual, Regexp, NotRegexp all untested) |
| **RETURNING clause** | **0% tested** (both dialects) |
| **CTEs on DML** | **0% tested** (both dialects) |
| **LATERAL Joins** | **0% tested** (PgSql only) |

**Bottom line:** The PgSql side has substantially better test coverage (~85-90%) than the MySql side (~60-95% depending on area). The main gaps are: (1) MySql SELECT subquery and aggregate tests, (2) MySql operators completely untested, (3) RETURNING clause untested on both sides, (4) Window function edge cases, and (5) DML CTEs untested.