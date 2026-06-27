using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Dialect;

public class MySqlSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================
    
    /// <summary>
    /// Builds a backtick-quoted identifier: `identifier`
    /// </summary>
    public static string BuildIdentifier(string identifier)
    {
        return $"`{identifier}`";
    }

    /// <summary>
    /// Builds a table name: `database`.`table` or just `table`
    /// </summary>
    public static string BuildTableName(string schemaName, string tableName)
    {
        return string.IsNullOrEmpty(schemaName) 
            ? $"`{tableName}`" 
            : $"`{schemaName}`.`{tableName}`";
    }

    /// <summary>
    /// Builds a column reference: `table`.`column`
    /// </summary>
    public static string BuildColumnName(string refName, string columnName)
    {
        return $"`{refName}`.`{columnName}`";
    }

    /// <summary>
    /// Builds a named parameter: @paramName
    /// </summary>
    public static string BuildParameterName(string parameterName)
    {
        return $"@{parameterName}";
    }

    /// <summary>
    /// Builds an indexed parameter: @p0, @p1, etc.
    /// </summary>
    public static string BuildParameterName(int parameterIndex)
    {
        return $"@p{parameterIndex}";
    }
    
    // ======================================================================
    // Limit / Offset
    // ======================================================================
    
    /// <summary>
    /// Builds LIMIT/OFFSET clause.
    /// MySQL supports both syntaxes: LIMIT {limit} OFFSET {offset} or LIMIT {offset}, {limit}
    /// We use the standard LIMIT ... OFFSET ... form.
    /// </summary>
    public static string BuildLimitOffset(int? limit, int? offset)
        => SqlDialectDefaults.BuildLimitOffset(limit, offset);
    
    // ======================================================================
    // Feature Flags
    // ======================================================================
    
    /// <summary>
    /// MySQL does not support RETURNING. Use LAST_INSERT_ID() instead.
    /// </summary>
    public static bool SupportsReturning => false;
    
    /// <summary>
    /// MySQL does not support array types.
    /// </summary>
    public static bool SupportsArrays => false;
    
    /// <summary>
    /// MySQL 8.0+ supports JSON natively.
    /// </summary>
    public static bool SupportsJson => true;
    
    /// <summary>
    /// MySQL 8.0+ supports window functions.
    /// </summary>
    public static bool SupportsWindowFunctions => true;
    
    /// <summary>
    /// MySQL 8.0+ supports CTEs (WITH clause).
    /// </summary>
    public static bool SupportsCte => true;
    
    /// <summary>
    /// MySQL 8.0+ supports recursive CTEs.
    /// </summary>
    public static bool SupportsRecursiveCte => true;
    
    /// <summary>
    /// MySQL does not support DELETE ... USING syntax (uses JOIN instead).
    /// </summary>
    public static bool SupportsDeleteUsing => false;
    
    /// <summary>
    /// MySQL does not support IS DISTINCT FROM.
    /// </summary>
    public static bool SupportsIsDistinctFrom => false;
    
    /// <summary>
    /// MySQL does not support FILTER (WHERE ...) for aggregate functions.
    /// </summary>
    public static bool SupportsFilteredAggregates => false;
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    /// <summary>
    /// MySQL string escaping — replaces single quotes with two single quotes.
    /// </summary>
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
}
