using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;


public interface IColumn<T> : IAliasedSql<T>, IGenericColumn
{
}

public interface IGenericColumn: ISql
{
}


public interface IColumnOfTable<TTable>
{
    public string Identifier { get; }
}

public interface IColumnOfDialect<T, TDialect> : IColumn<T> where TDialect : ISqlDialect
{
    
}
