using Drizzle_Like.Shared;

namespace Drizzle_Like.Schema.Tables;

public interface ITable: ISql<object>
{
    public string TableName { get; }
    public string? Alias { get; }
}

