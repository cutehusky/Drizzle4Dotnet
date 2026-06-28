using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Operators;

// ======================================================================
// MSSQL Numeric Functions
// ======================================================================
public static partial class MssqlFunctions
{
    /// <summary>
    /// MSSQL numeric/mathematical functions.
    /// </summary>
    public static class Numeric
    {
        /// <summary>ABS(value) — absolute value.</summary>
        public static UnaryNode<int, int> Abs(ISql<int> value)
            => new(value, "ABS", true);

        /// <summary>ABS(value) — absolute value (long).</summary>
        public static UnaryNode<long, long> Abs(ISql<long> value)
            => new(value, "ABS", true);

        /// <summary>ABS(value) — absolute value (double).</summary>
        public static UnaryNode<double, double> Abs(ISql<double> value)
            => new(value, "ABS", true);

        /// <summary>CEILING(value) — rounds up to nearest integer.</summary>
        public static UnaryNode<double, int> Ceiling(ISql<double> value)
            => new(value, "CEILING", true);

        /// <summary>FLOOR(value) — rounds down to nearest integer.</summary>
        public static UnaryNode<double, int> Floor(ISql<double> value)
            => new(value, "FLOOR", true);

        /// <summary>ROUND(value, decimals) — rounds to specified decimal places.</summary>
        public static FunctionCallNode<double> Round(ISql<double> value, int decimals)
            => new FunctionCallNode<double>("ROUND", value, new SqlValueNode<int>(decimals));

        /// <summary>ROUND(value, decimals, truncate) — rounds or truncates.</summary>
        public static FunctionCallNode<double> Round(ISql<double> value, int decimals, int truncate)
            => new FunctionCallNode<double>("ROUND", value, new SqlValueNode<int>(decimals), new SqlValueNode<int>(truncate));

        /// <summary>RAND() — returns a random float between 0 and 1.</summary>
        public static FunctionCallNode<double> Rand()
            => new FunctionCallNode<double>("RAND");

        /// <summary>RAND(seed) — returns a seeded random float.</summary>
        public static FunctionCallNode<double> Rand(int seed)
            => new FunctionCallNode<double>("RAND", new SqlValueNode<int>(seed));

        /// <summary>SIGN(value) — returns sign (-1, 0, or 1).</summary>
        public static UnaryNode<int, int> Sign(ISql<int> value)
            => new(value, "SIGN", true);

        /// <summary>SQRT(value) — square root.</summary>
        public static UnaryNode<double, double> Sqrt(ISql<double> value)
            => new(value, "SQRT", true);

        /// <summary>POWER(value, exp) — raises value to the power of exp.</summary>
        public static FunctionCallNode<double> Power(ISql<double> value, double exp)
            => new FunctionCallNode<double>("POWER", value, new SqlValueNode<double>(exp));

        /// <summary>SQUARE(value) — square of a value.</summary>
        public static UnaryNode<double, double> Square(ISql<double> value)
            => new(value, "SQUARE", true);

        /// <summary>LOG(value) — natural logarithm.</summary>
        public static UnaryNode<double, double> Log(ISql<double> value)
            => new(value, "LOG", true);

        /// <summary>LOG10(value) — base-10 logarithm.</summary>
        public static UnaryNode<double, double> Log10(ISql<double> value)
            => new(value, "LOG10", true);

        /// <summary>EXP(value) — exponential function (e^x).</summary>
        public static UnaryNode<double, double> Exp(ISql<double> value)
            => new(value, "EXP", true);

        /// <summary>PI() — returns the constant PI.</summary>
        public static FunctionCallNode<double> Pi()
            => new FunctionCallNode<double>("PI");
    }
}
