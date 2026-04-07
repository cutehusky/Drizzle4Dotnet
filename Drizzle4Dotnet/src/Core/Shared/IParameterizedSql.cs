using System.Data.Common;

namespace Drizzle4Dotnet.Core.Shared;

public interface IParameterizedSql<TReturn, TDialect> : ISql<TReturn> where TDialect : ISqlDialect
{
    public Dictionary<string, object?> Parameters { get; } 
    
    protected ISelectedColumns<TReturn, TDialect> SelectedColumns { get;  }

    public Func<DbDataReader, TReturn> Mapper => SelectedColumns.Mapper;
}

public interface IParameterizedSql : ISql
{
    public Dictionary<string, object?> Parameters { get; } 
}