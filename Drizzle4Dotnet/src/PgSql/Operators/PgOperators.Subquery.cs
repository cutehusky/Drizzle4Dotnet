using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgOperators
{
    public static BinaryNode<T, T, bool> All<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, OpsAll, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Any<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, OpsAny, wrapInParentheses: true);
    public static BinaryNode<T, T, bool> Some<T>(ISql<T> c1, ISql<T> subquery)
        => new(c1, subquery, OpsSome, wrapInParentheses: true);
}

public static class PgOperatorsSubqueryExtensions
{
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
