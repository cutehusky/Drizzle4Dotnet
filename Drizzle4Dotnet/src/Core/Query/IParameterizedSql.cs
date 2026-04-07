using System.Data.Common;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

public interface IParameterizedSql<TReturn> : ISql<TReturn>
{
    public Dictionary<string, object?> Parameters { get; } 
    
    protected ISelectedColumns<TReturn> SelectedColumns { get;  }

    public Func<DbDataReader, TReturn> Mapper => SelectedColumns.Mapper;
}

public interface IParameterizedSql : ISql
{
    public Dictionary<string, object?> Parameters { get; } 
}