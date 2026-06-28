using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Mssql.Nodes;

namespace Drizzle4Dotnet.Mssql;

/// <summary>
/// Static utility class providing factory methods for MSSQL-specific SQL expression patterns.
/// Analogous to PgSqlStatics in the PostgreSQL namespace.
/// </summary>
public static class MssqlStatics
{
    /// <summary>
    /// Creates a TOP(n) expression for SELECT queries.
    /// Renders as: TOP (count)
    /// </summary>
    public static MssqlTopNode Top(int count) => new(count);

    /// <summary>
    /// Creates a TOP(n) WITH TIES expression.
    /// Renders as: TOP (count) WITH TIES
    /// Requires ORDER BY to determine ties.
    /// </summary>
    public static MssqlTopNode TopWithTies(int count) => new(count, withTies: true);

    /// <summary>
    /// Creates a TOP(n) PERCENT expression.
    /// Renders as: TOP (count) PERCENT
    /// </summary>
    public static MssqlTopNode TopPercent(int count) => new(count, percent: true);

    /// <summary>
    /// Creates an OUTPUT INSERTED column reference.
    /// Use in INSERT/UPDATE statements to return inserted values.
    /// Renders as: OUTPUT INSERTED.[column]
    /// </summary>
    public static MssqlOutputNode OutputInserted<T>(IAliasedSql<T> column)
        => new(column.Identifier, "INSERTED");

    /// <summary>
    /// Creates an OUTPUT DELETED column reference.
    /// Use in UPDATE/DELETE statements to return old/deleted values.
    /// Renders as: OUTPUT DELETED.[column]
    /// </summary>
    public static MssqlOutputNode OutputDeleted<T>(IAliasedSql<T> column)
        => new(column.Identifier, "DELETED");

    /// <summary>
    /// Creates a table hint: WITH (NOLOCK)
    /// Renders as: WITH (NOLOCK)
    /// </summary>
    public static MssqlTableHintNode NoLock()
        => new("NOLOCK");

    /// <summary>
    /// Creates a table hint with arbitrary hint text.
    /// Renders as: WITH (hint)
    /// </summary>
    public static MssqlTableHintNode TableHint(string hint)
        => new(hint);

    /// <summary>
    /// Creates a NEXT VALUE FOR sequence reference.
    /// Renders as: NEXT VALUE FOR [dbo].[sequence_name]
    /// </summary>
    public static MssqlSequenceNode NextValueFor(string sequenceName, string? schemaName = "dbo")
        => new(sequenceName, schemaName);

    /// <summary>
    /// Returns the last inserted identity value. Equivalent to LAST_INSERT_ID().
    /// Renders as: SELECT SCOPE_IDENTITY()
    /// </summary>
    public static ISql<long> ScopeIdentity() => new RawSql<long>("SELECT SCOPE_IDENTITY()");

    /// <summary>
    /// Returns @@ROWCOUNT (rows affected by last statement).
    /// Renders as: SELECT @@ROWCOUNT
    /// </summary>
    public static ISql<int> RowCount() => new RawSql<int>("SELECT @@ROWCOUNT");
}
