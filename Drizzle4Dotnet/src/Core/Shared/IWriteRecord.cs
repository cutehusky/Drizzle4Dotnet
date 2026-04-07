namespace Drizzle4Dotnet.Core.Shared;

public interface IWriteRecord
{
    public void Writer(Dictionary<string, object?> values);
}