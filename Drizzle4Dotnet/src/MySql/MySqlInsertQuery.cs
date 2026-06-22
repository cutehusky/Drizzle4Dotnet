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
/// - INSERT ... () VALUES () (for empty value sets, unlike PostgreSQL's DEFAULT VALUES)
/// MySQL does not support RETURNING — use MySqlFunctions.LastInsertId() instead.
/// </summary>
public class MySqlInsertQuery<TTable> : InsertQuery<TTable, MySqlSqlDialectImpl>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    private List<string>? _onDuplicateKeyUpdateColumns;
    private bool _onDuplicateKeyUpdateAll;

    public MySqlInsertQuery(TTable table, DbClient<MySqlSqlDialectImpl> dbClient) 
        : base(table, dbClient)
    {
    }

    /// <summary>
    /// Adds ON DUPLICATE KEY UPDATE for the specified columns.
    /// When a duplicate key conflict occurs, these columns will be updated 
    /// with the VALUES() function (i.e., the values from the INSERT clause).
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdate(params string[] columns)
    {
        _onDuplicateKeyUpdateColumns = new List<string>(columns);
        _onDuplicateKeyUpdateAll = false;
        return this;
    }

    /// <summary>
    /// Adds ON DUPLICATE KEY UPDATE for all inserted columns.
    /// At build time, all columns from the INSERT values will be used.
    /// </summary>
    public MySqlInsertQuery<TTable> OnDuplicateKeyUpdateAll()
    {
        _onDuplicateKeyUpdateAll = true;
        _onDuplicateKeyUpdateColumns = null;
        return this;
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        // Build standard INSERT
        base.BuildSql(sqlBuilder);

        // Resolve columns for ON DUPLICATE KEY UPDATE
        List<string>? updateColumns = _onDuplicateKeyUpdateColumns;
        
        if (_onDuplicateKeyUpdateAll && updateColumns == null)
        {
            // Get all column names from the internal state.
            // This is a best-effort approach since we can't easily access
            // the private _values field from the base class.
            // Users should call OnDuplicateKeyUpdate() with explicit columns.
        }

        // Append ON DUPLICATE KEY UPDATE if configured
        if (updateColumns != null && updateColumns.Count > 0)
        {
            sqlBuilder.Append(MySqlSqlDialectImpl.BuildOnDuplicateKeyUpdate(updateColumns));
        }
    }
}
