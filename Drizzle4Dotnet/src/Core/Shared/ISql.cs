using System.Text;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;



public interface ISqlBuilder
{
    public string AddParameter(object? value);
    public ISqlBuilder Append(string sql);
    public ISqlBuilder Append(char sql);
}

public struct SqlBuilder<TDialect>: ISqlBuilder where TDialect : ISqlDialect
{
    private readonly StringBuilder _sb = new StringBuilder();
    private readonly Dictionary<string, object?> _parameters = new Dictionary<string, object?>();

    public SqlBuilder()
    {
    }

    public string AddParameter(object? value)
    {
        var parameterName = TDialect.BuildParameterName(_parameters.Count);
        _parameters.Add(parameterName, value);
        return parameterName;
    }

    public ISqlBuilder Append(string sql)
    {
        _sb.Append(sql);
        return this;
    }

    public ISqlBuilder Append(char sql)
    {
        _sb.Append(sql);
        return this;
    }
    
    public (string, Dictionary<string, object?>) Build()
    {
        return (_sb.ToString(), _parameters);
    }
}

public interface IGenericSql
{
    void BuildSql(ISqlBuilder sqlBuilder);
}

public interface ISql<TReturn>: IGenericSql
{
}

public interface ISql: IGenericSql
{
}

public interface IAliasedSql<T>: ISql<T>
{
    string Identifier { get; }
}

public readonly struct AliasedSql<T>: IAliasedSql<T>
{
    private readonly string _alias;
    private readonly IGenericSql _sql;
    public string Identifier => _alias;

    public AliasedSql(
        ISql<T> sql,
        string alias
    )
    {
        _alias = alias;
        _sql = sql;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append('(');
        _sql.BuildSql(sqlBuilder);
        sqlBuilder.Append(") AS ").Append(_alias);
    }
}

public static class SqlExtensions {
    public static RawSubqueryTableSql<TDialect> AsSubQuery<TDialect>(this RawSql<TDialect> sql, string alias) where TDialect : ISqlDialect 
        => new RawSubqueryTableSql<TDialect>(sql, alias);
    
    public static AliasedSql<T> As<T>(this ISql<T> sql, string alias) 
        => new AliasedSql<T>(sql, alias);
}


public class RawSql<TDialect>: ISql where TDialect : ISqlDialect
{
    private readonly string _sql;
    private readonly Dictionary<string, object?> _parameters;
    public RawSql(string sql)
    {
        _sql = sql;
        _parameters = new Dictionary<string, object?>();
    }
    
    public RawSql(string sql, Dictionary<string, object?> parameters)
    {
        _sql = sql;
        _parameters = parameters;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        var sql = _sql;
        foreach (var kv in _parameters) {
            var parmName = sqlBuilder.AddParameter(kv.Value);
            sql = sql.Replace(kv.Key, parmName);
        }
        sqlBuilder.Append(sql);
    }
}

public class RawSql<TReturn, TDialect>: ISql<TReturn> where TDialect : ISqlDialect
{
    private readonly string _sql;
    private readonly Dictionary<string, object?> _parameters;
    public RawSql(string sql)
    {
        _sql = sql;
        _parameters = new Dictionary<string, object?>();
    }
    
    public RawSql(string sql, Dictionary<string, object?> parameters)
    {
        _sql = sql;
        _parameters = parameters;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        var sql = _sql;
        foreach (var kv in _parameters) {
            var parmName = sqlBuilder.AddParameter(kv.Value);
            sql = sql.Replace(kv.Key, parmName);
        }
        sqlBuilder.Append(sql);
    }
}


public class RawSubqueryTableSql<TDialect>: 
    IVirtualTable<TDialect>, ICteTable<TDialect>,
    IGetFieldByName
    where TDialect : ISqlDialect
{
    private readonly RawSql<TDialect> _sql;
    private readonly string _alias;
    private readonly bool _isCte = false;

    public RawSubqueryTableSql(
        RawSql<TDialect> sql,
        string alias,
        bool isCte = false
    )
    {
        _alias = alias;
        _sql = sql;
        _isCte = isCte;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_isCte) {
            sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
            sqlBuilder.Append(" AS (");
            _sql.BuildSql(sqlBuilder);
            sqlBuilder.Append(')');
            return;
        }
        
        sqlBuilder.Append('(');
        _sql.BuildSql(sqlBuilder);
        sqlBuilder.Append(") AS ").Append(_alias);
    }

    public void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        if (_isCte) {
            sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
            return;
        }
        BuildSql(sqlBuilder);
    }
    
    public RawSubqueryTableSql<TDialect> AsCte() {
        return new RawSubqueryTableSql<TDialect>(_sql, _alias, true);
    }
    
    public static IVirtualTable<TDialect> Create(IGenericSql baseQuery, string alias, object selectedColumns)
    {
        throw new NotImplementedException();
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        return new VirtualColumn<T,TDialect>(_alias, columnName);
    }

    public IAliasedSql<T> Field<T>(IAliasedSql<T> column)
    {
        return new VirtualColumn<T,TDialect>(_alias, column.Identifier);
    }
}