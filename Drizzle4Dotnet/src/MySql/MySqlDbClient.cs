using System.Data.Common;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.MySql.Query;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific database client.
/// Returns MySQL-specific query types (MySqlSelectQuery, MySqlInsertQuery, etc.)
/// that support MySQL-specific features like ON DUPLICATE KEY UPDATE, INSERT IGNORE,
/// DELETE/UPDATE with JOIN, LIMIT/ORDER BY on DELETE/UPDATE.
/// </summary>
public class MySqlDbClient : DbClientWithTransaction<MySqlDbClient, MySqlSqlDialectImpl>
{
    public MySqlDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
    {
        return new MySqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, this);
    }

    public MySqlSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, MySqlSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<MySqlSqlDialectImpl>
    {
        return new MySqlSelectQuery<TReturn, TVirtualTable>(selectedColumns, this).Distinct();
    }

    public MySqlInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlInsertQuery<TTable>(table, this);
    }

    public MySqlUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlUpdateQuery<TTable>(table, this);
    }

    public MySqlDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlDeleteQuery<TTable>(table, this);
    }

    public MySqlReplaceQuery<TTable> Replace<TTable>(TTable table)
        where TTable : ITable<MySqlSqlDialectImpl>
    {
        return new MySqlReplaceQuery<TTable>(table, this);
    }

    protected override MySqlDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
    {
        return new MySqlDbClient(conn, transaction);
    }
}
