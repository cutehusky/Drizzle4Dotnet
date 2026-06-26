# Drizzle4Dotnet — Design Review

> **Review Date:** 2026-06-26  
> **Scope:** Class design, architecture patterns, code organization, and potential improvements  
> **Reference:** [`class.md`](class.md) (class hierarchy), [`feature.md`](feature.md) (feature inventory)

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Strengths & Good Practices](#2-strengths--good-practices)
3. [Design Concerns & Risks](#3-design-concerns--risks)
4. [Component-by-Component Review](#4-component-by-component-review)
   - 4.1 [Query Builder Hierarchy](#41-query-builder-hierarchy)
   - 4.2 [Interface Design](#42-interface-design)
   - 4.3 [Dialect System](#43-dialect-system)
   - 4.4 [Expression AST](#44-expression-ast)
   - 4.5 [Schema / Migration](#45-schema--migration)
   - 4.6 [Source Generators](#46-source-generators)
   - 4.7 [Execution Layer](#47-execution-layer)
5. [Architecture Diagram Review](#5-architecture-diagram-review)
6. [Recommendations](#6-recommendations)
7. [Priority Action Items](#7-priority-action-items)

---

## 1. Executive Summary

Drizzle4Dotnet is a well-structured .NET SQL builder with a strong type-safe design philosophy. The architecture follows a layered pattern with clean separation between core query building, dialect-specific implementations, and execution. The use of CRTP (Curiously Recurring Template Pattern) for fluent APIs and static abstract interface members for dialect abstraction is modern and idiomatic for .NET 10.

**Overall Rating: 7.5/10** — Solid foundation with some areas needing improvement in duplication reduction, interface segregation, and error handling.

---

## 2. Strengths & Good Practices

### 2.1 Type Safety
- **Strong typing throughout**: `DbColumn<T, TTable, TDialect>` ensures column types are checked at compile time
- **ValueTuple projections**: 1–16 column overloads provide type-safe multi-column results without dynamic mapping
- **Dialect-as-type-parameter**: `TDialect : ISqlDialect` prevents mixing dialect implementations at compile time

### 2.2 Modern .NET Usage
- **Static abstract interfaces** (`ISqlDialect`): Enables polymorphic static dispatch without runtime overhead
- **CRTP pattern**: Fluent methods return concrete subclass types, eliminating the need for casting
- **`IAsyncDisposable`**: Proper async cleanup
- **`TaskAwaiter`**: Direct `await` support on queries; elegant for quick execution

### 2.3 Architecture
- **Layered separation**: Core → Dialect → PgSql/MySql — clear dependency direction
- **Extensible dialect system**: Adding a new dialect (e.g., SQLite, SQL Server) requires implementing `ISqlDialect` and creating subclass queries
- **Expression AST**: SQL expressions are represented as composable node objects, enabling analysis, transformation, and dialect-specific rendering
- **Source generators**: Compile-time code generation avoids runtime reflection — critical for Native AOT

### 2.4 Migration System
- **Snapshot-based diffing**: `SchemaSnapshot.Compare()` cleanly separates schema state from migration generation
- **Serializable snapshots**: JSON serialization for storing schema state between runs
- **Checksum verification**: SHA-256 checksums prevent tampered/duplicate migrations

---

## 3. Design Concerns & Risks

### 3.1 Code Duplication (High)
The most significant issue is massive code duplication between:

| Duplicate Pair | Lines | Problem |
|----------------|-------|---------|
| `SelectQuery<TReturn, TDialect, TSelf>` vs `SelectQuery<TReturn, TDialect, TVirtualTable, TSelf>` | ~200+ lines each | Nearly identical code duplicated for virtual table support |
| `CompoundQuery<TReturn, TDialect>` vs `CompoundQuery<TReturn, TDialect, TVirtualTable>` | ~45 lines each | Same structure duplicated |
| `ReturningExtensions` 16 overloads | ~170 lines | Manually written overloads for each column count |
| `PgSelectQuery<TReturn>` vs `PgSelectQuery<TReturn, TVirtualTable>` | ~105 lines each | Identical lock/lateral logic duplicated |
| `MySqlSelectQuery<TReturn>` vs `MySqlSelectQuery<TReturn, TVirtualTable>` | ~70 lines each | Same pattern |
| Functions extension vs non-extension overloads | Throughout | `Count<T>(ISql<T>)` and `Count<T,TDialect>(IColumnOfDialect<T,TDialect>)` are duplicated |

**Risk:** Maintenance burden — changes to one variant must be manually replicated. Bug fixes easily missed.

### 3.2 Interface Design Issues

#### 3.2.1 `ISelectedColumns` Split
```csharp
ISelectedColumns<TReturn, TDialect>          // Without virtual table
ISelectedColumns<TReturn, TDialect, TVirtualTable>  // With virtual table
```
- These are separate interfaces despite nearly identical contracts
- Forces duplicate generic type parameters throughout the entire query hierarchy
- Alternative: Single interface with covariant/contravariant type parameters where possible

#### 3.2.2 `IReturning` Interface Duplication
```csharp
IReturning<TReturn, TDialect>
IReturning<TReturn, TDialect, TVirtualTable>
```
- Same problem as `ISelectedColumns` — TVirtualTable variant adds only `AsSubQuery()`
- Could use default interface methods to unify

#### 3.2.3 Missing `IWhere` / `IOrderBy` / `IGroupBy` Interfaces
- `Where()`, `OrderBy()`, `GroupBy()` are re-implemented in every query class
- No shared contract for "query that supports WHERE" or "query that supports ORDER BY"

### 3.3 Structural Concerns

#### 3.3.1 `SqlBuilder` is a Mutable Struct
- `SqlBuilder<TDialect>` is a `struct` but contains mutable `StringBuilder` and `Dictionary`
- Passing by value (as `ISqlBuilder` interface) causes boxing
- If passed as struct, mutations may be lost on copies

#### 3.3.2 `QueryBase` Direct `DbClient` Reference
- `QueryBase<TDialect>` holds a reference to `DbClient<TDialect>`
- This couples query objects to a specific client instance
- Queries cannot be serialized, cached, or reused across connections
- A "build-only" (no execution) mode would be cleaner

#### 3.3.3 Static DDL Methods on `DbClient`
- `CreateTable`, `DropTable`, etc. are `protected static` methods on `DbClient`
- These should be standalone classes or factory methods; they don't need instance state
- Tight coupling: DDL generation is tied to having a `DbClient` instance

#### 3.3.4 `CompoundQuery<T>` Extension Methods
- Extension methods are spread across `CompoundQueryExtensions` and `Query<TReturn, ...>` doesn't directly support UNION
- The API surface is fragmented — `query.Union(other)` isn't discoverable from `SelectQuery`

### 3.4 Testability & SOLID

#### 3.4.1 Single Responsibility Principle (SRP) Violations
- `Query<TReturn, TDialect>` handles: SQL building + result mapping + execution (`GetAwaiter`)
- `DbClient<TDialect>` handles: Connection management + SQL building + parameter management + execution

#### 3.4.2 Dependency Inversion
- `QueryBase` depends concretely on `DbClient<TDialect>` (concrete class, not interface)
- Difficult to unit test query building without instantiating a `DbClient`
- `IReturning.Mapper` is a `Func<DbDataReader, TReturn>` — tightly coupled to ADO.NET data readers

#### 3.4.3 Error Handling
- `InsertQuery.BuildSql()` throws `InvalidOperationException` when no values provided — no compile-time check
- Same for `UpdateQuery.BuildSql()` when no SET values
- No validation at API entry point (e.g., `.Value()` could validate immediately)

### 3.5 Code Organization

#### 3.5.1 Fragmented Operator Extensions
- `Operators.cs` defines operators as extension methods on `IColumnOfDialect<T, TDialect>` AND as static methods on `ISql<T>`
- Users must know both `Operators.Eq(col, val)` and `col.Eq(val)` patterns — inconsistent
- PgSql operators in separate `PgOperators.cs` — users must discover this separately

#### 3.5.2 File Size
- `FunctionCallNode.cs`: 43,852 chars — extremely long single file for an AST node
- `Operators.cs`: 18,516 chars — could be broken into `ComparisonOperators.cs`, `LogicalOperators.cs`, etc.
- `PgSqlQueryBuilderExtensions.cs`: 20,000 chars — very large file

### 3.6 Performance Considerations

#### 3.6.1 Allocation Patterns
- Every `Value()` call allocates `Dictionary<string, object?>` for each row
- Each `BuildSql()` call allocates a new `SqlBuilder<TDialect>` struct (with heap-allocated StringBuilder)
- Expression nodes are heap objects — complex queries create many allocations

#### 3.6.2 Reflection in DDL
- `DbClient.GetTableDefinition<TTable>()` uses reflection to extract column metadata
- Called at migration time only, so acceptable, but should be noted

#### 3.6.3 String Concatenation
- SQL building uses `ISqlBuilder.Append(string)` which calls `StringBuilder.Append()`
- Some methods still use string interpolation for SQL fragments (e.g., `$" LIMIT {limit}"`) — this bypasses parameterization

---

## 4. Component-by-Component Review

### 4.1 Query Builder Hierarchy

```
QueryBase<TDialect>
  ├── Query<TDialect>          (DML)
  ├── Query<TReturn, TDialect> (SELECT with return)
  └── Query<TReturn, TDialect, TVirtualTable> (SELECT with VT)
```

**Rating: 6/10**

| Aspect | Assessment |
|--------|------------|
| **Inheritance depth** | 3–4 levels — acceptable |
| **Duplication** | High — two parallel hierarchies for VT vs non-VT |
| **Fluent API** | Good — CRTP returns concrete type |
| **Discoverability** | Fair — methods spread across base + subclasses |

**Recommendation:** Introduce a `ISupportWhere`, `ISupportOrderBy`, `ISupportLimit` interfaces to share clause-building logic.

### 4.2 Interface Design

**Rating: 6.5/10**

| Interface | Assessment |
|-----------|------------|
| `IGenericSql` | Clean, minimal — the foundation type |
| `ISql<T>` / `ISql` | Good separation of typed vs untyped |
| `IAliasedSql<T>` | Clear contract for aliased expressions |
| `ISqlDialect` | Excellent — static abstract pattern is ideal for this |
| `ISelectedColumns<T, TD>` | Over-complex — two variants needed |
| `IReturning<T, TD>` | Reasonable but duplicates |
| `IGenericTable<T>` | Good — `BuildSql` vs `BuildRefSql` separation is elegant |
| `IGetFieldByName` | Useful for dynamic subquery field access |
| `IWriteRecord` | Minimal, clean |

### 4.3 Dialect System

**Rating: 8.5/10**

| Aspect | Assessment |
|--------|------------|
| **Extensibility** | Excellent — new dialect = implement `ISqlDialect` + subclass queries |
| **Static dispatch** | Zero runtime overhead — calls resolved at compile time |
| **Feature flags** | Well-designed — `SupportsReturning`, `SupportsArrays`, etc. |
| **Default sharing** | Good — `SqlDialectDefaults` reduces duplication |
| **Downside** | Dialect-specific queries require duplicate code (e.g., 2 lock implementations for PG) |

**Recommendation:** Move locking logic to dialect interface (`BuildLockClause()`) instead of overriding `BuildSqlLock()` in subclasses.

### 4.4 Expression AST

**Rating: 7.5/10**

| Aspect | Assessment |
|--------|------------|
| **Completeness** | Covers all standard SQL operators and functions |
| **Composability** | Nodes compose arbitrarily — works like syntax trees |
| **Type safety** | Generic type parameters track result types through operators |
| **File organization** | `FunctionCallNode.cs` at 43KB needs splitting |
| **Extensibility** | Adding new node types is straightforward |

### 4.5 Schema / Migration

**Rating: 8/10**

| Aspect | Assessment |
|--------|------------|
| **DDL coverage** | CREATE/DROP/ALTER table + index — comprehensive |
| **Snapshot design** | Clean serialization, clear diff model |
| **Diff algorithm** | Column-level granularity with typed changes |
| **Migration plan** | Ordered steps with SQL generation per dialect |
| **Migration manager** | Tracks applied, checksums, auto-diff |
| **Gap** | No rollback/undo support in migration plans |
| **Gap** | No support for seed data or data migrations |

### 4.6 Source Generators

**Rating: 7/10**

| Aspect | Assessment |
|--------|------------|
| **Incremental generation** | Uses `IIncrementalGenerator` — good for perf |
| **Output quality** | Generates strongly-typed Table and Selection types |
| **Dialect awareness** | Detects PgSql vs MySql from attributes |
| **Missing** | No `[DbSelect]` documented in `Attributes.cs` — exported from generators? |
| **Testability** | Source generators are hard to unit test |

### 4.7 Execution Layer

**Rating: 7/10**

| Aspect | Assessment |
|--------|------------|
| **Async support** | Full async with `ExecuteGetListAsync` / `ExecuteAsync` |
| **Transaction support** | `DbClientWithTransaction` with `RunInTransactionAsync` — well-designed |
| **Parameter handling** | Auto-named parameters with dictionary — clean |
| **Coupling** | Tightly coupled to `DbConnection` — hard to mock |
| **Missing** | No `ExecuteScalar` |
| **Missing** | No `ExecuteReader` (low-level access) |
| **Missing** | No batch execution or bulk operations |

---

## 5. Architecture Diagram Review

### 5.1 Current Architecture (Simplified)

```
┌────────────────────────────────────────────────────────┐
│                    DbClient<TDialect>                    │
│  (connection, execution, DDL factories)                 │
└──────────┬──────────────────────────────┬───────────────┘
           │                              │
           ▼                              ▼
┌─────────────────────┐    ┌──────────────────────────────┐
│  Query<TDialect>    │    │  Query<TReturn, TDialect>     │
│  (DML base)         │    │  (SELECT base)                │
└──────┬──────────────┘    └──────────┬───────────────────┘
       │                              │
       ▼                              ▼
┌─────────────────────┐    ┌──────────────────────────────┐
│  InsertQuery         │    │  SelectQuery                 │
│  UpdateQuery         │    │  (CRTP)                      │
│  DeleteQuery         │    └──────────┬───────────────────┘
│  (CRTP)              │               │
└──────┬──────────────┘               │
       │                              ▼
       ▼                    ┌──────────────────────────────┐
┌─────────────────────┐    │  PgSelectQuery               │
│  PgInsertQuery       │    │  MySqlSelectQuery            │
│  PgUpdateQuery       │    │  (PG LATERAL, locks)        │
│  PgDeleteQuery       │    └──────────────────────────────┘
│  MySqlInsertQuery    │
│  MySqlUpdateQuery    │
│  MySqlDeleteQuery    │
│  MySqlReplaceQuery   │
└─────────────────────┘
```

### 5.2 Dependency Graph Issues

```
Query<TR> ────► DbClient<TD>  (wrong direction — query depends on client)
    │
    ▼
SelectQuery ──► QueryBase ──► SqlBuilder<TD>
                                │
                                ▼
                            ISqlDialect  ✔ (correct direction)

Recommendation:
    Query<TR> ──► IQueryExecutor<TD>  (interface — inverted control)
    DbClient  ──► IQueryExecutor<TD>  (client implements executor)
```

---

## 6. Recommendations

### 6.1 High Priority

| # | Recommendation | Effort | Impact |
|---|---------------|--------|--------|
| R1 | **Eliminate VT/non-VT duplication**: Use single `ISelectedColumns<TReturn, TDialect>` with `IVirtualTable<TDialect>` as optional via default interface methods | Medium | High — removes ~50% of duplicated code |
| R2 | **Extract `IQueryExecutor` interface**: Decouple query building from execution — `QueryBase` depends on interface, `DbClient` implements it | Medium | High — enables unit testing without DbConnection |
| R3 | **Unify extension/non-extension operator overloads**: Use generic helper to generate both patterns from single implementation | Medium | High — removes hundreds of lines of duplication |

### 6.2 Medium Priority

| # | Recommendation | Effort | Impact |
|---|---------------|--------|--------|
| R4 | **Add `ISupportWhere`, `ISupportOrderBy`, `ISupportLimit`, `ISupportJoin` interfaces** | Low | Medium — clean API contracts |
| R5 | **Split large files**: `FunctionCallNode.cs` (43KB → multiple files), `Operators.cs` (18KB → category files) | Low | Medium — maintainability |
| R6 | **Add `ExecuteScalar<T>()` and raw `ExecuteReader()` methods** | Low | Medium — feature completeness |
| R7 | **Move DDL factories out of `DbClient`** into standalone static classes (`Ddl.CreateTable(...)`) | Low | Medium — cleaner separation |

### 6.3 Low Priority

| # | Recommendation | Effort | Impact |
|---|---------------|--------|--------|
| R8 | **Source-generate `ReturningExtensions` overloads** (1–16 columns) instead of hand-writing | Medium | Low — removes boilerplate |
| R9 | **Source-generate `TypedTupleSelectedColumns` overloads** (1–16) | Medium | Low — maintenance automation |
| R10 | **Add `SqlBuilder` pooling** using `ObjectPool<StringBuilder>` | Low | Low — allocation optimization |
| R11 | **Add batch/bulk insert support** | High | Medium — feature expansion |
| R12 | **Add migration rollback/undo** | High | Medium — reliability |
| R13 | **Add data seeding support** to migration system | Medium | Low — feature completeness |

---

## 7. Priority Action Items

### Short-term (Next Sprint)

1. ✅ [R1] **Design consolidation**: Merge `SelectQuery<TReturn,TDialect,TSelf>` and `SelectQuery<TReturn,TDialect,TVirtualTable,TSelf>` into a single class using conditional virtual table support
2. ✅ [R2] **Extract `IQueryExecutor`**: Define interface in `Core.Shared`, implement in `DbClient`, inject in `QueryBase`
3. ✅ [R4] **Add clause interfaces**: `ISupportWhere<TQuery>`, `ISupportOrderBy<TQuery>`, `ISupportLimit<TQuery>` with default implementations

### Medium-term (Next Release)

4. ✅ [R5] **Split large files** for better maintainability
5. ✅ [R6] **Add missing execution methods** (`ExecuteScalar`, `ExecuteReader`)
6. ✅ [R3] **Unify operator extension methods** using `#pragma` or T4 templates

### Long-term (Roadmap)

7. ✅ [R8/R9] **Source-generate repetitive overloads** (Returning, TypedTupleSelectedColumns)
8. ✅ [R10] **Performance optimization**: `SqlBuilder` pooling, reduce allocations
9. ✅ [R11-R13] **Feature expansion**: batch operations, migration rollback, data seeding

---

## Appendix: Code Quality Metrics

| Metric | Current State | Target |
|--------|--------------|--------|
| **Duplication** | ~30-40% (VT vs non-VT) | <10% |
| **Cyclomatic complexity** (avg) | ~3-5 per method | <8 |
| **Class depth** | 3-4 levels | <5 |
| **Method length** (avg) | ~20 lines | <30 |
| **File size** (max) | 43KB (FunctionCallNode.cs) | <15KB |
| **Interface segregation** | Some interfaces too large | Single responsibility |
| **Test coverage** | Extensive (SQL output tests) | Maintain |
| **Static analysis warnings** | Not measured | Zero |

---

## Final Assessment

Drizzle4Dotnet is a **well-architected project** that successfully brings Drizzle ORM's type-safe query building philosophy to .NET. The primary strength is the **strong type safety** and **dialect abstraction** via static abstract interfaces.

The main weakness is **code duplication** driven by the parallel "with virtual table" / "without virtual table" class hierarchies. This is the single biggest area for improvement and would yield the highest maintenance savings.

The **design is production-ready** for PostgreSQL and MySQL, with a clear path for adding new dialects. The migration system is particularly well-designed with snapshot-based diffing and checksum verification.

**Score: 7.5/10** — Solid, with clear improvement roadmap.
