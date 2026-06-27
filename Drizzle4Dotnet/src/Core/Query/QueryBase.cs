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

    /// <summary>
    /// Convenience accessor for backward compatibility.
    /// Returns the Executor as DbClient if applicable, otherwise null.
    /// </summary>
    [Obsolete("Use Executor instead of DbClient. This property will be removed in a future version.")]
    public DbClient<TDialect>? DbClient => Executor as DbClient<TDialect>;

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

}
