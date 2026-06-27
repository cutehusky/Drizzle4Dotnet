using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Conditional Expressions: Coalesce, NullIf, IIf
// ======================================================================
public static partial class Functions
{
    public static FunctionCallNode<T> Coalesce<T>(params IGenericSql[] columns) => new("COALESCE", columns);
    public static FunctionCallNode<T, T, T> Coalesce<T>(ISql<T> c1, ISql<T> c2) => new("COALESCE", c1, c2);
    public static FunctionCallNode<T, T, T> Coalesce<T>(ISql<T> c1, T defaultValue) =>
        new("COALESCE", c1, new SqlValueNode<T>(defaultValue));
    
    public static FunctionCallNode<T?> NullIf<T>(ISql<T> c1, IGenericSql value) => new("NULLIF", c1, value);
    public static FunctionCallNode<T, T, T?> NullIf<T>(ISql<T> c1, T value) =>
        new("NULLIF", c1, new SqlValueNode<T>(value));
    
    // IIf(condition, trueVal, falseVal) -> IIF(condition, trueVal, falseVal) (MySQL/SQLite)
    public static FunctionCallNode<T> IIf<T>(IGenericSql condition, IGenericSql trueVal, IGenericSql falseVal) =>
        new("IIF", condition, trueVal, falseVal);
}

// ======================================================================
// Extension methods for ISql — provide `column.Coalesce(defaultValue)` syntax
// ======================================================================
public static partial class FunctionsExtensions
{
    public static FunctionCallNode<T, T, T> Coalesce<T>(this ISql<T> c1, T defaultValue) =>
        new("COALESCE", c1, new SqlValueNode<T>(defaultValue));
    
    public static FunctionCallNode<T, T, T?> NullIf<T>(this ISql<T> c1, T value) =>
        new("NULLIF", c1, new SqlValueNode<T>(value));
}
