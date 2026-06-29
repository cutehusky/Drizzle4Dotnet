using System.Data.Common;
using Drizzle4Dotnet.Core.Default.Query;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Default;

/// <summary>
/// Default/generic SQL database client using <see cref="DefaultSqlDialectImpl"/>.
/// Provides standard SQL query building without dialect-specific features.
/// Use this for unsupported or unknown database dialects.
/// </summary>
public class SqlDbClient : DbClientWithTransaction<SqlDbClient, DefaultSqlDialectImpl>
{
    public SqlDbClient(DbConnection conn, DbTransaction? transaction = null)
        : base(conn, transaction)
    {
    }

    public DefaultSelectQuery<TReturn, TVirtualTable> Select<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, DefaultSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<DefaultSqlDialectImpl>
    {
        return new DefaultSelectQuery<TReturn, TVirtualTable>(selectedColumns, this);
    }

    public DefaultSelectQuery<TReturn, TVirtualTable> SelectDistinct<TReturn, TVirtualTable>(
        ISelectedColumns<TReturn, DefaultSqlDialectImpl, TVirtualTable> selectedColumns) where TVirtualTable : IVirtualTable<DefaultSqlDialectImpl>
    {
        return new DefaultSelectQuery<TReturn, TVirtualTable>(selectedColumns, this).Distinct();
    }

    public DefaultInsertQuery<TTable> Insert<TTable>(TTable table)
        where TTable : ITable<DefaultSqlDialectImpl>
    {
        return new DefaultInsertQuery<TTable>(table, this);
    }

    public DefaultUpdateQuery<TTable> Update<TTable>(TTable table)
        where TTable : ITable<DefaultSqlDialectImpl>
    {
        return new DefaultUpdateQuery<TTable>(table, this);
    }

    public DefaultDeleteQuery<TTable> Delete<TTable>(TTable table)
        where TTable : ITable<DefaultSqlDialectImpl>
    {
        return new DefaultDeleteQuery<TTable>(table, this);
    }

    protected override SqlDbClient CreateInstance(DbConnection conn, DbTransaction? transaction)
    {
        return new SqlDbClient(conn, transaction);
    }
}
