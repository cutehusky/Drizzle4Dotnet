using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Sqlite;

public static partial class SqliteFunctions
{
    /// <summary>
    /// SQLite string functions.
    /// SQLite uses || for string concatenation (same as PostgreSQL).
    /// </summary>
    public static class String
    {
        /// <summary>
        /// Returns the position of the first occurrence of substring in the string (1-based).
        /// Renders as: instr(haystack, needle)
        /// Returns 0 if not found.
        /// </summary>
        public static FunctionCallNode<int> Instr(ISql<string> haystack, ISql<string> needle)
            => new("instr", haystack, needle);
        
        /// <summary>
        /// Returns a substring starting at start (1-based) with optional length.
        /// Renders as: substr(str, start, length)
        /// </summary>
        public static FunctionCallNode<string> Substr(ISql<string> str, ISql<int> start, ISql<int>? length = null)
        {
            if (length != null)
                return new FunctionCallNode<string>("substr", str, start, length);
            return new FunctionCallNode<string>("substr", str, start);
        }
        
        /// <summary>
        /// Replaces all occurrences of 'from' with 'to' in the string.
        /// Renders as: replace(str, from, to)
        /// </summary>
        public static FunctionCallNode<string> Replace(ISql<string> str, ISql<string> from, ISql<string> to)
            => new("replace", str, from, to);
        
        /// <summary>
        /// Removes leading and trailing whitespace.
        /// Renders as: trim(str)
        /// </summary>
        public static FunctionCallNode<string> Trim(ISql<string> str)
            => new("trim", str);
        
        /// <summary>
        /// Removes leading whitespace.
        /// Renders as: ltrim(str)
        /// </summary>
        public static FunctionCallNode<string> LTrim(ISql<string> str)
            => new("ltrim", str);
        
        /// <summary>
        /// Removes trailing whitespace.
        /// Renders as: rtrim(str)
        /// </summary>
        public static FunctionCallNode<string> RTrim(ISql<string> str)
            => new("rtrim", str);
        
        /// <summary>
        /// Returns the number of characters in the string.
        /// Renders as: length(str)
        /// </summary>
        public static FunctionCallNode<int> Length(ISql<string> str)
            => new("length", str);
        
        /// <summary>
        /// Converts the string to uppercase.
        /// Renders as: upper(str)
        /// </summary>
        public static FunctionCallNode<string> Upper(ISql<string> str)
            => new("upper", str);
        
        /// <summary>
        /// Converts the string to lowercase.
        /// Renders as: lower(str)
        /// </summary>
        public static FunctionCallNode<string> Lower(ISql<string> str)
            => new("lower", str);
        
        /// <summary>
        /// SQL LIKE pattern matching.
        /// Renders as: str LIKE pattern
        /// </summary>
        public static ISql<bool> Like(ISql<string> str, ISql<string> pattern)
            => new RawSql<bool>($"{nameof(Like)} expression");
        
        /// <summary>
        /// SQLite GLOB pattern matching (Unix-style wildcards: *, ?).
        /// Renders as: str GLOB pattern
        /// </summary>
        public static ISql<bool> Glob(ISql<string> str, ISql<string> pattern)
            => new RawSql<bool>($"{nameof(Glob)} expression");
        
        /// <summary>
        /// Concatenates strings using the || operator.
        /// Renders as: left || right
        /// </summary>
        public static BinaryNode<string> Concat(ISql<string> left, ISql<string> right)
            => new(left, right, " || ");
        
        /// <summary>
        /// Aggregates string values with an optional separator.
        /// Renders as: group_concat(str, separator)
        /// </summary>
        public static FunctionCallNode<string> GroupConcat(ISql<string> str, string? separator = null)
        {
            if (separator != null)
                return new FunctionCallNode<string>("group_concat", str, Sql.Value(separator));
            return new FunctionCallNode<string>("group_concat", str);
        }
    }
}
