using Drizzle_Like.Shared;

namespace Drizzle_Like.Query;

public interface IParameterizedSql<T> : ISql<T>
{
    public Dictionary<string, object?> Parameters { get; } 
}