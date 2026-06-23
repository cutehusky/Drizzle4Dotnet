using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


public abstract class QueryBase<TDialect>: IGenericSql where TDialect : ISqlDialect
{
    public readonly DbClient<TDialect> DbClient;
    protected readonly List<ICteTable<TDialect>> CteTables = new();
    protected bool Recursive;

    public QueryBase(DbClient<TDialect> dbClient)
    {
        DbClient = dbClient;
    }

    public abstract void BuildSql(ISqlBuilder sqlBuilder);

    public (string, Dictionary<string, object?>) Build()
    {
        var builder = new SqlBuilder<TDialect>();
        BuildSql(builder);
        return builder.Build();
    }

    /// <summary>
    /// Builds the WITH / WITH RECURSIVE clause if CTEs are registered.
    /// Call this at the very beginning of BuildSql() in any query subclass
    /// to render the CTE prefix before the main statement.
    /// </summary>
    protected void BuildSqlCte(ISqlBuilder sqlBuilder)
    {
        if (CteTables.Count == 0) return;

        sqlBuilder.Append("WITH");
        if (Recursive) sqlBuilder.Append(" RECURSIVE");
        sqlBuilder.Append(' ');
        for (int i = 0; i < CteTables.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            CteTables[i].BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(' ');
    }
    
    protected void AppendClause(ISqlBuilder sqlBuilder, string header, string separator, IReadOnlyList<IGenericSql> items, 
         bool wrapInParentheses = false)
    {
        if (items.Count == 0) return;

        sqlBuilder.Append(header);
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(separator);
            if (wrapInParentheses) sqlBuilder.Append('(');
            items[i].BuildSql(sqlBuilder);
            if (wrapInParentheses) sqlBuilder.Append(')');
        }
    }
}
