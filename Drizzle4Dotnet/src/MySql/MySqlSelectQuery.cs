using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific SELECT query builder.
/// Extends the standard SelectQuery with MySQL-specific behavior.
/// MySQL does not support LATERAL joins, FOR NO KEY UPDATE, or FOR KEY SHARE.
/// </summary>
public class MySqlSelectQuery<TReturn> : SelectQuery<TReturn, MySqlSqlDialectImpl, MySqlSelectQuery<TReturn>>
{
    public MySqlSelectQuery(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl> selectedColumns,
        DbClient<MySqlSqlDialectImpl> dbClient
    ) : base(selectedColumns, dbClient)
    {
    }
}


/// <summary>
/// MySQL-specific SELECT query builder with virtual table support.
/// </summary>
public class MySqlSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, MySqlSqlDialectImpl, TVirtualTable, MySqlSelectQuery<TReturn, TVirtualTable>>
    where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
{
    public MySqlSelectQuery(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns,
        DbClient<MySqlSqlDialectImpl> dbClient
    ) : base(selectedColumns, dbClient)
    {
    }
}
