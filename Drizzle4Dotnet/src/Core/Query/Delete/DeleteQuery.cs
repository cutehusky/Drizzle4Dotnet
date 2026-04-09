using System.Text;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Delete;


public class DeleteQuery<TTable, TDialect> : Query<TDialect> where TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly TTable _table;
    private readonly List<IGenericSql> _wheres = new();

    public DeleteQuery(TTable table, DbClient<TDialect> dbClient) : base(dbClient)
    {
        _table = table;
    }

    public DeleteQuery<TTable, TDialect> Where(IGenericSql condition)
    {
        _wheres.Add(condition);
        return this;
    }
    
    public DeleteQuery<TTable, TDialect> Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }

    public override string BuildSql(Dictionary<string, object?> parameters)
    {
        var sb = new StringBuilder();
        sb.Append("DELETE FROM ");
        sb.Append(_table.BuildSql(parameters));

        AppendClause(sb, " WHERE ", " AND ", _wheres, parameters, wrapInParentheses: true);

        return sb.ToString();
    }
}
