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
    protected readonly List<Dictionary<string, object?>> ValuesToInsert = new();
    protected IGenericSql? FromQuery; // must be IReturning<TReturn, TDialect, TVirtualTable> but C# doesn't allow generic constraints on method parameters
    protected bool UseDefaultValues;

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
        ValuesToInsert.Add(value);
        return (TSelf)this;
    }
    
    public TSelf Values(params IInsertRecord<TTable, TDialect>[] records)
    {
        foreach (var record in records)
        {
            Dictionary<string, object?> value = new();
            record.Writer(value);
            ValuesToInsert.Add(value);
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
        ValuesToInsert.Add(value);
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
            ValuesToInsert.Add(value);
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

    
    /// <summary>
    /// Builds the INSERT statement without the VALUES clause.
    /// Produces: INSERT INTO table (col1, col2, ...)
    /// Subclasses can override BuildInsertValues to customize the VALUES / DEFAULT VALUES / FROM clause.
    /// </summary>
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        var hasValues = ValuesToInsert.Count > 0;
        var hasFrom = FromQuery != null;
        
        if (hasValues && hasFrom)
            throw new InvalidOperationException("Cannot combine Value(s) with From(). Use either explicit values or INSERT ... SELECT, not both.");
        
        if (hasValues && UseDefaultValues)
            throw new InvalidOperationException("Cannot combine Value(s) with DefaultValues(). Use either explicit values or DEFAULT VALUES, not both.");
        
        if (hasFrom && UseDefaultValues)
            throw new InvalidOperationException("Cannot combine From() with DefaultValues(). Use either INSERT ... SELECT or DEFAULT VALUES, not both.");
        
        if (!hasValues && !hasFrom && !UseDefaultValues)
            throw new InvalidOperationException("No values provided for insert. Use Value(s), DefaultValues(), or From().");
        
        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        BuildInsertKeywords(sqlBuilder);
        Table.BuildRefSql(sqlBuilder);

        if (UseDefaultValues)
        {
            BuildDefaultValues(sqlBuilder);
        } else if (FromQuery != null)
        {
            sqlBuilder.Append(' ');
            FromQuery.BuildSql(sqlBuilder);
        }
        else if (ValuesToInsert.Count > 0)
        {
            var allColumns = ValuesToInsert.SelectMany(d => d.Keys).Distinct().ToList();
            // Column list
            SqlStatics.BuildInsertColumnList<TDialect>(sqlBuilder, allColumns);
            SqlStatics.BuildInsertRowValues(sqlBuilder, ValuesToInsert, allColumns);
        }
    }
    
    protected virtual void BuildDefaultValues(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(" DEFAULT VALUES");
    }
    
    protected virtual void BuildInsertKeywords(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INSERT INTO ");
    }
}
