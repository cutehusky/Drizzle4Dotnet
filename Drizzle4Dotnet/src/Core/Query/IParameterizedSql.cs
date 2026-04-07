using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

public interface IParameterizedSql<T> : ISql<T>
{
    public Dictionary<string, object?> Parameters { get; } 
}