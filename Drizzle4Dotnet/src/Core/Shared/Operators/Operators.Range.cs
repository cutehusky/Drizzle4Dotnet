using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

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

    // --- Extension: Between ---
    public static TrinaryNode<T, bool> Between<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> lower, ISql<T> upper) where TDialect : ISqlDialect => new(c1, lower, upper, BetweenOp, AndOp);
    public static TrinaryNode<T, bool> Between<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T lower, ISql<T> upper) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(lower), upper, BetweenOp, AndOp);
    public static TrinaryNode<T, bool> Between<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> lower, T upper) where TDialect : ISqlDialect => new(c1, lower, new SqlValueNode<T>(upper), BetweenOp, AndOp);
    public static TrinaryNode<T, bool> Between<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T lower, T upper) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(lower), new SqlValueNode<T>(upper), BetweenOp, AndOp);

    // --- Extension: NotBetween ---
    public static TrinaryNode<T, bool> NotBetween<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> lower, ISql<T> upper) where TDialect : ISqlDialect => new(c1, lower, upper, NotBetweenOp, AndOp);
    public static TrinaryNode<T, bool> NotBetween<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T lower, ISql<T> upper) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(lower), upper, NotBetweenOp, AndOp);
    public static TrinaryNode<T, bool> NotBetween<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> lower, T upper) where TDialect : ISqlDialect => new(c1, lower, new SqlValueNode<T>(upper), NotBetweenOp, AndOp);
    public static TrinaryNode<T, bool> NotBetween<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T lower, T upper) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(lower), new SqlValueNode<T>(upper), NotBetweenOp, AndOp);
}
