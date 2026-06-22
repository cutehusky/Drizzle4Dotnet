using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific query builder for building SQL without a database connection.
/// Returns MySQL-specific query types for testing and query composition.
/// </summary>
public class MySqlQueryBuilder
{
    public MySqlSelectQuery<TReturn> Select<TReturn>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl> selectedColumns)
    {
        return new MySqlSelectQuery<TReturn>(selectedColumns, null);
    }

    public MySqlSelectQuery<TReturn> SelectDistinct<TReturn>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl> selectedColumns)
    {
        return new MySqlSelectQuery<TReturn>(selectedColumns, null).Distinct() as MySqlSelectQuery<TReturn>;
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
    {
        return new MySqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, null);
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
    {
        return new MySqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, null).Distinct() as MySqlSelectQuery<TReturn, TVirtualTable>;
    }

    public MySqlInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlInsertQuery<TTable>(table, null);
    }

    public MySqlUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlUpdateQuery<TTable>(table, null);
    }

    public MySqlDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlDeleteQuery<TTable>(table, null);
    }

    public MySqlReplaceQuery<TTable> Replace<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlReplaceQuery<TTable>(table, null);
    }
}
