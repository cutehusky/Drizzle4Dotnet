using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Oracle.Query;

namespace Drizzle4Dotnet.Oracle;

/// <summary>
/// Oracle-specific query builder for building SQL without a database connection.
/// Returns Oracle-specific query types for testing and query composition.
/// </summary>
public class OracleQueryBuilder
{
    // SQL-only builder — no executor needed (queries support Build() without execution)
    private static readonly IQueryExecutor<OracleSqlDialectImpl>? _nullExecutor = null;

    public OracleSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, OracleSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<OracleSqlDialectImpl>
    {
        return new OracleSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!);
    }

    public OracleSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, OracleSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<OracleSqlDialectImpl>
    {
        return new OracleSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!).Distinct();
    }

    public OracleInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleInsertQuery<TTable>(table, _nullExecutor!);
    }

    public OracleUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleUpdateQuery<TTable>(table, _nullExecutor!);
    }

    public OracleDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleDeleteQuery<TTable>(table, _nullExecutor!);
    }

    public OracleMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleMergeQuery<TTable>(table, _nullExecutor!);
    }
}
