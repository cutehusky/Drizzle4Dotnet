using System.Text;

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

public readonly struct AliasedSql: ISql
{
    private readonly string _alias;
    private readonly IGenericSql _sql;

    public AliasedSql(
        IGenericSql sql,
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


public readonly struct AliasedSql<T>: ISql<T>
{
    private readonly string _alias;
    private readonly IGenericSql _sql;

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
    public static AliasedSql As(this IGenericSql sql, string alias) 
        => new AliasedSql(sql, alias);
    
    public static AliasedSql As(this ISql sql, string alias) 
        => new AliasedSql(sql, alias);
    
    public static AliasedSql<T> As<T>(this ISql<T> sql, string alias) 
        => new AliasedSql<T>(sql, alias);
}
