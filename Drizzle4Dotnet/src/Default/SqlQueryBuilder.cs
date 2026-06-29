using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Default.Query;

namespace Drizzle4Dotnet.Default;

/// <summary>
/// Default/generic query builder for building SQL without a database connection.
/// Uses <see cref="DefaultSqlDialectImpl"/> for standard SQL syntax.
/// </summary>
public class SqlQueryBuilder
{
    // SQL-only builder — no executor needed (queries support Build() without execution)
    private static readonly IQueryExecutor<DefaultSqlDialectImpl>? _nullExecutor = null;

    public DefaultSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, DefaultSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<DefaultSqlDialectImpl>
    {
        return new DefaultSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!);
    }

    public DefaultSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, DefaultSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<DefaultSqlDialectImpl>
    {
        return new DefaultSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!).Distinct();
    }

    public DefaultInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<DefaultSqlDialectImpl>
    {
        return new DefaultInsertQuery<TTable>(table, _nullExecutor!);
    }

    public DefaultUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<DefaultSqlDialectImpl>
    {
        return new DefaultUpdateQuery<TTable>(table, _nullExecutor!);
    }

    public DefaultDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<DefaultSqlDialectImpl>
    {
        return new DefaultDeleteQuery<TTable>(table, _nullExecutor!);
    }
}
