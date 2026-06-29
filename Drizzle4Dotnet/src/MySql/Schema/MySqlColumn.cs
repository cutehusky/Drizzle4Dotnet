using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.MySql.Schema;

/// <summary>
/// MySQL-specific column type. Alias for DbColumn{T, TTable, MySqlSqlDialectImpl}.
/// </summary>
public class MySqlColumn<T, TTable> : DbColumn<T, TTable, MySqlSqlDialectImpl>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    public MySqlColumn(string columnName) : base(columnName) { }
}

/// <summary>
/// MySQL-specific virtual column for subqueries and aliases.
/// </summary>
public class MySqlVirtualColumn<T> : VirtualColumn<T, MySqlSqlDialectImpl>
{
    public MySqlVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
