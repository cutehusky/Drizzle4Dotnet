using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific UPDATE query builder.
/// Extends the standard UpdateQuery for MySQL compatibility.
/// MySQL does not support RETURNING — use LAST_INSERT_ID() or ROW_COUNT() instead.
/// </summary>
public class MySqlUpdateQuery<TTable> : UpdateQuery<TTable, MySqlSqlDialectImpl>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    public MySqlUpdateQuery(TTable table, DbClient<MySqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }
}
