using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Operators.Nodes;

/// <summary>
/// Represents the Oracle ROWID pseudo-column.
/// Renders as: ROWID
/// </summary>
public class OracleRowIdNode : IGenericSql
{
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("ROWID");
    }
}
