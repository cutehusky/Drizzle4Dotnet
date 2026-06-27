using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;
using Drizzle4Dotnet.MySql.Nodes;

namespace Drizzle4Dotnet.MySql;

public static partial class MySqlFunctions
{
    // ======================================================================
    // Date/Time Functions (MySQL-specific)
    // ======================================================================
    
    public static FunctionCallNode<DateTime> CurDate()
        => new("CURDATE");
    
    public static FunctionCallNode<TimeSpan> CurTime()
        => new("CURTIME");
    
    public static FunctionCallNode<DateTime> DateAdd(ISql<DateTime> date, int amount, string unit)
        => new("DATE_ADD", date, new MySqlIntervalNode(amount, unit));
    
    public static FunctionCallNode<DateTime> DateSub(ISql<DateTime> date, int amount, string unit)
        => new("DATE_SUB", date, new MySqlIntervalNode(amount, unit));
    
    public static FunctionCallNode<DateTime, string, string> DateFormat(ISql<DateTime> date, string format)
        => new("DATE_FORMAT", date, new SqlValueNode<string>(format));
    
    public static FunctionCallNode<DateTime, long> UnixTimestamp(ISql<DateTime> date)
        => new("UNIX_TIMESTAMP", date);
    
    public static FunctionCallNode<long, DateTime> FromUnixTime(ISql<long> timestamp)
        => new("FROM_UNIXTIME", timestamp);
    
    public static FunctionCallNode<DateTime> StrToDate(IGenericSql str, string format)
        => new("STR_TO_DATE", str, new SqlValueNode<string>(format));
}
