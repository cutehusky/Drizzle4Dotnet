using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators.Nodes;

/// <summary>
/// Wraps a literal value as an ISql for use as function arguments, 
/// without requiring a TDialect constraint.
/// </summary>
public readonly struct SqlValueNode<T> : IOperator<T>
{
    private readonly T _value;

    public SqlValueNode(T value)
    {
        _value = value;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        var paramName = sqlBuilder.AddParameter(_value);
        sqlBuilder.Append(paramName);
    }
}