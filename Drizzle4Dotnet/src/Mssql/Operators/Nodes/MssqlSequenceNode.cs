using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Operators.Nodes;

/// <summary>
/// Represents NEXT VALUE FOR [sequence_name].
/// MSSQL syntax: NEXT VALUE FOR [dbo].[my_sequence]
/// </summary>
public class MssqlSequenceNode : IGenericSql
{
    private readonly string _sequenceName;
    private readonly string? _schemaName;

    public MssqlSequenceNode(string sequenceName, string? schemaName = "dbo")
    {
        _sequenceName = sequenceName;
        _schemaName = schemaName;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("NEXT VALUE FOR ");
        sqlBuilder.Append(MssqlSqlDialectImpl.BuildTableName(_schemaName, _sequenceName));
    }
}
