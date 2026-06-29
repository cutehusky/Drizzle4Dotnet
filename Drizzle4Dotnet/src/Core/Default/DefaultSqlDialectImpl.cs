using Drizzle4Dotnet.Core.Schema.Migration;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Default;

/// <summary>
/// Default/generic SQL dialect implementation following ANSI SQL standards.
/// Serves as a fallback for unsupported or unknown database dialects.
/// Uses double-quoted identifiers ("identifier"), @named parameters,
/// standard LIMIT/OFFSET clauses, and standard SQL string escaping.
/// </summary>
public class DefaultSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================

    /// <summary>
    /// Builds a double-quoted identifier: "identifier" (ANSI SQL standard).
    /// </summary>
    public static string BuildIdentifier(string identifier)
        => $"\"{identifier}\"";

    /// <summary>
    /// Builds a table name: "schema"."table" or just "table".
    /// </summary>
    public static string BuildTableName(string schemaName, string tableName)
        => string.IsNullOrEmpty(schemaName)
            ? $"\"{tableName}\""
            : $"\"{schemaName}\".\"{tableName}\"";

    /// <summary>
    /// Builds a column reference: "refName"."columnName".
    /// </summary>
    public static string BuildColumnName(string refName, string columnName)
        => $"\"{refName}\".\"{columnName}\"";

    /// <summary>
    /// Builds a named parameter: @paramName.
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
    /// Builds a standard LIMIT/OFFSET clause:
    /// LIMIT {limit} OFFSET {offset}, LIMIT {limit}, OFFSET {offset}, or nothing.
    /// </summary>
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => SqlDialectDefaults.BuildLimitOffset(sqlBuilder, limit, offset);

    /// <summary>
    /// Default dialects generally do not support LIMIT/OFFSET for UPDATE/DELETE statements.
    /// </summary>
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => throw new NotSupportedException(
            "The default/generic dialect does not support LIMIT/OFFSET for UPDATE/DELETE statements.");

    // ======================================================================
    // Feature Flags — Conservative defaults (minimal feature set)
    // ======================================================================

    /// <summary>No RETURNING support by default.</summary>
    public static bool SupportsReturning => false;

    /// <summary>No array support by default.</summary>
    public static bool SupportsArrays => false;

    /// <summary>JSON support is unknown; default to false.</summary>
    public static bool SupportsJson => false;

    /// <summary>Window functions support is unknown; default to false.</summary>
    public static bool SupportsWindowFunctions => false;

    /// <summary>CTE support is unknown; default to false.</summary>
    public static bool SupportsCte => false;

    /// <summary>Recursive CTE support is unknown; default to false.</summary>
    public static bool SupportsRecursiveCte => false;

    /// <summary>No DELETE ... USING support by default.</summary>
    public static bool SupportsDeleteUsing => false;

    /// <summary>No IS DISTINCT FROM support by default.</summary>
    public static bool SupportsIsDistinctFrom => false;

    /// <summary>No FILTERED AGGREGATES support by default.</summary>
    public static bool SupportsFilteredAggregates => false;

    /// <summary>FULL OUTER JOIN support is unknown; default to false.</summary>
    public static bool SupportsFullOuterJoin => false;

    /// <summary>NATURAL JOIN is part of ANSI SQL; default to true.</summary>
    public static bool SupportsNaturalJoin => true;

    /// <summary>LATERAL join support is unknown; default to false.</summary>
    public static bool SupportsLateralJoin => false;

    /// <summary>APPLY join support is unknown; default to false.</summary>
    public static bool SupportsApplyJoin => false;

    // ======================================================================
    // String Escaping
    // ======================================================================

    /// <summary>
    /// Standard SQL string escaping — replaces single quotes with two single quotes.
    /// </summary>
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);

    // ======================================================================
    // Data Type Mapping — Maps CLR types to generic DefaultDataType
    // ======================================================================

    public static Dictionary<Type, ISqlDataType> ClrToSqlTypeMap => new()
    {
        [typeof(int)] = SqlDataType.Integer,
        [typeof(long)] = SqlDataType.BigInt,
        [typeof(short)] = SqlDataType.SmallInt,
        [typeof(byte)] = SqlDataType.TinyInt,
        [typeof(string)] = SqlDataType.Text,
        [typeof(bool)] = SqlDataType.Boolean,
        [typeof(decimal)] = SqlDataType.Decimal,
        [typeof(float)] = SqlDataType.Real,
        [typeof(double)] = SqlDataType.DoublePrecision,
        [typeof(DateTime)] = SqlDataType.Timestamp,
        [typeof(DateOnly)] = SqlDataType.Date,
        [typeof(TimeOnly)] = SqlDataType.Time,
        [typeof(Guid)] = SqlDataType.Uuid,
        [typeof(byte[])] = SqlDataType.Blob,
        [typeof(char)] = SqlDataType.Char,
    };
}
