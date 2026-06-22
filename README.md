# Drizzle4Dotnet

Drizzle4Dotnet is a modern, type-safe SQL builder for .NET inspired by Drizzle ORM (TypeScript). It provides a composable, compile-time safe query builder, result mapping, and source-generator powered code generation while being designed for high performance and Native AOT compatibility.

This repository contains the core library, source generators used to produce strong-typed table/select models, benchmarks, and sample projects.

Contents
- Overview and goals
- Key features
- Getting started (build, install, quick examples)
- Source generators and model definitions
- Detailed API reference for core types and attributes
- Native AOT and compatibility notes
- Contributing and license

Summary of project features
- Strongly-typed, compile-time safe query builder and SQL fragments
- Select/Insert/Update/Delete query builders with fluent API
- Typed result projections (generated `Record` / `Model` types)
- Source-generator based mapping (`DbSelect`, `MapWith`, `Table`, `Column`)
- Support for subqueries, CTEs, complex joins, DISTINCT, GROUP BY, HAVING
- PostgreSQL dialect implementation (extensible for more dialects)
- Native AOT friendly design (no runtime reflection)

Quick links to key files
- Core entry points: [Drizzle4Dotnet/src/Core/DbClient.cs](Drizzle4Dotnet/src/Core/DbClient.cs)
- Select API: [Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs)
- Query primitives and builders: [Drizzle4Dotnet/src/Core/Query](Drizzle4Dotnet/src/Core/Query)
- Schema definitions and attributes: [Drizzle4Dotnet/src/Core/Schema](Drizzle4Dotnet/src/Core/Schema)
- Dialects: [Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs](Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs)
- Source generators: [SourceGenerators/SourceGenerators/DbSelectGenerator.cs](SourceGenerators/SourceGenerators/DbSelectGenerator.cs) and [SourceGenerators/SourceGenerators/TableGenerator.cs](SourceGenerators/SourceGenerators/TableGenerator.cs)

Getting started

Prerequisites
- .NET SDK 8.0 or later (or the version used for this project)

# Drizzle4Dotnet

Drizzle4Dotnet is a modern, type-safe SQL builder for .NET inspired by Drizzle ORM (TypeScript). It provides a composable, compile-time safe query builder, generated mapping types, and source-generator powered code generation while prioritizing performance and Native AOT compatibility.

## Table of contents
- [Features](#features)
- [Getting Started](#getting-started)
- [Quick Examples](#quick-examples)
- [Source Generators](#source-generators)
- [API Reference (core)](#api-reference-core)
- [Contributing](#contributing)
- [License](#license)

## Features

- Strongly-typed, compile-time safe query builder
- Fluent Select/Insert/Update/Delete builders
- Generated result projections (`Record` / `Model`) via source generators
- Subqueries, CTEs, complex joins, DISTINCT, GROUP BY, HAVING
- PostgreSQL dialect implementation (extensible)
- Native AOT friendly (no runtime reflection)

## Getting Started

Prerequisites

- .NET SDK 8.0 or later

Build

```bash
dotnet restore Drizzle4Dotnet.sln
dotnet build -c Release
```

Run tests and benchmarks (optional)

```bash
dotnet test
dotnet run -p Benchmark/Benchmark.csproj -c Release
```

Install

Reference the `Drizzle4Dotnet` project directly for development. To publish, use `dotnet pack` and standard NuGet workflows.

## Quick Examples

Select

```csharp
var q = _db
  .Select(UserSelect.Record)
  .From(UsersTable)
  .Where(UsersTable.IsActive.Eq(true))
  .OrderBy(UsersTable.Name)
  .Limit(10);

var (sql, parameters) = q.Build();
```

Insert

```csharp
var insert = _db.Insert(UsersTable)
  .Values(new { Name = "Alice", Email = "alice@example.com" });

var (sql, parameters) = insert.Build();
```

Update

```csharp
var update = _db.Update(UsersTable)
  .Set(UsersTable.Name, "Alice B.")
  .Where(UsersTable.Id.Eq(42));

var (sql, parameters) = update.Build();
```

Delete

```csharp
var del = _db.Delete(UsersTable)
  .Where(UsersTable.Id.Eq(42));

var (sql, parameters) = del.Build();
```

## Source Generators

Two incremental Roslyn generators produce strongly-typed schema and projection types:

- `DbSelectGenerator` — for classes annotated with `DbSelectAttribute` ([SourceGenerators/SourceGenerators/DbSelectGenerator.cs](SourceGenerators/SourceGenerators/DbSelectGenerator.cs)).
- `TableGenerator` — for table/column models ([SourceGenerators/SourceGenerators/TableGenerator.cs](SourceGenerators/SourceGenerators/TableGenerator.cs)).

Common attributes

- `DbSelectAttribute`
- `MapWithAttribute`, `MapWithAliasAttribute`
- `TableAttribute`
- `ColumnAttribute`
- `AliasAttribute`

## API Reference (core)

Core entry points and most-used types

- `QueryBuilder<TDialect>` — primary fluent builder for queries ([Drizzle4Dotnet/src/Core/DbClient.cs](Drizzle4Dotnet/src/Core/DbClient.cs)).
- `SelectQuery<TReturn, TDialect>` — SELECT query type ([Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs](Drizzle4Dotnet/src/Core/Query/Select/SelectQuery.cs)).
- `InsertQuery<TTable, TDialect>` — INSERT builder ([Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs](Drizzle4Dotnet/src/Core/Query/Insert/InsertQuery.cs)).
- `UpdateQuery<TTable, TDialect>` — UPDATE builder ([Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs](Drizzle4Dotnet/src/Core/Query/Update/UpdateQuery.cs)).
- `DeleteQuery<TTable, TDialect>` — DELETE builder ([Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs](Drizzle4Dotnet/src/Core/Query/Delete/DeleteQuery.cs)).

Primitives and schema types

- `ISql`, `ISql<T>` — core SQL fragment interfaces ([Drizzle4Dotnet/src/Core/Shared/ISql.cs](Drizzle4Dotnet/src/Core/Shared/ISql.cs)).
- `ITable<TDialect>`, `IVirtualTable<TDialect>` — table and virtual table contracts ([Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs](Drizzle4Dotnet/src/Core/Schema/Tables/ITable.cs)).
- `IColumn<T>` — column descriptor ([Drizzle4Dotnet/src/Core/Schema/Columns/IColumn.cs](Drizzle4Dotnet/src/Core/Schema/Columns/IColumn.cs)).
- `ISqlDialect` — dialect abstraction and `PgSqlSqlDialectImpl` implementation ([Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs](Drizzle4Dotnet/src/Dialect/PgSqlSqlDialectImpl.cs)).

For a full, itemized reference of every public type, I can generate a separate `API.md` file listing and documenting public types.

## Contributing

- Open issues for design discussions or bugs.
- Create branches per feature and issue, run tests before opening a PR.

## License

This project is licensed under the MIT License.

---

To generate an expanded `API.md` with per-type documentation, reply with `API.md` and I will produce it from the code scan.
Contributing
- Open an issue to discuss breaking changes or new features.
- Submit pull requests against `main` following the project's coding and testing conventions.

License
- This project is provided under the MIT License.

Acknowledgements
- Inspired by the Drizzle ORM (TypeScript), adapted for .NET with a focus on performance and strong static typing.

If you would like, I can also generate a separate `API.md` file with an expanded, itemized reference for every public type found in the codebase. Would you like that? 
