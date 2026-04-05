using Drizzle_Like.Schema.Tables;

namespace Drizzle_Like.Schema.Columns;

public class DbColumn<T>: IColumn<T>
{
    public string ColumnName { get; }
    public string RefName { get; }
    
    public DbColumn(string refName, string columnName)
    {
        RefName = refName;
        ColumnName = columnName;
    }
    
    public string Sql => $"\"{RefName}\".\"{ColumnName}\"";
}

public class DbColumnInstance<T, TTable>: DbColumn<T> where TTable : ITable
{
    public DbColumnInstance(string refName, string columnName) : base(refName, columnName)
    {
    }
}