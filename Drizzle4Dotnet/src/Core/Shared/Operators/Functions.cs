using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

public static class Functions
{
    // ======================================================================
    // Aggregate Functions
    // ======================================================================
    
    public static FunctionCallNode<T, long> Count<T>(ISql<T> c1) => new("COUNT", c1);
    public static FunctionCallNode<T, T> Distinct<T>(ISql<T> c1) => new("DISTINCT", c1);
    public static FunctionCallNode<T, long> CountDistinct<T>(ISql<T> c1) 
        => Count(Distinct(c1));
    public static FunctionCallNode<T, T> Sum<T>(ISql<T> c1) => new("SUM", c1);
    public static FunctionCallNode<T, T> Avg<T>(ISql<T> c1) => new("AVG", c1);
    public static FunctionCallNode<T, T> Min<T>(ISql<T> c1) => new("MIN", c1);
    public static FunctionCallNode<T, T> Max<T>(ISql<T> c1) => new("MAX", c1);

    public static FunctionCallNode<T, double> StdDev<T>(ISql<T> c1) => new("STDDEV", c1);
    public static FunctionCallNode<T, double> Variance<T>(ISql<T> c1) => new("VARIANCE", c1);
    public static FunctionCallNode<T, double> VarSample<T>(ISql<T> c1) => new("VAR_SAMP", c1);
    public static FunctionCallNode<T, double> VarPop<T>(ISql<T> c1) => new("VAR_POP", c1);
    public static FunctionCallNode<T, double> StdDevSample<T>(ISql<T> c1) => new("STDDEV_SAMP", c1);
    public static FunctionCallNode<T, double> StdDevPop<T>(ISql<T> c1) => new("STDDEV_POP", c1);

    
    // ======================================================================
    // String Functions
    // ======================================================================
    
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
    

    // ======================================================================
    // Numeric & Math Functions
    // ======================================================================
    
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
    
    // ======================================================================
    // Date/Time Functions (Standard SQL)
    // ======================================================================
    
    public static FunctionCallNode<DateTime> Now() => new("NOW");
    public static FunctionCallNode<DateTime> CurrentTimestamp() => new("CURRENT_TIMESTAMP");
    public static FunctionCallNode<DateTime> CurrentDate() => new("CURRENT_DATE");
    

    // ======================================================================
    // Conditional Expressions
    // ======================================================================
    
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
    

    // ======================================================================
    // Type Casting
    // ======================================================================
    
    public static CastNode<T> Cast<T>(IGenericSql expression, string targetType) =>
        new(expression, targetType, usePostgresSyntax: false);
    
    public static CastNode<string> CastToString(IGenericSql expression) =>
        new(expression, "TEXT", usePostgresSyntax: false);
    public static CastNode<int> CastToInt(IGenericSql expression) =>
        new(expression, "INTEGER", usePostgresSyntax: false);
    public static CastNode<long> CastToLong(IGenericSql expression) =>
        new(expression, "BIGINT", usePostgresSyntax: false);
    public static CastNode<double> CastToDouble(IGenericSql expression) =>
        new(expression, "DOUBLE PRECISION", usePostgresSyntax: false);
    public static CastNode<DateTime> CastToDateTime(IGenericSql expression) =>
        new(expression, "TIMESTAMP", usePostgresSyntax: false);
}

// ======================================================================
// Extension methods for ISql — provide `column.Count()` syntax
// ======================================================================
public static class FunctionsExtensions
{
    // --- Aggregate Functions ---
    public static FunctionCallNode<T, long> Count<T>(this ISql<T> c1) =>
        new("COUNT", c1);
    public static FunctionCallNode<T, T> Distinct<T>(this ISql<T> c1) =>
        new("DISTINCT", c1);
    public static FunctionCallNode<T, long> CountDistinct<T>(this ISql<T> c1)
        => Functions.Count(Functions.Distinct(c1));
    public static FunctionCallNode<T, T> Sum<T>(this ISql<T> c1) =>
        new("SUM", c1);
    public static FunctionCallNode<T, T> Avg<T>(this ISql<T> c1) =>
        new("AVG", c1);
    public static FunctionCallNode<T, T> Min<T>(this ISql<T> c1) =>
        new("MIN", c1);
    public static FunctionCallNode<T, T> Max<T>(this ISql<T> c1) =>
        new("MAX", c1);

    public static FunctionCallNode<T, double> StdDev<T>(this ISql<T> c1) =>
        new("STDDEV", c1);
    public static FunctionCallNode<T, double> Variance<T>(this ISql<T> c1) =>
        new("VARIANCE", c1);
    public static FunctionCallNode<T, double> VarSample<T>(this ISql<T> c1) =>
        new("VAR_SAMP", c1);
    public static FunctionCallNode<T, double> VarPop<T>(this ISql<T> c1) =>
        new("VAR_POP", c1);
    public static FunctionCallNode<T, double> StdDevSample<T>(this ISql<T> c1) =>
        new("STDDEV_SAMP", c1);
    public static FunctionCallNode<T, double> StdDevPop<T>(this ISql<T> c1) =>
        new("STDDEV_POP", c1);

    // --- String Functions ---
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

    // --- Numeric & Math Functions ---
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

    // --- Conditional Expressions ---
    public static FunctionCallNode<T, T, T> Coalesce<T>(this ISql<T> c1, T defaultValue) =>
        new("COALESCE", c1, new SqlValueNode<T>(defaultValue));
    
    public static FunctionCallNode<T, T, T?> NullIf<T>(this ISql<T> c1, T value) =>
        new("NULLIF", c1, new SqlValueNode<T>(value));
}
