using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Delete;


public class DeleteQuery<TTable, TDialect, TSelf> : Query<TDialect>
    where TSelf : DeleteQuery<TTable, TDialect, TSelf>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    protected readonly TTable _table;
    protected readonly List<IGenericSql> _wheres = new();
    protected readonly List<ICteTable<TDialect>> _cteTables = new();

    public DeleteQuery(TTable table, DbClient<TDialect> dbClient) : base(dbClient)
    {
        _table = table;
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return (TSelf)this;
    }

    public TSelf Where(IGenericSql condition)
    {
        _wheres.Add(condition);
        return (TSelf)this;
    }
    
    public TSelf Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return (TSelf)this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_cteTables.Count > 0)
        {
            sqlBuilder.Append("WITH ");
            for (int i = 0; i < _cteTables.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                _cteTables[i].BuildSql(sqlBuilder);
            }
            sqlBuilder.Append(' ');
        }

        sqlBuilder.Append("DELETE FROM ");
        _table.BuildRefSql(sqlBuilder);
        AppendClause(sqlBuilder, " WHERE ", " AND ", _wheres, wrapInParentheses: true);
    }
}
