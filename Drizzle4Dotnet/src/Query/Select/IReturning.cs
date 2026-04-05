using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Drizzle4Dotnet.Query.Select;

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
