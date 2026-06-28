namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Interface for SQL data type representations.
/// Each dialect implements its own set of data types as readonly structs
/// with static readonly instances for common types.
/// </summary>
public interface ISqlDataType
{
    /// <summary>The SQL type name string (e.g., "BIGINT", "VARCHAR(255)").</summary>
    string Sql { get; }
}

// ============================================================================
// Custom / Raw Data Type
// ============================================================================

/// <summary>
/// A raw/custom SQL data type defined by an arbitrary string.
/// Used when a dialect-specific type is not available or for ad-hoc types.
/// </summary>
public readonly struct RawSqlDataType : ISqlDataType
{
    public string Sql { get; }

    public RawSqlDataType(string sql) => Sql = sql;
}
