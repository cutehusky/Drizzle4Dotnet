using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Update;

public class UpdateQuery<TTable, TDialect, TSelf> : Query<TDialect>,
    ISupportUpdateSet<TSelf, TTable, TDialect>,
    ISupportWhere<TSelf>,
    ISupportCte<TSelf, TDialect>
    where TSelf : UpdateQuery<TTable, TDialect, TSelf>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    protected readonly TTable Table;
    protected readonly Dictionary<string, object?> SetValues = new();
    protected readonly List<IGenericSql> Wheres = new();

    public UpdateQuery(
        TTable table, 
        IQueryExecutor<TDialect> executor
    ): base(executor)
    {
        Table = table;
    }

    public TSelf Set<T>(DbColumn<T, TTable, TDialect> column, T value)
    {
        SetValues[column.Identifier] = value;
        return (TSelf)this;
    }
    
    public TSelf With(params ICteTable<TDialect>[] cteTables)
    {
        Recursive = false;
        CteTables.AddRange(cteTables);
        return (TSelf)this;
    }
    
    public TSelf WithRecursive(params ICteTable<TDialect>[] cteTables)
    {
        Recursive = true;
        CteTables.AddRange(cteTables);
        return (TSelf)this;
    }
    
    public TSelf Set(IUpdateRecord<TTable, TDialect> record)
    {
        record.Writer(SetValues);
        return (TSelf)this;
    }
    
    public TSelf Set<T>(DbColumn<T, TTable, TDialect> column, ISql<T> value)
    {
        SetValues[column.Identifier] = value;
        return (TSelf)this;
    }
    
    public TSelf Set(Dictionary<IColumnOfTable<TTable>, object> columnValuePairs)
    {
        foreach (var kv in columnValuePairs)
        {
            SetValues[kv.Key.Identifier] = kv.Value;
        }
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
    protected override void ValidateQuery()
    {
        if (SetValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();
        
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);
        SqlStatics.BuildSqlSetClause<TDialect>(sqlBuilder, SetValues);

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }
}
