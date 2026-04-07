using System.Text;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;

namespace Drizzle4Dotnet.Core.Query.Update;

public class UpdateQuery<TTable, TDialect> : Query<TDialect> where  TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly TTable _table;
    private readonly Dictionary<string, object?> _setValues = new();
    private readonly List<IOperator> _wheres = new();

    public UpdateQuery(
        TTable table, 
        DbClient<TDialect> dbClient
    ): base(dbClient)
    {
        _table = table;
    }

    public UpdateQuery<TTable, TDialect> Set<T>(DbColumn<T, TTable, TDialect> column, T value)
    {
        _setValues[column.Identifier] = value;
        return this;
    }
    
    public UpdateQuery<TTable,TDialect> Set(IUpdateRecord<TTable, TDialect> record)
    {
        record.Writer(_setValues);
        return this;
    }
    
    public UpdateQuery<TTable, TDialect> Set<T>(DbColumn<T, TTable, TDialect> column, ISql value)
    {
        _setValues[column.Identifier] = value;
        return this;
    }
    
    public UpdateQuery<TTable, TDialect> Set(Dictionary<IColumnOfTable<TTable, TDialect>, object> columnValuePairs)
    {
        foreach (var kv in columnValuePairs)
        {
            _setValues[kv.Key.Identifier] = kv.Value;
        }
        return this;
    }

    public UpdateQuery<TTable, TDialect> Where(IOperator condition)
    {
        _wheres.Add(condition);
        return this;
    }
    
    public UpdateQuery<TTable, TDialect> Where(params IOperator[] conditions)
    {
        _wheres.AddRange(conditions);
        return this;
    }

    public override string BuildSql(Dictionary<string, object?> parameters)
    {
        if (_setValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }
        
        var sb = new StringBuilder();
        sb.Append("UPDATE ");
        sb.Append(_table.Sql);
        sb.Append(" SET ");

        var setClauses = new List<string>();
        foreach (var kv in _setValues)
        {
            if (kv.Value is ISql op)
            {
                setClauses.Add($"{kv.Key} = ({op.BuildSql(parameters)})");
            }
            else
            {
                string paramName = $"@p{parameters.Count}";
                parameters.Add(paramName, kv.Value);
                setClauses.Add($"{kv.Key} = {paramName}");
            }
        }

        sb.Append(string.Join(", ", setClauses));

        var wheres = _wheres.Select(w => $"({w.BuildSql(parameters)})").ToList();
        if (wheres.Count > 0)
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", wheres));
        }

        return sb.ToString();
    }
    
    public ReturningQuery<TReturn, TDialect> Returning<TReturn>(ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new ReturningQuery<TReturn, TDialect>(
            this,
            selectedColumns
        );
    }
}
