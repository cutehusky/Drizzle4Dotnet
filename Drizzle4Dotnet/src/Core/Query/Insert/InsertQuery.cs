using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Insert;

public class InsertQuery<TTable, TDialect> : Query<TDialect> where TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly TTable _table;
    private readonly List<Dictionary<string, object?>> _values = new();
    private readonly List<ICteTable<TDialect>> _cteTables = new List<ICteTable<TDialect>>();

    public InsertQuery(TTable table, DbClient<TDialect> dbClient) : base(dbClient)
    {
        _table = table;
    }
    
    public InsertQuery<TTable, TDialect> With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return this;
    }
    
    public InsertQuery<TTable, TDialect> Value(IInsertRecord<TTable, TDialect> record)
    {
        Dictionary<string, object?> value = new();
        record.Writer(value);
        _values.Add(value);
        return this;
    }
    
    public InsertQuery<TTable, TDialect> Values(params IInsertRecord<TTable, TDialect>[] records)
    {
        foreach (var record in records)
        {
            Dictionary<string, object?> value = new();
            record.Writer(value);
            _values.Add(value);
        }
        return this;
    }
    
    public InsertQuery<TTable, TDialect> Value(Dictionary<IColumnOfTable<TTable>, object?> columnValuePairs)
    {        
        var value = new Dictionary<string, object?>();
        foreach (var columnValuePair in columnValuePairs)
        {
            var col = columnValuePair.Key;
            var val = columnValuePair.Value;
            value[col.Identifier] = val;
        }
        _values.Add(value);
        return this;
    }
    
    public InsertQuery<TTable, TDialect> Values(params Dictionary<IColumnOfTable<TTable>, object?>[] columnValuePairsArray)
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
        return this;
    }
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_values.Count == 0)
            throw new InvalidOperationException("No values provided for insert.");

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
        sqlBuilder.Append(" (");

        for (int i = 0; i < allColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(TDialect.BuildIdentifier(allColumns[i]));
        }
        sqlBuilder.Append(") VALUES ");

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
