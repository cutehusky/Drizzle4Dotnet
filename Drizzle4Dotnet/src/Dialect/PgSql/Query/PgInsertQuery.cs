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
/// - ON CONFLICT (upsert) with separate target/action, column-list target,
///   ISql{T} / Excluded support, WHERE on target and SET
/// </summary>
public class PgInsertQuery<TTable> : InsertQuery<TTable, PgSqlSqlDialectImpl, PgInsertQuery<TTable>>
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    private List<string>? _conflictTargetColumns;
    private string? _conflictTargetString;
    private bool _isConstraintTarget;
    private string? _conflictAction;
    private readonly Dictionary<string, object?> _conflictUpdates = new();
    private IGenericSql? _conflictTargetWhere;
    private IGenericSql? _conflictSetWhere;
    
    public PgInsertQuery(TTable table, IQueryExecutor<PgSqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // CONFLICT TARGET — column list (type-safe)
    // ======================================================================

    /// <summary>
    /// Sets the ON CONFLICT target as a list of columns.
    /// Example: .OnConflict(table.Email, table.Name)
    /// Generates: ON CONFLICT ("email", "name")
    /// </summary>
    public PgInsertQuery<TTable> OnConflict(params IColumnOfTable<TTable>[] columns)
    {
        _conflictTargetColumns = columns.Select(c => c.Identifier).ToList();
        _conflictTargetString = null;
        _isConstraintTarget = false;
        return this;
    }

    // ======================================================================
    // CONFLICT TARGET — constraint name
    // ======================================================================

    /// <summary>
    /// Adds ON CONFLICT ON CONSTRAINT constraint_name DO NOTHING.
    /// PostgreSQL syntax: INSERT ... ON CONFLICT ON CONSTRAINT users_email_key DO NOTHING
    /// </summary>
    public PgInsertQuery<TTable> OnConflictOnConstraint(string constraintName)
    {
        _conflictTargetString = constraintName;
        _conflictTargetColumns = null;
        _isConstraintTarget = true;
        return this;
    }

    // ======================================================================
    // WHERE on conflict target
    // ======================================================================

    /// <summary>
    /// Adds a WHERE clause to the conflict target.
    /// PostgreSQL syntax: ON CONFLICT (col) WHERE condition
    /// </summary>
    public PgInsertQuery<TTable> WhereConflictTarget(IGenericSql condition)
    {
        _conflictTargetWhere = condition;
        return this;
    }

    // ======================================================================
    // CONFLICT ACTION — DO NOTHING
    // ======================================================================

    /// <summary>
    /// Sets the conflict action to DO NOTHING.
    /// Must be called after OnConflict() or OnConflictOnConstraint().
    /// Example: .OnConflict(table.Email).DoNothing()
    /// Generates: ON CONFLICT ("email") DO NOTHING
    /// </summary>
    public PgInsertQuery<TTable> DoNothing()
    {
        _conflictAction = "DO NOTHING";
        return this;
    }

    // ======================================================================
    // CONFLICT ACTION — DO UPDATE
    // ======================================================================

    /// <summary>
    /// Sets the conflict action to DO UPDATE SET.
    /// Must be called after OnConflict() or OnConflictOnConstraint().
    /// Use SetOnConflict() to add SET assignments.
    /// Example: .OnConflict(table.Email).DoUpdate().SetOnConflict(table.Name, "new")
    /// Generates: ON CONFLICT ("email") DO UPDATE SET "name" = @p0
    /// </summary>
    public PgInsertQuery<TTable> DoUpdate()
    {
        _conflictAction = "DO UPDATE SET";
        return this;
    }

    /// <summary>
    /// Sets a column to its EXCLUDED value in ON CONFLICT DO UPDATE SET.
    /// Convenience for: SetOnConflict(column, PgSqlStatics.Excluded(column))
    /// Generates: "name" = EXCLUDED."name"
    /// </summary>
    public PgInsertQuery<TTable> SetOnConflictExcluded<T>(params DbColumn<T, TTable, PgSqlSqlDialectImpl>[] columns)
    {
        foreach (var column in columns)
        {
            _conflictUpdates[column.Identifier] = PgSqlStatics.Excluded(column);
        }
        return this;
    }

    /// <summary>
    /// Sets a specific column in the ON CONFLICT DO UPDATE clause (DbColumn overload for backward compat).
    /// </summary>
    public PgInsertQuery<TTable> SetOnConflict<T>(DbColumn<T, TTable, PgSqlSqlDialectImpl> column, T value)
    {
        _conflictUpdates[column.Identifier] = value;
        return this;
    }
    
    public PgInsertQuery<TTable> SetOnConflict<T>(DbColumn<T, TTable, PgSqlSqlDialectImpl> column, ISql<T> value)
    {
        _conflictUpdates[column.Identifier] = value;
        return this;
    }
    
    public PgInsertQuery<TTable> SetOnConflict(Dictionary<IColumnOfTable<TTable>, object?> columnValuePairs)
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
    // WHERE on SET (for DO UPDATE)
    // ======================================================================

    /// <summary>
    /// Adds a WHERE clause to the ON CONFLICT DO UPDATE SET clause.
    /// PostgreSQL syntax: DO UPDATE SET col = val WHERE condition
    /// </summary>
    public PgInsertQuery<TTable> WhereOnConflictSet(IGenericSql condition)
    {
        _conflictSetWhere = condition;
        return this;
    }

    // ======================================================================
    // DEFAULT VALUES (PostgreSQL-specific — not in base InsertQuery)
    // ======================================================================

    /// <summary>
    /// INSERT DEFAULT VALUES — inserts a row with all default values.
    /// PostgreSQL-specific: only available in the PgSql dialect.
    /// </summary>
    public PgInsertQuery<TTable> DefaultValues()
    {
        UseDefaultValues = true;
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        base.BuildSql(sqlBuilder);
        PgConflictHelper.BuildOnConflictSql(sqlBuilder, 
            _conflictTargetColumns, 
            _conflictTargetString, 
            _isConstraintTarget, 
            _conflictAction, 
            _conflictUpdates,
            _conflictTargetWhere,
            _conflictSetWhere);
    }
}


/// <summary>
/// Shared helper for building the ON CONFLICT clause to avoid code duplication
/// between PgInsertQuery{TTable} and PgInsertQuery{TTable, TVirtualTable}.
/// </summary>
internal static class PgConflictHelper
{
    public static void BuildOnConflictSql(
        ISqlBuilder sqlBuilder,
        List<string>? conflictTargetColumns,
        string? conflictTargetString,
        bool isConstraintTarget,
        string? conflictAction,
        Dictionary<string, object?> conflictUpdates,
        IGenericSql? conflictTargetWhere = null,
        IGenericSql? conflictSetWhere = null)
    {
        if (conflictAction == null) return;

        sqlBuilder.Append(" ON CONFLICT");

        // Build conflict target
        if (conflictTargetColumns is { Count: > 0 })
        {
            // Column list target: ON CONFLICT (col1, col2)
            sqlBuilder.Append(" (");
            for (int i = 0; i < conflictTargetColumns.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(conflictTargetColumns[i]));
            }
            sqlBuilder.Append(')');
        }
        else if (!string.IsNullOrEmpty(conflictTargetString))
        {
            if (isConstraintTarget)
            {
                sqlBuilder.Append(" ON CONSTRAINT ").Append(conflictTargetString);
            }
            else
            {
                sqlBuilder.Append(' ').Append(conflictTargetString);
            }
        }

        // WHERE on conflict target
        if (conflictTargetWhere != null)
        {
            sqlBuilder.Append(" WHERE ");
            conflictTargetWhere.BuildSql(sqlBuilder);
        }

        // Conflict action
        sqlBuilder.Append(' ').Append(conflictAction);

        // SET values for DO UPDATE
        if (conflictUpdates.Count > 0)
        {
            sqlBuilder.Append(' ');
            SqlStatics.BuildSetValues<PgSqlSqlDialectImpl>(sqlBuilder, conflictUpdates);
        }

        // WHERE on SET (for DO UPDATE)
        if (conflictSetWhere != null)
        {
            sqlBuilder.Append(" WHERE ");
            conflictSetWhere.BuildSql(sqlBuilder);
        }
    }
}
