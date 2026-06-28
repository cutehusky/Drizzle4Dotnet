using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Operators;

// ======================================================================
// MSSQL String Functions
// ======================================================================
public static partial class MssqlFunctions
{
    /// <summary>
    /// MSSQL string functions.
    /// MSSQL uses different function names than PostgreSQL/MySQL.
    /// </summary>
    public static class String
    {
        /// <summary>LEN(str) — returns the number of characters, excluding trailing spaces.</summary>
        public static UnaryNode<string, int> Len(ISql<string> str)
            => new(str, "LEN", true);

        /// <summary>CHARINDEX(substr, str) — returns the starting position of substr in str.</summary>
        public static FunctionCallNode<int> CharIndex(ISql<string> substr, ISql<string> str)
            => new FunctionCallNode<int>("CHARINDEX", substr, str);

        /// <summary>CHARINDEX(substr, str, start) — returns starting position with offset.</summary>
        public static FunctionCallNode<int> CharIndex(ISql<string> substr, ISql<string> str, int start)
            => new FunctionCallNode<int>("CHARINDEX", substr, str, new SqlValueNode<int>(start));

        /// <summary>SUBSTRING(str, start, length) — extracts a substring.</summary>
        public static FunctionCallNode<string> Substring(ISql<string> str, int start, int length)
            => new FunctionCallNode<string>("SUBSTRING", str, new SqlValueNode<int>(start), new SqlValueNode<int>(length));

        /// <summary>LEFT(str, n) — returns the leftmost n characters.</summary>
        public static FunctionCallNode<string> Left(ISql<string> str, int n)
            => new FunctionCallNode<string>("LEFT", str, new SqlValueNode<int>(n));

        /// <summary>RIGHT(str, n) — returns the rightmost n characters.</summary>
        public static FunctionCallNode<string> Right(ISql<string> str, int n)
            => new FunctionCallNode<string>("RIGHT", str, new SqlValueNode<int>(n));

        /// <summary>CONCAT(str1, str2, ...) — concatenates strings (NULL-safe).</summary>
        public static FunctionCallNode<string> Concat(params IGenericSql[] strings)
            => new FunctionCallNode<string>("CONCAT", strings);

        /// <summary>CONCAT_WS(sep, str1, str2, ...) — concatenates with separator.</summary>
        public static FunctionCallNode<string> ConcatWs(IGenericSql separator, params IGenericSql[] strings)
        {
            var args = new List<IGenericSql> { separator };
            args.AddRange(strings);
            return new FunctionCallNode<string>("CONCAT_WS", args.ToArray());
        }

        /// <summary>REPLACE(str, from, to) — replaces all occurrences of a substring.</summary>
        public static FunctionCallNode<string> Replace(ISql<string> str, string from, string to)
            => new FunctionCallNode<string>("REPLACE", str, new SqlValueNode<string>(from), new SqlValueNode<string>(to));

        /// <summary>TRIM(str) — removes leading and trailing spaces.</summary>
        public static UnaryNode<string, string> Trim(ISql<string> str)
            => new(str, "TRIM", true);

        /// <summary>LTRIM(str) — removes leading spaces.</summary>
        public static UnaryNode<string, string> LTrim(ISql<string> str)
            => new(str, "LTRIM", true);

        /// <summary>RTRIM(str) — removes trailing spaces.</summary>
        public static UnaryNode<string, string> RTrim(ISql<string> str)
            => new(str, "RTRIM", true);

        /// <summary>UPPER(str) — converts to uppercase.</summary>
        public static UnaryNode<string, string> Upper(ISql<string> str)
            => new(str, "UPPER", true);

        /// <summary>LOWER(str) — converts to lowercase.</summary>
        public static UnaryNode<string, string> Lower(ISql<string> str)
            => new(str, "LOWER", true);

        /// <summary>REVERSE(str) — reverses a string.</summary>
        public static UnaryNode<string, string> Reverse(ISql<string> str)
            => new(str, "REVERSE", true);

        /// <summary>REPLICATE(str, n) — repeats a string n times.</summary>
        public static FunctionCallNode<string> Replicate(ISql<string> str, int n)
            => new FunctionCallNode<string>("REPLICATE", str, new SqlValueNode<int>(n));

        /// <summary>SPACE(n) — returns a string of n spaces.</summary>
        public static FunctionCallNode<string> Space(int n)
            => new FunctionCallNode<string>("SPACE", new SqlValueNode<int>(n));

        /// <summary>ASCII(str) — returns ASCII code of first character.</summary>
        public static UnaryNode<string, int> Ascii(ISql<string> str)
            => new(str, "ASCII", true);

        /// <summary>CHAR(n) — returns character for ASCII code.</summary>
        public static FunctionCallNode<string> Char(int n)
            => new FunctionCallNode<string>("CHAR", new SqlValueNode<int>(n));

        /// <summary>NCHAR(n) — returns Unicode character for code n.</summary>
        public static FunctionCallNode<string> NChar(int n)
            => new FunctionCallNode<string>("NCHAR", new SqlValueNode<int>(n));

        /// <summary>UNICODE(str) — returns Unicode code of first character.</summary>
        public static UnaryNode<string, int> Unicode(ISql<string> str)
            => new(str, "UNICODE", true);

        /// <summary>SOUNDEX(str) — returns Soundex code for a string.</summary>
        public static UnaryNode<string, string> Soundex(ISql<string> str)
            => new(str, "SOUNDEX", true);

        /// <summary>DIFFERENCE(str1, str2) — returns Soundex difference (0-4).</summary>
        public static FunctionCallNode<int> Difference(ISql<string> str1, ISql<string> str2)
            => new FunctionCallNode<int>("DIFFERENCE", str1, str2);

        /// <summary>PATINDEX(pattern, str) — returns starting position of pattern in str.</summary>
        public static FunctionCallNode<int> PatIndex(string pattern, ISql<string> str)
            => new FunctionCallNode<int>("PATINDEX", new SqlValueNode<string>(pattern), str);

        /// <summary>QUOTENAME(str) — returns a Unicode string with brackets added.</summary>
        public static UnaryNode<string, string> QuoteName(ISql<string> str)
            => new(str, "QUOTENAME", true);
    }
}
