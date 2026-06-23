using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

public static class Functions
{
    // ======================================================================
    // Aggregate Functions (existing)
    // ======================================================================
    
    public static FunctionCallNode<T, long> Count<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("COUNT", c1);
    public static FunctionCallNode<T, T> Distinct<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("DISTINCT", c1);
    public static FunctionCallNode<T, long> CountDistinct<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => Count(Distinct(c1));
    public static FunctionCallNode<T, T> Sum<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("SUM", c1);
    public static FunctionCallNode<T, T> Avg<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("AVG", c1);
    public static FunctionCallNode<T, T> Min<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("MIN", c1);
    public static FunctionCallNode<T, T> Max<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("MAX", c1);

    public static FunctionCallNode<T, double> StdDev<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("STDDEV", c1);
    public static FunctionCallNode<T, double> Variance<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("VARIANCE", c1);
    public static FunctionCallNode<T, double> VarSample<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("VAR_SAMP", c1);
    public static FunctionCallNode<T, double> VarPop<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("VAR_POP", c1);
    public static FunctionCallNode<T, double> StdDevSample<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("STDDEV_SAMP", c1);
    public static FunctionCallNode<T, double> StdDevPop<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("STDDEV_POP", c1);

    // Non-extension versions
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
    // 2.2: String Functions
    // ======================================================================
    
    public static FunctionCallNode<T, string> Upper<T>(ISql<T> c1) => new("UPPER", c1);
    public static FunctionCallNode<T, string> Upper<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("UPPER", c1);
    
    public static FunctionCallNode<T, string> Lower<T>(ISql<T> c1) => new("LOWER", c1);
    public static FunctionCallNode<T, string> Lower<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("LOWER", c1);
    
    public static FunctionCallNode<T, string> Trim<T>(ISql<T> c1) => new("TRIM", c1);
    public static FunctionCallNode<T, string> Trim<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("TRIM", c1);
    
    public static FunctionCallNode<T, string> LTrim<T>(ISql<T> c1) => new("LTRIM", c1);
    public static FunctionCallNode<T, string> LTrim<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("LTRIM", c1);
    
    public static FunctionCallNode<T, string> RTrim<T>(ISql<T> c1) => new("RTRIM", c1);
    public static FunctionCallNode<T, string> RTrim<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("RTRIM", c1);
    
    public static FunctionCallNode<T, long> Length<T>(ISql<T> c1) => new("LENGTH", c1);
    public static FunctionCallNode<T, long> Length<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("LENGTH", c1);
    
    // Substring(col, start, length) -> SUBSTRING(col, start, length)
    public static FunctionCallNode<string> Substring(ISql<string> c1, IGenericSql start, IGenericSql length) =>
        new("SUBSTRING", c1, start, length);
    public static FunctionCallNode<string, int, int, string> Substring(ISql<string> c1, int start, int length)
        => new("SUBSTRING", c1, new SqlValueNode<int>(start), new SqlValueNode<int>(length));
    public static FunctionCallNode<string> Substring<TDialect>(this IColumnOfDialect<string, TDialect> c1,
        IGenericSql start, IGenericSql length) where TDialect : ISqlDialect =>
        new("SUBSTRING", c1, start, length);
    public static FunctionCallNode<string, int, int, string> Substring<TDialect>(this IColumnOfDialect<string, TDialect> c1, int start, int length) 
        where TDialect : ISqlDialect
        => new("SUBSTRING", c1, new SqlValueNode<int>(start), new SqlValueNode<int>(length));
    
    // Replace(col, from, to) -> REPLACE(col, from, to)
    public static FunctionCallNode<string> Replace(ISql<string> c1, IGenericSql from, IGenericSql to) =>
        new("REPLACE", c1, from, to);
    public static FunctionCallNode<string, string, string, string> Replace(ISql<string> c1, string from, string to)
        => new("REPLACE", c1, new SqlValueNode<string>(from), new SqlValueNode<string>(to));
    public static FunctionCallNode<string> Replace<TDialect>(this IColumnOfDialect<string, TDialect> c1,
        IGenericSql from, IGenericSql to) where TDialect : ISqlDialect =>
        new("REPLACE", c1, from, to);
    public static FunctionCallNode<string, string, string, string> Replace<TDialect>(this IColumnOfDialect<string, TDialect> c1, string from, string to) 
        where TDialect : ISqlDialect
        => new("REPLACE", c1, new SqlValueNode<string>(from), new SqlValueNode<string>(to));
    
    // Position(substring IN col) -> POSITION(sub IN col)
    // Uses SQL-standard IN keyword syntax, handled by PgSql-specific PositionNode
    // For PostgreSQL dialect, use PgFunctions.Position()

    
    // ======================================================================
    // 2.3: Numeric & Math Functions
    // ======================================================================
    
    public static FunctionCallNode<T, T> Abs<T>(ISql<T> c1) => new("ABS", c1);
    public static FunctionCallNode<T, T> Abs<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("ABS", c1);
    
    public static FunctionCallNode<T, T> Ceil<T>(ISql<T> c1) => new("CEIL", c1);
    public static FunctionCallNode<T, T> Ceil<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("CEIL", c1);
    
    public static FunctionCallNode<T, T> Floor<T>(ISql<T> c1) => new("FLOOR", c1);
    public static FunctionCallNode<T, T> Floor<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("FLOOR", c1);
    
    public static FunctionCallNode<T, T> Round<T>(ISql<T> c1) => new("ROUND", c1);
    public static FunctionCallNode<T, T> Round<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("ROUND", c1);
    
    // Round(col, decimals)
    public static FunctionCallNode<T, int, T> Round<T>(ISql<T> c1, int decimals) =>
        new("ROUND", c1, new SqlValueNode<int>(decimals));
    public static FunctionCallNode<T, int, T> Round<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, int decimals)
        where TDialect : ISqlDialect =>
        new("ROUND", c1, new SqlValueNode<int>(decimals));
    
    // Power(base, exponent)
    public static FunctionCallNode<T> Power<T>(ISql<T> c1, IGenericSql exponent) => new("POWER", c1, exponent);
    public static FunctionCallNode<T, T, T> Power<T>(ISql<T> c1, T exponent) where T : notnull =>
        new("POWER", c1, new SqlValueNode<T>(exponent));
    public static FunctionCallNode<T> Power<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, IGenericSql exponent)
        where TDialect : ISqlDialect =>
        new("POWER", c1, exponent);
    public static FunctionCallNode<T, T, T> Power<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T exponent)
        where TDialect : ISqlDialect where T : notnull =>
        new("POWER", c1, new SqlValueNode<T>(exponent));
    
    public static FunctionCallNode<T, T> Sqrt<T>(ISql<T> c1) => new("SQRT", c1);
    public static FunctionCallNode<T, T> Sqrt<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("SQRT", c1);
    
    // Sign(col) -> SIGN(col) — returns -1, 0, or 1
    public static FunctionCallNode<T, int> Sign<T>(ISql<T> c1) => new("SIGN", c1);
    public static FunctionCallNode<T, int> Sign<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("SIGN", c1);
    
    // ======================================================================
    // 2.4: Date/Time Functions (Standard SQL)
    // ======================================================================
    
    // Now() -> NOW()
    public static FunctionCallNode<DateTime> Now() => new("NOW");
    public static FunctionCallNode<DateTime> CurrentTimestamp() => new("CURRENT_TIMESTAMP");
    public static FunctionCallNode<DateTime> CurrentDate() => new("CURRENT_DATE");
    
    // For PostgreSQL-specific date functions (Extract, DateTrunc, DateAdd, AtTimeZone, Age),
    // use PgFunctions from Drizzle4Dotnet.PgSql namespace.
    

    // ======================================================================
    // 2.5: Conditional Expressions
    // ======================================================================
    
    // Coalesce(col1, col2, ...) -> COALESCE(col1, col2, ...)
    public static FunctionCallNode<T> Coalesce<T>(params IGenericSql[] columns) => new("COALESCE", columns);
    public static FunctionCallNode<T, T, T> Coalesce<T>(ISql<T> c1, ISql<T> c2) => new("COALESCE", c1, c2);
    public static FunctionCallNode<T, T, T> Coalesce<T>(ISql<T> c1, T defaultValue) =>
        new("COALESCE", c1, new SqlValueNode<T>(defaultValue));
    public static FunctionCallNode<T, T, T> Coalesce<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T defaultValue)
        where TDialect : ISqlDialect =>
        new("COALESCE", c1, new SqlValueNode<T>(defaultValue));
    
    // NullIf(col, value) -> NULLIF(col, value)
    public static FunctionCallNode<T?> NullIf<T>(ISql<T> c1, IGenericSql value) => new("NULLIF", c1, value);
    public static FunctionCallNode<T, T, T?> NullIf<T>(ISql<T> c1, T value) =>
        new("NULLIF", c1, new SqlValueNode<T>(value));
    public static FunctionCallNode<T, T, T?> NullIf<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T value)
        where TDialect : ISqlDialect =>
        new("NULLIF", c1, new SqlValueNode<T>(value));
    
    // IIf(condition, trueVal, falseVal) -> IIF(condition, trueVal, falseVal) (MySQL/SQLite)
    public static FunctionCallNode<T> IIf<T>(IGenericSql condition, IGenericSql trueVal, IGenericSql falseVal) =>
        new("IIF", condition, trueVal, falseVal);
    
    // Case() is now available via the Case class in CaseNode.cs
    // Usage: Case.When(condition, result).When(condition2, result2).Else(defaultResult)
    

    // ======================================================================
    // 2.6: Type Casting
    // ======================================================================
    
    public static CastNode<T> Cast<T>(IGenericSql expression, string targetType) =>
        new(expression, targetType, usePostgresSyntax: false);
    // Note: PostgreSQL-specific CastPg has been moved to PgFunctions.CastPg
    
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

    // For PostgreSQL-specific functions (JSON, Array, Random, Position, etc.),
    // use PgFunctions from Drizzle4Dotnet.PgSql namespace.
}
