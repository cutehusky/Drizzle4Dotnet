namespace Drizzle_Like.Query.Shared.Operators;

public interface IOperator
{
    string BuildSql(Dictionary<string, object?> parameters);
}