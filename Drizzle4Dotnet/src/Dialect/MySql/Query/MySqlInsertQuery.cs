using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific INSERT query builder.
/// Extends InsertQuery with MySQL-specific features:
/// - ON DUPLICATE KEY UPDATE (upsert)
/// - INSERT IGNORE
/// - INSERT ... SET col=value syntax
/// MySQL does not support RETURNING — use MySqlFunctions.LastInsertId() instead.
/// </summary>
public class MySqlInsertQuery<TTable> : InsertQuery<TTable, MySqlSqlDialectImpl, MySqlInsertQuery<TTable>>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private List<string>? _onDuplicateKeyUpdateColumns;
    private bool _onDuplicateKeyUpdateAll;
    private bool _ignore;

    public MySqlInsertQuery(TTable table, IQueryExecutor<MySqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    /// <summary>
    /// Adds ON DUPLICATE KEY UPDATE for the specified columns.
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdate(params string[] columns)
    {
        _onDuplicateKeyUpdateColumns = new List<string>(columns);
        _onDuplicateKeyUpdateAll = false;
        return this;
    }

    /// <summary>
    /// Adds ON DUPLICATE KEY UPDATE for all inserted columns.
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdateAll()
    {
        _onDuplicateKeyUpdateAll = true;
        _onDuplicateKeyUpdateColumns = null;
        return this;
    }

    /// <summary>
    /// Adds INSERT IGNORE — silently skip rows that cause duplicate key errors.
    /// </summary>
    public MySqlInsertQuery<TTable> Ignore()
    {
        _ignore = true;
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        // INSERT IGNORE — needs full manual rebuild
        if (_ignore)
        {
            BuildInsertIgnore(sqlBuilder);
            return;
        }

        // Standard INSERT (delegates to base)
        base.BuildSql(sqlBuilder);

        // Append ON DUPLICATE KEY UPDATE if configured
        AppendOnDuplicateKeyUpdate(sqlBuilder);
    }

    private void BuildInsertIgnore(ISqlBuilder sqlBuilder)
    {
        // Validate: must have values, default values, or a select source
        if (NewValues.Count == 0 && !UseDefaultValues && FromQuery == null)
            throw new InvalidOperationException("No values provided for insert. Use Value(s), DefaultValues(), or From().");

        var allColumns = NewValues.SelectMany(d => d.Keys).Distinct().ToList();

        sqlBuilder.Append("INSERT IGNORE INTO ");
        Table.BuildRefSql(sqlBuilder);

        if (allColumns.Count > 0)
            SqlStatics.BuildInsertColumnList<MySqlSqlDialectImpl>(sqlBuilder, allColumns);

        if (UseDefaultValues)
        {
            sqlBuilder.Append(" DEFAULT VALUES");
        }
        else if (FromQuery != null)
        {
            sqlBuilder.Append(' ');
            FromQuery.BuildSql(sqlBuilder);
        }
        else
        {
            SqlStatics.BuildInsertRowValues(sqlBuilder, NewValues, allColumns);
        }
    }

    private void AppendOnDuplicateKeyUpdate(ISqlBuilder sqlBuilder)
    {
        List<string>? updateColumns = _onDuplicateKeyUpdateColumns;

        if (_onDuplicateKeyUpdateAll)
        {
            // Resolve all columns from current values
            updateColumns = NewValues.SelectMany(d => d.Keys).Distinct().ToList();
        }

        if (updateColumns != null && updateColumns.Count > 0)
        {
            sqlBuilder.Append(MySqlSqlDialectImpl.BuildOnDuplicateKeyUpdate(updateColumns));
        }
    }
}
