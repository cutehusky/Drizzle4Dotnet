using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.MySql.Operators.Nodes;

/// <summary>
/// Represents a MySQL POSITION function: POSITION(substring IN column)
/// Uses the SQL standard IN keyword syntax (not comma-separated arguments).
/// This is the same syntax as PostgreSQL's POSITION function.
/// </summary>
public readonly struct MySqlPositionNode : IOperator<long>
{
    private readonly IGenericSql _substring;
    private readonly ISql<string> _column;

    public MySqlPositionNode(IGenericSql substring, ISql<string> column)
    {
        _substring = substring;
        _column = column;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("POSITION(");
        _substring.BuildSql(sqlBuilder);
        sqlBuilder.Append(" IN ");
        _column.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}
