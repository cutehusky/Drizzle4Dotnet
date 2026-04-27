using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Columns;

public class DbColumn<T, TTable, TDialect>: IColumnOfTable<TTable>, IColumnOfDialect<T, TDialect> where TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly string _sql;
    private readonly string _identifier;
    public DbColumn(string columnName)
    {
        _sql = TDialect.BuildColumnName(TTable.TableRefName, columnName);
        _identifier = columnName;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder) => sqlBuilder.Append(_sql);
    
    public string Sql => _sql;

    public string Identifier => _identifier;
}
