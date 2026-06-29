using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Sqlite.Operators.Nodes;

namespace Drizzle4Dotnet.Sqlite.Query;

/// <summary>
/// SQLite-specific INSERT query builder.
/// Extends InsertQuery with:
/// - ON CONFLICT (upsert) with column-list target and DO UPDATE/NOTHING
/// - INSERT OR REPLACE, INSERT OR IGNORE
/// - RETURNING clause
/// 
/// SQLite does NOT support:
/// - INSERT ... DEFAULT VALUES
/// - ON CONFLICT ON CONSTRAINT (no constraint name target)
/// - WHERE on conflict target or SET
/// </summary>
public class SqliteInsertQuery<TTable> : InsertQuery<TTable, SqliteSqlDialectImpl, SqliteInsertQuery<TTable>>
    where TTable : ITable<SqliteSqlDialectImpl>
{
    private string? _orAction; // "OR REPLACE", "OR IGNORE", "OR ROLLBACK", "OR ABORT", "OR FAIL"
    private List<string>? _conflictTargetColumns;
    private string? _conflictAction;
    private readonly Dictionary<string, object?> _conflictUpdates = new();
    
    public SqliteInsertQuery(TTable table, IQueryExecutor<SqliteSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // INSERT OR REPLACE / OR IGNORE / etc.
    // ======================================================================

    /// <summary>
    /// Adds OR REPLACE to the INSERT statement.
    /// Equivalent to ON CONFLICT DO REPLACE (shorthand).
    /// Renders as: INSERT OR REPLACE INTO ...
    /// </summary>
    public SqliteInsertQuery<TTable> OrReplace()
    {
        _orAction = "OR REPLACE";
        return this;
    }

    /// <summary>
    /// Adds OR IGNORE to the INSERT statement.
    /// Equivalent to ON CONFLICT DO NOTHING (shorthand).
    /// Renders as: INSERT OR IGNORE INTO ...
    /// </summary>
    public SqliteInsertQuery<TTable> OrIgnore()
    {
        _orAction = "OR IGNORE";
        return this;
    }

    /// <summary>
    /// Adds OR ROLLBACK to the INSERT statement.
    /// Renders as: INSERT OR ROLLBACK INTO ...
    /// </summary>
    public SqliteInsertQuery<TTable> OrRollback()
    {
        _orAction = "OR ROLLBACK";
        return this;
    }

    /// <summary>
    /// Adds OR ABORT to the INSERT statement.
    /// Renders as: INSERT OR ABORT INTO ...
    /// </summary>
    public SqliteInsertQuery<TTable> OrAbort()
    {
        _orAction = "OR ABORT";
        return this;
    }

    /// <summary>
    /// Adds OR FAIL to the INSERT statement.
    /// Renders as: INSERT OR FAIL INTO ...
    /// </summary>
    public SqliteInsertQuery<TTable> OrFail()
    {
        _orAction = "OR FAIL";
        return this;
    }

    // ======================================================================
    // ON CONFLICT — Column list target
    // ======================================================================

    /// <summary>
    /// Sets the ON CONFLICT target as a list of columns.
    /// SQLite only supports column-list targets (no constraint name targets).
    /// Example: .OnConflict(table.Email, table.Name)
    /// Renders: ON CONFLICT ("email", "name")
    /// </summary>
    public SqliteInsertQuery<TTable> OnConflict(params IColumnOfTable<TTable>[] columns)
    {
        _conflictTargetColumns = columns.Select(c => c.Identifier).ToList();
        return this;
    }

    // ======================================================================
    // CONFLICT ACTION — DO NOTHING
    // ======================================================================

    /// <summary>
    /// Sets the conflict action to DO NOTHING.
    /// Must be called after OnConflict().
    /// Example: .OnConflict(table.Email).DoNothing()
    /// Renders: ON CONFLICT ("email") DO NOTHING
    /// </summary>
    public SqliteInsertQuery<TTable> DoNothing()
    {
        _conflictAction = "DO NOTHING";
        return this;
    }

    // ======================================================================
    // CONFLICT ACTION — DO UPDATE
    // ======================================================================

    /// <summary>
    /// Sets the conflict action to DO UPDATE SET.
    /// Must be called after OnConflict().
    /// Use SetOnConflict() to add SET assignments.
    /// Example: .OnConflict(table.Email).DoUpdate().SetOnConflict(table.Name, "new")
    /// Renders: ON CONFLICT ("email") DO UPDATE SET "name" = @p0
    /// </summary>
    public SqliteInsertQuery<TTable> DoUpdate()
    {
        _conflictAction = "DO UPDATE SET";
        return this;
    }

    /// <summary>
    /// Sets a column to its EXCLUDED value in ON CONFLICT DO UPDATE SET.
    /// Convenience for: SetOnConflict(column, new SqliteExcludedNode(column))
    /// Renders: "name" = excluded."name"
    /// </summary>
    public SqliteInsertQuery<TTable> SetOnConflictExcluded<T>(params DbColumn<T, TTable, SqliteSqlDialectImpl>[] columns)
    {
        foreach (var column in columns)
        {
            _conflictUpdates[column.Identifier] = new SqliteExcludedNode<T>(column.Identifier);
        }
        return this;
    }

    /// <summary>
    /// Sets a specific column in the ON CONFLICT DO UPDATE clause to a literal value.
    /// </summary>
    public SqliteInsertQuery<TTable> SetOnConflict<T>(DbColumn<T, TTable, SqliteSqlDialectImpl> column, T value)
    {
        _conflictUpdates[column.Identifier] = value;
        return this;
    }
    
    /// <summary>
    /// Sets a specific column in the ON CONFLICT DO UPDATE clause to an SQL expression.
    /// </summary>
    public SqliteInsertQuery<TTable> SetOnConflict<T>(DbColumn<T, TTable, SqliteSqlDialectImpl> column, ISql<T> value)
    {
        _conflictUpdates[column.Identifier] = value;
        return this;
    }
    
    /// <summary>
    /// Sets multiple column-value pairs for ON CONFLICT DO UPDATE SET.
    /// </summary>
    public SqliteInsertQuery<TTable> SetOnConflict(Dictionary<IColumnOfTable<TTable>, object?> columnValuePairs)
    {
        foreach (var columnValuePair in columnValuePairs)
        {
            var col = columnValuePair.Key;
            var val = columnValuePair.Value;
            _conflictUpdates[col.Identifier] = val;
        }
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    protected override void ValidateQuery()
    {
        base.ValidateQuery();

        // SQLite does not support DEFAULT VALUES
        if (UseDefaultValues)
            throw new NotSupportedException("SQLite does not support INSERT ... DEFAULT VALUES. " +
                "Use explicit values with the DEFAULT keyword per-column instead.");

        // ON CONFLICT validation
        if (_conflictAction != null)
        {
            var hasTarget = _conflictTargetColumns is { Count: > 0 };
            if (!hasTarget)
            {
                throw new InvalidOperationException(
                    "DoNothing() and DoUpdate() require a conflict target. " +
                    "Call OnConflict(...) first.");
            }
        }

        if (_conflictUpdates.Count > 0 && _conflictAction != "DO UPDATE SET")
        {
            throw new InvalidOperationException(
                "SetOnConflict() requires DoUpdate().");
        }

        if (_conflictAction == "DO UPDATE SET" && _conflictUpdates.Count == 0)
        {
            throw new InvalidOperationException(
                "DoUpdate() requires at least one SetOnConflict(...) assignment.");
        }

        // OR action cannot be combined with ON CONFLICT
        if (_orAction != null && _conflictAction != null)
        {
            throw new InvalidOperationException(
                $"Cannot combine INSERT {_orAction} with ON CONFLICT. " +
                $"Use either OR REPLACE/OR IGNORE syntax or ON CONFLICT syntax, not both.");
        }
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();
        base.BuildSql(sqlBuilder);
        BuildOnConflict(sqlBuilder);
    }
    
    protected override void BuildInsertKeywords(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INSERT");
        if (_orAction != null)
        {
            sqlBuilder.Append(' ').Append(_orAction);
        }
        sqlBuilder.Append(" INTO ");
    }

    private void BuildOnConflict(ISqlBuilder sqlBuilder)
    {
        if (_conflictAction == null) return;

        sqlBuilder.Append(" ON CONFLICT");

        // Build conflict target (column list only)
        if (_conflictTargetColumns is { Count: > 0 })
        {
            sqlBuilder.Append(" (");
            for (int i = 0; i < _conflictTargetColumns.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                sqlBuilder.Append(SqliteSqlDialectImpl.BuildIdentifier(_conflictTargetColumns[i]));
            }
            sqlBuilder.Append(')');
        }

        // Conflict action
        sqlBuilder.Append(' ').Append(_conflictAction);

        // SET values for DO UPDATE
        if (_conflictUpdates.Count > 0)
        {
            sqlBuilder.Append(' ');
            SqlStatics.BuildSetValues<SqliteSqlDialectImpl>(sqlBuilder, _conflictUpdates);
        }
    }
}
