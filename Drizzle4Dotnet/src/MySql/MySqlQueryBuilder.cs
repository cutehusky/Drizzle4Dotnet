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
    // SQL-only builder — no executor needed (queries support Build() without execution)
    private static readonly IQueryExecutor<MySqlSqlDialectImpl>? _nullExecutor = null;

    public MySqlSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
    {
        return new MySqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!);
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
    {
        return new MySqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!).Distinct();
    }

    public MySqlInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlInsertQuery<TTable>(table, _nullExecutor!);
    }

    public MySqlUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlUpdateQuery<TTable>(table, _nullExecutor!);
    }

    public MySqlDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlDeleteQuery<TTable>(table, _nullExecutor!);
    }

    public MySqlReplaceQuery<TTable> Replace<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlReplaceQuery<TTable>(table, _nullExecutor!);
    }
}
