using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Query.Insert;

public interface IInsertRecord<TTable> where TTable: ITable
{
    public void Writer(Dictionary<string, object?> values);
}