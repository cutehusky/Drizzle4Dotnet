using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Update;

public class UpdateQuery<TTable, TDialect, TSelf> : Query<TDialect>,
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
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        CteTables.Add(cteTable);
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
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (SetValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }
        
        BuildSqlCte(sqlBuilder);

        sqlBuilder.Append("UPDATE ");
        Table.BuildRefSql(sqlBuilder);
        BuildSqlSet(sqlBuilder, SetValues);

        AppendClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }
    
    /// <summary>
    /// Builds the SET clause for UPDATE queries from a dictionary of column → value pairs.
    /// Values can be raw scalars (added as parameters) or IGenericSql expressions (rendered inline).
    /// </summary>
    protected void BuildSqlSet(ISqlBuilder sqlBuilder, Dictionary<string, object?> setValues)
    {
        sqlBuilder.Append(" SET ");
        bool firstSet = true;
        foreach (var kv in setValues)
        {
            if (!firstSet) sqlBuilder.Append(", ");

            sqlBuilder.Append(TDialect.BuildIdentifier(kv.Key));
            sqlBuilder.Append(" = ");

            if (kv.Value is IGenericSql op)
            {
                sqlBuilder.Append('(');
                op.BuildSql(sqlBuilder);
                sqlBuilder.Append(')');
            }
            else
            {
                sqlBuilder.Append(sqlBuilder.AddParameter(kv.Value));
            }
            firstSet = false;
        }
    }
}
