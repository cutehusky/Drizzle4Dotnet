namespace Drizzle4Dotnet.Core.Shared;

public interface ISql<TReturn>
{
    string BuildSql(Dictionary<string, object?> parameters);
}

public interface ISql
{
    string BuildSql(Dictionary<string, object?> parameters);
}