using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators;

// ======================================================================
// String Functions: Upper, Lower, Trim, LTrim, RTrim, Length, Substring, Replace
// ======================================================================
public static partial class Functions
{
    public static FunctionCallNode<T, string> Upper<T>(ISql<T> c1) => new("UPPER", c1);
    public static FunctionCallNode<T, string> Lower<T>(ISql<T> c1) => new("LOWER", c1);
    public static FunctionCallNode<T, string> Trim<T>(ISql<T> c1) => new("TRIM", c1);
    public static FunctionCallNode<T, string> LTrim<T>(ISql<T> c1) => new("LTRIM", c1);
    public static FunctionCallNode<T, string> RTrim<T>(ISql<T> c1) => new("RTRIM", c1);
    public static FunctionCallNode<T, long> Length<T>(ISql<T> c1) => new("LENGTH", c1);
    
    // Substring(col, start, length) -> SUBSTRING(col, start, length)
    public static FunctionCallNode<string> Substring(ISql<string> c1, IGenericSql start, IGenericSql length) =>
        new("SUBSTRING", c1, start, length);
    public static FunctionCallNode<string, int, int, string> Substring(ISql<string> c1, int start, int length)
        => new("SUBSTRING", c1, new SqlValueNode<int>(start), new SqlValueNode<int>(length));
    
    // Replace(col, from, to) -> REPLACE(col, from, to)
    public static FunctionCallNode<string> Replace(ISql<string> c1, IGenericSql from, IGenericSql to) =>
        new("REPLACE", c1, from, to);
    public static FunctionCallNode<string, string, string, string> Replace(ISql<string> c1, string from, string to)
        => new("REPLACE", c1, new SqlValueNode<string>(from), new SqlValueNode<string>(to));
}

// ======================================================================
// Extension methods for ISql — provide `column.Upper()` syntax
// ======================================================================
public static partial class FunctionsExtensions
{
    public static FunctionCallNode<T, string> Upper<T>(this ISql<T> c1) =>
        new("UPPER", c1);
    public static FunctionCallNode<T, string> Lower<T>(this ISql<T> c1) =>
        new("LOWER", c1);
    public static FunctionCallNode<T, string> Trim<T>(this ISql<T> c1) =>
        new("TRIM", c1);
    public static FunctionCallNode<T, string> LTrim<T>(this ISql<T> c1) =>
        new("LTRIM", c1);
    public static FunctionCallNode<T, string> RTrim<T>(this ISql<T> c1) =>
        new("RTRIM", c1);
    public static FunctionCallNode<T, long> Length<T>(this ISql<T> c1) =>
        new("LENGTH", c1);
    
    // Substring(col, start, length) -> SUBSTRING(col, start, length)
    public static FunctionCallNode<string> Substring(this ISql<string> c1,
        IGenericSql start, IGenericSql length) =>
        new("SUBSTRING", c1, start, length);
    public static FunctionCallNode<string, int, int, string> Substring(this ISql<string> c1, int start, int length) 
        => new("SUBSTRING", c1, new SqlValueNode<int>(start), new SqlValueNode<int>(length));
    
    // Replace(col, from, to) -> REPLACE(col, from, to)
    public static FunctionCallNode<string> Replace(this ISql<string> c1,
        IGenericSql from, IGenericSql to) =>
        new("REPLACE", c1, from, to);
    public static FunctionCallNode<string, string, string, string> Replace(this ISql<string> c1, string from, string to) 
        => new("REPLACE", c1, new SqlValueNode<string>(from), new SqlValueNode<string>(to));
}
