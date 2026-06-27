using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Comparison Operators: Eq, Ne, Lt, Gt, Ltq, Gtq
// ======================================================================
public static partial class Operators
{
    // --- Static methods: column vs value ---
    public static BinaryNode<T, T, bool> Eq<T>(ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), EqOp);
    public static BinaryNode<T, T, bool> Lt<T>(ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), LtOp);
    public static BinaryNode<T, T, bool> Gt<T>(ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), GtOp);
    public static BinaryNode<T, T, bool> Ltq<T>(ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), LtEqOp);
    public static BinaryNode<T, T, bool> Gtq<T>(ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), GtEqOp);
    public static BinaryNode<T, T, bool> Ne<T>(ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), NeOp);

    // --- Static methods: column vs column ---
    public static BinaryNode<T1, T2, bool> Eq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, EqOp);
    public static BinaryNode<T1, T2, bool> Lt<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, LtOp);
    public static BinaryNode<T1, T2, bool> Gt<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, GtOp);
    public static BinaryNode<T1, T2, bool> Ltq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, LtEqOp);
    public static BinaryNode<T1, T2, bool> Gtq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, GtEqOp);
    public static BinaryNode<T1, T2, bool> Ne<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, NeOp);
}

// ======================================================================
// Extension methods for ISql — provide `column.Eq(value)` / `column.Eq(otherColumn)` syntax
// ======================================================================
public static partial class OperatorsExtensions
{
    // --- Extension methods: column.Eq(value) ---
    public static BinaryNode<T, T, bool> Eq<T>(this ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), Operators.EqOp);
    public static BinaryNode<T, T, bool> Lt<T>(this ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), Operators.LtOp);
    public static BinaryNode<T, T, bool> Gt<T>(this ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), Operators.GtOp);
    public static BinaryNode<T, T, bool> Ltq<T>(this ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), Operators.LtEqOp);
    public static BinaryNode<T, T, bool> Gtq<T>(this ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), Operators.GtEqOp);
    public static BinaryNode<T, T, bool> Ne<T>(this ISql<T> c1, T value) => new(c1, new SqlValueNode<T>(value), Operators.NeOp);

    // --- Extension methods: column.Eq(otherColumn) ---
    public static BinaryNode<T1, T2, bool> Eq<T1, T2>(this ISql<T1> c1, ISql<T2> c2) => new(c1, c2, Operators.EqOp);
    public static BinaryNode<T1, T2, bool> Lt<T1, T2>(this ISql<T1> c1, ISql<T2> c2) => new(c1, c2, Operators.LtOp);
    public static BinaryNode<T1, T2, bool> Gt<T1, T2>(this ISql<T1> c1, ISql<T2> c2) => new(c1, c2, Operators.GtOp);
    public static BinaryNode<T1, T2, bool> Ltq<T1, T2>(this ISql<T1> c1, ISql<T2> c2) => new(c1, c2, Operators.LtEqOp);
    public static BinaryNode<T1, T2, bool> Gtq<T1, T2>(this ISql<T1> c1, ISql<T2> c2) => new(c1, c2, Operators.GtEqOp);
    public static BinaryNode<T1, T2, bool> Ne<T1, T2>(this ISql<T1> c1, ISql<T2> c2) => new(c1, c2, Operators.NeOp);
}
