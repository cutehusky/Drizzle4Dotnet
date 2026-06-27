using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific SQL operators.
/// </summary>
public static class MySqlOperators
{
    // ======================================================================
    // Null-Safe Equality: <=> (MySQL equivalent of IS NOT DISTINCT FROM)
    // ======================================================================
    internal const string OpsNullSafeEqual = " <=> ";
    
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, OpsNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), OpsNullSafeEqual);
    
    
    // ======================================================================
    // Regular Expression Operators
    // ======================================================================
    internal const string OpsRegexp = " REGEXP ";
    internal const string OpsNotRegexp = " NOT REGEXP ";
    
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, OpsRegexp);
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), OpsRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, OpsNotRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), OpsNotRegexp);
}

/// <summary>
/// Extension methods for MySQL-specific operators.
/// </summary>
public static class MySqlOperatorsExtensions
{
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2>(
        this ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, MySqlOperators.OpsNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T>(
        this ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), MySqlOperators.OpsNullSafeEqual);
    
    public static BinaryNode<string, string, bool> Regexp(
        this ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), MySqlOperators.OpsRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(
        this ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), MySqlOperators.OpsNotRegexp);
}
