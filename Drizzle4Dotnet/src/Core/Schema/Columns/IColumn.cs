using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;


public interface IColumn<T>: ISql<T>
{ }


public interface IColumnOfTableType<TTable, TDialect>: IColumnType<TDialect> where TTable : ITable<TDialect> where TDialect: ISqlDialect
{
}


public interface IColumnType<TDialect>: ISql where TDialect: ISqlDialect
{
    public string Identifier { get; }
}