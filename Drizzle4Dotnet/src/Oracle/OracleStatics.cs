using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Oracle.Operators.Nodes;

namespace Drizzle4Dotnet.Oracle;

/// <summary>
/// Static utility class providing factory methods for Oracle-specific SQL expression patterns.
/// Analogous to MssqlStatics in the MSSQL namespace.
/// </summary>
public static class OracleStatics
{
    /// <summary>
    /// Creates a sequence NEXTVAL expression.
    /// Renders as: "schema"."sequence".NEXTVAL
    /// </summary>
    public static OracleSequenceNode NextVal(string sequenceName, string? schemaName = null)
        => new(sequenceName, schemaName, isNextVal: true);

    /// <summary>
    /// Creates a sequence CURRVAL expression.
    /// Renders as: "schema"."sequence".CURRVAL
    /// </summary>
    public static OracleSequenceNode CurrVal(string sequenceName, string? schemaName = null)
        => new(sequenceName, schemaName, isNextVal: false);

    /// <summary>
    /// Creates a ROWID reference for the given column/table.
    /// Renders as: ROWID
    /// </summary>
    public static OracleRowIdNode RowId()
        => new();

    /// <summary>
    /// Creates a RETURNING ... INTO clause node.
    /// Used in INSERT/UPDATE/DELETE statements to return values from DML operations.
    /// </summary>
    public static OracleReturningNode ReturningInto(params string[] columnNames)
        => new(columnNames);

    /// <summary>
    /// Returns a DUAL table reference for scalar SELECT statements.
    /// Renders as: DUAL
    /// Oracle requires FROM DUAL for any SELECT that doesn't reference a table.
    /// </summary>
    public static IGenericSql Dual => new RawSql("DUAL");
}
