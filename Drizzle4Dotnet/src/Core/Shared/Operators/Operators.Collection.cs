using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Collection Operators: In, NotIn, IsNull, IsNotNull, Exists
// ======================================================================
public static partial class Operators
{
    // --- IsNull / IsNotNull ---
    public static UnaryNode<T, bool> IsNull<T>(ISql<T> c1) => new(c1, IsNullOp);
    public static UnaryNode<T, bool> IsNotNull<T>(ISql<T> c1) => new(c1, IsNotNullOp);
    public static UnaryNode<T, bool> IsNull<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) where TDialect : ISqlDialect => new(c1, IsNullOp);
    public static UnaryNode<T, bool> IsNotNull<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) where TDialect : ISqlDialect => new(c1, IsNotNullOp);

    // --- In / NotIn (static) ---
    public static BinarySqlListValueNode<T, bool> In<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, InOp);
    public static BinarySqlListValueNode<T, bool> NotIn<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, NotInOp);
    public static BinaryNode<T, T, bool> In<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, InOp, true);
    public static BinaryNode<T, T, bool> NotIn<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, NotInOp, true);
    public static NnaryAnyNode<T, bool, TDialect> In<T, TDialect>(ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, InOp);
    public static NnaryAnyNode<T, bool, TDialect> NotIn<T, TDialect>(ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, NotInOp);

    // --- In / NotIn (extension) ---
    public static BinarySqlListValueNode<T, bool> In<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, IEnumerable<T> values) where TDialect : ISqlDialect => new(c1, values, InOp);
    public static BinarySqlListValueNode<T, bool> NotIn<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, IEnumerable<T> values) where TDialect : ISqlDialect => new(c1, values, NotInOp);
    public static BinaryNode<T, T, bool> In<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> c2) where TDialect : ISqlDialect => new(c1, c2, InOp, true);
    public static BinaryNode<T, T, bool> NotIn<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> c2) where TDialect : ISqlDialect => new(c1, c2, NotInOp, true);
    public static NnaryAnyNode<T, bool, TDialect> In<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, InOp);
    public static NnaryAnyNode<T, bool, TDialect> NotIn<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, NotInOp);

    // --- Exists ---
    public static UnaryNode<T, bool> Exists<T>(ISql<T> subquery) => new(subquery, ExistsOp, prefix: true);
}
