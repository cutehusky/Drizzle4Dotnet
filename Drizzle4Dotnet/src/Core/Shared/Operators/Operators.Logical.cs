using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Logical Operators: And, Or, Xor, Not
// ======================================================================
public static partial class Operators
{
    public static BinaryNode<bool> And(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, AndOp);
    public static BinaryNode<bool> Or(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, OrOp);
    public static BinaryNode<bool> Xor(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, XorOp);
    public static NnaryNode<bool, bool> And(params ISql<bool>[] conditions) => new(conditions, AndOp);
    public static NnaryNode<bool, bool> Or(params ISql<bool>[] conditions) => new(conditions, OrOp);
    public static NnaryNode<bool, bool> Xor(params ISql<bool>[] conditions) => new(conditions, XorOp);
    public static UnaryNode<bool> Not(ISql<bool> condition) => new(condition, NotOp, prefix: true);
}
