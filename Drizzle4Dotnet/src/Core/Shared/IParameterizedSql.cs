namespace Drizzle4Dotnet.Core.Shared;

public interface IParameterizedSql<TReturn>
{
    string BuildSql(Dictionary<string, object?> parameters);
}

public interface IParameterizedSql
{
    string BuildSql(Dictionary<string, object?> parameters);
}