using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.PgSql.Nodes;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// Static utility class providing factory methods for PostgreSQL-specific SQL expression patterns.
/// Analogous to Sql in the shared namespace, but for PostgreSQL-specific constructs.
/// </summary>
public static class PgSqlStatics
{
    /// <summary>
    /// Creates a PostgreSQL INTERVAL literal: INTERVAL 'amount unit'
    /// Usage: PgSqlStatics.Interval(1, "day") → INTERVAL '1 day'
    /// </summary>
    public static PgIntervalNode Interval(int amount, string unit)
        => new(amount, unit);

    /// <summary>
    /// Creates a PostgreSQL INTERVAL literal with decimal amount.
    /// </summary>
    public static PgIntervalNode Interval(double amount, string unit)
        => new(amount, unit);

    /// <summary>
    /// Creates a PostgreSQL timezone name as a SQL string literal: 'UTC', 'Asia/Saigon', etc.
    /// Use with AtTimeZone: AtTimeZone(col, PgSqlStatics.TimeZone("UTC"))
    /// </summary>
    public static PgTimeZoneNode TimeZone(string timeZone) => new(timeZone);

    /// <summary>
    /// Creates an EXCLUDED column reference for use in ON CONFLICT DO UPDATE SET.
    /// Renders as: EXCLUDED."column_name"
    /// PostgreSQL syntax: INSERT ... ON CONFLICT DO UPDATE SET col = EXCLUDED.col
    /// Usage: PgSqlStatics.Excluded(table.Email)
    /// </summary>
    public static PgExcludedNode<T> Excluded<T>(IAliasedSql<T> column) 
        => new(column.Identifier);
}
