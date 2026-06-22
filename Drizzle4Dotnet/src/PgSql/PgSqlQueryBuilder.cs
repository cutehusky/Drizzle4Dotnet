
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific query builder for building SQL without a database connection.
/// Returns PostgreSQL-specific query types for testing and query composition.
/// </summary>
public class PgSqlQueryBuilder
{
    public PgSelectQuery<TReturn> Select<TReturn>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns)
    {
        return new PgSelectQuery<TReturn>(selectedColumns, null);
    }

    public PgSelectQuery<TReturn> SelectDistinct<TReturn>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns)
    {
        return new PgSelectQuery<TReturn>(selectedColumns, null).Distinct() as PgSelectQuery<TReturn>;
    }

    public PgSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
    {
        return new PgSelectQuery<TReturn, TVirtualTable>(selectedColumns, null);
    }

    public PgSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
    {
        return new PgSelectQuery<TReturn, TVirtualTable>(selectedColumns, null).Distinct() as PgSelectQuery<TReturn, TVirtualTable>;
    }

    public PgInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgInsertQuery<TTable>(table, null);
    }

    public PgUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgUpdateQuery<TTable>(table, null);
    }

    public PgDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgDeleteQuery<TTable>(table, null);
    }
}
