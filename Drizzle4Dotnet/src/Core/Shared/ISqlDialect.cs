namespace Drizzle4Dotnet.Core.Shared;

public interface ISqlDialect
{ 
    static abstract string BuildIdentifier(string identifier);
    
    static abstract string BuildTableName(string schemaName, string tableName);
    
    static abstract string BuildColumnName(string tableName, string columnName);
    
    static abstract string BuildParameterName(string parameterName);
}