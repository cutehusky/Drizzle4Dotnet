namespace Drizzle4Dotnet.Core.Shared;

/// <summary>
/// Default implementations for ISqlDialect members.
/// Provides standard SQL behavior that dialects can reuse.
/// </summary>
public static class SqlDialectDefaults
{
    /// <summary>
    /// Builds a standard LIMIT/OFFSET clause directly to the SQL builder.
    /// Produces: LIMIT {limit} OFFSET {offset}, LIMIT {limit}, OFFSET {offset}, or nothing.
    /// </summary>
    public static void BuildLimitOffset(ISqlBuilder sqlBuilder, int? limit, int? offset)
    {
        if (limit.HasValue && offset.HasValue)
        {
            sqlBuilder.Append(" LIMIT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
        }
        else if (limit.HasValue)
        {
            sqlBuilder.Append(" LIMIT ");
            sqlBuilder.Append(sqlBuilder.AddParameter(limit.Value));
        }
        else if (offset.HasValue)
        {
            sqlBuilder.Append(" OFFSET ");
            sqlBuilder.Append(sqlBuilder.AddParameter(offset.Value));
        }
    }

    /// <summary>
    /// Default string escaping — replaces single quotes with two single quotes.
    /// </summary>
    public static string EscapeString(string value)
    {
        return value.Replace("'", "''");
    }
}
