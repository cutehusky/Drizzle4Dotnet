using Drizzle_Like.Schema.Tables;

namespace Drizzle_Like.Schema.Columns;

public class DbColumn<T, TTable>: IColumn<T>, IColumnBase<TTable> where TTable : ITable
{
    private readonly string _columnName;
    
    public DbColumn(
        string columnName
        )
    {
        _columnName = columnName;
    }
    
    public string Sql => $"\"{TTable.TableRefName}\".\"{_columnName}\"";
    
    public string Identifier => $"\"{_columnName}\"";
}

public interface IColumnBase<TTable> where TTable : ITable
{
    public string Identifier { get; }
}