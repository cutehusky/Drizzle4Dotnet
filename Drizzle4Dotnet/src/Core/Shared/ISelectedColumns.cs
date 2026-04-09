using System.Data.Common;

namespace Drizzle4Dotnet.Core.Shared;

public interface ISelectedColumns<TReturn, TDialect>: ISql where TDialect : ISqlDialect
{
    TReturn Mapper(DbDataReader r);
}


public interface ISelection<TReturnModel, TReturnRecord, TDialect> where TDialect : ISqlDialect
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect> Mapping { get; }
}