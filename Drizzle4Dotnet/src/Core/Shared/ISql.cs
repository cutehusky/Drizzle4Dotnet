using System.Text;

namespace Drizzle4Dotnet.Core.Shared;

public interface IGenericSql
{
    string BuildSql(Dictionary<string, object?> parameters);

    void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb);
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

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"({_sql.BuildSql(parameters)}) AS {_alias}";
    }

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        sb.Append('(');
        _sql.BuildSql(parameters, sb);
        sb.Append(") AS ").Append(_alias);
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

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"({_sql.BuildSql(parameters)}) AS {_alias}";
    }

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        sb.Append('(');
        _sql.BuildSql(parameters, sb);
        sb.Append(") AS ").Append(_alias);
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
