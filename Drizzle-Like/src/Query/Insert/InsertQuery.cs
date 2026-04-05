
using Drizzle_Like.Query.Select;
using Drizzle_Like.Schema.Columns;
using Drizzle_Like.Schema.Tables;

namespace Drizzle_Like.Query.Insert;

public class InsertQuery<TTable> : Query<object> where TTable : ITable
{
    private readonly TTable _table;
    private readonly Dictionary<string, object?> _values = new();

    public InsertQuery(TTable table, DbClient dbClient) : base(null, dbClient)
    {
        _table = table;
    }

    public InsertQuery<TTable> Values<T>(DbColumn<T, TTable> column, T value)
    {
        _values[column.Identifier] = value;
        return this;
    }

    public override string Sql
    {
        get
        {
            var colNames = _values.Keys;
            var paramNames = new List<string>();

            foreach (var val in _values.Values)
            {
                string pName = $"p{Parameters.Count}";
                Parameters.Add(pName, val);
                paramNames.Add($"@{pName}");
            }

            return $"INSERT INTO {_table.Sql} ({string.Join(", ", colNames)}) VALUES ({string.Join(", ", paramNames)})";
        }
    }

    public ReturningQuery<TReturn> Returning<TReturn>(ISelectedColumns<TReturn> selectedColumns)
    {
        return new ReturningQuery<TReturn>(this, selectedColumns, DbClient);
    }
}
