using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Operators.Nodes;

/// <summary>
/// Represents an OUTPUT clause column reference: INSERTED.column or DELETED.column
/// MSSQL syntax: OUTPUT INSERTED.[id], INSERTED.[name] or OUTPUT DELETED.[id]
/// </summary>
public class MssqlOutputNode : IGenericSql
{
    private readonly string _columnName;
    private readonly string _table; // "INSERTED" or "DELETED"

    public MssqlOutputNode(string columnName, string table)
    {
        _columnName = columnName;
        _table = table;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_table).Append('.');
        sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_columnName));
    }
}
