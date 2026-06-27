using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Arithmetic Operators: Add, Sub, Mul, Div, Mod
// ======================================================================
public static partial class Operators
{
    // --- Static: column vs column ---
    public static BinaryNode<T> Add<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, AddOp);
    public static BinaryNode<T> Sub<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, SubOp);
    public static BinaryNode<T> Mul<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, MulOp);
    public static BinaryNode<T> Div<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, DivOp);
    public static BinaryNode<T> Mod<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, ModOp);

    // --- Static: column vs value ---
    public static BinaryNode<T> Add<T>(ISql<T> c1, T c2) => new(c1, new SqlValueNode<T>(c2), AddOp);
    public static BinaryNode<T> Sub<T>(ISql<T> c1, T c2) => new(c1, new SqlValueNode<T>(c2), SubOp);
    public static BinaryNode<T> Mul<T>(ISql<T> c1, T c2) => new(c1, new SqlValueNode<T>(c2), MulOp);
    public static BinaryNode<T> Div<T>(ISql<T> c1, T c2) => new(c1, new SqlValueNode<T>(c2), DivOp);
    public static BinaryNode<T> Mod<T>(ISql<T> c1, T c2) => new(c1, new SqlValueNode<T>(c2), ModOp);

    // --- Extension: column vs column ---
    public static BinaryNode<T> Add<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> c2) where TDialect : ISqlDialect => new(c1, c2, AddOp);
    public static BinaryNode<T> Sub<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> c2) where TDialect : ISqlDialect => new(c1, c2, SubOp);
    public static BinaryNode<T> Mul<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> c2) where TDialect : ISqlDialect => new(c1, c2, MulOp);
    public static BinaryNode<T> Div<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> c2) where TDialect : ISqlDialect => new(c1, c2, DivOp);
    public static BinaryNode<T> Mod<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, ISql<T> c2) where TDialect : ISqlDialect => new(c1, c2, ModOp);

    // --- Extension: column vs value ---
    public static BinaryNode<T> Add<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T c2) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(c2), AddOp);
    public static BinaryNode<T> Sub<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T c2) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(c2), SubOp);
    public static BinaryNode<T> Mul<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T c2) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(c2), MulOp);
    public static BinaryNode<T> Div<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T c2) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(c2), DivOp);
    public static BinaryNode<T> Mod<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T c2) where TDialect : ISqlDialect => new(c1, new SqlValueNode<T>(c2), ModOp);
}
