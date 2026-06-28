using Drizzle4Dotnet.Core.Schema.Migration;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.PgSql.Schema;

namespace Drizzle4Dotnet.PgSql;

public class PgSqlSqlDialectImpl: ISqlDialect
{
    // ======================================================================
    // Identifier & Naming
    // ======================================================================
    
    public static string BuildIdentifier(string identifier)
    {
        return $"\"{identifier}\"";
    }

    public static string BuildTableName(string schemaName, string tableName)
    {
        return $"\"{schemaName}\".\"{tableName}\"";
    }

    public static string BuildColumnName(string refName, string columnName)
    {
        return $"\"{refName}\".\"{columnName}\"";
    }

    public static string BuildParameterName(string parameterName)
    {
        return $"@{parameterName}";
    }

    public static string BuildParameterName(int parameterIndex)
    {
        return $"@p{parameterIndex}";
    }
    
    // ======================================================================
    // Limit / Offset
    // ======================================================================
    
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => SqlDialectDefaults.BuildLimitOffset(sqlBuilder, limit, offset);
    
    public static void BuildLimitOffsetForUpdateDelete(ISqlBuilder sqlBuilder, int? limit, int? offset)
        => throw new NotSupportedException("PostgreSQL does not support LIMIT/OFFSET for UPDATE/DELETE statements.");
    
    // ======================================================================
    // Feature Flags
    // ======================================================================
    
    public static bool SupportsReturning => true;
    public static bool SupportsArrays => true;
    public static bool SupportsJson => true;
    public static bool SupportsWindowFunctions => true;
    public static bool SupportsCte => true;
    public static bool SupportsRecursiveCte => true;
    public static bool SupportsDeleteUsing => true;
    public static bool SupportsIsDistinctFrom => true;
    public static bool SupportsFilteredAggregates => true;
    public static bool SupportsFullOuterJoin => true;
    public static bool SupportsNaturalJoin => true;
    public static bool SupportsLateralJoin => true;
    public static bool SupportsApplyJoin => false;
    public static bool UseLimitPairMode => false;
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
    
    public static Dictionary<Type, ISqlDataType> ClrToSqlTypeMap => new()
    {
        [typeof(int)] = PgSqlDataType.Integer,
        [typeof(long)] = PgSqlDataType.BigInt,
        [typeof(short)] = PgSqlDataType.SmallInt,
        [typeof(byte)] = PgSqlDataType.SmallInt,
        [typeof(string)] = PgSqlDataType.Text,
        [typeof(bool)] = PgSqlDataType.Boolean,
        [typeof(decimal)] = PgSqlDataType.Numeric,
        [typeof(float)] = PgSqlDataType.Real,
        [typeof(double)] = PgSqlDataType.DoublePrecision,
        [typeof(DateTime)] = PgSqlDataType.Timestamp,
        [typeof(DateOnly)] = PgSqlDataType.Date,
        [typeof(TimeOnly)] = PgSqlDataType.Time,
        [typeof(Guid)] = PgSqlDataType.Uuid,
        [typeof(byte[])] = PgSqlDataType.Bytea,
        [typeof(char)] = PgSqlDataType.Char,
    };
}