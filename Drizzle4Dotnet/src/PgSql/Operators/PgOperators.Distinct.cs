using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgOperators
{
    public static BinaryNode<T1, T2, bool> IsDistinctFrom<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, OpsIsDistinctFrom);
    public static BinaryNode<T1, T2, bool> IsNotDistinctFrom<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, OpsIsNotDistinctFrom);
    public static BinaryNode<T, T, bool> IsDistinctFrom<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), OpsIsDistinctFrom);
    public static BinaryNode<T, T, bool> IsNotDistinctFrom<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), OpsIsNotDistinctFrom);
}

public static class PgOperatorsDistinctExtensions
{
    public static BinaryNode<T1, T2, bool> IsDistinctFrom<T1, T2>(
        this ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, PgOperators.OpsIsDistinctFrom);
    public static BinaryNode<T1, T2, bool> IsNotDistinctFrom<T1, T2>(
        this ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, PgOperators.OpsIsNotDistinctFrom);
    public static BinaryNode<T, T, bool> IsDistinctFrom<T>(
        this ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), PgOperators.OpsIsDistinctFrom);
    public static BinaryNode<T, T, bool> IsNotDistinctFrom<T>(
        this ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), PgOperators.OpsIsNotDistinctFrom);
}
