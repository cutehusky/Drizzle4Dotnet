using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Sqlite;

/// <summary>
/// SQLite-specific query builder for building SQL without a database connection.
/// Returns SQLite-specific query types for testing and query composition.
/// </summary>
public class SqliteQueryBuilder
{
    // SQL-only builder — no executor needed (queries support Build() without execution)
    private static readonly IQueryExecutor<SqliteSqlDialectImpl>? _nullExecutor = null;

    public SqliteSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
    {
        return new SqliteSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!);
    }

    public SqliteSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
    {
        return new SqliteSelectQuery<TReturn, TVirtualTable>(selectedColumns, _nullExecutor!).Distinct();
    }

    public SqliteInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
    {
        return new SqliteInsertQuery<TTable>(table, _nullExecutor!);
    }

    public SqliteUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
    {
        return new SqliteUpdateQuery<TTable>(table, _nullExecutor!);
    }

    public SqliteDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
    {
        return new SqliteDeleteQuery<TTable>(table, _nullExecutor!);
    }
}
