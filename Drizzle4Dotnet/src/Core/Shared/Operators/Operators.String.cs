using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// String Operators: Like, NotLike, Contains, StartsWith, EndsWith, Concat
// ======================================================================
public static partial class Operators
{
    // --- Static methods ---
    public static BinaryNode<string, string, bool> Like(ISql<string> c1, string value) => new(c1, new SqlValueNode<string>(value), LikeOp);
    public static BinaryNode<string, string, bool> NotLike(ISql<string> c1, string value) => new(c1, new SqlValueNode<string>(value), NotLikeOp);
    public static BinaryNode<string, string, bool> Like(ISql<string> c1, ISql<string> value) => new(c1, value, LikeOp);
    public static BinaryNode<string, string, bool> NotLike(ISql<string> c1, ISql<string> value) => new(c1, value, NotLikeOp);
    public static BinaryNode<string, string, bool> Contains(ISql<string> c1, string value) => new(c1, new SqlValueNode<string>($"%{value}%"), LikeOp);
    public static BinaryNode<string, string, bool> StartsWith(ISql<string> c1, string value) => new(c1, new SqlValueNode<string>($"{value}%"), LikeOp);
    public static BinaryNode<string, string, bool> EndsWith(ISql<string> c1, string value) => new(c1, new SqlValueNode<string>($"%{value}"), LikeOp);
    public static BinaryNode<string> Concat(ISql<string> c1, ISql<string> c2) => new(c1, c2, ConcatOp);
    public static BinaryNode<string, string, string> Concat(ISql<string> c1, string value) => new(c1, new SqlValueNode<string>(value), ConcatOp);

    // --- Extension methods ---
    public static BinaryNode<string, string, bool> Like<TDialect>(this IColumnOfDialect<string, TDialect> c1, string value) where TDialect : ISqlDialect => new(c1, new SqlValueNode<string>(value), LikeOp);
    public static BinaryNode<string, string, bool> NotLike<TDialect>(this IColumnOfDialect<string, TDialect> c1, string value) where TDialect : ISqlDialect => new(c1, new SqlValueNode<string>(value), NotLikeOp);
    public static BinaryNode<string, string, bool> Like<TDialect>(this IColumnOfDialect<string, TDialect> c1, ISql<string> value) where TDialect : ISqlDialect => new(c1, value, LikeOp);
    public static BinaryNode<string, string, bool> NotLike<TDialect>(this IColumnOfDialect<string, TDialect> c1, ISql<string> value) where TDialect : ISqlDialect => new(c1, value, NotLikeOp);
    public static BinaryNode<string, string, string> Contains<TDialect>(this IColumnOfDialect<string, TDialect> c1, string value) where TDialect : ISqlDialect => new(c1, new SqlValueNode<string>($"%{value}%"), LikeOp);
    public static BinaryNode<string, string, bool> StartsWith<TDialect>(this IColumnOfDialect<string, TDialect> c1, string value) where TDialect : ISqlDialect => new(c1, new SqlValueNode<string>($"{value}%"), LikeOp);
    public static BinaryNode<string, string, bool> EndsWith<TDialect>(this IColumnOfDialect<string, TDialect> c1, string value) where TDialect : ISqlDialect => new(c1, new SqlValueNode<string>($"%{value}"), LikeOp);
    public static BinaryNode<string> Concat<TDialect>(this IColumnOfDialect<string, TDialect> c1, ISql<string> c2) where TDialect : ISqlDialect => new(c1, c2, ConcatOp);
    public static BinaryNode<string, string, string> Concat<TDialect>(this IColumnOfDialect<string, TDialect> c1, string value) where TDialect : ISqlDialect => new(c1, new SqlValueNode<string>(value), ConcatOp);
}
