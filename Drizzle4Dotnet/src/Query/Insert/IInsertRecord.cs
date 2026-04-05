using Drizzle4Dotnet.Schema.Tables;

namespace Drizzle4Dotnet.Query.Insert;

public interface IInsertRecord<TTable> where TTable: ITable
{
    public void Writer(Dictionary<string, object?> values);
}