using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

public static class Functions
{
    // ======================================================================
    // Aggregate Functions (existing)
    // ======================================================================
    
    public static UnaryNode<T, long> Count<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "COUNT", true);
    public static UnaryNode<T> Distinct<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "DISTINCT", true);
    public static UnaryNode<T, long> CountDistinct<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => Count(Distinct(c1));
    public static UnaryNode<T> Sum<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "SUM", true);
    public static UnaryNode<T> Avg<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "AVG", true);
    public static UnaryNode<T> Min<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "MIN", true);
    public static UnaryNode<T> Max<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "MAX", true);

    public static UnaryNode<T, double> StdDev<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "STDDEV", true);
    public static UnaryNode<T, double> Variance<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "VARIANCE", true);
    public static UnaryNode<T, double> VarSample<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "VAR_SAMP", true);
    public static UnaryNode<T, double> VarPop<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "VAR_POP", true);
    public static UnaryNode<T, double> StdDevSample<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "STDDEV_SAMP", true);
    public static UnaryNode<T, double> StdDevPop<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
         where TDialect : ISqlDialect 
        => new(c1, "STDDEV_POP", true);

    // Non-extension versions
    public static UnaryNode<T, long> Count<T>(ISql<T> c1) 
        => new(c1, "COUNT", true);
    public static UnaryNode<T> Distinct<T>(ISql<T> c1) 
        => new(c1, "DISTINCT", true);
    public static UnaryNode<T, long> CountDistinct<T>(ISql<T> c1) 
        => Count(Distinct(c1));
    public static UnaryNode<T> Sum<T>(ISql<T> c1) 
        => new(c1, "SUM", true);
    public static UnaryNode<T> Avg<T>(ISql<T> c1) 
        => new(c1, "AVG", true);
    public static UnaryNode<T> Min<T>(ISql<T> c1) 
        => new(c1, "MIN", true);
    public static UnaryNode<T> Max<T>(ISql<T> c1) 
        => new(c1, "MAX", true);

    public static UnaryNode<T, double> StdDev<T>(ISql<T> c1) 
        => new(c1, "STDDEV", true);
    public static UnaryNode<T, double> Variance<T>(ISql<T> c1) 
        => new(c1, "VARIANCE", true);
    public static UnaryNode<T, double> VarSample<T>(ISql<T> c1) 
        => new(c1, "VAR_SAMP", true);
    public static UnaryNode<T, double> VarPop<T>(ISql<T> c1) 
        => new(c1, "VAR_POP", true);
    public static UnaryNode<T, double> StdDevSample<T>(ISql<T> c1) 
        => new(c1, "STDDEV_SAMP", true);
    public static UnaryNode<T, double> StdDevPop<T>(ISql<T> c1) 
        => new(c1, "STDDEV_POP", true);

    
    // ======================================================================
    // 2.2: String Functions
    // ======================================================================
    
    public static UnaryNode<T, string> Upper<T>(ISql<T> c1) 
        => new(c1, "UPPER", true);
    public static UnaryNode<T, string> Upper<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "UPPER", true);
    
    public static UnaryNode<T, string> Lower<T>(ISql<T> c1) 
        => new(c1, "LOWER", true);
    public static UnaryNode<T, string> Lower<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "LOWER", true);
    
    public static UnaryNode<T, string> Trim<T>(ISql<T> c1) 
        => new(c1, "TRIM", true);
    public static UnaryNode<T, string> Trim<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "TRIM", true);
    
    public static UnaryNode<T, string> LTrim<T>(ISql<T> c1) 
        => new(c1, "LTRIM", true);
    public static UnaryNode<T, string> LTrim<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "LTRIM", true);
    
    public static UnaryNode<T, string> RTrim<T>(ISql<T> c1) 
        => new(c1, "RTRIM", true);
    public static UnaryNode<T, string> RTrim<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "RTRIM", true);
    
    public static UnaryNode<T, long> Length<T>(ISql<T> c1) 
        => new(c1, "LENGTH", true);
    public static UnaryNode<T, long> Length<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "LENGTH", true);
    
    // Substring(col, start, length) -> SUBSTRING(col, start, length)
    public static FunctionCallNode<string> Substring(ISql<string> c1, IGenericSql start, IGenericSql length)
        => new FunctionCallNode<string>("SUBSTRING", c1, start, length);
    public static FunctionCallNode<string> Substring(ISql<string> c1, int start, int length)
        => new FunctionCallNode<string>("SUBSTRING", c1, new SqlValueNode<int>(start), new SqlValueNode<int>(length));
    public static FunctionCallNode<string> Substring<TDialect>(this IColumnOfDialect<string, TDialect> c1, IGenericSql start, IGenericSql length) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<string>("SUBSTRING", c1, start, length);
    public static FunctionCallNode<string> Substring<TDialect>(this IColumnOfDialect<string, TDialect> c1, int start, int length) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<string>("SUBSTRING", c1, new SqlValueNode<int>(start), new SqlValueNode<int>(length));
    
    // Replace(col, from, to) -> REPLACE(col, from, to)
    public static FunctionCallNode<string> Replace(ISql<string> c1, IGenericSql from, IGenericSql to)
        => new FunctionCallNode<string>("REPLACE", c1, from, to);
    public static FunctionCallNode<string> Replace(ISql<string> c1, string from, string to)
        => new FunctionCallNode<string>("REPLACE", c1, new SqlValueNode<string>(from), new SqlValueNode<string>(to));
    public static FunctionCallNode<string> Replace<TDialect>(this IColumnOfDialect<string, TDialect> c1, IGenericSql from, IGenericSql to) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<string>("REPLACE", c1, from, to);
    public static FunctionCallNode<string> Replace<TDialect>(this IColumnOfDialect<string, TDialect> c1, string from, string to) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<string>("REPLACE", c1, new SqlValueNode<string>(from), new SqlValueNode<string>(to));
    
    // Position(substring IN col) -> POSITION(sub IN col)
    public static FunctionCallNode<long> Position(IGenericSql substring, ISql<string> c1)
        => new FunctionCallNode<long>("POSITION", substring, new SqlRawNode<string>("IN "), c1);
    // Simpler: POSITION(substring, col) - uses comma syntax instead of IN
    public static FunctionCallNode<long> Position(ISql<string> c1, string substring)
        => new FunctionCallNode<long>("POSITION", new SqlValueNode<string>(substring), new SqlRawNode<string>("IN "), c1);
    public static FunctionCallNode<long> Position<TDialect>(this IColumnOfDialect<string, TDialect> c1, string substring) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<long>("POSITION", new SqlValueNode<string>(substring), new SqlRawNode<string>("IN "), c1);
    
    // ConcatWs(separator, col1, col2, ...) -> CONCAT_WS(',', col1, col2)
    public static FunctionCallNode<string> ConcatWs(IGenericSql separator, params IGenericSql[] columns)
    {
        var args = new List<IGenericSql> { separator };
        args.AddRange(columns);
        return new FunctionCallNode<string>("CONCAT_WS", args.ToArray());
    }
    public static FunctionCallNode<string> ConcatWs(string separator, params IGenericSql[] columns)
    {
        var args = new List<IGenericSql> { new SqlValueNode<string>(separator) };
        args.AddRange(columns);
        return new FunctionCallNode<string>("CONCAT_WS", args.ToArray());
    }

    
    // ======================================================================
    // 2.3: Numeric & Math Functions
    // ======================================================================
    
    public static UnaryNode<T, T> Abs<T>(ISql<T> c1) 
        => new(c1, "ABS", true);
    public static UnaryNode<T, T> Abs<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "ABS", true);
    
    public static UnaryNode<T, T> Ceil<T>(ISql<T> c1) 
        => new(c1, "CEIL", true);
    public static UnaryNode<T, T> Ceil<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "CEIL", true);
    
    public static UnaryNode<T, T> Floor<T>(ISql<T> c1) 
        => new(c1, "FLOOR", true);
    public static UnaryNode<T, T> Floor<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "FLOOR", true);
    
    public static UnaryNode<T, T> Round<T>(ISql<T> c1) 
        => new(c1, "ROUND", true);
    public static UnaryNode<T, T> Round<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "ROUND", true);
    
    // Round(col, decimals)
    public static FunctionCallNode<T> Round<T>(ISql<T> c1, int decimals) 
        => new FunctionCallNode<T>("ROUND", c1, new SqlValueNode<int>(decimals));
    public static FunctionCallNode<T> Round<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, int decimals) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<T>("ROUND", c1, new SqlValueNode<int>(decimals));
    
    // Power(base, exponent)
    public static FunctionCallNode<T> Power<T>(ISql<T> c1, IGenericSql exponent) 
        => new FunctionCallNode<T>("POWER", c1, exponent);
    public static FunctionCallNode<T> Power<T>(ISql<T> c1, T exponent) 
        where T : notnull
        => new FunctionCallNode<T>("POWER", c1, new SqlValueNode<T>(exponent));
    public static FunctionCallNode<T> Power<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, IGenericSql exponent) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<T>("POWER", c1, exponent);
    public static FunctionCallNode<T> Power<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T exponent) 
        where TDialect : ISqlDialect
        where T : notnull
        => new FunctionCallNode<T>("POWER", c1, new SqlValueNode<T>(exponent));
    
    public static UnaryNode<T, T> Sqrt<T>(ISql<T> c1) 
        => new(c1, "SQRT", true);
    public static UnaryNode<T, T> Sqrt<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "SQRT", true);
    
    public static FunctionCallNode<T> Random<T>() 
        => new FunctionCallNode<T>("RANDOM");
    
    public static UnaryNode<T, T> Sign<T>(ISql<T> c1) 
        => new(c1, "SIGN", true);
    public static UnaryNode<T, T> Sign<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "SIGN", true);
    

    // ======================================================================
    // 2.4: Date/Time Functions
    // ======================================================================
    
    // Extract(field FROM col) -> EXTRACT(year FROM col)
    public static FunctionCallNode<double> Extract(IGenericSql field, ISql c1)
        => new FunctionCallNode<double>("EXTRACT", field, new SqlRawNode<double>("FROM "), c1);
    public static FunctionCallNode<double> Extract(string field, ISql c1)
        => new FunctionCallNode<double>("EXTRACT", new SqlRawNode<double>(field), new SqlRawNode<double>("FROM "), c1);
    
    // DateTrunc(field, col) -> DATE_TRUNC('day', col)
    public static FunctionCallNode<DateTime> DateTrunc(IGenericSql precision, ISql c1)
        => new FunctionCallNode<DateTime>("DATE_TRUNC", precision, c1);
    public static FunctionCallNode<DateTime> DateTrunc(string precision, ISql c1)
        => new FunctionCallNode<DateTime>("DATE_TRUNC", new SqlValueNode<string>(precision), c1);
    
    // DateAdd(interval, amount, col) -> col + INTERVAL 'amount interval'
    // Uses simple binary add with INTERVAL literal
    public static BinaryNode<DateTime> DateAdd(ISql<DateTime> c1, int amount, string unit)
        => new(c1, Sql.Interval(amount, unit), " + ");
    
    public static BinaryNode<DateTime> DateDiff(ISql<DateTime> c1, int amount, string unit)
        => new(c1, Sql.Interval(amount, unit), " - ");
    
    public static BinaryNode<DateTime> DateAdd(ISql<DateTime> c1, double amount, string unit)
        => new(c1, Sql.Interval(amount, unit), " + ");
    
    public static BinaryNode<DateTime> DateDiff(ISql<DateTime> c1, double amount, string unit)
        => new(c1, Sql.Interval(amount, unit), " - ");
    
    public static FunctionCallNode<DateTime> Now()
        => new FunctionCallNode<DateTime>("NOW");
    public static FunctionCallNode<DateTime> CurrentTimestamp()
        => new FunctionCallNode<DateTime>("CURRENT_TIMESTAMP");
    public static FunctionCallNode<DateTime> CurrentDate()
        => new FunctionCallNode<DateTime>("CURRENT_DATE");
    
    // AtTimeZone(col, zone) -> col AT TIME ZONE 'zone'
    public static BinaryNode<DateTime, string, DateTime> AtTimeZone(ISql<DateTime> c1, ISql<string> timezone)
        => new(c1, timezone, " AT TIME ZONE ");
    
    public static BinaryNode<DateTime, string, DateTime> AtTimeZone(ISql<DateTime> c1, string timezone)
        => new(c1, Sql.TimeZone(timezone), " AT TIME ZONE ");
    
    // Age(col) / Age(col1, col2) - PostgreSQL specific
    public static UnaryNode<DateTime, TimeSpan> Age(ISql<DateTime> c1) 
        => new(c1, "AGE", true);
    public static FunctionCallNode<TimeSpan> Age(ISql<DateTime> c1, ISql<DateTime> c2) 
        => new FunctionCallNode<TimeSpan>("AGE", c1, c2);
    

    // ======================================================================
    // 2.5: Conditional Expressions
    // ======================================================================
    
    // Coalesce(col1, col2, ...) -> COALESCE(col1, col2, ...)
    public static FunctionCallNode<T> Coalesce<T>(params IGenericSql[] columns) 
        => new FunctionCallNode<T>("COALESCE", columns);
    public static FunctionCallNode<T> Coalesce<T>(ISql<T> c1, ISql<T> c2) 
        => new FunctionCallNode<T>("COALESCE", c1, c2);
    public static FunctionCallNode<T> Coalesce<T>(ISql<T> c1, T defaultValue) 
        => new FunctionCallNode<T>("COALESCE", c1, new SqlValueNode<T>(defaultValue));
    public static FunctionCallNode<T> Coalesce<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T defaultValue) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<T>("COALESCE", c1, new SqlValueNode<T>(defaultValue));
    
    // NullIf(col, value) -> NULLIF(col, value)
    public static FunctionCallNode<T?> NullIf<T>(ISql<T> c1, IGenericSql value) 
        => new FunctionCallNode<T?>("NULLIF", c1, value);
    public static FunctionCallNode<T?> NullIf<T>(ISql<T> c1, T value) 
        => new FunctionCallNode<T?>("NULLIF", c1, new SqlValueNode<T>(value));
    public static FunctionCallNode<T?> NullIf<T, TDialect>(this IColumnOfDialect<T, TDialect> c1, T value) 
        where TDialect : ISqlDialect
        => new FunctionCallNode<T?>("NULLIF", c1, new SqlValueNode<T>(value));
    
    // IIf(condition, trueVal, falseVal) -> IIF(condition, trueVal, falseVal) (MySQL/SQLite)
    public static FunctionCallNode<T> IIf<T>(IGenericSql condition, IGenericSql trueVal, IGenericSql falseVal) 
        => new FunctionCallNode<T>("IIF", condition, trueVal, falseVal);
    
    // Case() is now available via the Case class in CaseNode.cs
    // Usage: Case.When(condition, result).When(condition2, result2).Else(defaultResult)
    

    // ======================================================================
    // 2.6: Type Casting
    // ======================================================================
    
    public static CastNode<T> Cast<T>(IGenericSql expression, string targetType) 
        => new CastNode<T>(expression, targetType, usePostgresSyntax: false);
    public static CastNode<T> CastPg<T>(IGenericSql expression, string targetType) 
        => new CastNode<T>(expression, targetType, usePostgresSyntax: true);
    
    public static CastNode<string> CastToString(IGenericSql expression) 
        => new CastNode<string>(expression, "TEXT", usePostgresSyntax: false);
    public static CastNode<int> CastToInt(IGenericSql expression) 
        => new CastNode<int>(expression, "INTEGER", usePostgresSyntax: false);
    public static CastNode<long> CastToLong(IGenericSql expression) 
        => new CastNode<long>(expression, "BIGINT", usePostgresSyntax: false);
    public static CastNode<double> CastToDouble(IGenericSql expression) 
        => new CastNode<double>(expression, "DOUBLE PRECISION", usePostgresSyntax: false);
    public static CastNode<DateTime> CastToDateTime(IGenericSql expression) 
        => new CastNode<DateTime>(expression, "TIMESTAMP", usePostgresSyntax: false);
    

    // ======================================================================
    // 2.7: JSON Functions (PostgreSQL)
    // ======================================================================
    
    // JsonExtract(col, path) using -> operator
    public static BinaryNode<T, string, V> JsonExtract<T, V>(ISql<T> c1, ISql<string> path)
        => new(c1, path, " -> ");
    public static BinaryNode<T, string, string> JsonExtractText<T>(ISql<T> c1, ISql<string> path)
        => new(c1, path, " ->> ");
    
    // JsonAgg(col) -> JSON_AGG(col)
    public static UnaryNode<T> JsonAgg<T>(ISql<T> c1) 
        => new(c1, "JSON_AGG", true);
    public static UnaryNode<T> JsonAgg<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "JSON_AGG", true);
    
    // JsonBuildObject(key1, val1, key2, val2, ...)
    public static FunctionCallNode<string> JsonBuildObject(params IGenericSql[] keyValuePairs)
        => new FunctionCallNode<string>("JSON_BUILD_OBJECT", keyValuePairs);
    
    // ToJson(col) -> TO_JSON(col)
    public static UnaryNode<T> ToJson<T>(ISql<T> c1) 
        => new(c1, "TO_JSON", true);
    
    // RowToJson(col) -> ROW_TO_JSON(col)
    public static UnaryNode<T> RowToJson<T>(ISql<T> c1) 
        => new(c1, "ROW_TO_JSON", true);
    

    // ======================================================================
    // 2.8: Array Functions (PostgreSQL)
    // ======================================================================
    
    // ArrayAgg(col) -> ARRAY_AGG(col)
    public static UnaryNode<T> ArrayAgg<T>(ISql<T> c1) 
        => new(c1, "ARRAY_AGG", true);
    public static UnaryNode<T> ArrayAgg<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "ARRAY_AGG", true);
    
    // Unnest(col) -> UNNEST(col)
    public static UnaryNode<T> Unnest<T>(ISql<T> c1) 
        => new(c1, "UNNEST", true);
    public static UnaryNode<T> Unnest<T, TDialect>(this IColumnOfDialect<T, TDialect> c1) 
        where TDialect : ISqlDialect 
        => new(c1, "UNNEST", true);
    
    // ArrayLength(col, dimension) -> ARRAY_LENGTH(col, 1)
    public static FunctionCallNode<int> ArrayLength(IGenericSql c1, int dimension = 1)
        => new FunctionCallNode<int>("ARRAY_LENGTH", c1, new SqlValueNode<int>(dimension));
}
