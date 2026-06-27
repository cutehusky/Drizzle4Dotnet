using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Collection Operators: In, NotIn, IsNull, IsNotNull, Exists
// ======================================================================
public static partial class Operators
{
    // --- IsNull / IsNotNull ---
    public static UnaryNode<T, bool> IsNull<T>(ISql<T> c1) => new(c1,  IsNullOp);
    public static UnaryNode<T, bool> IsNotNull<T>(ISql<T> c1) => new(c1,  IsNotNullOp);

    // --- In / NotIn (static) ---
    public static BinarySqlListValueNode<T, bool> In<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, InOp);
    public static BinarySqlListValueNode<T, bool> NotIn<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, NotInOp);
    public static BinaryNode<T, T, bool> In<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, InOp, true);
    public static BinaryNode<T, T, bool> NotIn<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, NotInOp, true);
    public static NnaryAnyNode<T, bool, TDialect> In<T, TDialect>(ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, InOp);
    public static NnaryAnyNode<T, bool, TDialect> NotIn<T, TDialect>(ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, NotInOp);

    // --- Exists ---
    public static UnaryNode<T, bool> Exists<T>(ISql<T> subquery) => new(subquery, ExistsOp, prefix: true);
}



public static partial class OperatorsExtensions
{
    // --- IsNull / IsNotNull ---
    public static UnaryNode<T, bool> IsNull<T>(this ISql<T> c1) => new(c1,  Operators.IsNullOp);
    public static UnaryNode<T, bool> IsNotNull<T>(this ISql<T> c1) => new(c1,  Operators.IsNotNullOp);

    // --- In / NotIn (static) ---
    public static BinarySqlListValueNode<T, bool> In<T>(this ISql<T> c1, IEnumerable<T> values) => new(c1, values, Operators.InOp);
    public static BinarySqlListValueNode<T, bool> NotIn<T>(this ISql<T> c1, IEnumerable<T> values) => new(c1, values, Operators.NotInOp);
    public static BinaryNode<T, T, bool> In<T>(this ISql<T> c1, ISql<T> c2) => new(c1, c2, Operators.InOp, true);
    public static BinaryNode<T, T, bool> NotIn<T>(this ISql<T> c1, ISql<T> c2) => new(c1, c2, Operators.NotInOp, true);
    public static NnaryAnyNode<T, bool, TDialect> In<T, TDialect>(this ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, Operators.InOp);
    public static NnaryAnyNode<T, bool, TDialect> NotIn<T, TDialect>(this ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, Operators.NotInOp);

    // --- Exists ---
    public static UnaryNode<T, bool> Exists<T>(this ISql<T> subquery) => new(subquery, Operators.ExistsOp, prefix: true);
}
