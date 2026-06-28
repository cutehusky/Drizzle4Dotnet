using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgOperators
{
    internal const string OpsIsDistinctFrom = " IS DISTINCT FROM ";
    internal const string OpsIsNotDistinctFrom = " IS NOT DISTINCT FROM ";
    internal const string OpsAll = " = ALL ";
    internal const string OpsAny = " = ANY ";
    internal const string OpsSome = " = SOME ";
}
