using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle;

public static partial class OracleFunctions
{
    /// <summary>
    /// Oracle date and time functions.
    /// Oracle provides SYSDATE, SYSTIMESTAMP, ADD_MONTHS(), TRUNC(), EXTRACT(), etc.
    /// </summary>
    public static class DateTime
    {
        /// <summary>Current date in session time zone. Renders: CURRENT_DATE</summary>
        public static ISql<string> CurrentDate() => new RawSql<string>("CURRENT_DATE");

        /// <summary>Current timestamp in session time zone. Renders: CURRENT_TIMESTAMP</summary>
        public static ISql<string> CurrentTimestamp() => new RawSql<string>("CURRENT_TIMESTAMP");

        /// <summary>Current date/time (database server). Renders: SYSDATE</summary>
        public static ISql<string> Sysdate() => new RawSql<string>("SYSDATE");

        /// <summary>Current timestamp with time zone. Renders: SYSTIMESTAMP</summary>
        public static ISql<string> Systimestamp() => new RawSql<string>("SYSTIMESTAMP");

        /// <summary>Add months to date. Renders: ADD_MONTHS(date, n)</summary>
        public static FunctionCallNode<string> AddMonths(ISql<string> date, int n)
            => new("ADD_MONTHS", date, Sql.Value(n));

        /// <summary>Months between two dates. Renders: MONTHS_BETWEEN(d1, d2)</summary>
        public static FunctionCallNode<double> MonthsBetween(ISql<string> d1, ISql<string> d2)
            => new("MONTHS_BETWEEN", d1, d2);

        /// <summary>Truncate date. Renders: TRUNC(date, fmt)</summary>
        public static FunctionCallNode<string> Trunc(ISql<string> date, string? format = null)
            => format != null ? new FunctionCallNode<string>("TRUNC", date, Sql.Value(format)) : new FunctionCallNode<string>("TRUNC", date);

        /// <summary>Extract date parts. Renders: EXTRACT(YEAR/MONTH/DAY FROM date)</summary>
        public static IGenericSql Extract(string part, ISql<string> date)
            => new RawSql($"EXTRACT({part} FROM {date})");

        /// <summary>String to date. Renders: TO_DATE(str, fmt)</summary>
        public static FunctionCallNode<string> ToDate(ISql<string> str, string format)
            => new("TO_DATE", str, Sql.Value(format));

        /// <summary>Date to string. Renders: TO_CHAR(date, fmt)</summary>
        public static FunctionCallNode<string> ToChar(ISql<string> date, string format)
            => new("TO_CHAR", date, Sql.Value(format));

        /// <summary>Next occurrence of day. Renders: NEXT_DAY(date, 'MONDAY')</summary>
        public static FunctionCallNode<string> NextDay(ISql<string> date, string dayOfWeek)
            => new("NEXT_DAY", date, Sql.Value(dayOfWeek));

        /// <summary>Last day of month. Renders: LAST_DAY(date)</summary>
        public static FunctionCallNode<string> LastDay(ISql<string> date)
            => new("LAST_DAY", date);

        /// <summary>Number to day-second interval. Renders: NUMTODSINTERVAL(n, 'DAY')</summary>
        public static FunctionCallNode<string> NumToDsInterval(int n, string unit)
            => new("NUMTODSINTERVAL", Sql.Value(n), Sql.Value(unit));

        /// <summary>Number to year-month interval. Renders: NUMTOYMINTERVAL(n, 'MONTH')</summary>
        public static FunctionCallNode<string> NumToYmInterval(int n, string unit)
            => new("NUMTOYMINTERVAL", Sql.Value(n), Sql.Value(unit));

        /// <summary>Timestamp with time zone. Renders: FROM_TZ(timestamp, tz)</summary>
        public static FunctionCallNode<string> FromTz(ISql<string> timestamp, string tz)
            => new("FROM_TZ", timestamp, Sql.Value(tz));

        /// <summary>Time zone conversion. Renders: timestamp AT TIME ZONE 'tz'</summary>
        public static IGenericSql AtTimeZone(ISql<string> timestamp, string tz)
            => new RawSql($"{timestamp} AT TIME ZONE '{tz}'");
    }
}
