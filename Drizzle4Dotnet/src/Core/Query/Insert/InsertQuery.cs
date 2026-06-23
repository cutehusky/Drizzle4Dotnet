using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Insert;

public class InsertQuery<TTable, TDialect, TSelf> : Query<TDialect>
    where TSelf : InsertQuery<TTable, TDialect, TSelf>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    protected readonly TTable _table;
    protected readonly List<Dictionary<string, object?>> _values = new();
    protected readonly List<ICteTable<TDialect>> _cteTables = new();
    private bool _useDefaultValues;
    private IGenericSql? _fromQuery;

    public InsertQuery(TTable table, DbClient<TDialect> dbClient) : base(dbClient)
    {
        _table = table;
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return (TSelf)this;
    }
    
    public TSelf Value(IInsertRecord<TTable, TDialect> record)
    {
        Dictionary<string, object?> value = new();
        record.Writer(value);
        _values.Add(value);
        return (TSelf)this;
    }
    
    public TSelf Values(params IInsertRecord<TTable, TDialect>[] records)
    {
        foreach (var record in records)
        {
            Dictionary<string, object?> value = new();
            record.Writer(value);
            _values.Add(value);
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
        _values.Add(value);
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
            _values.Add(value);
        }
        return (TSelf)this;
    }

    /// <summary>
    /// INSERT ... SELECT — inserts rows from a subquery.
    /// Usage: _db.Insert(table).From(_db.Select(...).From(otherTable).Where(...))
    /// </summary>
    public TSelf From(IGenericSql selectQuery)
    {
        _fromQuery = selectQuery;
        return (TSelf)this;
    }

    /// <summary>
    /// INSERT DEFAULT VALUES — inserts a row with all default values.
    /// </summary>
    public TSelf DefaultValues()
    {
        _useDefaultValues = true;
        return (TSelf)this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_values.Count == 0 && !_useDefaultValues && _fromQuery == null)
            throw new InvalidOperationException("No values provided for insert. Use Value(s), DefaultValues(), or From().");

        var allColumns = _values.SelectMany(d => d.Keys).Distinct().ToList();
        
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

        sqlBuilder.Append("INSERT INTO ");
        _table.BuildRefSql(sqlBuilder);

        // Column list
        if (allColumns.Count > 0)
        {
            sqlBuilder.Append(" (");
            for (int i = 0; i < allColumns.Count; i++)
            {
                if (i > 0) sqlBuilder.Append(", ");
                sqlBuilder.Append(TDialect.BuildIdentifier(allColumns[i]));
            }
            sqlBuilder.Append(')');
        }

        if (_useDefaultValues)
        {
            sqlBuilder.Append(" DEFAULT VALUES");
        }
        else if (_fromQuery != null)
        {
            sqlBuilder.Append(' ');
            _fromQuery.BuildSql(sqlBuilder);
        }
        else
        {
            sqlBuilder.Append(" VALUES ");
            for (int rowIndex = 0; rowIndex < _values.Count; rowIndex++)
            {
                if (rowIndex > 0) sqlBuilder.Append(", ");
            
                sqlBuilder.Append('(');
                var row = _values[rowIndex];
            
                for (int colIndex = 0; colIndex < allColumns.Count; colIndex++)
                {
                    if (colIndex > 0) sqlBuilder.Append(", ");
                
                    if (row.TryGetValue(allColumns[colIndex], out var val))
                    {
                        sqlBuilder.Append(sqlBuilder.AddParameter(val));
                    }
                    else
                    {
                        sqlBuilder.Append("NULL");
                    }
                }
                sqlBuilder.Append(')');
            }
        }
    }
}
