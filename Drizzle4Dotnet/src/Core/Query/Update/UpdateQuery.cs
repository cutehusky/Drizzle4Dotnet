using System.Text;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Update;

public class UpdateQuery<TTable, TDialect> : Query<TDialect> where  TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly TTable _table;
    private readonly Dictionary<string, object?> _setValues = new();
    private readonly List<IGenericSql> _wheres = new();

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
    
    public UpdateQuery<TTable, TDialect> Set<T>(DbColumn<T, TTable, TDialect> column, ISql<T> value)
    {
        _setValues[column.Identifier] = value;
        return this;
    }
    
    public UpdateQuery<TTable, TDialect> Set(Dictionary<IColumnOfTable<TTable>, object> columnValuePairs)
    {
        foreach (var kv in columnValuePairs)
        {
            _setValues[kv.Key.Identifier] = kv.Value;
        }
        return this;
    }

    public UpdateQuery<TTable, TDialect> Where(IGenericSql condition)
    {
        _wheres.Add(condition);
        return this;
    }
    
    public UpdateQuery<TTable, TDialect> Where(params IGenericSql[] conditions)
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
        sb.Append(_table.BuildSql(parameters));
        sb.Append(" SET ");

        bool firstSet = true;
        foreach (var kv in _setValues)
        {
            if (!firstSet) sb.Append(", ");
        
            sb.Append(kv.Key);
            sb.Append(" = ");

            if (kv.Value is IGenericSql op)
            {
                sb.Append('(').Append(op.BuildSql(parameters)).Append(')');
            }
            else
            {
                string paramName = $"@p{parameters.Count}";
                parameters.Add(paramName, kv.Value);
                sb.Append(paramName);
            }
            firstSet = false;
        }

        if (_wheres.Count > 0)
        {
            sb.Append(" WHERE ");
            for (int i = 0; i < _wheres.Count; i++)
            {
                if (i > 0) sb.Append(" AND ");
            
                sb.Append('(');
                sb.Append(_wheres[i].BuildSql(parameters));
                sb.Append(')');
            }
        }

        return sb.ToString();
    }
}
