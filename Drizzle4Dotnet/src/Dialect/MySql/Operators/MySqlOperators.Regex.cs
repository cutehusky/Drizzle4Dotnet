using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.MySql;

public static partial class MySqlOperators
{
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, OpsRegexp);
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), OpsRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, OpsNotRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), OpsNotRegexp);
}

public static class MySqlOperatorsRegexExtensions
{
    public static BinaryNode<string, string, bool> Regexp(
        this ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), MySqlOperators.OpsRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(
        this ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), MySqlOperators.OpsNotRegexp);
}
