using Drizzle_Like.Shared;

namespace Drizzle_Like.Schema.Columns;


public interface IColumn<T>: ISql<T>
{
    public string Identifier { get; }
}