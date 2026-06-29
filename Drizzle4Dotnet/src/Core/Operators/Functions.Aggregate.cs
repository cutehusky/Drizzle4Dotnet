using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators;

// ======================================================================
// Aggregate Functions: Count, Distinct, CountDistinct, Sum, Avg, Min, Max,
//                      StdDev, Variance, VarSample, VarPop, StdDevSample, StdDevPop
// ======================================================================
public static partial class Functions
{
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
}

// ======================================================================
// Extension methods for ISql — provide `column.Count()` syntax
// ======================================================================
public static partial class FunctionsExtensions
{
    public static FunctionCallNode<T, long> Count<T>(this ISql<T> c1) =>
        new("COUNT", c1);
    public static FunctionCallNode<T, T> Distinct<T>(this ISql<T> c1) =>
        new("DISTINCT", c1);
    public static FunctionCallNode<T, long> CountDistinct<T>(this ISql<T> c1)
        => Functions.Count(Functions.Distinct(c1));
    public static FunctionCallNode<T, T> Sum<T>(this ISql<T> c1) =>
        new("SUM", c1);
    public static FunctionCallNode<T, T> Avg<T>(this ISql<T> c1) =>
        new("AVG", c1);
    public static FunctionCallNode<T, T> Min<T>(this ISql<T> c1) =>
        new("MIN", c1);
    public static FunctionCallNode<T, T> Max<T>(this ISql<T> c1) =>
        new("MAX", c1);

    public static FunctionCallNode<T, double> StdDev<T>(this ISql<T> c1) =>
        new("STDDEV", c1);
    public static FunctionCallNode<T, double> Variance<T>(this ISql<T> c1) =>
        new("VARIANCE", c1);
    public static FunctionCallNode<T, double> VarSample<T>(this ISql<T> c1) =>
        new("VAR_SAMP", c1);
    public static FunctionCallNode<T, double> VarPop<T>(this ISql<T> c1) =>
        new("VAR_POP", c1);
    public static FunctionCallNode<T, double> StdDevSample<T>(this ISql<T> c1) =>
        new("STDDEV_SAMP", c1);
    public static FunctionCallNode<T, double> StdDevPop<T>(this ISql<T> c1) =>
        new("STDDEV_POP", c1);
}
