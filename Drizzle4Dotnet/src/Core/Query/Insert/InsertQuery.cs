using System.Text;
using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Query.Insert;

public class InsertQuery<TTable> : Query<object> where TTable : ITable
{
    private readonly TTable _table;
    private readonly List<Dictionary<string, object?>> _values = new();

    public InsertQuery(TTable table, DbClient dbClient) : base(null, dbClient)
    {
        _table = table;
    }
    
    public InsertQuery<TTable> Value(IInsertRecord<TTable> record)
    {
        Dictionary<string, object?> value = new();
        record.Writer(value);
        _values.Add(value);
        return this;
    }
    
    public InsertQuery<TTable> Values(params IInsertRecord<TTable>[] records)
    {
        foreach (var record in records)
        {
            Dictionary<string, object?> value = new();
            record.Writer(value);
            _values.Add(value);
        }
        return this;
    }
    
    public InsertQuery<TTable> Value(Dictionary<IColumnBase<TTable>, object?> columnValuePairs)
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
    
    public InsertQuery<TTable> Values(params Dictionary<IColumnBase<TTable>, object?>[] columnValuePairsArray)
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
                        string pName = $"p{Parameters.Count}";
                        Parameters.Add(pName, val);
                        rowParamNames.Add($"@{pName}");
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

    public ReturningQuery<TReturn> Returning<TReturn>(ISelectedColumns<TReturn> selectedColumns)
    {
        return new ReturningQuery<TReturn>(this, selectedColumns, DbClient);
    }
}
