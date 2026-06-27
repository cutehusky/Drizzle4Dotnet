using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;
using Drizzle4Dotnet.PgSql.Nodes;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgFunctions
{
    // ======================================================================
    // String Functions (PostgreSQL-specific syntax)
    // ======================================================================
    public static PositionNode Position(ISql<string> c1, string substring) =>
        new(new SqlValueNode<string>(substring), c1);
}

public static class PgFunctionsStringExtensions
{
    public static PositionNode Position(this ISql<string> c1, string substring) =>
        new(new SqlValueNode<string>(substring), c1);
}
