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
    private readonly Dictionary<string, object?> _setValues = new();

    public MySqlInsertQuery(TTable table, DbClient<MySqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
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

    /// <summary>
    /// MySQL INSERT ... SET col = value syntax.
    /// Alternative to the standard VALUES syntax.
    /// </summary>
    public MySqlInsertQuery<TTable> Set<T>(DbColumn<T, TTable, MySqlSqlDialectImpl> column, T value)
    {
        _setValues[column.Identifier] = value;
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        BuildSqlCte(sqlBuilder);

        // INSERT IGNORE or INSERT ... SET syntax
        if (_setValues.Count > 0)
        {
            // INSERT ... SET col=value syntax
            sqlBuilder.Append("INSERT");
            if (_ignore) sqlBuilder.Append(" IGNORE");
            sqlBuilder.Append(" INTO ");
            Table.BuildRefSql(sqlBuilder);
            sqlBuilder.Append(" SET ");

            bool first = true;
            foreach (var kv in _setValues)
            {
                if (!first) sqlBuilder.Append(", ");
                first = false;
                sqlBuilder.Append(MySqlSqlDialectImpl.BuildIdentifier(kv.Key));
                sqlBuilder.Append(" = ");
                sqlBuilder.Append(sqlBuilder.AddParameter(kv.Value));
            }
            return;
        }

        // Standard INSERT with optional IGNORE
        if (_ignore)
        {
            // Need to rebuild with INSERT IGNORE
            BuildInsertIgnore(sqlBuilder);
            return;
        }

        // Standard INSERT
        base.BuildSql(sqlBuilder);

        // Append ON DUPLICATE KEY UPDATE if configured
        AppendOnDuplicateKeyUpdate(sqlBuilder);
    }

    private void BuildInsertIgnore(ISqlBuilder sqlBuilder)
    {
        if (NewValues.Count == 0)
            throw new InvalidOperationException("No values provided for insert.");

        var allColumns = NewValues.SelectMany(d => d.Keys).Distinct().ToList();

        sqlBuilder.Append("INSERT IGNORE INTO ");
        Table.BuildRefSql(sqlBuilder);
        sqlBuilder.Append(" (");

        for (int i = 0; i < allColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(MySqlSqlDialectImpl.BuildIdentifier(allColumns[i]));
        }
        sqlBuilder.Append(") VALUES ");

        for (int rowIndex = 0; rowIndex < NewValues.Count; rowIndex++)
        {
            if (rowIndex > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append('(');
            var row = NewValues[rowIndex];

            for (int colIndex = 0; colIndex < allColumns.Count; colIndex++)
            {
                if (colIndex > 0) sqlBuilder.Append(", ");
                if (row.TryGetValue(allColumns[colIndex], out var val))
                {
                    sqlBuilder.Append(sqlBuilder.AddParameter(val));
                }
                else
                {
                    sqlBuilder.Append("NULL");
                }
            }
            sqlBuilder.Append(')');
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
