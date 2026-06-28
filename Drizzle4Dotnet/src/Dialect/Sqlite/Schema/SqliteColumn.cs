using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Sqlite;

/// <summary>
/// SQLite-specific column type. Alias for DbColumn{T, TTable, SqliteSqlDialectImpl}.
/// </summary>
public class SqliteColumn<T, TTable> : DbColumn<T, TTable, SqliteSqlDialectImpl>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    public SqliteColumn(string columnName) : base(columnName) { }
}

/// <summary>
/// SQLite-specific virtual column for subqueries and aliases.
/// </summary>
public class SqliteVirtualColumn<T> : VirtualColumn<T, SqliteSqlDialectImpl>
{
    public SqliteVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
