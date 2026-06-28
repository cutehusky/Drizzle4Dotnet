namespace Drizzle4Dotnet.Core.Shared;

/// <summary>
/// Default implementations for ISqlDialect members.
/// Provides standard SQL behavior that dialects can reuse.
/// </summary>
public static class SqlDialectDefaults
{
    /// <summary>
    /// Builds a standard LIMIT/OFFSET clause directly to the SQL builder.
    /// Produces: LIMIT {limit} OFFSET {offset}, LIMIT {limit}, OFFSET {offset}, or nothing.
    /// </summary>
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
    {
        if (limit.HasValue && offset.HasValue)
        {
            sqlBuilder.Append(" LIMIT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
        }
        else if (limit.HasValue)
        {
            sqlBuilder.Append(" LIMIT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
        }
        else if (offset.HasValue)
        {
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
        }
    }

    /// <summary>
    /// Default string escaping — replaces single quotes with two single quotes.
    /// </summary>
    public static string EscapeString(string value)
    {
        return value.Replace("'", "''");
    }

    // ======================================================================
    // Default Feature Flags
    // ======================================================================

    public static bool SupportsReturning => false;
    public static bool SupportsArrays => false;
    public static bool SupportsJson => true;
    public static bool SupportsWindowFunctions => true;
    public static bool SupportsCte => true;
    public static bool SupportsRecursiveCte => true;
    public static bool SupportsDeleteUsing => false;
    public static bool SupportsIsDistinctFrom => false;
    public static bool SupportsFilteredAggregates => false;
    public static bool SupportsFullOuterJoin => false;
    public static bool SupportsNaturalJoin => false;
    public static bool SupportsLateralJoin => false;
    public static bool SupportsApplyJoin => false;
    public static bool UseLimitPairMode => false;
}
