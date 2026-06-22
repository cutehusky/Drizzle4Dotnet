using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql.Nodes;

/// <summary>
/// Fluent builder for the OVER clause: PARTITION BY, ORDER BY, and frame specification.
/// Usage: PgOver.Create().PartitionBy(col1, col2).OrderBy(col3.Asc()).RowsBetween(...)
/// </summary>
public readonly struct PgOverNode : IGenericSql
{
    private readonly IGenericSql? _partitionBy;
    private readonly IGenericSql? _orderBy;
    private readonly PgWindowFrame? _frame;

    public PgOverNode(
        IGenericSql? partitionBy = null,
        IGenericSql? orderBy = null,
        PgWindowFrame? frame = null)
    {
        _partitionBy = partitionBy;
        _orderBy = orderBy;
        _frame = frame;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("OVER (");
        if (_partitionBy != null)
        {
            sqlBuilder.Append("PARTITION BY ");
            _partitionBy.BuildSql(sqlBuilder);
        }
        if (_orderBy != null)
        {
            if (_partitionBy != null) sqlBuilder.Append(' ');
            sqlBuilder.Append("ORDER BY ");
            _orderBy.BuildSql(sqlBuilder);
        }
        if (_frame.HasValue)
        {
            if (_partitionBy != null || _orderBy != null) sqlBuilder.Append(' ');
            _frame.Value.BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Fluent builder for constructing OVER clauses.
/// </summary>
public class PgOverBuilder
{
    private IGenericSql? _partitionBy;
    private IGenericSql? _orderBy;
    private PgWindowFrame? _frame;

    /// <summary>
    /// Adds PARTITION BY columns.
    /// </summary>
    public PgOverBuilder PartitionBy(params IGenericSql[] columns)
    {
        _partitionBy = new PgPartitionByNode(columns);
        return this;
    }

    /// <summary>
    /// Adds ORDER BY columns with direction.
    /// </summary>
    public PgOverBuilder OrderBy(params (IGenericSql column, bool asc)[] columns)
    {
        _orderBy = new PgOrderByOverNode(columns);
        return this;
    }
    
    public PgOverBuilder OrderBy(IGenericSql column, bool asc = true)
    {
        _orderBy = new PgOrderByOverNode([(column, asc)]);
        return this;
    }

    /// <summary>
    /// Adds ROWS BETWEEN frame clause.
    /// </summary>
    public PgOverBuilder RowsBetween(PgWindowFrameBoundary start, PgWindowFrameBoundary end)
    {
        _frame = PgWindowFrame.RowsBetween(start, end);
        return this;
    }

    /// <summary>
    /// Adds RANGE BETWEEN frame clause.
    /// </summary>
    public PgOverBuilder RangeBetween(PgWindowFrameBoundary start, PgWindowFrameBoundary end)
    {
        _frame = PgWindowFrame.RangeBetween(start, end);
        return this;
    }

    /// <summary>
    /// Adds GROUPS BETWEEN frame clause (PostgreSQL).
    /// </summary>
    public PgOverBuilder GroupsBetween(PgWindowFrameBoundary start, PgWindowFrameBoundary end)
    {
        _frame = PgWindowFrame.GroupsBetween(start, end);
        return this;
    }

    /// <summary>
    /// Builds the PgOverNode.
    /// </summary>
    public PgOverNode Build() => new(_partitionBy, _orderBy, _frame);

    /// <summary>
    /// Implicit conversion to PgOverNode.
    /// </summary>
    public static implicit operator PgOverNode(PgOverBuilder builder) => builder.Build();
}

/// <summary>
/// Entry point: PgOver.Create() returns a PgOverBuilder.
/// Usage: PgOver.Create().PartitionBy(col1).OrderBy(col2).RowsBetween(...)
/// </summary>
public static class PgOver
{
    public static PgOverBuilder Create() => new();
}

/// <summary>
/// Internal node for rendering PARTITION BY columns.
/// </summary>
internal readonly struct PgPartitionByNode : IGenericSql
{
    private readonly IGenericSql[] _columns;
    public PgPartitionByNode(IGenericSql[] columns) => _columns = columns;
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        for (int i = 0; i < _columns.Length; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            _columns[i].BuildSql(sqlBuilder);
        }
    }
}

/// <summary>
/// Internal node for rendering ORDER BY in OVER clause.
/// </summary>
internal readonly struct PgOrderByOverNode : IGenericSql
{
    private readonly (IGenericSql column, bool asc)[] _columns;
    public PgOrderByOverNode((IGenericSql column, bool asc)[] columns) => _columns = columns;
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        for (int i = 0; i < _columns.Length; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            var (col, asc) = _columns[i];
            col.BuildSql(sqlBuilder);
            sqlBuilder.Append(asc ? " ASC" : " DESC");
        }
    }
}
