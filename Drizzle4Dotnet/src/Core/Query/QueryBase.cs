using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


public abstract class QueryBase<TDialect>: IGenericSql where TDialect : ISqlDialect
{
    /// <summary>
    /// The executor used to run this query against a database.
    /// Decoupled via IQueryExecutor for testability.
    /// </summary>
    public readonly IQueryExecutor<TDialect> Executor;
    protected readonly List<ICteTable<TDialect>> CteTables = new();
    protected bool Recursive;
    
    public QueryBase(IQueryExecutor<TDialect> executor)
    {
        Executor = executor;
    }

    public abstract void BuildSql(ISqlBuilder sqlBuilder);

    public (string, Dictionary<string, object?>) Build()
    {
        var builder = new SqlBuilder<TDialect>();
        BuildSql(builder);
        return builder.Build();
    }
    
    /// <summary>
    /// Validates the query state before building SQL.
    /// Override in dialect-specific subclasses to add custom validation.
    /// </summary>
    protected virtual void ValidateQuery()
    {
        if (Recursive && CteTables.Count == 0)
            throw new InvalidOperationException("Recursive CTE tables must be provided for a recursive compound query.");
    }
}
