namespace Drizzle_Like.Query.Update;

public interface IUpdateRecord<TTable>
{ 
    public void Writer(Dictionary<string, object?> values);
}