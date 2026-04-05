using Drizzle_Like.Schema.Tables;

namespace Drizzle_Like.Query.Insert;

public interface IInsertRecord<TTable> where TTable: ITable
{
    public void Writer(Dictionary<string, object?> values);
}