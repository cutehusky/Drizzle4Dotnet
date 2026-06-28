using System.Data.Common;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Oracle.Query;

namespace Drizzle4Dotnet.Oracle;

/// <summary>
/// Oracle-specific database client.
/// Returns Oracle-specific query types (OracleSelectQuery, OracleInsertQuery, etc.)
/// that support Oracle-specific features like OFFSET/FETCH, RETURNING ... INTO,
/// FOR UPDATE, FULL OUTER JOIN, NATURAL JOIN, and MERGE.
/// </summary>
public class OracleDbClient : DbClientWithTransaction<OracleDbClient, OracleSqlDialectImpl>
{
    public OracleDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public OracleSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, OracleSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<OracleSqlDialectImpl>
    {
        return new OracleSelectQuery<TReturn, TVirtualTable>(selectedColumns, this);
    }

    public OracleSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, OracleSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<OracleSqlDialectImpl>
    {
        return new OracleSelectQuery<TReturn, TVirtualTable>(selectedColumns, this).Distinct();
    }

    public OracleInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleInsertQuery<TTable>(table, this);
    }

    public OracleUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleUpdateQuery<TTable>(table, this);
    }

    public OracleDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleDeleteQuery<TTable>(table, this);
    }

    public OracleMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<OracleSqlDialectImpl>
    {
        return new OracleMergeQuery<TTable>(table, this);
    }

    protected override OracleDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
    {
        return new OracleDbClient(conn, transaction);
    }
}
