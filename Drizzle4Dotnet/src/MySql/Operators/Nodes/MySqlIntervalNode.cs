using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.MySql.Nodes;

/// <summary>
/// Represents a MySQL INTERVAL expression: INTERVAL expr unit
/// MySQL uses unquoted syntax: INTERVAL 1 DAY
/// PostgreSQL equivalent would be: INTERVAL '1 day'
/// </summary>
public readonly struct MySqlIntervalNode : IOperator<DateTime>
{
    private readonly object _amount;
    private readonly string _unit;

    public MySqlIntervalNode(int amount, string unit)
    {
        _amount = amount;
        _unit = unit;
    }

    public MySqlIntervalNode(double amount, string unit)
    {
        _amount = amount;
        _unit = unit;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INTERVAL ");
        sqlBuilder.Append(_amount.ToString()!);
        sqlBuilder.Append(' ').Append(_unit);
    }
}
