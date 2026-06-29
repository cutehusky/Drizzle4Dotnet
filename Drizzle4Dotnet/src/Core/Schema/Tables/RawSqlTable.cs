using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Tables;

/// <summary>
/// Represents a raw SQL table expression for use in FROM/JOIN clauses.
/// Allows embedding arbitrary SQL as a table source with an alias.
/// Usage: new RawSqlTable&lt;PgSqlSqlDialectImpl&gt;("SELECT * FROM users WHERE is_active = true", "active_users")
/// </summary>
public class RawSqlTable<TDialect> : IGenericTable<TDialect>, IGetFieldByName
    where TDialect : ISqlDialect
{
    private readonly string _sql;
    private readonly string _alias;
    private readonly Dictionary<string, object?> _parameters;

    public RawSqlTable(string alias, string sql, Dictionary<string, object?> parameters)
    {
        _sql = sql;
        _alias = alias;
        _parameters = parameters;
    }

    public string Alias => _alias;

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append('(');
        AppendRawSql(sqlBuilder);
        sqlBuilder.Append(") AS ");
        sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
    }

    public void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        return new VirtualColumn<T, TDialect>(_alias, columnName);
    }

    public IAliasedSql<T> Field<T>(IAliasedSql<T> column)
    {
        return new VirtualColumn<T, TDialect>(_alias, column.Identifier);
    }

    private void AppendRawSql(ISqlBuilder sqlBuilder)
    {
        var sql = _sql;
        foreach (var kv in _parameters)
        {
            var paramName = sqlBuilder.AddParameter(kv.Value);
            sql = sql.Replace(kv.Key, paramName);
        }
        sqlBuilder.Append(sql);
    }
}


public class RawSqlTableAlias<TDialect> : IGenericTable<TDialect>, IGetFieldByName
    where TDialect : ISqlDialect
{
    private readonly string _alias;

    public RawSqlTableAlias(string alias)
    {
        _alias = alias;
    }

    public string Alias => _alias;

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
    }

    public void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        return new VirtualColumn<T, TDialect>(_alias, columnName);
    }

    public IAliasedSql<T> Field<T>(IAliasedSql<T> column)
    {
        return new VirtualColumn<T, TDialect>(_alias, column.Identifier);
    }
}

