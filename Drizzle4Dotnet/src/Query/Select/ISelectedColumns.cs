using System.Data.Common;

namespace Drizzle4Dotnet.Query.Select;

public interface ISelectedColumns<TReturn>
{
    string Sql { get; }
    TReturn Mapper(DbDataReader r);
}

