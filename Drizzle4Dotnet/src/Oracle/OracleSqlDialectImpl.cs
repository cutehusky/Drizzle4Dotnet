using Drizzle4Dotnet.Core.Schema.Migration;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Oracle.Schema;

namespace Drizzle4Dotnet.Oracle;

/// <summary>
/// Oracle (PL/SQL) dialect implementation.
/// Oracle uses double quotes for identifiers: "identifier"
/// Oracle uses colon-prefixed parameters: :paramName (compatible with Oracle.ManagedDataAccess.Core).
/// </summary>
public class OracleSqlDialectImpl : ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================

    /// <summary>
    /// Builds a double-quoted identifier: "identifier"
    /// Oracle folds unquoted identifiers to UPPERCASE.
    /// </summary>
    public static string BuildIdentifier(string identifier)
        => $"\"{identifier}\"";

    /// <summary>
    /// Builds a table name: "schema"."table" or just "table" (default schema is current user/schema).
    /// </summary>
    public static string BuildTableName(string schemaName, string tableName)
        => string.IsNullOrEmpty(schemaName) 
            ? $"\"{tableName}\"" 
            : $"\"{schemaName}\".\"{tableName}\"";

    /// <summary>
    /// Builds a column reference: "table"."column"
    /// </summary>
    public static string BuildColumnName(string refName, string columnName)
        => $"\"{refName}\".\"{columnName}\"";

    /// <summary>
    /// Builds a named parameter: :paramName
    /// Oracle.ManagedDataAccess.Core uses :named parameters.
    /// </summary>
    public static string BuildParameterName(string parameterName)
        => $":{parameterName}";

    /// <summary>
    /// Builds an indexed parameter: :p0, :p1, etc.
    /// </summary>
    public static string BuildParameterName(int parameterIndex)
        => $":p{parameterIndex}";

    // ======================================================================
    // Limit / Offset — Oracle 12c+ uses OFFSET/FETCH
    // ======================================================================

    /// <summary>
    /// Builds OFFSET/FETCH clause.
    /// Oracle 12c+ syntax: OFFSET {offset} ROWS FETCH NEXT {limit} ROWS ONLY
    /// Pre-12c fallback: ROWNUM (handled in SelectQuery override).
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
    /// Oracle does not support LIMIT/OFFSET for UPDATE/DELETE statements.
    /// </summary>
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => throw new NotSupportedException(
            "Oracle does not support LIMIT/OFFSET for UPDATE/DELETE statements.");

    // ======================================================================
    // Feature Flags
    // ======================================================================

    /// <summary>Oracle supports RETURNING ... INTO clause (different from PG/MSSQL).</summary>
    public static bool SupportsReturning => true;

    /// <summary>Oracle does not support array types (VARRAYs are different).</summary>
    public static bool SupportsArrays => false;

    /// <summary>Oracle 12c+ supports JSON functions (JSON_VALUE, JSON_QUERY, JSON_TABLE).</summary>
    public static bool SupportsJson => true;

    /// <summary>Oracle 9i+ supports window functions.</summary>
    public static bool SupportsWindowFunctions => true;

    /// <summary>Oracle 9i+ supports CTEs (WITH clause).</summary>
    public static bool SupportsCte => true;

    /// <summary>Oracle 11gR2+ supports recursive CTEs.</summary>
    public static bool SupportsRecursiveCte => true;

    /// <summary>Oracle does not support DELETE ... USING syntax.</summary>
    public static bool SupportsDeleteUsing => false;

    /// <summary>Oracle does not support IS DISTINCT FROM (use DECODE/NVL2 instead).</summary>
    public static bool SupportsIsDistinctFrom => false;

    /// <summary>Oracle does not support FILTER (WHERE ...) for aggregate functions (use CASE WHEN instead).</summary>
    public static bool SupportsFilteredAggregates => false;

    /// <summary>Oracle supports FULL OUTER JOIN.</summary>
    public static bool SupportsFullOuterJoin => true;

    /// <summary>Oracle supports NATURAL JOIN.</summary>
    public static bool SupportsNaturalJoin => true;

    /// <summary>Oracle 12c+ supports LATERAL join.</summary>
    public static bool SupportsLateralJoin => true;

    /// <summary>Oracle does not support APPLY joins (uses LATERAL instead).</summary>
    public static bool SupportsApplyJoin => false;

    /// <summary>Oracle uses standard OFFSET/FETCH (not pair mode like MySQL).</summary>
    public static bool UseLimitPairMode => false;

    // ======================================================================
    // String Escaping
    // ======================================================================

    /// <summary>
    /// Oracle string escaping — replaces single quotes with two single quotes (standard SQL).
    /// Oracle also supports alternative quoting: q'[text]'
    /// </summary>
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
    
    public static Dictionary<Type, ISqlDataType> ClrToSqlTypeMap => new()
    {
        [typeof(int)] = OracleDataType.Integer,
        [typeof(long)] = OracleDataType.BigInt,
        [typeof(short)] = OracleDataType.SmallInt,
        [typeof(byte)] = OracleDataType.TinyInt,
        [typeof(string)] = OracleDataType.Text,
        [typeof(bool)] = OracleDataType.Boolean,
        [typeof(decimal)] = OracleDataType.Decimal,
        [typeof(float)] = OracleDataType.BinaryFloat,
        [typeof(double)] = OracleDataType.BinaryDouble,
        [typeof(DateTime)] = OracleDataType.Timestamp,
        [typeof(DateOnly)] = OracleDataType.Date,
        [typeof(TimeOnly)] = OracleDataType.Time,
        [typeof(Guid)] = OracleDataType.Uuid,
        [typeof(byte[])] = OracleDataType.Blob,
        [typeof(char)] = OracleDataType.Char,
    };
}
