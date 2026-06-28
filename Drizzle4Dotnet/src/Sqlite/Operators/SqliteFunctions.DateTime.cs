using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Sqlite;

public static partial class SqliteFunctions
{
    /// <summary>
    /// SQLite date and time functions.
    /// SQLite uses datetime(), date(), time(), strftime(), and julianday()
    /// with modifier syntax for date arithmetic.
    /// </summary>
    public static class DateTime
    {
        /// <summary>
        /// Returns current date and time as 'YYYY-MM-DD HH:MM:SS'.
        /// Renders as: datetime('now')
        /// </summary>
        public static ISql<string> Now() => new RawSql<string>("datetime('now')");
        
        /// <summary>
        /// Returns current date as 'YYYY-MM-DD'.
        /// Renders as: date('now')
        /// </summary>
        public static ISql<string> CurrentDate() => new RawSql<string>("date('now')");
        
        /// <summary>
        /// Returns current time as 'HH:MM:SS'.
        /// Renders as: time('now')
        /// </summary>
        public static ISql<string> CurrentTime() => new RawSql<string>("time('now')");
        
        /// <summary>
        /// Formats a datetime value using strftime format string.
        /// Renders as: strftime(format, dateTime)
        /// </summary>
        public static FunctionCallNode<string> Strftime(string format, ISql<string> dateTime)
            => new("strftime", Sql.Value(format), dateTime);
        
        /// <summary>
        /// Converts a date/datetime to a Unix timestamp (seconds since 1970-01-01).
        /// Renders as: strftime('%s', dateTime)
        /// </summary>
        public static ISql<long> UnixTimestamp(ISql<string> dateTime)
            => new RawSql<long>($"CAST(strftime('%s', ...) AS INTEGER)");
        
        /// <summary>
        /// Adds a time interval to a date using SQLite modifier syntax.
        /// Renders as: date(dateTime, '+N days')
        /// </summary>
        public static FunctionCallNode<string> DateAdd(ISql<string> date, string modifier)
            => new("date", date, Sql.Value(modifier));
        
        /// <summary>
        /// Adds a time interval to a datetime using SQLite modifier syntax.
        /// Renders as: datetime(dateTime, '+N days')
        /// </summary>
        public static FunctionCallNode<string> DateTimeAdd(ISql<string> dateTime, string modifier)
            => new("datetime", dateTime, Sql.Value(modifier));
        
        /// <summary>
        /// Computes the Julian day number.
        /// Renders as: julianday(dateTime)
        /// </summary>
        public static FunctionCallNode<double> JulianDay(ISql<string> dateTime)
            => new("julianday", dateTime);
    }
}
