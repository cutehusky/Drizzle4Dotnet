using System.Text;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Insert;

public class InsertQuery<TTable, TDialect> : Query<TDialect> where TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly TTable _table;
    private readonly List<Dictionary<string, object?>> _values = new();

    public InsertQuery(TTable table, DbClient<TDialect> dbClient) : base(dbClient)
    {
        _table = table;
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
    
    public InsertQuery<TTable, TDialect> Value(Dictionary<IColumnOfTableType<TTable, TDialect>, object?> columnValuePairs)
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
    
    public InsertQuery<TTable, TDialect> Values(params Dictionary<IColumnOfTableType<TTable, TDialect>, object?>[] columnValuePairsArray)
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

    public override string Sql
    {
        get
        {
            if (_values.Count == 0)
                throw new InvalidOperationException("No values provided for insert.");

            var allColumns = _values.SelectMany(d => d.Keys).Distinct().ToList();
        
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {_table.Sql} (");
            sb.Append(string.Join(", ", allColumns));
            sb.Append(") VALUES ");

            var rows = new List<string>();

            foreach (var row in _values)
            {
                var rowParamNames = new List<string>();
            
                foreach (var col in allColumns)
                {
                    if (row.TryGetValue(col, out var val))
                    {
                        string pName = $"@p{Parameters.Count}";
                        Parameters.Add(pName, val);
                        rowParamNames.Add($"{pName}");
                    }
                    else
                    {
                        rowParamNames.Add("NULL");
                    }
                }
                rows.Add($"({string.Join(", ", rowParamNames)})");
            }

            sb.Append(string.Join(", ", rows));
            return sb.ToString();
        }
    }

    public ReturningQuery<TReturn, TDialect> Returning<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new ReturningQuery<TReturn, TDialect>(this, selectedColumns);
    }
}
