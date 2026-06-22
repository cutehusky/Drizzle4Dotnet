using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific DELETE query builder.
/// Extends the standard DeleteQuery for MySQL compatibility.
/// MySQL does not support RETURNING — use ROW_COUNT() instead.
/// </summary>
public class MySqlDeleteQuery<TTable> : DeleteQuery<TTable, MySqlSqlDialectImpl>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    public MySqlDeleteQuery(TTable table, DbClient<MySqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }
}
