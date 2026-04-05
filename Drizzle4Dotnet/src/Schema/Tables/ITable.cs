using Drizzle4Dotnet.Shared;

namespace Drizzle4Dotnet.Schema.Tables;

public interface ITable: ISql<object>
{
    public static abstract string TableName { get; }
    public static abstract string? Alias { get; }
    public static abstract string TableRefName { get; }
    
    public string Identifier { get; }
}