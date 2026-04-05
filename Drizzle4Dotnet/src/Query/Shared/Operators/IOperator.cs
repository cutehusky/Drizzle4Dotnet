namespace Drizzle4Dotnet.Query.Shared.Operators;

public interface IOperator
{
    string BuildSql(Dictionary<string, object?> parameters);
}