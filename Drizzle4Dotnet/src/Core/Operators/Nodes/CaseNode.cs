using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators.Nodes;

/// <summary>
/// Represents a SQL CASE ... WHEN ... THEN ... ELSE ... END expression.
/// Usage: Case.When(condition1, result1).When(condition2, result2).Else(defaultResult).As("alias")
/// Implements IOperator&lt;object&gt; so .As(alias) works.
/// </summary>
public readonly struct CaseNode : IOperator<object>
{
    private readonly (IGenericSql Condition, IGenericSql Result)[] _whenClauses;
    private readonly IGenericSql? _elseResult;

    public CaseNode((IGenericSql Condition, IGenericSql Result)[] whenClauses, IGenericSql? elseResult)
    {
        _whenClauses = whenClauses;
        _elseResult = elseResult;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("CASE");
        foreach (var (condition, result) in _whenClauses)
        {
            sqlBuilder.Append(" WHEN (");
            condition.BuildSql(sqlBuilder);
            sqlBuilder.Append(") THEN ");
            result.BuildSql(sqlBuilder);
        }
        if (_elseResult != null)
        {
            sqlBuilder.Append(" ELSE ");
            _elseResult.BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(" END");
    }
}

/// <summary>
/// Fluent builder for CASE expressions.
/// </summary>
public static class Case
{
    public static CaseBuilder When(IGenericSql condition, IGenericSql result)
        => new CaseBuilder().When(condition, result);
}

public class CaseBuilder
{
    private readonly List<(IGenericSql Condition, IGenericSql Result)> _whenClauses = new();
    private IGenericSql? _elseResult;

    public CaseBuilder When(IGenericSql condition, IGenericSql result)
    {
        _whenClauses.Add((condition, result));
        return this;
    }

    public CaseBuilder Else(IGenericSql result)
    {
        _elseResult = result;
        return this;
    }

    public CaseNode Build()
    {
        return new CaseNode(_whenClauses.ToArray(), _elseResult);
    }

    /// <summary>
    /// Implicitly converts to CaseNode for use as ISql/IGenericSql.
    /// </summary>
    public static implicit operator CaseNode(CaseBuilder builder) => builder.Build();
}
