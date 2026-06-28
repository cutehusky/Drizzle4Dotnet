using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Mssql.Nodes;

/// <summary>
/// Represents a TOP(n) clause in MSSQL SELECT statements.
/// Syntax: TOP (count) or TOP (count) PERCENT or TOP (count) WITH TIES
/// </summary>
public class MssqlTopNode : IGenericSql
{
    private readonly int _count;
    private readonly bool _withTies;
    private readonly bool _percent;

    public MssqlTopNode(int count, bool withTies = false, bool percent = false)
    {
        _count = count;
        _withTies = withTies;
        _percent = percent;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("TOP (");
        sqlBuilder.Append(sqlBuilder.AddParameter(_count));
        sqlBuilder.Append(')');
        if (_percent) sqlBuilder.Append(" PERCENT");
        if (_withTies) sqlBuilder.Append(" WITH TIES");
        sqlBuilder.Append(' ');
    }
}
