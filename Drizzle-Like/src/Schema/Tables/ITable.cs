using Drizzle_Like.Shared;

namespace Drizzle_Like.Schema.Tables;

public interface ITable: ISql<object>
{
    public static abstract string TableName { get; }
    public static abstract string? Alias { get; }
    public static abstract string TableRefName { get; }
}