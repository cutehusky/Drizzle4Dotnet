using Drizzle4Dotnet.Shared;

namespace Drizzle4Dotnet.Schema.Columns;


public interface IColumn<T>: ISql<T>
{
    public string Identifier { get; }
}