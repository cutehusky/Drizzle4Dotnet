using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators.Nodes;

/// <summary>
/// Represents a SQL CAST expression or PostgreSQL-style :: cast.
/// PostgreSQL dialect emits "expr::type", others emit "CAST(expr AS type)".
/// </summary>
public readonly struct CastNode<T> : IOperator<T>
{
    private readonly IGenericSql _expression;
    private readonly string _targetType;
    private readonly bool _usePostgresSyntax;

    public CastNode(IGenericSql expression, string targetType, bool usePostgresSyntax = false)
    {
        _expression = expression;
        _targetType = targetType;
        _usePostgresSyntax = usePostgresSyntax;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_usePostgresSyntax)
        {
            _expression.BuildSql(sqlBuilder);
            sqlBuilder.Append("::").Append(_targetType);
        }
        else
        {
            sqlBuilder.Append("CAST(");
            _expression.BuildSql(sqlBuilder);
            sqlBuilder.Append(" AS ").Append(_targetType).Append(')');
        }
    }
}
