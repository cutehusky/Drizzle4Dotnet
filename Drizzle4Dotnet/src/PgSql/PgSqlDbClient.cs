using System.Data.Common;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific database client.
/// Returns PostgreSQL-specific query types (PgSelectQuery, PgInsertQuery, etc.)
/// that support PostgreSQL-specific features like LATERAL joins, RETURNING,
/// ON CONFLICT (upsert), and advanced lock types.
/// </summary>
public class PgSqlDbClient : DbClientWithTransaction<PgSqlDbClient, PgSqlSqlDialectImpl>
{
    public PgSqlDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public PgSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
    {
        return new PgSelectQuery<TReturn, TVirtualTable>(selectedColumns, this);
    }

    public PgSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
    {
        return new PgSelectQuery<TReturn, TVirtualTable>(selectedColumns, this).Distinct();
    }

    public PgInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgInsertQuery<TTable>(table, this);
    }

    public PgUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgUpdateQuery<TTable>(table, this);
    }

    public PgDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<PgSqlSqlDialectImpl>
    {
        return new PgDeleteQuery<TTable>(table, this);
    }


    protected override PgSqlDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
    {
        return new PgSqlDbClient(conn, transaction);
    }
}
