using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;


public interface IColumn<T>: ISql<T>
{
    public string Identifier { get; }
}