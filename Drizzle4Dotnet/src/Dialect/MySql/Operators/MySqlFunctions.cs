using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;
using Drizzle4Dotnet.MySql.Nodes;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL-specific SQL functions.
/// </summary>
public static class MySqlFunctions
{
    // ======================================================================
    // String Functions (MySQL-specific)
    // ======================================================================
    
    /// <summary>
    /// MySQL CONCAT function: CONCAT(arg1, arg2, ...)
    /// MySQL uses CONCAT(), not the || operator.
    /// </summary>
    public static FunctionCallNode<string> Concat(params IGenericSql[] args)
        => new("CONCAT", args);
    
    /// <summary>
    /// MySQL CONCAT with literal string arguments.
    /// </summary>
    public static FunctionCallNode<string> Concat(params string[] args)
        => new("CONCAT", 
            args.Select(a => new SqlValueNode<string>(a)).Cast<IGenericSql>().ToArray());
    
    /// <summary>
    /// MySQL CONCAT_WS(separator, col1, col2, ...)
    /// </summary>
    public static FunctionCallNode<string> ConcatWs(IGenericSql separator, params IGenericSql[] columns)
    {
        var args = new List<IGenericSql> { separator };
        args.AddRange(columns);
        return new FunctionCallNode<string>("CONCAT_WS", args.ToArray());
    }
    
    /// <summary>
    /// MySQL CONCAT_WS with literal separator.
    /// </summary>
    public static FunctionCallNode<string> ConcatWs(string separator, params IGenericSql[] columns)
    {
        var args = new List<IGenericSql> { new SqlValueNode<string>(separator) };
        args.AddRange(columns);
        return new FunctionCallNode<string>("CONCAT_WS", args.ToArray());
    }
    
    /// <summary>
    /// MySQL CHAR_LENGTH (returns character count).
    /// Unlike LENGTH which returns byte count in MySQL.
    /// </summary>
    public static FunctionCallNode<T, long> CharLength<T>(ISql<T> c1) 
        => new("CHAR_LENGTH", c1);
    
    
    /// <summary>
    /// MySQL LOCATE(substr, str) — returns position of first occurrence.
    /// </summary>
    public static FunctionCallNode<long> Locate(IGenericSql substr, IGenericSql str)
        => new("LOCATE", substr, str);
    
    /// <summary>
    /// MySQL LOCATE(substr, str, pos) — starting from position pos.
    /// </summary>
    public static FunctionCallNode<long> Locate(IGenericSql substr, IGenericSql str, int pos)
        => new("LOCATE", substr, str, new SqlValueNode<int>(pos));
    
    /// <summary>
    /// MySQL SUBSTRING_INDEX(str, delim, count).
    /// Returns the substring from string str before count occurrences of delimiter delim.
    /// </summary>
    public static FunctionCallNode<string> SubstringIndex(IGenericSql str, IGenericSql delim, int count)
        => new("SUBSTRING_INDEX", str, delim, new SqlValueNode<int>(count));
    
    /// <summary>
    /// MySQL POSITION(substr IN str) — standard SQL syntax.
    /// </summary>
    public static MySqlPositionNode Position(IGenericSql substring, ISql<string> c1)
        => new(substring, c1);
    
    /// <summary>
    /// MySQL POSITION with literal substring.
    /// </summary>
    public static MySqlPositionNode Position(ISql<string> c1, string substring)
        => new(new SqlValueNode<string>(substring), c1);
    
    
    // ======================================================================
    // Date/Time Functions (MySQL-specific)
    // ======================================================================
    
    /// <summary>
    /// MySQL CURDATE() — returns current date.
    /// </summary>
    public static FunctionCallNode<DateTime> CurDate()
        => new("CURDATE");
    
    /// <summary>
    /// MySQL CURTIME() — returns current time.
    /// </summary>
    public static FunctionCallNode<TimeSpan> CurTime()
        => new("CURTIME");
    
    /// <summary>
    /// MySQL DATE_ADD(date, INTERVAL expr unit).
    /// Uses IntervalMySqlNode for MySQL INTERVAL syntax (no quotes).
    /// </summary>
    public static FunctionCallNode<DateTime> DateAdd(ISql<DateTime> date, int amount, string unit)
        => new("DATE_ADD", date, new MySqlIntervalNode(amount, unit));
    
    /// <summary>
    /// MySQL DATE_SUB(date, INTERVAL expr unit).
    /// </summary>
    public static FunctionCallNode<DateTime> DateSub(ISql<DateTime> date, int amount, string unit)
        => new("DATE_SUB", date, new MySqlIntervalNode(amount, unit));
    
    /// <summary>
    /// MySQL DATE_FORMAT(date, format).
    /// Formats the date value according to the format string.
    /// </summary>
    public static FunctionCallNode<DateTime, string, string> DateFormat(ISql<DateTime> date, string format)
        => new("DATE_FORMAT", date, new SqlValueNode<string>(format));
    
    /// <summary>
    /// MySQL UNIX_TIMESTAMP(date) — returns Unix timestamp.
    /// </summary>
    public static FunctionCallNode<DateTime, long> UnixTimestamp(ISql<DateTime> date)
        => new("UNIX_TIMESTAMP", date);
    
    /// <summary>
    /// MySQL FROM_UNIXTIME(timestamp) — converts Unix timestamp to datetime.
    /// </summary>
    public static FunctionCallNode<long, DateTime> FromUnixTime(ISql<long> timestamp)
        => new("FROM_UNIXTIME", timestamp);
    
    /// <summary>
    /// MySQL STR_TO_DATE(str, format).
    /// </summary>
    public static FunctionCallNode<DateTime> StrToDate(IGenericSql str, string format)
        => new("STR_TO_DATE", str, new SqlValueNode<string>(format));
    
    
    // ======================================================================
    // Numeric / Math Functions (MySQL-specific)
    // ======================================================================
    
    /// <summary>
    /// MySQL RAND() — returns random float.
    /// PostgreSQL uses RANDOM(), MySQL uses RAND().
    /// </summary>
    public static FunctionCallNode<T> Rand<T>()
        => new("RAND");
    
    /// <summary>
    /// MySQL TRUNCATE(col, decimals) — truncates to specified decimal places.
    /// Note: MySQL uses TRUNCATE, not TRUNC.
    /// </summary>
    public static FunctionCallNode<T, int, T> Truncate<T>(ISql<T> c1, int decimals)
        => new("TRUNCATE", c1, new SqlValueNode<int>(decimals));
    
    /// <summary>
    /// MySQL BIT_COUNT(col) — returns number of bits set.
    /// </summary>
    public static FunctionCallNode<T, long> BitCount<T>(ISql<T> c1)
        => new("BIT_COUNT", c1);
    
    /// <summary>
    /// MySQL CRC32(col) — computes cyclic redundancy check value.
    /// </summary>
    public static FunctionCallNode<T, long> Crc32<T>(ISql<T> c1)
        => new("CRC32", c1);
    
    
    // ======================================================================
    // JSON Functions (MySQL 8.0+)
    // ======================================================================
    
    /// <summary>
    /// MySQL JSON_EXTRACT(col, path) — extracts data from JSON document.
    /// Path format: '$.key.subkey'
    /// </summary>
    public static FunctionCallNode<T, string, V> JsonExtract<T, V>(ISql<T> c1, string path)
        => new("JSON_EXTRACT", c1, new SqlValueNode<string>(path));
    
    /// <summary>
    /// MySQL JSON_UNQUOTE(JSON_EXTRACT(col, path)) — extracts and unquotes text.
    /// Equivalent to the ->> operator in MySQL 8.0+.
    /// </summary>
    public static FunctionCallNode<string> JsonExtractText<T>(ISql<T> c1, string path)
        => new("JSON_UNQUOTE", 
            new FunctionCallNode<string>("JSON_EXTRACT", c1, new SqlValueNode<string>(path)));
    
    /// <summary>
    /// MySQL JSON column -> path operator (MySQL 8.0+).
    /// Equivalent to JSON_EXTRACT(col, path).
    /// </summary>
    public static BinaryNode<T, string, V> JsonArrow<T, V>(ISql<T> c1, string path)
        => new(c1, new SqlValueNode<string>(path), " -> ");
    
    /// <summary>
    /// MySQL JSON column ->> path operator (MySQL 8.0+).
    /// Equivalent to JSON_UNQUOTE(JSON_EXTRACT(col, path)).
    /// </summary>
    public static BinaryNode<T, string, string> JsonArrowText<T>(ISql<T> c1, string path)
        => new(c1, new SqlValueNode<string>(path), " ->> ");
    
    /// <summary>
    /// MySQL JSON_ARRAYAGG(col) — aggregates result sets as a JSON array.
    /// Note: MySQL uses JSON_ARRAYAGG (no underscore).
    /// </summary>
    public static FunctionCallNode<T, T> JsonArrayAgg<T>(ISql<T> c1)
        => new("JSON_ARRAYAGG", c1);
    
    /// <summary>
    /// MySQL JSON_OBJECTAGG(key, value) — aggregates result sets as a JSON object.
    /// </summary>
    public static FunctionCallNode<string> JsonObjectAgg(IGenericSql key, IGenericSql value)
        => new("JSON_OBJECTAGG", key, value);
    
    /// <summary>
    /// MySQL JSON_ARRAY(val1, val2, ...) — creates a JSON array.
    /// </summary>
    public static FunctionCallNode<string> JsonArray(params IGenericSql[] values)
        => new("JSON_ARRAY", values);
    
    /// <summary>
    /// MySQL JSON_OBJECT(key1, val1, key2, val2, ...) — creates a JSON object.
    /// </summary>
    public static FunctionCallNode<string> JsonObject(params IGenericSql[] keyValuePairs)
        => new("JSON_OBJECT", keyValuePairs);
    
    /// <summary>
    /// MySQL JSON_CONTAINS(col, value) — checks if JSON document contains a value.
    /// </summary>
    public static FunctionCallNode<bool> JsonContains(IGenericSql c1, IGenericSql value)
        => new("JSON_CONTAINS", c1, value);
    
    /// <summary>
    /// MySQL JSON_LENGTH(col) — returns length of JSON document.
    /// </summary>
    public static FunctionCallNode<T, int> JsonLength<T>(ISql<T> c1)
        => new("JSON_LENGTH", c1);
    
    /// <summary>
    /// MySQL JSON_KEYS(col) — returns keys from JSON object.
    /// </summary>
    public static FunctionCallNode<T, T> JsonKeys<T>(ISql<T> c1)
        => new("JSON_KEYS", c1);
    
    
    // ======================================================================
    // Info / Utility Functions
    // ======================================================================
    
    /// <summary>
    /// MySQL LAST_INSERT_ID() — returns the last inserted auto-increment value.
    /// </summary>
    public static FunctionCallNode<long> LastInsertId()
        => new("LAST_INSERT_ID");
    
    /// <summary>
    /// MySQL DATABASE() — returns the current database name.
    /// </summary>
    public static FunctionCallNode<string> Database()
        => new("DATABASE");
    
    /// <summary>
    /// MySQL USER() — returns the current MySQL user.
    /// </summary>
    public static FunctionCallNode<string> User()
        => new("USER");
    
    /// <summary>
    /// MySQL VERSION() — returns the MySQL server version.
    /// </summary>
    public static FunctionCallNode<string> Version()
        => new("VERSION");
    
    /// <summary>
    /// MySQL ROW_COUNT() — returns the number of rows affected by previous statement.
    /// </summary>
    public static FunctionCallNode<long> RowCount()
        => new("ROW_COUNT");
    
    /// <summary>
    /// MySQL FOUND_ROWS() — returns the number of rows in the result set.
    /// Used after SELECT with LIMIT to get total rows without limit.
    /// </summary>
    public static FunctionCallNode<long> FoundRows()
        => new("FOUND_ROWS");
}

/// <summary>
/// Extension methods for MySQL-specific functions.
/// </summary>
public static class MySqlFunctionsExtensions
{
    /// <summary>
    /// Extension method version of CHAR_LENGTH.
    /// </summary>
    public static FunctionCallNode<T, long> CharLength<T>(this ISql<T> c1)
        => new("CHAR_LENGTH", c1);
    
    /// <summary>
    /// Extension method version of POSITION.
    /// </summary>
    public static MySqlPositionNode Position(this ISql<string> c1, string substring) 
        => new(new SqlValueNode<string>(substring), c1);
}
