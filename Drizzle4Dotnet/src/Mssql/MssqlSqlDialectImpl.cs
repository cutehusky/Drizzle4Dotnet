using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Dialect;

/// <summary>
/// MSSQL (T-SQL) dialect implementation.
/// MSSQL uses square brackets for identifiers: [identifier]
/// Uses @named parameters (compatible with Microsoft.Data.SqlClient).
/// </summary>
public class MssqlSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================

    /// <summary>
    /// Builds a square-bracket quoted identifier: [identifier]
    /// </summary>
    public static string BuildIdentifier(string identifier)
        => $"[{identifier}]";

    /// <summary>
    /// Builds a table name: [schema].[table] or just [table] (default schema is dbo)
    /// </summary>
    public static string BuildTableName(string schemaName, string tableName)
        => string.IsNullOrEmpty(schemaName) 
            ? $"[{tableName}]" 
            : $"[{schemaName}].[{tableName}]";

    /// <summary>
    /// Builds a column reference: [table].[column]
    /// </summary>
    public static string BuildColumnName(string refName, string columnName)
        => $"[{refName}].[{columnName}]";

    /// <summary>
    /// Builds a named parameter: @paramName
    /// Microsoft.Data.SqlClient uses @named parameters.
    /// </summary>
    public static string BuildParameterName(string parameterName)
        => $"@{parameterName}";

    /// <summary>
    /// Builds an indexed parameter: @p0, @p1, etc.
    /// </summary>
    public static string BuildParameterName(int parameterIndex)
        => $"@p{parameterIndex}";

    // ======================================================================
    // Limit / Offset — MSSQL uses OFFSET/FETCH or TOP
    // ======================================================================

    /// <summary>
    /// Builds OFFSET/FETCH clause.
    /// MSSQL syntax: OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY
    /// Note: MSSQL requires ORDER BY when using OFFSET/FETCH.
    /// </summary>
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
    {
        if (limit.HasValue && offset.HasValue)
        {
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
            sqlBuilder.Append(" ROWS FETCH NEXT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
            sqlBuilder.Append(" ROWS ONLY");
        }
        else if (limit.HasValue)
        {
            sqlBuilder.Append(" OFFSET 0 ROWS FETCH NEXT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
            sqlBuilder.Append(" ROWS ONLY");
        }
        else if (offset.HasValue)
        {
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
            sqlBuilder.Append(" ROWS");
        }
    }

    /// <summary>
    /// MSSQL does not support LIMIT/OFFSET for UPDATE/DELETE statements.
    /// Use TOP() in the SELECT clause instead.
    /// </summary>
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => throw new NotSupportedException(
            "MSSQL does not support LIMIT/OFFSET for UPDATE/DELETE statements. " +
            "Use TOP() in the SELECT clause instead.");

    // ======================================================================
    // Feature Flags
    // ======================================================================

    /// <summary>MSSQL supports OUTPUT clause (equivalent to RETURNING).</summary>
    public static bool SupportsReturning => true;

    /// <summary>MSSQL does not support array types.</summary>
    public static bool SupportsArrays => false;

    /// <summary>MSSQL 2016+ supports JSON functions (JSON_VALUE, JSON_QUERY, etc.).</summary>
    public static bool SupportsJson => true;

    /// <summary>MSSQL 2005+ supports window functions.</summary>
    public static bool SupportsWindowFunctions => true;

    /// <summary>MSSQL 2005+ supports CTEs (WITH clause).</summary>
    public static bool SupportsCte => true;

    /// <summary>MSSQL 2005+ supports recursive CTEs.</summary>
    public static bool SupportsRecursiveCte => true;

    /// <summary>MSSQL does not support DELETE ... USING syntax (uses JOIN instead).</summary>
    public static bool SupportsDeleteUsing => false;

    /// <summary>MSSQL does not support IS DISTINCT FROM.</summary>
    public static bool SupportsIsDistinctFrom => false;

    /// <summary>MSSQL does not support FILTER (WHERE ...) for aggregate functions.</summary>
    public static bool SupportsFilteredAggregates => false;

    /// <summary>MSSQL supports FULL OUTER JOIN.</summary>
    public static bool SupportsFullOuterJoin => true;

    /// <summary>MSSQL does NOT support NATURAL JOIN.</summary>
    public static bool SupportsNaturalJoin => false;

    /// <summary>MSSQL does not support LATERAL joins (uses APPLY instead).</summary>
    public static bool SupportsLateralJoin => false;

    /// <summary>MSSQL supports CROSS/OUTER APPLY (alternative to LATERAL).</summary>
    public static bool SupportsApplyJoin => true;

    /// <summary>MSSQL uses standard LIMIT/OFFSET (not pair mode like MySQL).</summary>
    public static bool UseLimitPairMode => false;

    // ======================================================================
    // String Escaping
    // ======================================================================

    /// <summary>
    /// MSSQL string escaping — replaces single quotes with two single quotes (standard SQL).
    /// </summary>
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
}
