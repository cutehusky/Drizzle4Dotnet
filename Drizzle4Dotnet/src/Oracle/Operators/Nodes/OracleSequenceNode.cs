using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Oracle.Nodes;

/// <summary>
/// Represents a sequence reference in Oracle.
/// Renders as: "schema"."sequence".NEXTVAL or "schema"."sequence".CURRVAL
/// </summary>
public class OracleSequenceNode : IGenericSql
{
    private readonly string _sequenceName;
    private readonly string? _schemaName;
    private readonly bool _isNextVal;

    public OracleSequenceNode(string sequenceName, string? schemaName, bool isNextVal)
    {
        _sequenceName = sequenceName;
        _schemaName = schemaName;
        _isNextVal = isNextVal;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(OracleSqlDialectImpl.BuildTableName(_schemaName, _sequenceName));
        sqlBuilder.Append(_isNextVal ? ".NEXTVAL" : ".CURRVAL");
    }
}
