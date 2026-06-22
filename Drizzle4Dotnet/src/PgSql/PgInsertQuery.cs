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
    private bool _isConstraintTarget;

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
        _isConstraintTarget = false;
        _conflictAction = "DO NOTHING";
        return this;
    }

    /// <summary>
    /// Adds ON CONFLICT (columns) DO UPDATE SET ... — upsert.
    /// </summary>
    public PgInsertQuery<TTable> OnConflictDoUpdate(string conflictTarget, params (string column, object? value)[] updates)
    {
        _conflictTarget = conflictTarget;
        _isConstraintTarget = false;
        _conflictAction = "DO UPDATE SET";
        _conflictUpdates.Clear();
        foreach (var (col, val) in updates)
        {
            _conflictUpdates[col] = val;
        }
        return this;
    }

    /// <summary>
    /// Adds ON CONFLICT ON CONSTRAINT constraint_name DO NOTHING.
    /// PostgreSQL syntax: INSERT ... ON CONFLICT ON CONSTRAINT users_email_key DO NOTHING
    /// </summary>
    public PgInsertQuery<TTable> OnConflictOnConstraint(string constraintName)
    {
        _conflictTarget = constraintName;
        _isConstraintTarget = true;
        _conflictAction = "DO NOTHING";
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

    /// <summary>
    /// INSERT ... SELECT — inserts rows from a subquery.
    /// </summary>
    public new PgInsertQuery<TTable> From(IGenericSql selectQuery)
    {
        base.From(selectQuery);
        return this;
    }

    /// <summary>
    /// INSERT DEFAULT VALUES — inserts a row with all default values.
    /// </summary>
    public new PgInsertQuery<TTable> DefaultValues()
    {
        base.DefaultValues();
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        base.BuildSql(sqlBuilder);
        PgConflictHelper.BuildOnConflictSql(sqlBuilder, _conflictTarget, _isConstraintTarget, _conflictAction, _conflictUpdates);
    }
}


/// <summary>
/// PostgreSQL-specific INSERT query builder with virtual table support.
/// Extends InsertQuery with RETURNING and ON CONFLICT support.
/// </summary>
public class PgInsertQuery<TTable, TVirtualTable> : InsertQuery<TTable, PgSqlSqlDialectImpl>
    where TTable : ITable<PgSqlSqlDialectImpl>
    where TVirtualTable : IVirtualTable<PgSqlSqlDialectImpl>
{
    private string? _conflictTarget;
    private string? _conflictAction;
    private readonly Dictionary<string, object?> _conflictUpdates = new();
    private bool _isConstraintTarget;

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

    /// <summary>
    /// Adds ON CONFLICT DO NOTHING — silently skip on conflict.
    /// </summary>
    public PgInsertQuery<TTable, TVirtualTable> OnConflictDoNothing(string? conflictTarget = null)
    {
        _conflictTarget = conflictTarget;
        _isConstraintTarget = false;
        _conflictAction = "DO NOTHING";
        return this;
    }

    /// <summary>
    /// Adds ON CONFLICT (columns) DO UPDATE SET ... — upsert.
    /// </summary>
    public PgInsertQuery<TTable, TVirtualTable> OnConflictDoUpdate(string conflictTarget, params (string column, object? value)[] updates)
    {
        _conflictTarget = conflictTarget;
        _isConstraintTarget = false;
        _conflictAction = "DO UPDATE SET";
        _conflictUpdates.Clear();
        foreach (var (col, val) in updates)
        {
            _conflictUpdates[col] = val;
        }
        return this;
    }

    /// <summary>
    /// Adds ON CONFLICT ON CONSTRAINT constraint_name DO NOTHING.
    /// </summary>
    public PgInsertQuery<TTable, TVirtualTable> OnConflictOnConstraint(string constraintName)
    {
        _conflictTarget = constraintName;
        _isConstraintTarget = true;
        _conflictAction = "DO NOTHING";
        return this;
    }

    /// <summary>
    /// Sets a specific column in the ON CONFLICT DO UPDATE clause.
    /// </summary>
    public PgInsertQuery<TTable, TVirtualTable> SetOnConflict<T>(DbColumn<T, TTable, PgSqlSqlDialectImpl> column, T value)
    {
        _conflictUpdates[column.Identifier] = value;
        return this;
    }

    /// <summary>
    /// INSERT ... SELECT — inserts rows from a subquery.
    /// </summary>
    public new PgInsertQuery<TTable, TVirtualTable> From(IGenericSql selectQuery)
    {
        base.From(selectQuery);
        return this;
    }

    /// <summary>
    /// INSERT DEFAULT VALUES — inserts a row with all default values.
    /// </summary>
    public new PgInsertQuery<TTable, TVirtualTable> DefaultValues()
    {
        base.DefaultValues();
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        base.BuildSql(sqlBuilder);
        PgConflictHelper.BuildOnConflictSql(sqlBuilder, _conflictTarget, _isConstraintTarget, _conflictAction, _conflictUpdates);
    }
}

/// <summary>
/// Shared helper for building the ON CONFLICT clause to avoid code duplication
/// between PgInsertQuery&lt;TTable&gt; and PgInsertQuery&lt;TTable, TVirtualTable&gt;.
/// </summary>
internal static class PgConflictHelper
{
    public static void BuildOnConflictSql(
        ISqlBuilder sqlBuilder,
        string? conflictTarget,
        bool isConstraintTarget,
        string? conflictAction,
        Dictionary<string, object?> conflictUpdates)
    {
        if (conflictAction == null) return;

        sqlBuilder.Append(" ON CONFLICT");
        if (!string.IsNullOrEmpty(conflictTarget))
        {
            if (isConstraintTarget)
            {
                sqlBuilder.Append(" ON CONSTRAINT ").Append(conflictTarget);
            }
            else
            {
                sqlBuilder.Append(" (").Append(conflictTarget).Append(')');
            }
        }
        sqlBuilder.Append(' ').Append(conflictAction);

        if (conflictUpdates.Count > 0)
        {
            sqlBuilder.Append(' ');
            bool first = true;
            foreach (var kv in conflictUpdates)
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
