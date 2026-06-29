using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators;

// ======================================================================
// Range Operators: Between, NotBetween
// ======================================================================
public static partial class Operators
{
    // --- Static: Between ---
    public static TrinaryNode<T, bool> Between<T>(ISql<T> c1, ISql<T> lower, ISql<T> upper) => new(c1, lower, upper, BetweenOp, AndOp);
    public static TrinaryNode<T, bool> Between<T>(ISql<T> c1, T lower, ISql<T> upper) => new(c1, new SqlValueNode<T>(lower), upper, BetweenOp, AndOp);
    public static TrinaryNode<T, bool> Between<T>(ISql<T> c1, ISql<T> lower, T upper) => new(c1, lower, new SqlValueNode<T>(upper), BetweenOp, AndOp);
    public static TrinaryNode<T, bool> Between<T>(ISql<T> c1, T lower, T upper) => new(c1, new SqlValueNode<T>(lower), new SqlValueNode<T>(upper), BetweenOp, AndOp);

    // --- Static: NotBetween ---
    public static TrinaryNode<T, bool> NotBetween<T>(ISql<T> c1, ISql<T> lower, ISql<T> upper) => new(c1, lower, upper, NotBetweenOp, AndOp);
    public static TrinaryNode<T, bool> NotBetween<T>(ISql<T> c1, T lower, ISql<T> upper) => new(c1, new SqlValueNode<T>(lower), upper, NotBetweenOp, AndOp);
    public static TrinaryNode<T, bool> NotBetween<T>(ISql<T> c1, ISql<T> lower, T upper) => new(c1, lower, new SqlValueNode<T>(upper), NotBetweenOp, AndOp);
    public static TrinaryNode<T, bool> NotBetween<T>(ISql<T> c1, T lower, T upper) => new(c1, new SqlValueNode<T>(lower), new SqlValueNode<T>(upper), NotBetweenOp, AndOp);
}

// ======================================================================
// Extension methods for ISql — provide `column.Between(lower, upper)` syntax
// ======================================================================
public static partial class OperatorsExtensions
{
    // --- Extension: Between ---
    public static TrinaryNode<T, bool> Between<T>(this ISql<T> c1, ISql<T> lower, ISql<T> upper) => new(c1, lower, upper, Operators.BetweenOp, Operators.AndOp);
    public static TrinaryNode<T, bool> Between<T>(this ISql<T> c1, T lower, ISql<T> upper) => new(c1, new SqlValueNode<T>(lower), upper, Operators.BetweenOp, Operators.AndOp);
    public static TrinaryNode<T, bool> Between<T>(this ISql<T> c1, ISql<T> lower, T upper) => new(c1, lower, new SqlValueNode<T>(upper), Operators.BetweenOp, Operators.AndOp);
    public static TrinaryNode<T, bool> Between<T>(this ISql<T> c1, T lower, T upper) => new(c1, new SqlValueNode<T>(lower), new SqlValueNode<T>(upper), Operators.BetweenOp, Operators.AndOp);

    // --- Extension: NotBetween ---
    public static TrinaryNode<T, bool> NotBetween<T>(this ISql<T> c1, ISql<T> lower, ISql<T> upper) => new(c1, lower, upper, Operators.NotBetweenOp, Operators.AndOp);
    public static TrinaryNode<T, bool> NotBetween<T>(this ISql<T> c1, T lower, ISql<T> upper) => new(c1, new SqlValueNode<T>(lower), upper, Operators.NotBetweenOp, Operators.AndOp);
    public static TrinaryNode<T, bool> NotBetween<T>(this ISql<T> c1, ISql<T> lower, T upper) => new(c1, lower, new SqlValueNode<T>(upper), Operators.NotBetweenOp, Operators.AndOp);
    public static TrinaryNode<T, bool> NotBetween<T>(this ISql<T> c1, T lower, T upper) => new(c1, new SqlValueNode<T>(lower), new SqlValueNode<T>(upper), Operators.NotBetweenOp, Operators.AndOp);
}
