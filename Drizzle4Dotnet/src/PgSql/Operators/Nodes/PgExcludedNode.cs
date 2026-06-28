using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql.Nodes;

/// <summary>
/// Represents an EXCLUDED column reference for PostgreSQL ON CONFLICT DO UPDATE SET.
/// Renders as: EXCLUDED."column_name"
/// Usage: PgSqlStatics.Excluded(table.Email)
/// </summary>
public class PgExcludedNode<T> : ISql<T>
{
    private readonly string _identifier;

    public PgExcludedNode(string identifier)
    {
        _identifier = identifier;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("EXCLUDED.");
        sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(_identifier));
    }
}
