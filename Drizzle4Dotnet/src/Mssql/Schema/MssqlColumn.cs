using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Mssql;

/// <summary>
/// MSSQL-specific column type. Alias for DbColumn{T, TTable, MssqlSqlDialectImpl}.
/// </summary>
public class MssqlColumn<T, TTable> : DbColumn<T, TTable, MssqlSqlDialectImpl>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    public MssqlColumn(string columnName) : base(columnName) { }
}

/// <summary>
/// MSSQL-specific virtual column for subqueries and aliases.
/// </summary>
public class MssqlVirtualColumn<T> : VirtualColumn<T, MssqlSqlDialectImpl>
{
    public MssqlVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
