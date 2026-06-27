using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Numeric & Math Functions: Abs, Ceil, Floor, Round, Power, Sqrt, Sign
// ======================================================================
public static partial class Functions
{
    public static FunctionCallNode<T, T> Abs<T>(ISql<T> c1) => new("ABS", c1);
    public static FunctionCallNode<T, T> Ceil<T>(ISql<T> c1) => new("CEIL", c1);
    public static FunctionCallNode<T, T> Floor<T>(ISql<T> c1) => new("FLOOR", c1);
    public static FunctionCallNode<T, T> Round<T>(ISql<T> c1) => new("ROUND", c1);
    
    // Round(col, decimals)
    public static FunctionCallNode<T, int, T> Round<T>(ISql<T> c1, int decimals) =>
        new("ROUND", c1, new SqlValueNode<int>(decimals));
    
    // Power(base, exponent)
    public static FunctionCallNode<T> Power<T>(ISql<T> c1, IGenericSql exponent) => new("POWER", c1, exponent);
    public static FunctionCallNode<T, T, T> Power<T>(ISql<T> c1, T exponent) where T : notnull =>
        new("POWER", c1, new SqlValueNode<T>(exponent));
    
    public static FunctionCallNode<T, T> Sqrt<T>(ISql<T> c1) => new("SQRT", c1);
    public static FunctionCallNode<T, int> Sign<T>(ISql<T> c1) => new("SIGN", c1);
}

// ======================================================================
// Extension methods for ISql — provide `column.Abs()` syntax
// ======================================================================
public static partial class FunctionsExtensions
{
    public static FunctionCallNode<T, T> Abs<T>(this ISql<T> c1) =>
        new("ABS", c1);
    public static FunctionCallNode<T, T> Ceil<T>(this ISql<T> c1) =>
        new("CEIL", c1);
    public static FunctionCallNode<T, T> Floor<T>(this ISql<T> c1) =>
        new("FLOOR", c1);
    public static FunctionCallNode<T, T> Round<T>(this ISql<T> c1) =>
        new("ROUND", c1);
    
    // Round(col, decimals)
    public static FunctionCallNode<T, int, T> Round<T>(this ISql<T> c1, int decimals) =>
        new("ROUND", c1, new SqlValueNode<int>(decimals));
    
    // Power(base, exponent)
    public static FunctionCallNode<T> Power<T>(this ISql<T> c1, IGenericSql exponent) =>
        new("POWER", c1, exponent);
    public static FunctionCallNode<T, T, T> Power<T>(this ISql<T> c1, T exponent) where T : notnull =>
        new("POWER", c1, new SqlValueNode<T>(exponent));
    
    public static FunctionCallNode<T, T> Sqrt<T>(this ISql<T> c1) =>
        new("SQRT", c1);
    
    // Sign(col) -> SIGN(col)
    public static FunctionCallNode<T, int> Sign<T>(this ISql<T> c1) =>
        new("SIGN", c1);
}
