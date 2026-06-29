using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Operators.Nodes;

/// <summary>
/// Represents Oracle's RETURNING ... INTO clause.
/// Renders as: RETURNING col1, col2 INTO :outP0, :outP1
/// Used in INSERT/UPDATE/DELETE statements to return values from DML operations.
/// The output parameters are bound at execution time in OracleDbClient.
/// </summary>
public class OracleReturningNode : IGenericSql
{
    private readonly List<string> _columnNames;

    public OracleReturningNode(params string[] columnNames)
    {
        _columnNames = columnNames.ToList();
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_columnNames.Count == 0) return;

        sqlBuilder.Append(" RETURNING ");
        for (int i = 0; i < _columnNames.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(OracleSqlDialectImpl.BuildIdentifier(_columnNames[i]));
        }

        sqlBuilder.Append(" INTO ");
        for (int i = 0; i < _columnNames.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append($":outP{i}");
        }
    }
}
