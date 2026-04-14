# Drizzle4Dotnet

A modern, type-safe SQL builder for .NET inspired by Drizzle ORM (TypeScript).

Drizzle4Dotnet brings the same philosophy of **type safety**, **composability**, and **developer ergonomics** into the .NET ecosystem, with strong focus on performance and Native AOT compatibility.

---

## ✨ Features

* Strongly typed query builder (compile-time safety)
* CRUD operations
* Typed result mapping (record / model)
* Source Generator powered
* Column-level selection
* CTE (Common Table Expressions)
* Subqueries (nested + reusable)
* Complex joins (inner, left, self, many-to-many)
* Insert from SELECT
* DISTINCT, GROUP BY, HAVING
* ORDER BY, LIMIT, OFFSET
* Null-safe expressions
* PostgreSQL support (more dialects planned)
* Native AOT friendly
* Custom SQL functions (planned)

---

## 🚀 Philosophy

* No runtime reflection
* Everything is type-safe
* SQL is explicit but composable
* Generated code over magic
---

## 🧱 Defining Select Models

Use `[DbSelect]` with `[MapWith]` to define typed result projections.

```csharp
[DbSelect]
public partial class UserSelect
{
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Id)]
    public int Id { get; set; }

    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Email)]
    public string Email { get; set; }
    
    [MapWith(typeof(UsersTable), UsersTable.ColumnNames.Name)]
    public string Name { get; set; }
}
```

The source generator will automatically:

* Generate SQL projection
* Generate mapping logic
* Generate `Record` and `Model`

---

## 🧩 Basic Query

```csharp
var query = _db
    .Select(UserSelect.Record)
    .From(users);

var (sql, parameters) = query.Build();
```

---

## 🔍 WHERE Conditions

```csharp
var query = _db
    .Select(UserSelect.Record)
    .From(users)
    .Where(And(
        UsersTable.Id.Eq(1),
        Like(UsersTable.Email, "%@example.com")
    ));
```

---

## 🔗 Joins

### Inner Join

```csharp
var query = _db
    .Select(UserSelect.Record)
    .From(users)
    .InnerJoin(departments,
        Eq(UsersTable.DepartmentId, DepartmentsTable.Id));
```

### Multiple Joins

```csharp
var query = _db
    .Select(UserSelect.Record)
    .From(users)
    .InnerJoin(departments,
        Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
    .InnerJoin(roles,
        Eq(UsersTable.RoleId, RolesTable.Id));
```

### Left Join

```csharp
.LeftJoin(managers,
    Eq(UsersTable.ManagerId, ManagersTable.Id))
```

---

## 🔁 Subqueries

```csharp
var subQuery = _db
    .Select(UsersTable.Id)
    .From(users)
    .Where(Eq(UsersTable.IsActive, true));

var query = _db
    .Select(UserSelect.Record)
    .From(users)
    .Where(In(UsersTable.Id, subQuery));
```

---

## 📊 Group By + Having

```csharp
var query = _db
    .Select(
        DepartmentsTable.Id,
        DepartmentsTable.Name,
        Count(UsersTable.Id).As("UserCount")
    )
    .From(users)
    .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
    .GroupBy(DepartmentsTable.Id, DepartmentsTable.Name)
    .Having(Gt(Count(UsersTable.Id), 5));
```

---

## 📌 Ordering & Pagination

```csharp
var query = _db
    .Select(UserSelect.Record)
    .From(users)
    .OrderBy(UsersTable.Name)
    .OrderBy(UsersTable.Age, false)
    .Limit(10)
    .Offset(20);
```

---

## 🧠 Distinct

```csharp
var query = _db
    .SelectDistinct(UserSelect.Record)
    .From(users);
```

---

## 🧪 Complex Query Example

```csharp
var query = _db
    .Select(UserWithRelationsSelect.Record)
    .From(users)
    .InnerJoin(departments, Eq(UsersTable.DepartmentId, DepartmentsTable.Id))
    .InnerJoin(roles, Eq(UsersTable.RoleId, RolesTable.Id))
    .LeftJoin(managers, Eq(UsersTable.ManagerId, ManagersTable.Id))
    .Where(
        Eq(UsersTable.IsActive, true),
        Gt(UsersTable.Age, 25),
        Like(UsersTable.Email, "%@company.com")
    )
    .OrderBy(UsersTable.Name)
    .Limit(50);
```

---

## 🏗 Table Definition Example

```csharp
partial class DepartmentsTable : IDbTable<PgSqlSqlDialectImpl>
{
    public static string TableName => "Departments";
    public static string SchemaName => "public";

    public static DbColumn<int, DepartmentsTable, PgSqlSqlDialectImpl> Id { get; set; }
    public static DbColumn<string, DepartmentsTable, PgSqlSqlDialectImpl> Name { get; set; }
    public static DbColumn<string, DepartmentsTable, PgSqlSqlDialectImpl> Code { get; set; }
}
```

Generated features include:

* Typed columns
* SQL fragments
* Result record structs
* Model classes
* Subquery/CTE support


---

## ⚡ Native AOT

Drizzle4Dotnet is designed with Native AOT in mind:

* No reflection
* Source generators only
* Predictable runtime behavior

---

## 🛠 Roadmap

* Custom SQL functions
* More database dialects (MySQL, SQLite, MSSQL)
* Migration tooling
* Query caching

---

## 🤝 Contributing

Contributions are welcome. Feel free to open issues or submit pull requests.

---

## 📄 License

MIT License
