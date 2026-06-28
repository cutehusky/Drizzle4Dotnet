using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Dialect;

/// <summary>
/// SQLite dialect implementation.
/// SQLite uses double-quote identifiers like PostgreSQL, but has no schema support.
/// Uses @named parameters (compatible with Microsoft.Data.Sqlite).
/// </summary>
public class SqliteSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================
    
    /// <summary>
    /// Builds a double-quoted identifier: "identifier"
    /// </summary>
    public static string BuildIdentifier(string identifier)
        => $"\"{identifier}\"";

    /// <summary>
    /// SQLite has no schema — ignore schemaName, build just "tableName"
    /// </summary>
    public static string BuildTableName(string schemaName, string tableName)
        => $"\"{tableName}\"";

    /// <summary>
    /// Builds a column reference: "table"."column"
    /// </summary>
    public static string BuildColumnName(string refName, string columnName)
        => $"\"{refName}\".\"{columnName}\"";

    /// <summary>
    /// Builds a named parameter: @paramName
    /// Microsoft.Data.Sqlite uses @named parameters.
    /// </summary>
    public static string BuildParameterName(string parameterName)
        => $"@{parameterName}";

    /// <summary>
    /// Builds an indexed parameter: @p0, @p1, etc.
    /// </summary>
    public static string BuildParameterName(int parameterIndex)
        => $"@p{parameterIndex}";
    
    // ======================================================================
    // Limit / Offset
    // ======================================================================
    
    /// <summary>
    /// Standard LIMIT/OFFSET: LIMIT ? OFFSET ?
    /// </summary>
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => SqlDialectDefaults.BuildLimitOffset(sqlBuilder, limit, offset);
    
    /// <summary>
    /// SQLite does not support LIMIT/OFFSET for UPDATE/DELETE statements.
    /// </summary>
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => throw new NotSupportedException("SQLite does not support LIMIT/OFFSET for UPDATE/DELETE statements.");
    
    // ======================================================================
    // Feature Flags
    // ======================================================================
    
    /// <summary>SQLite 3.35+ supports RETURNING clause.</summary>
    public static bool SupportsReturning => true;
    
    /// <summary>SQLite does not support array types.</summary>
    public static bool SupportsArrays => false;
    
    /// <summary>SQLite 3.38+ has built-in JSON functions.</summary>
    public static bool SupportsJson => true;
    
    /// <summary>SQLite 3.25+ supports window functions.</summary>
    public static bool SupportsWindowFunctions => true;
    
    /// <summary>SQLite 3.8.3+ supports CTEs.</summary>
    public static bool SupportsCte => true;
    
    /// <summary>SQLite 3.8.3+ supports recursive CTEs.</summary>
    public static bool SupportsRecursiveCte => true;
    
    /// <summary>SQLite does not support DELETE ... USING syntax.</summary>
    public static bool SupportsDeleteUsing => false;
    
    /// <summary>SQLite does not support IS DISTINCT FROM.</summary>
    public static bool SupportsIsDistinctFrom => false;
    
    /// <summary>SQLite does not support FILTER (WHERE ...) for aggregate functions.</summary>
    public static bool SupportsFilteredAggregates => false;
    
    /// <summary>SQLite 3.39+ supports FULL OUTER JOIN — keep false for maximum compatibility.</summary>
    public static bool SupportsFullOuterJoin => false;
    
    /// <summary>SQLite supports NATURAL JOIN and NATURAL LEFT JOIN.</summary>
    public static bool SupportsNaturalJoin => true;
    
    /// <summary>SQLite does not support LATERAL joins.</summary>
    public static bool SupportsLateralJoin => false;
    
    /// <summary>SQLite does not support CROSS/OUTER APPLY.</summary>
    public static bool SupportsApplyJoin => false;
    
    /// <summary>SQLite uses standard LIMIT/OFFSET (not pair mode like MySQL).</summary>
    public static bool UseLimitPairMode => false;
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    /// <summary>
    /// SQLite string escaping — replaces single quotes with two single quotes (standard SQL).
    /// </summary>
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
}
