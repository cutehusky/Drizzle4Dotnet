namespace Drizzle_Like.Schema.Columns;

public class DbColumn<T>: IColumn<T>
{
    public string Name { get; }
    public string Table { get; }
    
    public DbColumn(string table, string name)
    {
        Table = table;
        Name = name;
    }
    
    public string Sql => $"\"{Table}\".\"{Name}\"";
}