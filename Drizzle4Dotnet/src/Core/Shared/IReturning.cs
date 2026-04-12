using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;

public interface IReturning<TReturn, TDialect> : ISql<TReturn> where TDialect : ISqlDialect
{
    protected ISelectedColumns<TReturn, TDialect> SelectedColumns { get;  }

    public Func<DbDataReader, TReturn> Mapper => SelectedColumns.Mapper;
}

public interface IReturning<TReturn, TDialect, TVirtualTable> : ISql<TReturn> where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    protected ISelectedColumns<TReturn, TDialect, TVirtualTable> SelectedColumns { get;  }

    public Func<DbDataReader, TReturn> Mapper => SelectedColumns.Mapper;

    public TVirtualTable AsSubQuery(string alias);
}