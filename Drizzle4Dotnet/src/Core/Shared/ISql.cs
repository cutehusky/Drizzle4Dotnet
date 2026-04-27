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
    
    public static AliasedSql<T> As<T>(this ISql<T> sql, string alias) 
        => new AliasedSql<T>(sql, alias);
}
