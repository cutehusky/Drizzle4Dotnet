using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Dialect;

public class PgSqlSqlDialectImpl: ISqlDialect
{
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
}