namespace Drizzle4Dotnet.Core.Shared;

public interface ISqlDialect
{ 
    // ======================================================================
    // Identifier & Naming
    // ======================================================================
    
    static abstract string BuildIdentifier(string identifier);
    
    static abstract string BuildTableName(string schemaName, string tableName);
    
    static abstract string BuildColumnName(string tableName, string columnName);
    
    static abstract string BuildParameterName(string parameterName);
    
    static abstract string BuildParameterName(int parameterIndex);
    
    // ======================================================================
    // Limit / Offset
    // ======================================================================
    
    static abstract string BuildLimitOffset(int? limit, int? offset);
    
    // ======================================================================
    // Feature Flags
    // ======================================================================
    
    static abstract bool SupportsReturning { get; }
    static abstract bool SupportsArrays { get; }
    static abstract bool SupportsJson { get; }
    static abstract bool SupportsWindowFunctions { get; }
    static abstract bool SupportsCte { get; }
    static abstract bool SupportsRecursiveCte { get; }
    static abstract bool SupportsDeleteUsing { get; }
    static abstract bool SupportsIsDistinctFrom { get; }
    static abstract bool SupportsFilteredAggregates { get; }
    
    // ======================================================================
    // String Escaping
    // ======================================================================
    
    static abstract string EscapeString(string value);
}