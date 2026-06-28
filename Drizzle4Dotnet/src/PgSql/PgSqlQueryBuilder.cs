
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.PgSql.Query;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific query builder for building SQL without a database connection.
/// Returns PostgreSQL-specific query types for testing and query composition.
/// </summary>
public class PgSqlQueryBuilder
{
    // SQL-only builder — no executor needed (queries support Build() without execution)
    private static readonly IQueryExecutor<PgSqlSqlDialectImpl>? _nullExecutor = null;

    public PgSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
    {
        return new PgSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!);
    }

    public PgSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
    {
        return new PgSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!).Distinct();
    }

    public PgInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgInsertQuery<TTable>(table, _nullExecutor!);
    }

    public PgUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgUpdateQuery<TTable>(table, _nullExecutor!);
    }

    public PgDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgDeleteQuery<TTable>(table, _nullExecutor!);
    }

    public PgMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgMergeQuery<TTable>(table, _nullExecutor!);
    }
}
