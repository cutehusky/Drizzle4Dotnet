using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Operators;

public static partial class OracleFunctions
{
    /// <summary>
    /// Oracle analytic (window) functions.
    /// Most require OVER() clause which should be added using PgOverNode or similar.
    /// </summary>
    public static class Analytic
    {
        /// <summary>Row number. Renders: ROW_NUMBER()</summary>
        public static FunctionCallNode<int> RowNumber() => new("ROW_NUMBER");

        /// <summary>Rank with gaps. Renders: RANK()</summary>
        public static FunctionCallNode<int> Rank() => new("RANK");

        /// <summary>Rank without gaps. Renders: DENSE_RANK()</summary>
        public static FunctionCallNode<int> DenseRank() => new("DENSE_RANK");

        /// <summary>Previous row value. Renders: LAG(expr, offset, default)</summary>
        public static FunctionCallNode<T> Lag<T>(ISql<T> expr, int? offset = null, ISql<T>? defaultVal = null)
        {
            var args = new List<IGenericSql> { expr };
            if (offset.HasValue) args.Add(Sql.Value(offset.Value));
            if (defaultVal != null) args.Add(defaultVal);
            return new FunctionCallNode<T>("LAG", args.ToArray());
        }

        /// <summary>Next row value. Renders: LEAD(expr, offset, default)</summary>
        public static FunctionCallNode<T> Lead<T>(ISql<T> expr, int? offset = null, ISql<T>? defaultVal = null)
        {
            var args = new List<IGenericSql> { expr };
            if (offset.HasValue) args.Add(Sql.Value(offset.Value));
            if (defaultVal != null) args.Add(defaultVal);
            return new FunctionCallNode<T>("LEAD", args.ToArray());
        }

        /// <summary>First value in window. Renders: FIRST_VALUE(expr)</summary>
        public static FunctionCallNode<T> FirstValue<T>(ISql<T> expr) => new("FIRST_VALUE", expr);

        /// <summary>Last value in window. Renders: LAST_VALUE(expr)</summary>
        public static FunctionCallNode<T> LastValue<T>(ISql<T> expr) => new("LAST_VALUE", expr);

        /// <summary>Nth value in window. Renders: NTH_VALUE(expr, n)</summary>
        public static FunctionCallNode<T> NthValue<T>(ISql<T> expr, int n) => new("NTH_VALUE", expr, Sql.Value(n));

        /// <summary>Bucket rows into n tiles. Renders: NTILE(n)</summary>
        public static FunctionCallNode<int> NTile(int n) => new("NTILE", Sql.Value(n));

        /// <summary>Cumulative distribution. Renders: CUME_DIST()</summary>
        public static FunctionCallNode<double> CumeDist() => new("CUME_DIST");

        /// <summary>Percent rank. Renders: PERCENT_RANK()</summary>
        public static FunctionCallNode<double> PercentRank() => new("PERCENT_RANK");

        /// <summary>Continuous percentile. Renders: PERCENTILE_CONT(p) WITHIN GROUP (ORDER BY ...)</summary>
        public static IGenericSql PercentileCont(double p)
            => new RawSql($"PERCENTILE_CONT({p})");

        /// <summary>Discrete percentile. Renders: PERCENTILE_DISC(p) WITHIN GROUP (ORDER BY ...)</summary>
        public static IGenericSql PercentileDisc(double p)
            => new RawSql($"PERCENTILE_DISC({p})");

        /// <summary>Median. Renders: MEDIAN(expr)</summary>
        public static FunctionCallNode<double> Median(ISql<double> expr) => new("MEDIAN", expr);

        /// <summary>Ratio of value to total. Renders: RATIO_TO_REPORT(expr)</summary>
        public static FunctionCallNode<double> RatioToReport(ISql<double> expr) => new("RATIO_TO_REPORT", expr);

        /// <summary>Standard deviation. Renders: STDDEV(expr)</summary>
        public static FunctionCallNode<double> Stddev(ISql<double> expr) => new("STDDEV", expr);

        /// <summary>Variance. Renders: VARIANCE(expr)</summary>
        public static FunctionCallNode<double> Variance(ISql<double> expr) => new("VARIANCE", expr);
    }
}
