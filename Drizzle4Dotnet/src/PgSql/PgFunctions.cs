using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;
using Drizzle4Dotnet.PgSql.Nodes;

namespace Drizzle4Dotnet.PgSql;

/// <summary>
/// PostgreSQL-specific SQL functions.
/// </summary>
public static class PgFunctions
{
    // ======================================================================
    // String Functions (PostgreSQL-specific syntax)
    // ======================================================================
    
    // Position(substring IN col) -> POSITION(sub IN col)
    // Uses custom PositionNode to handle the IN keyword (not comma-separated args)
    public static PositionNode Position(IGenericSql substring, ISql<string> c1) => new(substring, c1);
    public static PositionNode Position(ISql<string> c1, string substring) =>
        new(new SqlValueNode<string>(substring), c1);
    public static PositionNode Position<TDialect>(this IColumnOfDialect<string, TDialect> c1, string substring)
        where TDialect : ISqlDialect =>
        new(new SqlValueNode<string>(substring), c1);
    
    // ======================================================================
    // Date/Time Functions (PostgreSQL-specific)
    // ======================================================================
    
    // Extract(field FROM col) -> EXTRACT(year FROM col)
    // Note: EXTRACT is SQL standard but included here for the PostgreSQL-specific overloads
    public static FunctionCallNode<double> Extract(IGenericSql field, ISql c1) =>
        new("EXTRACT", field, new RawSql<double>("FROM "), c1);
    public static FunctionCallNode<double> Extract(string field, ISql c1)
        => new("EXTRACT", new SqlValueNode<string>(field), new RawSql<double>("FROM "), c1);
    
    // DateTrunc(field, col) -> DATE_TRUNC('day', col)
    public static FunctionCallNode<DateTime> DateTrunc(IGenericSql precision, ISql c1) =>
        new("DATE_TRUNC", precision, c1);
    public static FunctionCallNode<DateTime> DateTrunc(string precision, ISql c1) =>
        new("DATE_TRUNC", new SqlValueNode<string>(precision), c1);
    
    // DateAdd(interval, amount, col) -> col + INTERVAL 'amount interval'
    public static BinaryNode<DateTime> DateAdd(ISql<DateTime> c1, int amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " + ");
    public static BinaryNode<DateTime> DateDiff(ISql<DateTime> c1, int amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " - ");
    public static BinaryNode<DateTime> DateAdd(ISql<DateTime> c1, double amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " + ");
    public static BinaryNode<DateTime> DateDiff(ISql<DateTime> c1, double amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " - ");
    
    // AtTimeZone(col, zone) -> col AT TIME ZONE 'zone'
    public static BinaryNode<DateTime, string, DateTime> AtTimeZone(ISql<DateTime> c1, ISql<string> timezone)
        => new(c1, timezone, " AT TIME ZONE ");
    public static BinaryNode<DateTime, string, DateTime> AtTimeZone(ISql<DateTime> c1, string timezone)
        => new(c1, PgSqlStatics.TimeZone(timezone), " AT TIME ZONE ");
    
    // Age(col) / Age(col1, col2) - PostgreSQL specific
    public static FunctionCallNode<DateTime, TimeSpan> Age(ISql<DateTime> c1) => new("AGE", c1);
    public static FunctionCallNode<DateTime, DateTime, TimeSpan> Age(ISql<DateTime> c1, ISql<DateTime> c2) =>
        new("AGE", c1, c2);
    
    
    // ======================================================================
    // JSON Functions (PostgreSQL)
    // ======================================================================
    
    // JsonExtract(col, path) using -> operator
    public static BinaryNode<T, string, V> JsonExtract<T, V>(ISql<T> c1, ISql<string> path)
        => new(c1, path, " -> ");
    public static BinaryNode<T, string, string> JsonExtractText<T>(ISql<T> c1, ISql<string> path)
        => new(c1, path, " ->> ");
    
    // JsonAgg(col) -> JSON_AGG(col)
    public static FunctionCallNode<T, T> JsonAgg<T>(ISql<T> c1) => new("JSON_AGG", c1);
    public static FunctionCallNode<T, T> JsonAgg<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("JSON_AGG", c1);
    
    // JsonBuildObject(key1, val1, key2, val2, ...)
    public static FunctionCallNode<string> JsonBuildObject(params IGenericSql[] keyValuePairs) =>
        new("JSON_BUILD_OBJECT", keyValuePairs);
    
    // JsonArrayLength(col) -> JSON_ARRAY_LENGTH(col)
    public static FunctionCallNode<T, int> JsonArrayLength<T>(ISql<T> c1) => new("JSON_ARRAY_LENGTH", c1);
    public static FunctionCallNode<T, int> JsonArrayLength<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("JSON_ARRAY_LENGTH", c1);
    
    // ToJson(col) -> TO_JSON(col)
    public static FunctionCallNode<T, T> ToJson<T>(ISql<T> c1) => new("TO_JSON", c1);
    
    // RowToJson(col) -> ROW_TO_JSON(col)
    public static FunctionCallNode<T, T> RowToJson<T>(ISql<T> c1) => new("ROW_TO_JSON", c1);
    
    
    // ======================================================================
    // Array Functions (PostgreSQL)
    // ======================================================================
    
    // ArrayAgg(col) -> ARRAY_AGG(col)
    public static FunctionCallNode<T, T> ArrayAgg<T>(ISql<T> c1) => new("ARRAY_AGG", c1);
    public static FunctionCallNode<T, T> ArrayAgg<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("ARRAY_AGG", c1);
    
    // Unnest(col) -> UNNEST(col)
    public static FunctionCallNode<T, T> Unnest<T>(ISql<T> c1) => new("UNNEST", c1);
    public static FunctionCallNode<T, T> Unnest<T, TDialect>(this IColumnOfDialect<T, TDialect> c1)
        where TDialect : ISqlDialect =>
        new("UNNEST", c1);
    
    // ArrayLength(col, dimension) -> ARRAY_LENGTH(col, 1)
    public static FunctionCallNode<int> ArrayLength(IGenericSql c1, int dimension = 1) =>
        new("ARRAY_LENGTH", c1, new SqlValueNode<int>(dimension));
    
    // Array any (array version): value = ANY(array_col)
    public static BinaryNode<T, T[], bool> ArrayAny<T>(ISql<T> value, ISql<T[]> arrayCol)
        => new(value, arrayCol, " = ANY ");
    
    // Array all (array version): value = ALL(array_col)
    public static BinaryNode<T, T[], bool> ArrayAll<T>(ISql<T> value, ISql<T[]> arrayCol)
        => new(value, arrayCol, " = ALL ");
    
    
    // ======================================================================
    // Other PostgreSQL-specific Functions
    // ======================================================================
    
    // Random() -> RANDOM() (PostgreSQL uses RANDOM, SQL standard uses RAND)
    public static FunctionCallNode<T> Random<T>() => new("RANDOM");
    
    // CastPg(expr, type) -> expr::type (PostgreSQL-style cast)
    public static CastNode<T> CastPg<T>(IGenericSql expression, string targetType) =>
        new(expression, targetType, usePostgresSyntax: true);
    
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
    // Window Functions (PostgreSQL)
    // ======================================================================

    /// <summary>
    /// ROW_NUMBER() — assigns a unique sequential integer to each row within a partition.
    /// Use .Over(over) to add the OVER clause.
    /// </summary>
    public static FunctionCallNode<long> RowNumber()
        => new("ROW_NUMBER");

    /// <summary>
    /// RANK() — ranks rows with gaps (e.g., 1, 1, 3).
    /// </summary>
    public static FunctionCallNode<long> Rank()
        => new("RANK");

    /// <summary>
    /// DENSE_RANK() — ranks rows without gaps (e.g., 1, 1, 2).
    /// </summary>
    public static FunctionCallNode<long> DenseRank()
        => new("DENSE_RANK");

    /// <summary>
    /// NTILE(n) — divides rows into n roughly equal buckets.
    /// </summary>
    public static FunctionCallNode<int, long> Ntile(int n)
        => new("NTILE", new SqlValueNode<int>(n));

    /// <summary>
    /// LEAD(col, offset, default) — access a row at a given physical offset after the current row.
    /// </summary>
    public static FunctionCallNode<T> Lead<T>(ISql<T> c1, int offset, IGenericSql defaultValue)
        => new("LEAD", c1, new SqlValueNode<int>(offset), defaultValue);
    public static FunctionCallNode<T, int, T> Lead<T>(ISql<T> c1, int offset) =>
        new("LEAD", c1, new SqlValueNode<int>(offset));
    public static FunctionCallNode<T, T> Lead<T>(ISql<T> c1) => new("LEAD", c1);

    /// <summary>
    /// LAG(col, offset, default) — access a row at a given physical offset before the current row.
    /// </summary>
    public static FunctionCallNode<T> Lag<T>(ISql<T> c1, int offset, IGenericSql defaultValue)
        => new("LAG", c1, new SqlValueNode<int>(offset), defaultValue);
    public static FunctionCallNode<T, int, T> Lag<T>(ISql<T> c1, int offset) =>
        new("LAG", c1, new SqlValueNode<int>(offset));
    public static FunctionCallNode<T, T> Lag<T>(ISql<T> c1) => new("LAG", c1);

    /// <summary>
    /// FIRST_VALUE(col) — returns the first value in the window frame.
    /// </summary>
    public static FunctionCallNode<T, T> FirstValue<T>(ISql<T> c1)
        => new("FIRST_VALUE", c1);

    /// <summary>
    /// LAST_VALUE(col) — returns the last value in the window frame.
    /// </summary>
    public static FunctionCallNode<T, T> LastValue<T>(ISql<T> c1)
        => new ("LAST_VALUE", c1);

    /// <summary>
    /// NTH_VALUE(col, n) — returns the nth value in the window frame.
    /// </summary>
    public static FunctionCallNode<T, int, T> NthValue<T>(ISql<T> c1, int n)
        => new("NTH_VALUE", c1, new SqlValueNode<int>(n));
}
