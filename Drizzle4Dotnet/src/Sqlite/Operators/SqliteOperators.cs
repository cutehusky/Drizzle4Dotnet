using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Sqlite.Operators;

/// <summary>
/// SQLite-specific operators.
/// Most standard operators (comparison, arithmetic, logical, string) are already in
/// Core/Shared/Operators/ and work with SQLite unchanged.
/// SQLite-specific additions:
/// - || (string concat) — shared with PostgreSQL via Operators.String
/// - GLOB — Unix-style pattern matching
/// - MATCH — Full-text search (optional, requires FTS extension)
/// - REGEXP — Regular expression matching (optional, requires regex extension)
/// </summary>
public static class SqliteOperators
{
    /// <summary>
    /// SQLite GLOB pattern matching operator.
    /// Uses Unix-style wildcards: * matches any sequence, ? matches single character.
    /// Renders as: str GLOB pattern
    /// </summary>
    public static ISql<bool> Glob(ISql<string> str, ISql<string> pattern)
        => new RawSql<bool>("GLOB expression");
    
    /// <summary>
    /// SQLite MATCH operator for full-text search (requires FTS extension).
    /// Renders as: column MATCH pattern
    /// </summary>
    public static ISql<bool> Match(ISql<string> column, ISql<string> pattern)
        => new RawSql<bool>("MATCH expression");
    
    /// <summary>
    /// SQLite REGEXP operator (requires regex extension).
    /// Renders as: str REGEXP pattern
    /// </summary>
    public static ISql<bool> Regexp(ISql<string> str, ISql<string> pattern)
        => new RawSql<bool>("REGEXP expression");
}
