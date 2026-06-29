using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.MySql.Operators.Nodes;

/// <summary>
/// Represents a MySQL VALUES(column) reference for ON DUPLICATE KEY UPDATE.
/// Renders as: VALUES(`column_name`)
/// Usage: .OnDuplicateKeyUpdate("Name", "Email")  -- uses this internally
/// Or: .SetOnDuplicateKey(table.Name, MySqlStatics.Values(table.Name))
/// </summary>
public class MySqlValuesNode : IGenericSql
{
    private readonly string _identifier;

    public MySqlValuesNode(string identifier)
    {
        _identifier = identifier;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("VALUES(");
        sqlBuilder.Append(MySqlSqlDialectImpl.BuildIdentifier(_identifier));
        sqlBuilder.Append(')');
    }
}
