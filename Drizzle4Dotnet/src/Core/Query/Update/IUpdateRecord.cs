using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Update;

public interface IUpdateRecord<TTable>: IWriteRecord where TTable : ITable
{ 
}