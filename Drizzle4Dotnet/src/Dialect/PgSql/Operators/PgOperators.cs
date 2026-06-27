using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific SQL operators.
/// </summary>
public static class PgOperators
{
    // ====== IS DISTINCT FROM / IS NOT DISTINCT FROM (Null-safe equality) ======
    const string _operatorIsDistinctFrom = " IS DISTINCT FROM ";
    const string _operatorIsNotDistinctFrom = " IS NOT DISTINCT FROM ";
    
    public static BinaryNode<T1, T2, bool> IsDistinctFrom<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, _operatorIsDistinctFrom);
    public static BinaryNode<T1, T2, bool> IsNotDistinctFrom<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, _operatorIsNotDistinctFrom);
    public static BinaryNode<T, T, bool> IsDistinctFrom<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), _operatorIsDistinctFrom);
    public static BinaryNode<T, T, bool> IsNotDistinctFrom<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), _operatorIsNotDistinctFrom);
    
    public static BinaryNode<T1, T2, bool> IsDistinctFrom<T1, T2, TDialect>(
        this IColumnOfDialect<T1, TDialect> c1, IColumnOfDialect<T2, TDialect> c2)
        where TDialect : ISqlDialect
        => new(c1, c2, _operatorIsDistinctFrom);
    public static BinaryNode<T1, T2, bool> IsNotDistinctFrom<T1, T2, TDialect>(
        this IColumnOfDialect<T1, TDialect> c1, IColumnOfDialect<T2, TDialect> c2)
        where TDialect : ISqlDialect
        => new(c1, c2, _operatorIsNotDistinctFrom);
    public static BinaryNode<T, T, bool> IsDistinctFrom<T, TDialect>(
        this IColumnOfDialect<T, TDialect> c1, T value)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<T>(value), _operatorIsDistinctFrom);
    public static BinaryNode<T, T, bool> IsNotDistinctFrom<T, TDialect>(
        this IColumnOfDialect<T, TDialect> c1, T value)
        where TDialect : ISqlDialect
        => new(c1, new SqlValueNode<T>(value), _operatorIsNotDistinctFrom);
    
    
    // ====== ALL / ANY / SOME (Subquery Quantifiers) ======
    const string _operatorAll = " = ALL ";
    const string _operatorAny = " = ANY ";
    const string _operatorSome = " = SOME ";
    
    public static BinaryNode<T, T, bool> All<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, _operatorAll, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Any<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, _operatorAny, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Some<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, _operatorSome, wrapInParentheses: true);
    
    public static BinaryNode<T, T, bool> All<T, TDialect>(
        this IColumnOfDialect<T, TDialect> c1, ISql<T> subquery)
        where TDialect : ISqlDialect
        => new(c1, subquery, _operatorAll, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Any<T, TDialect>(
        this IColumnOfDialect<T, TDialect> c1, ISql<T> subquery)
        where TDialect : ISqlDialect
        => new(c1, subquery, _operatorAny, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Some<T, TDialect>(
        this IColumnOfDialect<T, TDialect> c1, ISql<T> subquery)
        where TDialect : ISqlDialect
        => new(c1, subquery, _operatorSome, wrapInParentheses: true);
}
