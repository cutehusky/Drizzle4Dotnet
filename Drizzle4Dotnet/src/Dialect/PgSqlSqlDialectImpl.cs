using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Dialect;

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
    
    public static string BuildLimitOffset(int? limit, int? offset)
        => SqlDialectDefaults.BuildLimitOffset(limit, offset);
    
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
    
    // ======================================================================
    // Upsert
    // ======================================================================
    
    public static string BuildOnDuplicateKeyUpdate(IReadOnlyList<string> columns)
        => SqlDialectDefaults.BuildOnDuplicateKeyUpdate(columns);
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    public static string EscapeString(string value)
        => SqlDialectDefaults.EscapeString(value);
}