using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Update;

public class UpdateQuery<TTable, TDialect, TSelf> : Query<TDialect>
    where TSelf : UpdateQuery<TTable, TDialect, TSelf>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    protected readonly TTable _table;
    protected readonly Dictionary<string, object?> _setValues = new();
    protected readonly List<IGenericSql> _wheres = new();
    protected readonly List<ICteTable<TDialect>> _cteTables = new List<ICteTable<TDialect>>();

    public UpdateQuery(
        TTable table, 
        DbClient<TDialect> dbClient
    ): base(dbClient)
    {
        _table = table;
    }

    public TSelf Set<T>(DbColumn<T, TTable, TDialect> column, T value)
    {
        _setValues[column.Identifier] = value;
        return (TSelf)this;
    }
    
    public TSelf With(ICteTable<TDialect> cteTable)
    {
        _cteTables.Add(cteTable);
        return (TSelf)this;
    }
    
    public TSelf Set(IUpdateRecord<TTable, TDialect> record)
    {
        record.Writer(_setValues);
        return (TSelf)this;
    }
    
    public TSelf Set<T>(DbColumn<T, TTable, TDialect> column, ISql<T> value)
    {
        _setValues[column.Identifier] = value;
        return (TSelf)this;
    }
    
    public TSelf Set(Dictionary<IColumnOfTable<TTable>, object> columnValuePairs)
    {
        foreach (var kv in columnValuePairs)
        {
            _setValues[kv.Key.Identifier] = kv.Value;
        }
        return (TSelf)this;
    }

    public TSelf Where(IGenericSql condition)
    {
        _wheres.Add(condition);
        return (TSelf)this;
    }
    
    public TSelf Where(params IGenericSql[] conditions)
    {
        _wheres.AddRange(conditions);
        return (TSelf)this;
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
