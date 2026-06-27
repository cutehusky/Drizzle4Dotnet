namespace Drizzle4Dotnet.Core.Shared;

/// <summary>
/// Default implementations for ISqlDialect members.
/// Provides standard SQL behavior that dialects can reuse.
/// </summary>
public static class SqlDialectDefaults
{
    /// <summary>
    /// Builds a standard LIMIT/OFFSET clause.
    /// Produces: LIMIT {limit} OFFSET {offset}, LIMIT {limit}, or empty string.
    /// </summary>
    public static string BuildLimitOffset(int? limit, int? offset)
    {
        if (limit.HasValue && offset.HasValue)
            return $" LIMIT {limit} OFFSET {offset}";
        if (limit.HasValue)
            return $" LIMIT {limit}";
        if (offset.HasValue)
            return $" OFFSET {offset}";
        return "";
    }

    /// <summary>
    /// Default RETURNING clause — empty string (not supported).
    /// Override in PostgreSQL dialect which supports RETURNING.
    /// </summary>
    public static string BuildReturning() => "";

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
}
