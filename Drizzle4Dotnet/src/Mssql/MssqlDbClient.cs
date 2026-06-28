using System.Data.Common;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.Mssql.Query;

namespace Drizzle4Dotnet.Mssql;

/// <summary>
/// MSSQL-specific database client.
/// Returns MSSQL-specific query types (MssqlSelectQuery, MssqlInsertQuery, etc.)
/// that support MSSQL-specific features like TOP, OFFSET/FETCH, APPLY joins,
/// OUTPUT clause, and MERGE.
/// </summary>
public class MssqlDbClient : DbClientWithTransaction<MssqlDbClient, MssqlSqlDialectImpl>
{
    public MssqlDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public MssqlSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
    {
        return new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, this);
    }

    public MssqlSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MssqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MssqlSqlDialectImpl>
    {
        return new MssqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, this).Distinct();
    }

    public MssqlInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlInsertQuery<TTable>(table, this);
    }

    public MssqlUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlUpdateQuery<TTable>(table, this);
    }

    public MssqlDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlDeleteQuery<TTable>(table, this);
    }

    public MssqlMergeQuery<TTable> Merge<TTable>(TTable table)
        where TTable : ITable<MssqlSqlDialectImpl>
    {
        return new MssqlMergeQuery<TTable>(table, this);
    }

    protected override MssqlDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
    {
        return new MssqlDbClient(conn, transaction);
    }
}
