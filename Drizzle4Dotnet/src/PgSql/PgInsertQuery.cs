using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific INSERT query builder.
/// Extends InsertQuery with RETURNING support.
/// </summary>
public class PgInsertQuery<TTable> : InsertQuery<TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    public PgInsertQuery(TTable table, DbClient<PgSqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    /// <summary>
    /// Adds a RETURNING clause to capture inserted rows.
    /// </summary>
    public new ReturningQuery<TReturn, PgSqlSqlDialectImpl> Returning<TReturn>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl> selectedColumns)
    {
        return new ReturningQuery<TReturn, PgSqlSqlDialectImpl>(this, selectedColumns);
    }
}


/// <summary>
/// PostgreSQL-specific INSERT query builder with virtual table support.
/// </summary>
public class PgInsertQuery<TTable, TVirtualTable> : InsertQuery<TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
    where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
{
    public PgInsertQuery(TTable table, DbClient<PgSqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    /// <summary>
    /// Adds a RETURNING clause to capture inserted rows.
    /// </summary>
    public ReturningQuery<TReturn, PgSqlSqlDialectImpl, TVirtualTable> Returning<TReturn>(
        ISelectedColumns<TReturn, PgSqlSqlDialectImpl, TVirtualTable> selectedColumns)
    {
        return new ReturningQuery<TReturn, PgSqlSqlDialectImpl, TVirtualTable>(this, selectedColumns);
    }
}
