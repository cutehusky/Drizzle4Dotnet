using System.Data.Common;

namespace Drizzle_Like.Query.Select;

public interface ISelectedColumns<TReturn>
{
    string Sql { get; }
    TReturn Mapper(DbDataReader r);
}

