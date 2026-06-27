using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Insert;

public class InsertQuery<TTable, TDialect, TSelf> : Query<TDialect>,
    ISupportInsertValue<TSelf, TTable, TDialect>,
    ISupportCte<TSelf, TDialect>
    where TSelf : InsertQuery<TTable, TDialect, TSelf>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    protected readonly TTable Table;
    protected readonly List<Dictionary<string, object?>> NewValues = new();
    protected bool UseDefaultValues;
    protected IGenericSql? FromQuery; // must be IReturning<TReturn, TDialect, TVirtualTable> but C# doesn't allow generic constraints on method parameters

    public InsertQuery(TTable table, IQueryExecutor<TDialect> executor) : base(executor)
    {
        Table = table;
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        CteTables.Add(cteTable);
        return (TSelf)this;
    }
    
    public TSelf Value(IInsertRecord<TTable, TDialect> record)
    {
        Dictionary<string, object?> value = new();
        record.Writer(value);
        NewValues.Add(value);
        return (TSelf)this;
    }
    
    public TSelf Values(params IInsertRecord<TTable, TDialect>[] records)
    {
        foreach (var record in records)
        {
            Dictionary<string, object?> value = new();
            record.Writer(value);
            NewValues.Add(value);
        }
        return (TSelf)this;
    }
    
    public TSelf Value(Dictionary<IColumnOfTable<TTable>, object?> columnValuePairs)
    {        
        var value = new Dictionary<string, object?>();
        foreach (var columnValuePair in columnValuePairs)
        {
            var col = columnValuePair.Key;
            var val = columnValuePair.Value;
            value[col.Identifier] = val;
        }
        NewValues.Add(value);
        return (TSelf)this;
    }
    
    public TSelf Values(params Dictionary<IColumnOfTable<TTable>, object?>[] columnValuePairsArray)
    {
        foreach (var columnValuePairs in columnValuePairsArray)
        {
            var value = new Dictionary<string, object?>();
            foreach (var columnValuePair in columnValuePairs)
            {
                var col = columnValuePair.Key;
                var val = columnValuePair.Value;
                value[col.Identifier] = val;
            }
            NewValues.Add(value);
        }
        return (TSelf)this;
    }

    /// <summary>
    /// INSERT ... SELECT — inserts rows from a subquery.
    /// Usage: _db.Insert(table).From(_db.Select(...).From(otherTable).Where(...))
    /// </summary>
    public TSelf From<TReturn, TVirtualTable>(IReturning<TReturn, TDialect, TVirtualTable> selectQuery) where TVirtualTable : IVirtualTable<TDialect>
    {
        FromQuery = selectQuery;
        return (TSelf)this;
    }

    /// <summary>
    /// INSERT DEFAULT VALUES — inserts a row with all default values.
    /// </summary>
    public TSelf DefaultValues()
    {
        UseDefaultValues = true;
        return (TSelf)this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (NewValues.Count == 0 && !UseDefaultValues && FromQuery == null)
            throw new InvalidOperationException("No values provided for insert. Use Value(s), DefaultValues(), or From().");

        var allColumns = NewValues.SelectMany(d => d.Keys).Distinct().ToList();
        
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("INSERT INTO ");
        Table.BuildRefSql(sqlBuilder);

        // Column list
        SqlStatics.BuildInsertColumnList<TDialect>(sqlBuilder, allColumns);

        if (UseDefaultValues)
        {
            sqlBuilder.Append(" DEFAULT VALUES");
        }
        else if (FromQuery != null)
        {
            sqlBuilder.Append(' ');
            FromQuery.BuildSql(sqlBuilder);
        }
        else
        {
            SqlStatics.BuildInsertRowValues(sqlBuilder, NewValues, allColumns);
        }
    }
}
