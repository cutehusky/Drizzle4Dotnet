using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql.Nodes;

/// <summary>
/// Represents a SQL POSITION function: POSITION(substring IN column)
/// Uses the SQL standard IN keyword syntax (not comma-separated arguments).
/// PostgreSQL-specific due to the IN keyword syntax handling.
/// </summary>
public readonly struct PositionNode : IOperator<long>
{
    private readonly IGenericSql _substring;
    private readonly ISql<string> _column;

    public PositionNode(IGenericSql substring, ISql<string> column)
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
