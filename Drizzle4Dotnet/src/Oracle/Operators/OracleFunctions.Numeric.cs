using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle;

public static partial class OracleFunctions
{
    /// <summary>
    /// Oracle numeric functions.
    /// </summary>
    public static class Numeric
    {
        /// <summary>Absolute value. Renders: ABS(n)</summary>
        public static FunctionCallNode<double> Abs(ISql<double> n) => new("ABS", n);

        /// <summary>Ceiling. Renders: CEIL(n)</summary>
        public static FunctionCallNode<double> Ceil(ISql<double> n) => new("CEIL", n);

        /// <summary>Floor. Renders: FLOOR(n)</summary>
        public static FunctionCallNode<double> Floor(ISql<double> n) => new("FLOOR", n);

        /// <summary>Round. Renders: ROUND(n, d)</summary>
        public static FunctionCallNode<double> Round(ISql<double> n, int? decimals = null)
            => decimals.HasValue ? new FunctionCallNode<double>("ROUND", n, Sql.Value(decimals.Value)) : new FunctionCallNode<double>("ROUND", n);

        /// <summary>Truncate. Renders: TRUNC(n, d)</summary>
        public static FunctionCallNode<double> Trunc(ISql<double> n, int? decimals = null)
            => decimals.HasValue ? new FunctionCallNode<double>("TRUNC", n, Sql.Value(decimals.Value)) : new FunctionCallNode<double>("TRUNC", n);

        /// <summary>Modulo. Renders: MOD(a, b)</summary>
        public static FunctionCallNode<int> Mod(ISql<int> a, ISql<int> b) => new("MOD", a, b);

        /// <summary>Power. Renders: POWER(a, b)</summary>
        public static FunctionCallNode<double> Power(ISql<double> a, ISql<double> b) => new("POWER", a, b);

        /// <summary>Square root. Renders: SQRT(n)</summary>
        public static FunctionCallNode<double> Sqrt(ISql<double> n) => new("SQRT", n);

        /// <summary>Exponential. Renders: EXP(n)</summary>
        public static FunctionCallNode<double> Exp(ISql<double> n) => new("EXP", n);

        /// <summary>Natural log. Renders: LN(n)</summary>
        public static FunctionCallNode<double> Ln(ISql<double> n) => new("LN", n);

        /// <summary>Logarithm. Renders: LOG(base, n)</summary>
        public static FunctionCallNode<double> Log(ISql<double> baseVal, ISql<double> n) => new("LOG", baseVal, n);

        /// <summary>Sine. Renders: SIN(n)</summary>
        public static FunctionCallNode<double> Sin(ISql<double> n) => new("SIN", n);

        /// <summary>Cosine. Renders: COS(n)</summary>
        public static FunctionCallNode<double> Cos(ISql<double> n) => new("COS", n);

        /// <summary>Tangent. Renders: TAN(n)</summary>
        public static FunctionCallNode<double> Tan(ISql<double> n) => new("TAN", n);

        /// <summary>Sign (-1, 0, 1). Renders: SIGN(n)</summary>
        public static FunctionCallNode<int> Sign(ISql<double> n) => new("SIGN", n);

        /// <summary>Greatest value. Renders: GREATEST(v1, v2, ...)</summary>
        public static FunctionCallNode<double> Greatest(params ISql<double>[] values)
            => new FunctionCallNode<double>("GREATEST", values);

        /// <summary>Least value. Renders: LEAST(v1, v2, ...)</summary>
        public static FunctionCallNode<double> Least(params ISql<double>[] values)
            => new FunctionCallNode<double>("LEAST", values);

        /// <summary>Histogram buckets. Renders: WIDTH_BUCKET(expr, min, max, buckets)</summary>
        public static FunctionCallNode<int> WidthBucket(ISql<double> expr, ISql<double> min, ISql<double> max, int buckets)
            => new("WIDTH_BUCKET", expr, min, max, Sql.Value(buckets));
    }
}
