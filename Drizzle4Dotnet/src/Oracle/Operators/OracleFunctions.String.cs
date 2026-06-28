using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Operators;

public static partial class OracleFunctions
{
    /// <summary>
    /// Oracle string functions.
    /// </summary>
    public static class String
    {
        /// <summary>String length. Renders: LENGTH(str)</summary>
        public static FunctionCallNode<int> Length(ISql<string> str) => new("LENGTH", str);

        /// <summary>Substring (1-based). Renders: SUBSTR(str, pos, len)</summary>
        public static FunctionCallNode<string> Substr(ISql<string> str, int pos, int? len = null)
            => len.HasValue ? new FunctionCallNode<string>("SUBSTR", str, Sql.Value(pos), Sql.Value(len.Value)) : new FunctionCallNode<string>("SUBSTR", str, Sql.Value(pos));

        /// <summary>Find substring position. Renders: INSTR(str, substr, pos, occ)</summary>
        public static FunctionCallNode<int> Instr(ISql<string> str, ISql<string> substr, int? pos = null, int? occ = null)
        {
            var args = new List<IGenericSql> { str, substr };
            if (pos.HasValue) args.Add(Sql.Value(pos.Value));
            if (occ.HasValue) args.Add(Sql.Value(occ.Value));
            return new FunctionCallNode<int>("INSTR", args.ToArray());
        }

        /// <summary>Concatenation. Renders: CONCAT(str1, str2)</summary>
        public static FunctionCallNode<string> Concat(ISql<string> str1, ISql<string> str2)
            => new("CONCAT", str1, str2);

        /// <summary>Uppercase. Renders: UPPER(str)</summary>
        public static FunctionCallNode<string> Upper(ISql<string> str) => new("UPPER", str);

        /// <summary>Lowercase. Renders: LOWER(str)</summary>
        public static FunctionCallNode<string> Lower(ISql<string> str) => new("LOWER", str);

        /// <summary>Capitalize first letter of each word. Renders: INITCAP(str)</summary>
        public static FunctionCallNode<string> InitCap(ISql<string> str) => new("INITCAP", str);

        /// <summary>Trim whitespace. Renders: TRIM(str)</summary>
        public static FunctionCallNode<string> Trim(ISql<string> str) => new("TRIM", str);

        /// <summary>Left trim. Renders: LTRIM(str, chars)</summary>
        public static FunctionCallNode<string> LTrim(ISql<string> str, ISql<string>? chars = null)
            => chars != null ? new FunctionCallNode<string>("LTRIM", str, chars) : new FunctionCallNode<string>("LTRIM", str);

        /// <summary>Right trim. Renders: RTRIM(str, chars)</summary>
        public static FunctionCallNode<string> RTrim(ISql<string> str, ISql<string>? chars = null)
            => chars != null ? new FunctionCallNode<string>("RTRIM", str, chars) : new FunctionCallNode<string>("RTRIM", str);

        /// <summary>Left pad. Renders: LPAD(str, len, pad)</summary>
        public static FunctionCallNode<string> LPad(ISql<string> str, int len, ISql<string>? pad = null)
            => pad != null ? new FunctionCallNode<string>("LPAD", str, Sql.Value(len), pad) : new FunctionCallNode<string>("LPAD", str, Sql.Value(len));

        /// <summary>Right pad. Renders: RPAD(str, len, pad)</summary>
        public static FunctionCallNode<string> RPad(ISql<string> str, int len, ISql<string>? pad = null)
            => pad != null ? new FunctionCallNode<string>("RPAD", str, Sql.Value(len), pad) : new FunctionCallNode<string>("RPAD", str, Sql.Value(len));

        /// <summary>Replace substring. Renders: REPLACE(str, from, to)</summary>
        public static FunctionCallNode<string> Replace(ISql<string> str, ISql<string> from, ISql<string> to)
            => new("REPLACE", str, from, to);

        /// <summary>Character-level translation. Renders: TRANSLATE(str, from, to)</summary>
        public static FunctionCallNode<string> Translate(ISql<string> str, ISql<string> from, ISql<string> to)
            => new("TRANSLATE", str, from, to);

        /// <summary>Regex matching. Renders: REGEXP_LIKE(str, pattern)</summary>
        public static FunctionCallNode<bool> RegexpLike(ISql<string> str, ISql<string> pattern)
            => new("REGEXP_LIKE", str, pattern);

        /// <summary>Regex substring. Renders: REGEXP_SUBSTR(str, pattern)</summary>
        public static FunctionCallNode<string> RegexpSubstr(ISql<string> str, ISql<string> pattern)
            => new("REGEXP_SUBSTR", str, pattern);

        /// <summary>Regex replace. Renders: REGEXP_REPLACE(str, pattern, repl)</summary>
        public static FunctionCallNode<string> RegexpReplace(ISql<string> str, ISql<string> pattern, ISql<string> repl)
            => new("REGEXP_REPLACE", str, pattern, repl);

        /// <summary>Regex position. Renders: REGEXP_INSTR(str, pattern)</summary>
        public static FunctionCallNode<int> RegexpInstr(ISql<string> str, ISql<string> pattern)
            => new("REGEXP_INSTR", str, pattern);

        /// <summary>String aggregation (11g+). Renders: LISTAGG(expr, delimiter)</summary>
        public static FunctionCallNode<string> Listagg(ISql<string> expr, string delimiter)
            => new("LISTAGG", expr, Sql.Value(delimiter));
    }
}
