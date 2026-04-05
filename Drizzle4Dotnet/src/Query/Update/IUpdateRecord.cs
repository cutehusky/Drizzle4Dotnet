namespace Drizzle4Dotnet.Query.Update;

public interface IUpdateRecord<TTable>
{ 
    public void Writer(Dictionary<string, object?> values);
}