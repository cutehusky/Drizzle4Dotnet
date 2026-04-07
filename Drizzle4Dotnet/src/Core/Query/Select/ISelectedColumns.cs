using System.Data.Common;

namespace Drizzle4Dotnet.Core.Query.Select;

public interface ISelectedColumns<TReturn>
{
    string Sql { get; }
    TReturn Mapper(DbDataReader r);
}

