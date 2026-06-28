using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Delete;


public class DeleteQuery<TTable, TDialect, TSelf> : Query<TDialect>,
    ISupportWhere<TSelf>,
    ISupportCte<TSelf, TDialect>
    where TSelf : DeleteQuery<TTable, TDialect, TSelf>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    protected readonly TTable Table;
    protected readonly List<IGenericSql> Wheres = new();

    public DeleteQuery(TTable table, IQueryExecutor<TDialect> executor) : base(executor)
    {
        Table = table;
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        CteTables.Add(cteTable);
        return (TSelf)this;
    }

    public TSelf Where(IGenericSql condition)
    {
        Wheres.Add(condition);
        return (TSelf)this;
    }
    
    public TSelf Where(params IGenericSql[] conditions)
    {
        Wheres.AddRange(conditions);
        return (TSelf)this;
    }
    
    /// <summary>
    /// Validates the query state before building SQL.
    /// Override in dialect-specific subclasses to add custom validation.
    /// </summary>
    protected virtual void ValidateQuery()
    {
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("DELETE FROM ");
        Table.BuildRefSql(sqlBuilder);
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }
}
