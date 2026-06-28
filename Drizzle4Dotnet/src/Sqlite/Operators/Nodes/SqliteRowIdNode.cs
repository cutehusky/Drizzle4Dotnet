using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Sqlite.Nodes;

/// <summary>
/// Represents a SQLite _rowid_ expression.
/// SQLite has an implicit rowid column on all tables (unless WITHOUT ROWID is specified).
/// Renders as: rowid or "tableAlias".rowid
/// </summary>
public class SqliteRowIdNode : IOperator<long>
{
    private readonly string? _tableAlias;
    
    /// <summary>
    /// Creates a rowid reference.
    /// </summary>
    /// <param name="tableAlias">Optional table alias for qualified references.</param>
    public SqliteRowIdNode(string? tableAlias = null)
    {
        _tableAlias = tableAlias;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_tableAlias != null)
            sqlBuilder.Append(SqliteSqlDialectImpl.BuildColumnName(_tableAlias, "rowid"));
        else
            sqlBuilder.Append("rowid");
    }
}
