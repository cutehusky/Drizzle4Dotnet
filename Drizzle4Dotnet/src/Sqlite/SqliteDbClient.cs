using System.Data.Common;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Sqlite.Query;

namespace Drizzle4Dotnet.Sqlite;

/// <summary>
/// SQLite-specific database client.
/// Returns SQLite-specific query types (SqliteSelectQuery, SqliteInsertQuery, etc.)
/// that support SQLite-specific features like RETURNING, ON CONFLICT (upsert),
/// INSERT OR REPLACE/IGNORE, and NATURAL JOIN.
/// </summary>
public class SqliteDbClient : DbClientWithTransaction<SqliteDbClient, SqliteSqlDialectImpl>
{
    public SqliteDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public SqliteSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
    {
        return new SqliteSelectQuery<TReturn, TVirtualTable>(selectedColumns, this);
    }

    public SqliteSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
    {
        return new SqliteSelectQuery<TReturn, TVirtualTable>(selectedColumns, this).Distinct();
    }

    public SqliteInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
    {
        return new SqliteInsertQuery<TTable>(table, this);
    }

    public SqliteUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
    {
        return new SqliteUpdateQuery<TTable>(table, this);
    }

    public SqliteDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<SqliteSqlDialectImpl>
    {
        return new SqliteDeleteQuery<TTable>(table, this);
    }

    protected override SqliteDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
    {
        return new SqliteDbClient(conn, transaction);
    }
}
