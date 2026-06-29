using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;


public interface IColumn<T> : IAliasedSql<T>
{
}


public interface IColumnOfTable<TTable>
{
    public string Identifier { get; }
}