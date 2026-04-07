using System.Data.Common;

namespace Drizzle4Dotnet.Core.Shared;

public interface ISelectedColumns<TReturn, TDialect> where TDialect : ISqlDialect
{
    string Sql { get; }
    TReturn Mapper(DbDataReader r);
}