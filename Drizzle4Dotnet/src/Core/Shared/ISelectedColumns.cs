using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;

public interface ISelectedColumns<TReturn, TDialect, TVirtualTable>: ISql where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    TReturn Mapper(DbDataReader r);
}


public interface ISelection<TReturnModel, TReturnRecord, TDialect, TVirtualTable> where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect, TVirtualTable> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect, TVirtualTable> Mapping { get; }
}


public interface ISelectedColumns<TReturn, TDialect>: ISql where TDialect : ISqlDialect
{
    TReturn Mapper(DbDataReader r);
}


public interface ISelection<TReturnModel, TReturnRecord, TDialect> where TDialect : ISqlDialect 
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect> Mapping { get; }
}