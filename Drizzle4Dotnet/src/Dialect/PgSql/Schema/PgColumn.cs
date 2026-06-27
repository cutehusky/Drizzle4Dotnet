using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific column type. Alias for DbColumn{T, TTable, PgSqlSqlDialectImpl}.
/// </summary>
public class PgColumn<T, TTable> : DbColumn<T, TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    public PgColumn(string columnName) : base(columnName) { }
}

/// <summary>
/// PostgreSQL-specific virtual column for subqueries and aliases.
/// </summary>
public class PgVirtualColumn<T> : VirtualColumn<T, PgSqlSqlDialectImpl>
{
    public PgVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
