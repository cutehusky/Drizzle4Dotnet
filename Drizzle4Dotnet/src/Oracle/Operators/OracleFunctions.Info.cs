using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle.Operators;

public static partial class OracleFunctions
{
    /// <summary>
    /// Oracle informational functions and pseudo-columns.
    /// </summary>
    public static class Info
    {
        /// <summary>Current database user. Renders: USER</summary>
        public static ISql<string> User() => new RawSql<string>("USER");

        /// <summary>User ID. Renders: UID</summary>
        public static ISql<int> Uid() => new RawSql<int>("UID");

        /// <summary>Row physical identifier. Renders: ROWID</summary>
        public static ISql<string> RowId() => new RawSql<string>("ROWID");

        /// <summary>Row number (before ordering). Renders: ROWNUM</summary>
        public static ISql<int> RowNum() => new RawSql<int>("ROWNUM");

        /// <summary>Current schema. Renders: SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')</summary>
        public static ISql<string> CurrentSchema()
            => new RawSql<string>("SYS_CONTEXT('USERENV', 'CURRENT_SCHEMA')");

        /// <summary>Database name. Renders: SYS_CONTEXT('USERENV', 'DB_NAME')</summary>
        public static ISql<string> DbName()
            => new RawSql<string>("SYS_CONTEXT('USERENV', 'DB_NAME')");

        /// <summary>Instance name. Renders: SYS_CONTEXT('USERENV', 'INSTANCE_NAME')</summary>
        public static ISql<string> InstanceName()
            => new RawSql<string>("SYS_CONTEXT('USERENV', 'INSTANCE_NAME')");

        /// <summary>Session ID. Renders: SYS_CONTEXT('USERENV', 'SID')</summary>
        public static ISql<int> SessionId()
            => new RawSql<int>("SYS_CONTEXT('USERENV', 'SID')");

        /// <summary>Client host. Renders: SYS_CONTEXT('USERENV', 'HOST')</summary>
        public static ISql<string> Host()
            => new RawSql<string>("SYS_CONTEXT('USERENV', 'HOST')");

        /// <summary>Server host. Renders: SYS_CONTEXT('USERENV', 'SERVER_HOST')</summary>
        public static ISql<string> ServerHost()
            => new RawSql<string>("SYS_CONTEXT('USERENV', 'SERVER_HOST')");

        /// <summary>NLS language. Renders: SYS_CONTEXT('USERENV', 'NLS_LANGUAGE')</summary>
        public static ISql<string> NlsLanguage()
            => new RawSql<string>("SYS_CONTEXT('USERENV', 'NLS_LANGUAGE')");

        /// <summary>NLS territory. Renders: SYS_CONTEXT('USERENV', 'NLS_TERRITORY')</summary>
        public static ISql<string> NlsTerritory()
            => new RawSql<string>("SYS_CONTEXT('USERENV', 'NLS_TERRITORY')");
    }
}
