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
/// Extends InsertQuery with:
/// - RETURNING clause
/// - ON CONFLICT (upsert)
/// </summary>
public class PgInsertQuery<TTable> : InsertQuery<TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    private string? _conflictTarget;
    private string? _conflictAction; // "DO NOTHING" or "DO UPDATE SET ..."
    private readonly Dictionary<string, object?> _conflictUpdates = new();

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

    /// <summary>
    /// Adds ON CONFLICT DO NOTHING — silently skip on conflict.
    /// </summary>
    public PgInsertQuery<TTable> OnConflictDoNothing(string? conflictTarget = null)
    {
        _conflictTarget = conflictTarget;
        _conflictAction = "DO NOTHING";
        return this;
    }

    /// <summary>
    /// Adds ON CONFLICT (columns) DO UPDATE SET ... — upsert.
    /// </summary>
    public PgInsertQuery<TTable> OnConflictDoUpdate(string conflictTarget, params (string column, object? value)[] updates)
    {
        _conflictTarget = conflictTarget;
        _conflictAction = "DO UPDATE SET";
        _conflictUpdates.Clear();
        foreach (var (col, val) in updates)
        {
            _conflictUpdates[col] = val;
        }
        return this;
    }

    /// <summary>
    /// Sets a specific column in the ON CONFLICT DO UPDATE clause.
    /// </summary>
    public PgInsertQuery<TTable> SetOnConflict<T>(DbColumn<T, TTable, PgSqlSqlDialectImpl> column, T value)
    {
        _conflictUpdates[column.Identifier] = value;
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        base.BuildSql(sqlBuilder);

        // ON CONFLICT clause
        if (_conflictAction != null)
        {
            sqlBuilder.Append(" ON CONFLICT");
            if (!string.IsNullOrEmpty(_conflictTarget))
            {
                sqlBuilder.Append(" (").Append(_conflictTarget).Append(')');
            }
            sqlBuilder.Append(' ').Append(_conflictAction);

            if (_conflictUpdates.Count > 0)
            {
                sqlBuilder.Append(' ');
                bool first = true;
                foreach (var kv in _conflictUpdates)
                {
                    if (!first) sqlBuilder.Append(", ");
                    else first = false;

                    sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(kv.Key));
                    sqlBuilder.Append(" = ");
                    if (kv.Value is IGenericSql op)
                    {
                        op.BuildSql(sqlBuilder);
                    }
                    else if (kv.Value is string s && s == "EXCLUDED")
                    {
                        // Reference to the EXCLUDED pseudo-table
                        sqlBuilder.Append("EXCLUDED.");
                        sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(kv.Key));
                    }
                    else
                    {
                        sqlBuilder.Append(sqlBuilder.AddParameter(kv.Value));
                    }
                }
            }
        }
    }
}


/// <summary>
/// PostgreSQL-specific INSERT query builder with virtual table support.
/// Extends InsertQuery with RETURNING support.
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
