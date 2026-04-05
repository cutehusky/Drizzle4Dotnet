namespace Drizzle_Like.Schema.Tables;

public abstract class DbTable: ITable
{
    public abstract string TableName { get; }
    
    public abstract string? Alias { get; }

    public string Sql => string.IsNullOrEmpty(Alias)
        ? $"\"{TableName}\""
        : $"\"{TableName}\" AS {Alias}";
}