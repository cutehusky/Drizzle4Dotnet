using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific UPDATE query builder.
/// Extends UpdateQuery with RETURNING support.
/// </summary>
public class PgUpdateQuery<TTable> : UpdateQuery<TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    public PgUpdateQuery(TTable table, DbClient<PgSqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    /// <summary>
    /// Adds a RETURNING clause to capture updated rows.
    /// </summary>
    public new ReturningQuery<TReturn, PgSqlSqlDialectImpl> Returning<TReturn>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns)
    {
        return new ReturningQuery<TReturn, PgSqlSqlDialectImpl>(this, selectedColumns);
    }
}
