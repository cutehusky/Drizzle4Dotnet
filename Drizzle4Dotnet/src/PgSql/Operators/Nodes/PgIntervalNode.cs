using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql.Nodes;

/// <summary>
/// Represents a PostgreSQL INTERVAL literal: INTERVAL 'amount unit'
/// PostgreSQL syntax: INTERVAL '1 day' (with quotes around the amount and unit)
/// MySQL syntax differs: INTERVAL 1 DAY (without quotes)
/// </summary>
public readonly struct PgIntervalNode : IOperator<DateTime>
{
    private readonly object _amount;
    private readonly string _unit;

    public PgIntervalNode(int amount, string unit)
    {
        _amount = amount;
        _unit = unit;
    }

    public PgIntervalNode(double amount, string unit)
    {
        _amount = amount;
        _unit = unit;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INTERVAL '").Append(_amount.ToString()!).Append(' ').Append(_unit).Append('\'');
    }
}
