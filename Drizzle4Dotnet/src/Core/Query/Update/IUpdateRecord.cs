namespace Drizzle4Dotnet.Core.Query.Update;

public interface IUpdateRecord<TTable>
{ 
    public void Writer(Dictionary<string, object?> values);
}