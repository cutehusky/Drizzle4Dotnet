using System.Data.Common;

namespace Drizzle4Dotnet.Core.Shared;

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