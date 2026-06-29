using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Operators;

// ======================================================================
// MSSQL Date/Time Functions
// ======================================================================
public static partial class MssqlFunctions
{
    /// <summary>
    /// MSSQL date and time functions.
    /// MSSQL uses different function names than PostgreSQL/MySQL.
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        /// GETDATE() — current date and time.
        /// </summary>
        public static FunctionCallNode<System.DateTime> GetDate()
            => new FunctionCallNode<System.DateTime>("GETDATE");

        /// <summary>
        /// SYSDATETIME() — more precise current date and time.
        /// </summary>
        public static FunctionCallNode<System.DateTime> SysDateTime()
            => new FunctionCallNode<System.DateTime>("SYSDATETIME");

        /// <summary>
        /// GETUTCDATE() — current UTC date and time.
        /// </summary>
        public static FunctionCallNode<System.DateTime> GetUtcDate()
            => new FunctionCallNode<System.DateTime>("GETUTCDATE");

        /// <summary>
        /// CURRENT_TIMESTAMP — current date and time (ANSI standard).
        /// </summary>
        public static FunctionCallNode<System.DateTime> CurrentTimestamp()
            => new FunctionCallNode<System.DateTime>("CURRENT_TIMESTAMP");

        /// <summary>
        /// DATEADD(unit, amount, date) — adds a specified number of date/time units to a date.
        /// </summary>
        public static FunctionCallNode<System.DateTime> DateAdd(IGenericSql unit, IGenericSql amount, ISql<System.DateTime> date)
            => new FunctionCallNode<System.DateTime>("DATEADD", unit, amount, date);

        /// <summary>
        /// DATEDIFF(unit, start, end) — returns the count of date/time boundaries crossed.
        /// </summary>
        public static FunctionCallNode<long> DateDiff(IGenericSql unit, ISql<System.DateTime> start, ISql<System.DateTime> end)
            => new FunctionCallNode<long>("DATEDIFF", unit, start, end);

        /// <summary>
        /// DATEPART(unit, date) — returns a single part of a date as an integer.
        /// </summary>
        public static FunctionCallNode<int> DatePart(IGenericSql unit, ISql<System.DateTime> date)
            => new FunctionCallNode<int>("DATEPART", unit, date);

        /// <summary>
        /// DATENAME(unit, date) — returns a character string representing the specified date part.
        /// </summary>
        public static FunctionCallNode<string> DateName(IGenericSql unit, ISql<System.DateTime> date)
            => new FunctionCallNode<string>("DATENAME", unit, date);

        /// <summary>
        /// YEAR(date) — returns the year as an integer.
        /// </summary>
        public static UnaryNode<System.DateTime, int> Year(ISql<System.DateTime> date)
            => new(date, "YEAR", true);

        /// <summary>
        /// MONTH(date) — returns the month as an integer.
        /// </summary>
        public static UnaryNode<System.DateTime, int> Month(ISql<System.DateTime> date)
            => new(date, "MONTH", true);

        /// <summary>
        /// DAY(date) — returns the day of the month as an integer.
        /// </summary>
        public static UnaryNode<System.DateTime, int> Day(ISql<System.DateTime> date)
            => new(date, "DAY", true);

        /// <summary>
        /// EOMONTH(date) — returns the last day of the month containing the specified date.
        /// </summary>
        public static UnaryNode<System.DateTime, System.DateTime> EoMonth(ISql<System.DateTime> date)
            => new(date, "EOMONTH", true);

        /// <summary>
        /// FORMAT(date, format) — formats a date using .NET format string (slow, use only when necessary).
        /// </summary>
        public static FunctionCallNode<string> Format(ISql<System.DateTime> date, string format)
            => new FunctionCallNode<string>("FORMAT", date, new SqlValueNode<string>(format));

        /// <summary>
        /// ISDATE(value) — checks if a value is a valid date (returns 1/0).
        /// </summary>
        public static UnaryNode<string, int> IsDate(ISql<string> value)
            => new(value, "ISDATE", true);
    }
}
