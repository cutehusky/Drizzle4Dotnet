using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.MySql.Nodes;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific INSERT query builder.
/// Extends InsertQuery with MySQL-specific features:
/// - ON DUPLICATE KEY UPDATE (upsert) with VALUES() and custom expression support
/// - INSERT IGNORE
/// - INSERT ... SET col=value syntax
/// MySQL does not support RETURNING — use MySqlFunctions.LastInsertId() instead.
/// </summary>
public class MySqlInsertQuery<TTable> : InsertQuery<TTable, MySqlSqlDialectImpl, MySqlInsertQuery<TTable>>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private bool _ignore;
    private readonly Dictionary<string, object?> _onDuplicateKeyUpdates = new();

    public MySqlInsertQuery(TTable table, IQueryExecutor<MySqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // ON DUPLICATE KEY UPDATE — column names (shorthand for VALUES(col))
    // ======================================================================

    /// <summary>
    /// Adds ON DUPLICATE KEY UPDATE for the specified columns.
    /// Each column is set to VALUES(`column`) — the value that would have been inserted.
    /// Example: .OnDuplicateKeyUpdate("Name", "Age")
    /// Generates: ON DUPLICATE KEY UPDATE `Name` = VALUES(`Name`), `Age` = VALUES(`Age`)
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdateValues(params IColumnOfTable<TTable>[] columns)
    {
        foreach (var col in columns)
        {
            _onDuplicateKeyUpdates[col.Identifier] = new MySqlValuesNode(col.Identifier);
        }
        return this;
    }

    // ======================================================================
    // ON DUPLICATE KEY UPDATE — custom SET expressions (like PgSql SetOnConflict)
    // ======================================================================

    /// <summary>
    /// Sets a column to a literal value in ON DUPLICATE KEY UPDATE.
    /// Example: .OnDuplicateKeyUpdate(table.Age, 25)
    /// Generates: ON DUPLICATE KEY UPDATE `age` = @p0
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdate<T>(DbColumn<T, TTable, MySqlSqlDialectImpl> column, T value)
    {
        _onDuplicateKeyUpdates[column.Identifier] = value;
        return this;
    }

    /// <summary>
    /// Sets a column to an SQL expression in ON DUPLICATE KEY UPDATE.
    /// Example: .OnDuplicateKeyUpdate(table.Age, table.Age + 1)
    /// Generates: ON DUPLICATE KEY UPDATE `age` = `users`.`age` + 1
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdate<T>(DbColumn<T, TTable, MySqlSqlDialectImpl> column, ISql<T> value)
    {
        _onDuplicateKeyUpdates[column.Identifier] = value;
        return this;
    }

    /// <summary>
    /// Adds multiple column-value pairs for ON DUPLICATE KEY UPDATE.
    /// Values can be literal values or IGenericSql expressions.
    /// </summary>
    public MySqlInsertQuery<TTable> SetOnDuplicateKey(Dictionary<IColumnOfTable<TTable>, object?> columnValuePairs)
    {
        foreach (var kv in columnValuePairs)
        {
            _onDuplicateKeyUpdates[kv.Key.Identifier] = kv.Value;
        }
        return this;
    }

    // ======================================================================
    // IGNORE
    // ======================================================================

    /// <summary>
    /// Adds INSERT IGNORE — silently skip rows that cause duplicate key errors.
    /// </summary>
    public MySqlInsertQuery<TTable> Ignore()
    {
        _ignore = true;
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        base.BuildSql(sqlBuilder);
        // Append ON DUPLICATE KEY UPDATE if configured
        AppendOnDuplicateKeyUpdate(sqlBuilder);
    }
    
    protected override void BuildInsertKeywords(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INSERT");
        if (_ignore) sqlBuilder.Append(" IGNORE");
        sqlBuilder.Append(" INTO ");
    }

    private void AppendOnDuplicateKeyUpdate(ISqlBuilder sqlBuilder)
    {
        if (_onDuplicateKeyUpdates.Count == 0) return;

        sqlBuilder.Append(" ON DUPLICATE KEY UPDATE ");
        SqlStatics.BuildSetValues<MySqlSqlDialectImpl>(sqlBuilder, _onDuplicateKeyUpdates);
    }
}
