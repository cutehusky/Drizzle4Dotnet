using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific INSERT query builder.
/// Extends InsertQuery with MySQL-specific features like ON DUPLICATE KEY UPDATE.
/// MySQL does not support RETURNING — use LAST_INSERT_ID() instead.
/// </summary>
public class MySqlInsertQuery<TTable> : InsertQuery<TTable, MySqlSqlDialectImpl>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private List<string>? _onDuplicateKeyUpdateColumns;

    public MySqlInsertQuery(TTable table, DbClient<MySqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    /// <summary>
    /// Adds ON DUPLICATE KEY UPDATE clause for MySQL upsert behavior.
    /// Specifies which columns to update when a duplicate key conflict occurs.
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdate(params string[] columns)
    {
        _onDuplicateKeyUpdateColumns = new List<string>(columns);
        return this;
    }

    /// <summary>
    /// Adds ON DUPLICATE KEY UPDATE for all inserted columns.
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdateAll()
    {
        _onDuplicateKeyUpdateColumns = null; // Will be resolved to all columns at build time
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        // Build standard INSERT
        base.BuildSql(sqlBuilder);

        // Append ON DUPLICATE KEY UPDATE if configured
        if (_onDuplicateKeyUpdateColumns != null && _onDuplicateKeyUpdateColumns.Count > 0)
        {
            sqlBuilder.Append(MySqlSqlDialectImpl.BuildOnDuplicateKeyUpdate(_onDuplicateKeyUpdateColumns));
        }
    }
}
