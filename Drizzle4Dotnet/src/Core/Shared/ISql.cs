namespace Drizzle4Dotnet.Core.Shared;

public interface IGenericSql
{
    string BuildSql(Dictionary<string, object?> parameters);
}

public interface ISql<TReturn>: IGenericSql
{
}

public interface ISql: IGenericSql
{
}