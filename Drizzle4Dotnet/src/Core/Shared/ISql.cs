namespace Drizzle4Dotnet.Core.Shared;

public interface ISql<TReturn>
{
    public string Sql { get; }
}

public interface ISql
{
    public string Sql { get; }
}