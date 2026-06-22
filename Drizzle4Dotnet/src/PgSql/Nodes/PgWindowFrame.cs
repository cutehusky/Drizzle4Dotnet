using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql.Nodes;

/// <summary>
/// Represents a window frame unit: ROWS, RANGE, or GROUPS (PostgreSQL).
/// </summary>
public enum PgWindowFrameUnit
{
    Rows,
    Range,
    Groups
}

/// <summary>
/// Represents a window frame boundary: UNBOUNDED PRECEDING, offset PRECEDING, 
/// CURRENT ROW, offset FOLLOWING, UNBOUNDED FOLLOWING.
/// </summary>
public readonly struct PgWindowFrameBoundary : IGenericSql
{
    private readonly string _sql;

    private PgWindowFrameBoundary(string sql) => _sql = sql;

    public static readonly PgWindowFrameBoundary UnboundedPreceding = new("UNBOUNDED PRECEDING");
    public static readonly PgWindowFrameBoundary CurrentRow = new("CURRENT ROW");
    public static readonly PgWindowFrameBoundary UnboundedFollowing = new("UNBOUNDED FOLLOWING");

    public static PgWindowFrameBoundary Preceding(int offset) 
        => new($"{offset} PRECEDING");
    public static PgWindowFrameBoundary Following(int offset) 
        => new($"{offset} FOLLOWING");

    public void BuildSql(ISqlBuilder sqlBuilder) => sqlBuilder.Append(_sql);
}

/// <summary>
/// Represents a complete window frame specification:
/// ROWS/RANGE/GROUPS BETWEEN ... AND ...
/// </summary>
public readonly struct PgWindowFrame : IGenericSql
{
    private readonly PgWindowFrameUnit _unit;
    private readonly PgWindowFrameBoundary _start;
    private readonly PgWindowFrameBoundary? _end;

    public PgWindowFrame(PgWindowFrameUnit unit, PgWindowFrameBoundary start, PgWindowFrameBoundary? end = null)
    {
        _unit = unit;
        _start = start;
        _end = end;
    }

    /// <summary>
    /// ROWS BETWEEN start AND end
    /// </summary>
    public static PgWindowFrame RowsBetween(PgWindowFrameBoundary start, PgWindowFrameBoundary end)
        => new(PgWindowFrameUnit.Rows, start, end);

    /// <summary>
    /// RANGE BETWEEN start AND end
    /// </summary>
    public static PgWindowFrame RangeBetween(PgWindowFrameBoundary start, PgWindowFrameBoundary end)
        => new(PgWindowFrameUnit.Range, start, end);

    /// <summary>
    /// GROUPS BETWEEN start AND end (PostgreSQL)
    /// </summary>
    public static PgWindowFrame GroupsBetween(PgWindowFrameBoundary start, PgWindowFrameBoundary end)
        => new(PgWindowFrameUnit.Groups, start, end);

    private static readonly Dictionary<PgWindowFrameUnit, string> UnitSql = new()
    {
        [PgWindowFrameUnit.Rows] = "ROWS",
        [PgWindowFrameUnit.Range] = "RANGE",
        [PgWindowFrameUnit.Groups] = "GROUPS",
    };

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(UnitSql[_unit]);
        sqlBuilder.Append(" BETWEEN ");
        _start.BuildSql(sqlBuilder);
        sqlBuilder.Append(" AND ");
        (_end ?? _start).BuildSql(sqlBuilder);
    }
}
