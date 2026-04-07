using System.Data.Common;

namespace Drizzle4Dotnet.Core.Query.Select;

public interface IReturning<TReturn>: IParameterizedSql<TReturn>
{
    protected ISelectedColumns<TReturn>? SelectedColumns { get;  }

    public Func<DbDataReader, TReturn>? Mapper
    {
        get
        {
            if (SelectedColumns == null)
            {
                return null;
            }
            return SelectedColumns.Mapper;
        }
    }
}
