using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Tables;

public interface ITable: ISql
{
    public static abstract string TableName { get; }
    public static abstract string? Alias { get; }
    public static abstract string TableRefName { get; }
    
    public string Identifier { get; }
}