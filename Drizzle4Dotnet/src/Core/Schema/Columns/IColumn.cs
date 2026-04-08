using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;


public interface IColumn<T> : ISql<T>
{
    public string Identifier { get; }
}


public interface IColumnOfTable<TTable>
{
    public string Identifier { get; }
}

public interface IColumnOfDialect<T, TDialect> : IColumn<T> where TDialect : ISqlDialect
{
    
}
