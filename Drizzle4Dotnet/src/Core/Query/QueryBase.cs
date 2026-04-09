using System.Text;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;


public abstract class QueryBase<TDialect>: IGenericSql where TDialect : ISqlDialect
{
    public readonly DbClient<TDialect> DbClient;

    public QueryBase(DbClient<TDialect> dbClient)
    {
        DbClient = dbClient;
    }

    public abstract string BuildSql(Dictionary<string, object?> parameters);
    public abstract void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb);

    public (string, Dictionary<string, object?>) Build()
    {
        var parameters = new Dictionary<string, object?>();
        var sb = new StringBuilder();
        BuildSql(parameters, sb);
        return (sb.ToString(), parameters);
    }
    
    protected void AppendClause(StringBuilder sb, string header, string separator, IReadOnlyList<IGenericSql> items, 
        Dictionary<string, object?> parameters, bool wrapInParentheses = false)
    {
        if (items.Count == 0) return;

        sb.Append(header);
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) sb.Append(separator);
            if (wrapInParentheses) sb.Append('(');
            items[i].BuildSql(parameters, sb);
            if (wrapInParentheses) sb.Append(')');
        }
    }
}
