using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Update;

public class UpdateQuery<TTable, TDialect> : Query<TDialect> where  TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly TTable _table;
    private readonly Dictionary<string, object?> _setValues = new();
    private readonly List<IGenericSql> _wheres = new();
    private readonly List<ICteTable<TDialect>> _cteTables = new List<ICteTable<TDialect>>();

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
    
    public UpdateQuery<TTable, TDialect> With(IVirtualTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable.AsCte());
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
    
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_setValues.Count == 0)
        {
            throw new InvalidOperationException("No columns set for update.");
        }
        
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

        sqlBuilder.Append("UPDATE ");
        _table.BuildRefSql(sqlBuilder);
        sqlBuilder.Append(" SET ");

        bool firstSet = true;
        foreach (var kv in _setValues)
        {
            if (!firstSet) sqlBuilder.Append(", ");
        
            sqlBuilder.Append(TDialect.BuildIdentifier(kv.Key));
            sqlBuilder.Append(" = ");

            if (kv.Value is IGenericSql op)
            {
                sqlBuilder.Append('(');
                op.BuildSql(sqlBuilder);
                sqlBuilder.Append(')');
            }
            else
            {
                sqlBuilder.Append(sqlBuilder.AddParameter(kv.Value));
            }
            firstSet = false;
        }

        AppendClause(sqlBuilder, " WHERE ", " AND ", _wheres, wrapInParentheses: true);
    }
}
