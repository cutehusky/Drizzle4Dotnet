using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql;

// ======================================================================
// MSSQL System/Information Functions
// ======================================================================
public static partial class MssqlFunctions
{
    /// <summary>
    /// MSSQL system and information functions.
    /// </summary>
    public static class Info
    {
        /// <summary>
        /// Returns the last inserted identity value (scope-scoped).
        /// Equivalent to LAST_INSERT_ID() in MySQL.
        /// </summary>
        public static ISql<long> ScopeIdentity() => new RawSql<long>("SELECT SCOPE_IDENTITY()");

        /// <summary>
        /// Returns @@IDENTITY — last identity value in any scope.
        /// </summary>
        public static ISql<long> Identity() => new RawSql<long>("SELECT @@IDENTITY");

        /// <summary>
        /// Returns @@ROWCOUNT — number of rows affected by the last statement.
        /// </summary>
        public static ISql<int> RowCount() => new RawSql<int>("SELECT @@ROWCOUNT");

        /// <summary>
        /// Returns @@VERSION — SQL Server version information.
        /// </summary>
        public static ISql<string> Version() => new RawSql<string>("SELECT @@VERSION");

        /// <summary>
        /// Returns @@SERVERNAME — name of the local server.
        /// </summary>
        public static ISql<string> ServerName() => new RawSql<string>("SELECT @@SERVERNAME");

        /// <summary>
        /// Returns DB_NAME() — current database name.
        /// </summary>
        public static ISql<string> DbName() => new RawSql<string>("SELECT DB_NAME()");

        /// <summary>
        /// Returns @@DBTS — current database timestamp.
        /// </summary>
        public static ISql<byte[]> DbTs() => new RawSql<byte[]>("SELECT @@DBTS");
    }
}
