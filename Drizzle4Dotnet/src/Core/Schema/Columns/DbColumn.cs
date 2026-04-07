using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Schema.Columns;

public class DbColumn<T, TTable>: IColumn<T>, IDbColumn<TTable> where TTable : ITable
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

public interface IDbColumn<TTable> where TTable : ITable
{
    public string Identifier { get; }
}