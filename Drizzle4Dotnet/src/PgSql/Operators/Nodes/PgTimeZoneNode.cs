using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql.Operators.Nodes;

/// <summary>
/// Represents a PostgreSQL timezone name as a SQL string literal: 'UTC', 'Asia/Saigon', etc.
/// Used with AT TIME ZONE which requires a literal timezone name, not a parameterized value.
/// </summary>
public readonly struct PgTimeZoneNode : IOperator<string>
{
    private readonly string _timeZone;

    public PgTimeZoneNode(string timeZone)
    {
        _timeZone = timeZone;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append('\'').Append(_timeZone).Append('\'');
    }
}
