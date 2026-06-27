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
}

// ======================================================================
// Extension methods for ISql — provide `column.Like(value)` syntax
// ======================================================================
public static partial class OperatorsExtensions
{
    public static BinaryNode<string, string, bool> Like(this ISql<string> c1, string value) => new(c1, new SqlValueNode<string>(value), Operators.LikeOp);
    public static BinaryNode<string, string, bool> NotLike(this ISql<string> c1, string value) => new(c1, new SqlValueNode<string>(value), Operators.NotLikeOp);
    public static BinaryNode<string, string, bool> Like(this ISql<string> c1, ISql<string> value) => new(c1, value, Operators.LikeOp);
    public static BinaryNode<string, string, bool> NotLike(this ISql<string> c1, ISql<string> value) => new(c1, value, Operators.NotLikeOp);
    public static BinaryNode<string, string, string> Contains(this ISql<string> c1, string value) => new(c1, new SqlValueNode<string>($"%{value}%"), Operators.LikeOp);
    public static BinaryNode<string, string, bool> StartsWith(this ISql<string> c1, string value) => new(c1, new SqlValueNode<string>($"{value}%"), Operators.LikeOp);
    public static BinaryNode<string, string, bool> EndsWith(this ISql<string> c1, string value) => new(c1, new SqlValueNode<string>($"%{value}"), Operators.LikeOp);
    public static BinaryNode<string> Concat(this ISql<string> c1, ISql<string> c2) => new(c1, c2, Operators.ConcatOp);
    public static BinaryNode<string, string, string> Concat(this ISql<string> c1, string value) => new(c1, new SqlValueNode<string>(value), Operators.ConcatOp);
}
