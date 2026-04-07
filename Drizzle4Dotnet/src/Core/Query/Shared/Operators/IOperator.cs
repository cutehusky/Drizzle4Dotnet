namespace Drizzle4Dotnet.Core.Query.Shared.Operators;

public interface IOperator
{
    string BuildSql(Dictionary<string, object?> parameters);
}