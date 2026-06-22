using Drizzle4Dotnet.Core.Schema.Columns;
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
    const string _operatorNullSafeEqual = " <=> ";
    
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, _operatorNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), _operatorNullSafeEqual);
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2, TDialect>(
        this IColumnOfDialect<T1, TDialect> c1, IColumnOfDialect<T2, TDialect> c2)
        where TDialect : ISqlDialect
        => new(c1, c2, _operatorNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T, TDialect>(
        this IColumnOfDialect<T, TDialect> c1, T value)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<T>(value), _operatorNullSafeEqual);
    
    
    // ======================================================================
    // Regular Expression Operators
    // ======================================================================
    const string _operatorRegexp = " REGEXP ";
    const string _operatorNotRegexp = " NOT REGEXP ";
    
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, _operatorRegexp);
    public static BinaryNode<string, string, bool> Regexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), _operatorRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, ISql<string> pattern)
        => new(c1, pattern, _operatorNotRegexp);
    public static BinaryNode<string, string, bool> NotRegexp(ISql<string> c1, string pattern)
        => new(c1, new SqlValueNode<string>(pattern), _operatorNotRegexp);
    
    public static BinaryNode<string, string, bool> Regexp<TDialect>(
        this IColumnOfDialect<string, TDialect> c1, string pattern)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<string>(pattern), _operatorRegexp);
    public static BinaryNode<string, string, bool> NotRegexp<TDialect>(
        this IColumnOfDialect<string, TDialect> c1, string pattern)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<string>(pattern), _operatorNotRegexp);
}
