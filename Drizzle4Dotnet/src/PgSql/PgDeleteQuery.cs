using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific DELETE query builder.
/// Extends DeleteQuery with RETURNING support.
/// </summary>
public class PgDeleteQuery<TTable> : DeleteQuery<TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    public PgDeleteQuery(TTable table, DbClient<PgSqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    /// <summary>
    /// Adds a RETURNING clause to capture deleted rows.
    /// </summary>
    public new ReturningQuery<TReturn, PgSqlSqlDialectImpl> Returning<TReturn>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns)
    {
        return new ReturningQuery<TReturn, PgSqlSqlDialectImpl>(this, selectedColumns);
    }
}
