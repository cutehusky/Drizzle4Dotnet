using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

public static class Functions
{
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
}