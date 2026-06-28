using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Sqlite.Operators.Nodes;

/// <summary>
/// Represents an EXCLUDED column reference for SQLite ON CONFLICT DO UPDATE SET.
/// Renders as: excluded."column_name"
/// Note: SQLite uses lowercase 'excluded' (not 'EXCLUDED' like PostgreSQL).
/// Usage: var excludedCol = new SqliteExcludedNode(table.Email);
/// </summary>
public class SqliteExcludedNode<T> : ISql<T>
{
    private readonly string _identifier;

    public SqliteExcludedNode(string identifier)
    {
        _identifier = identifier;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("excluded.");
        sqlBuilder.Append(SqliteSqlDialectImpl.BuildIdentifier(_identifier));
    }
}
