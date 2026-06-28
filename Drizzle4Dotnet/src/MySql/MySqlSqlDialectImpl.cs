using Drizzle4Dotnet.Core.Schema.Migration;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.MySql.Schema;

namespace Drizzle4Dotnet.MySql;

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
    /// When both offset and limit are set, uses LIMIT {offset}, {limit} form.
    /// </summary>
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => SqlDialectDefaults.BuildLimitOffset(sqlBuilder, limit, offset);
    
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
    {
        if (limit.HasValue && offset.HasValue)
        {
            // Use pair mode: LIMIT {offset}, {limit}
            sqlBuilder.Append(" LIMIT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
            sqlBuilder.Append(", ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
        }
        else if (limit.HasValue)
        {
            sqlBuilder.Append(" LIMIT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
        }
        else if (offset.HasValue)
        {
            // MySQL does not support OFFSET without LIMIT, so we can throw an exception or ignore it.
            throw new NotSupportedException("MySQL does not support OFFSET without LIMIT.");
        }
    }
    
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
    
    /// <summary>
    /// MySQL does not support FULL OUTER JOIN.
    /// </summary>
    public static bool SupportsFullOuterJoin => false;
    
    /// <summary>
    /// MySQL supports NATURAL JOIN and NATURAL LEFT JOIN.
    /// </summary>
    public static bool SupportsNaturalJoin => true;
    
    /// <summary>
    /// MySQL 8.0.14+ supports LATERAL joins.
    /// </summary>
    public static bool SupportsLateralJoin => true;
    
    /// <summary>
    /// MySQL does not support CROSS/OUTER APPLY.
    /// </summary>
    public static bool SupportsApplyJoin => false;
    
    /// <summary>
    /// MySQL uses LIMIT {offset}, {limit} syntax (pair mode) when both are specified.
    /// </summary>
    public static bool UseLimitPairMode => true;
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    /// <summary>
    /// MySQL string escaping — replaces single quotes with two single quotes.
    /// </summary>
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
    
    public static Dictionary<Type, ISqlDataType> ClrToSqlTypeMap => new()
    {
        [typeof(int)] = MySqlDataType.Int,
        [typeof(long)] = MySqlDataType.BigInt,
        [typeof(short)] = MySqlDataType.SmallInt,
        [typeof(byte)] = MySqlDataType.TinyInt,
        [typeof(string)] = MySqlDataType.Text,
        [typeof(bool)] = MySqlDataType.Boolean,
        [typeof(decimal)] = MySqlDataType.Decimal,
        [typeof(float)] = MySqlDataType.Float,
        [typeof(double)] = MySqlDataType.Double,
        [typeof(DateTime)] = MySqlDataType.DateTime,
        [typeof(DateOnly)] = MySqlDataType.Date,
        [typeof(TimeOnly)] = MySqlDataType.Time,
        [typeof(Guid)] = MySqlDataType.Uuid,
        [typeof(byte[])] = MySqlDataType.Blob,
        [typeof(char)] = MySqlDataType.Char,
    };
}
