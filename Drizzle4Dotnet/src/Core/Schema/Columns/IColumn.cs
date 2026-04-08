using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;


public interface IColumn<T>: ISql<T>
{ }


public interface IGenericColumn<TDialect>: ISql where TDialect: ISqlDialect
{
    public string Identifier { get; }
}


public interface IColumnOfTable<TTable, TDialect>: IGenericColumn<TDialect> where TTable : ITable<TDialect> where TDialect: ISqlDialect
{
}

public interface IColumnOfDialect<T, TDialect> : IColumn<T> where TDialect : ISqlDialect
{
    
}
