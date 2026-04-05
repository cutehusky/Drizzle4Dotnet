using System.Text;
using Drizzle_Like.Query.Select;
using Drizzle_Like.Query.Shared.Operators;
using Drizzle_Like.Schema.Columns;
using Drizzle_Like.Schema.Tables;

namespace Drizzle_Like.Query.Update;

public class UpdateQuery<TTable> : Query<object> where  TTable : ITable
{
    private readonly TTable _table;
    private readonly Dictionary<string, object?> _setValues = new();
    private readonly List<string> _wheres = new();

    public UpdateQuery(
        TTable table, 
        DbClient dbClient
    ): base(null, dbClient)
    {
        _table = table;
    }


    public UpdateQuery<TTable> Set<T>(DbColumnInstance<T, TTable> column, T value)
    {
        var columnName = column.Sql;
        var columnNameOnly = columnName.Contains(".") 
            ? columnName.Split('.').Last() 
            : columnName;
        _setValues[columnNameOnly] = value;
        return this;
    }
    
    public UpdateQuery<TTable> Set<T>(DbColumnInstance<T, TTable> column, IOperator value)
    {
        var columnName = column.Sql;
        var columnNameOnly = columnName.Contains(".") 
            ? columnName.Split('.').Last() 
            : columnName;
        _setValues[columnNameOnly] = value.BuildSql(Parameters);
        return this;
    }
    
    public UpdateQuery<TTable> Set(Dictionary<string, object> columnValuePairs)
    {
        foreach (var kv in columnValuePairs)
        {
            _setValues[kv.Key] = kv.Value;
        }
        return this;
    }

    public UpdateQuery<TTable> Where(IOperator condition)
    {
        _wheres.Add(condition.BuildSql(Parameters));
        return this;
    }

    public override string Sql
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append("UPDATE ");
            sb.Append(_table.Sql);
            sb.Append(" SET ");

            var setClauses = new List<string>();
            foreach (var kv in _setValues)
            {
                string paramName = $"@p{Parameters.Count}";
                Parameters.Add(paramName, kv.Value);
                setClauses.Add($"{kv.Key} = {paramName}");
            }

            sb.Append(string.Join(", ", setClauses));

            if (_wheres.Count > 0)
            {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", _wheres));
            }

            return sb.ToString();
        }
    }
    
    public ReturningQuery<TReturn> Returning<TReturn>(ISelectedColumns<TReturn> selectedColumns)
    {
        return new ReturningQuery<TReturn>(
            this,
            selectedColumns,
            DbClient
        );
    }
}
