using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Nodes;

/// <summary>
/// Represents a table hint in MSSQL: WITH (NOLOCK), WITH (TABLOCK), etc.
/// Syntax: WITH (hint)
/// </summary>
public class MssqlTableHintNode : IGenericSql
{
    private readonly string _hint;

    public MssqlTableHintNode(string hint)
    {
        _hint = hint;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(" WITH (").Append(_hint).Append(')');
    }
}
