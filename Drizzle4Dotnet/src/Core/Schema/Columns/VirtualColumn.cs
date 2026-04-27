using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;

public class VirtualColumn<T, TDialect>: IColumnOfDialect<T, TDialect> where TDialect : ISqlDialect
{
    private readonly string _sql;
    private readonly string _identifier;
    public VirtualColumn(string alias, string identifier)
    {
        _sql = TDialect.BuildColumnName(alias, identifier);
        _identifier = identifier;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder) => sqlBuilder.Append(_sql);
    
    public string Sql => _sql;

    public string Identifier => _identifier;
}