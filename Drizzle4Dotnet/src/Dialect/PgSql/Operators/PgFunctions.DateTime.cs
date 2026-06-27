using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgFunctions
{
    // ======================================================================
    // Date/Time Functions (PostgreSQL-specific)
    // ======================================================================
    
    public static FunctionCallNode<double> Extract(IGenericSql field, ISql c1) =>
        new("EXTRACT", field, new RawSql<double>("FROM "), c1);
    public static FunctionCallNode<double> Extract(string field, ISql c1)
        => new("EXTRACT", new SqlValueNode<string>(field), new RawSql<double>("FROM "), c1);
    
    public static FunctionCallNode<DateTime> DateTrunc(IGenericSql precision, ISql c1) =>
        new("DATE_TRUNC", precision, c1);
    public static FunctionCallNode<DateTime> DateTrunc(string precision, ISql c1) =>
        new("DATE_TRUNC", new SqlValueNode<string>(precision), c1);
    
    public static BinaryNode<DateTime> DateAdd(ISql<DateTime> c1, int amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " + ");
    public static BinaryNode<DateTime> DateDiff(ISql<DateTime> c1, int amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " - ");
    public static BinaryNode<DateTime> DateAdd(ISql<DateTime> c1, double amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " + ");
    public static BinaryNode<DateTime> DateDiff(ISql<DateTime> c1, double amount, string unit)
        => new(c1, PgSqlStatics.Interval(amount, unit), " - ");
    
    public static BinaryNode<DateTime, string, DateTime> AtTimeZone(ISql<DateTime> c1, ISql<string> timezone)
        => new(c1, timezone, " AT TIME ZONE ");
    public static BinaryNode<DateTime, string, DateTime> AtTimeZone(ISql<DateTime> c1, string timezone)
        => new(c1, PgSqlStatics.TimeZone(timezone), " AT TIME ZONE ");
    
    public static FunctionCallNode<DateTime, TimeSpan> Age(ISql<DateTime> c1) => new("AGE", c1);
    public static FunctionCallNode<DateTime, DateTime, TimeSpan> Age(ISql<DateTime> c1, ISql<DateTime> c2) =>
        new("AGE", c1, c2);
}
