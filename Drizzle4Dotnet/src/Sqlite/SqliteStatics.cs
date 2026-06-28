using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Sqlite;

/// <summary>
/// Static utility class providing factory methods for SQLite-specific SQL expression patterns.
/// Analogous to PgSqlStatics in the PostgreSQL namespace.
/// </summary>
public static class SqliteStatics
{
    /// <summary>
    /// Returns the rowid of the last inserted row.
    /// Renders as: SELECT last_insert_rowid()
    /// </summary>
    public static ISql<long> LastInsertRowId() => new RawSql<long>("SELECT last_insert_rowid()");
    
    /// <summary>
    /// Returns the SQLite library version string.
    /// Renders as: SELECT sqlite_version()
    /// </summary>
    public static ISql<string> SqliteVersion() => new RawSql<string>("SELECT sqlite_version()");
    
    /// <summary>
    /// Returns the number of rows changed by the last statement.
    /// Renders as: SELECT changes()
    /// </summary>
    public static ISql<int> Changes() => new RawSql<int>("SELECT changes()");
}
