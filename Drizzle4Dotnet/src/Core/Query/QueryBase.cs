using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


public abstract class QueryBase<TDialect>: IGenericSql where TDialect : ISqlDialect
{
    public readonly DbClient<TDialect> DbClient;

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
