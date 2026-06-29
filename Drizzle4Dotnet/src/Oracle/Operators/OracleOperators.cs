using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Operators;

/// <summary>
/// Oracle-specific operators.
/// Most standard operators (comparison, arithmetic, logical, string) are already
/// in Core/Shared/Operators/ and work with Oracle unchanged.
/// This class provides Oracle-specific operator extensions and overrides.
/// </summary>
public static class OracleOperators
{
    // ======================================================================
    // String Concatenation: Oracle uses || operator (same as standard SQL)
    // Core Operators.Concat already uses " || " which is correct for Oracle.
    // ======================================================================

    // ======================================================================
    // Equality: Oracle-specific null-safe comparison via DECODE
    // ======================================================================

    /// <summary>
    /// Null-safe equality (IS NOT DISTINCT FROM equivalent).
    /// Oracle: DECODE(a, b, 1, 0) = 1
    /// </summary>
    public static IGenericSql IsNotDistinctFrom(IGenericSql a, IGenericSql b)
        => new RawSql($"DECODE({a}, {b}, 1, 0) = 1");

    /// <summary>
    /// Null-safe inequality (IS DISTINCT FROM equivalent).
    /// Oracle: DECODE(a, b, 1, 0) = 0
    /// </summary>
    public static IGenericSql IsDistinctFrom(IGenericSql a, IGenericSql b)
        => new RawSql($"DECODE({a}, {b}, 1, 0) = 0");

    // ======================================================================
    // Conditional
    // ======================================================================

    /// <summary>
    /// DECODE(expr, search, result, ..., default)
    /// Oracle's conditional expression similar to CASE.
    /// </summary>
    public static DecodeNode Decode(IGenericSql expr, IGenericSql? defaultVal = null, params (IGenericSql search, IGenericSql result)[] pairs)
        => new(expr, pairs, defaultVal);

    /// <summary>
    /// NVL(expr, default) — return default if expr is null.
    /// </summary>
    public static NvlNode Nvl(IGenericSql expr, IGenericSql defaultVal) => new(expr, defaultVal);

    /// <summary>
    /// NVL2(expr, notnull, nullval) — return notnull if expr is not null, else nullval.
    /// </summary>
    public static Nvl2Node Nvl2(IGenericSql expr, IGenericSql notnull, IGenericSql nullval) => new(expr, notnull, nullval);
}

/// <summary>
/// Oracle's DECODE expression node.
/// DECODE(expr, search, result, search, result, ..., default)
/// </summary>
public class DecodeNode : IGenericSql
{
    private readonly IGenericSql _expr;
    private readonly (IGenericSql search, IGenericSql result)[] _pairs;
    private readonly IGenericSql? _defaultVal;

    public DecodeNode(IGenericSql expr, (IGenericSql search, IGenericSql result)[] pairs, IGenericSql? defaultVal = null)
    {
        _expr = expr;
        _pairs = pairs;
        _defaultVal = defaultVal;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("DECODE(");
        _expr.BuildSql(sqlBuilder);
        foreach (var (search, result) in _pairs)
        {
            sqlBuilder.Append(", ");
            search.BuildSql(sqlBuilder);
            sqlBuilder.Append(", ");
            result.BuildSql(sqlBuilder);
        }
        if (_defaultVal != null)
        {
            sqlBuilder.Append(", ");
            _defaultVal.BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Oracle's NVL expression node.
/// NVL(expr, default)
/// </summary>
public class NvlNode : IGenericSql
{
    private readonly IGenericSql _expr;
    private readonly IGenericSql _defaultVal;

    public NvlNode(IGenericSql expr, IGenericSql defaultVal)
    {
        _expr = expr;
        _defaultVal = defaultVal;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("NVL(");
        _expr.BuildSql(sqlBuilder);
        sqlBuilder.Append(", ");
        _defaultVal.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Oracle's NVL2 expression node.
/// NVL2(expr, notnull, nullval)
/// </summary>
public class Nvl2Node : IGenericSql
{
    private readonly IGenericSql _expr;
    private readonly IGenericSql _notnull;
    private readonly IGenericSql _nullval;

    public Nvl2Node(IGenericSql expr, IGenericSql notnull, IGenericSql nullval)
    {
        _expr = expr;
        _notnull = notnull;
        _nullval = nullval;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("NVL2(");
        _expr.BuildSql(sqlBuilder);
        sqlBuilder.Append(", ");
        _notnull.BuildSql(sqlBuilder);
        sqlBuilder.Append(", ");
        _nullval.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}
