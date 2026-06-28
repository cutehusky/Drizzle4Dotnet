using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.Mssql.Query;

namespace Drizzle4Dotnet.Mssql;

/// <summary>
/// MSSQL-specific query builder for building SQL without a database connection.
/// Returns MSSQL-specific query types for testing and query composition.
/// </summary>
public class MssqlQueryBuilder
{
    // SQL-only builder — no executor needed (queries support Build() without execution)
    private static readonly IQueryExecutor<MssqlSqlDialectImpl>? _nullExecutor = null;

    public MssqlSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
    {
        return new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!);
    }

    public MssqlSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
    {
        return new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!).Distinct();
    }

    public MssqlInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlInsertQuery<TTable>(table, _nullExecutor!);
    }

    public MssqlUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlUpdateQuery<TTable>(table, _nullExecutor!);
    }

    public MssqlDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlDeleteQuery<TTable>(table, _nullExecutor!);
    }

    public MssqlMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlMergeQuery<TTable>(table, _nullExecutor!);
    }
}
