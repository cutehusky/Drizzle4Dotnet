using Drizzle4Dotnet.Shared;

namespace Drizzle4Dotnet.Query;

public interface IParameterizedSql<T> : ISql<T>
{
    public Dictionary<string, object?> Parameters { get; } 
}