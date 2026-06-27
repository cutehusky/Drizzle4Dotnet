using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific SQL operators.
/// </summary>
public static class PgOperators
{
    // ====== IS DISTINCT FROM / IS NOT DISTINCT FROM (Null-safe equality) ======
    internal const string _operatorIsDistinctFrom = " IS DISTINCT FROM ";
    internal const string _operatorIsNotDistinctFrom = " IS NOT DISTINCT FROM ";
    
    public static BinaryNode<T1, T2, bool> IsDistinctFrom<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, _operatorIsDistinctFrom);
    public static BinaryNode<T1, T2, bool> IsNotDistinctFrom<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, _operatorIsNotDistinctFrom);
    public static BinaryNode<T, T, bool> IsDistinctFrom<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), _operatorIsDistinctFrom);
    public static BinaryNode<T, T, bool> IsNotDistinctFrom<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), _operatorIsNotDistinctFrom);
    
    
    // ====== ALL / ANY / SOME (Subquery Quantifiers) ======
    internal const string OpsAll = " = ALL ";
    internal const string OpsAny = " = ANY ";
    internal const string OpsSome = " = SOME ";
    
    public static BinaryNode<T, T, bool> All<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, OpsAll, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Any<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, OpsAny, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Some<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, OpsSome, wrapInParentheses: true);
}

/// <summary>
/// Extension methods for PostgreSQL-specific operators.
/// </summary>
public static class PgOperatorsExtensions
{
    public static BinaryNode<T1, T2, bool> IsDistinctFrom<T1, T2>(
        this ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, PgOperators._operatorIsDistinctFrom);
    public static BinaryNode<T1, T2, bool> IsNotDistinctFrom<T1, T2>(
        this ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, PgOperators._operatorIsNotDistinctFrom);
    public static BinaryNode<T, T, bool> IsDistinctFrom<T>(
        this ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), PgOperators._operatorIsDistinctFrom);
    public static BinaryNode<T, T, bool> IsNotDistinctFrom<T>(
        this ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), PgOperators._operatorIsNotDistinctFrom);

    public static BinaryNode<T, T, bool> All<T>(
        this ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, PgOperators.OpsAll, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Any<T>(
        this ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, PgOperators.OpsAny, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Some<T>(
        this ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, PgOperators.OpsSome, wrapInParentheses: true);
}
