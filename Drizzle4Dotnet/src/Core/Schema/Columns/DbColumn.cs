using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;

public class DbColumn<T, TTable, TDialect>: IColumn<T>, IColumnOfTableType<TTable, TDialect> where TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly string _columnName;
    
    public DbColumn(
        string columnName
        )
    {
        _columnName = columnName;
    }
    
    public string Sql => TDialect.BuildColumnName(TTable.TableRefName, _columnName);
    
    public string Identifier => TDialect.BuildIdentifier(_columnName);
}
