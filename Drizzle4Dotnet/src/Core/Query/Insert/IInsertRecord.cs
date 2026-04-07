using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Insert;

public interface IInsertRecord<TTable>: IWriteRecord where TTable: ITable
{
}