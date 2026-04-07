using System.Data.Common;

namespace Drizzle4Dotnet.Core.Shared;

public interface IReturning<TReturn, TDialect> : ISql<TReturn> where TDialect : ISqlDialect
{
    protected ISelectedColumns<TReturn, TDialect> SelectedColumns { get;  }

    public Func<DbDataReader, TReturn> Mapper => SelectedColumns.Mapper;
}