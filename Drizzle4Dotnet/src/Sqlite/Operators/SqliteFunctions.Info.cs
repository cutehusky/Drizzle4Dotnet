using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Sqlite;

public static partial class SqliteFunctions
{
    /// <summary>
    /// SQLite information and metadata functions.
    /// </summary>
    public static class Info
    {
        /// <summary>
        /// Returns the rowid of the last inserted row in the current connection.
        /// Renders as: last_insert_rowid()
        /// </summary>
        public static FunctionCallNode<long> LastInsertRowId() => new("last_insert_rowid");
        
        /// <summary>
        /// Returns the SQLite library version string.
        /// Renders as: sqlite_version()
        /// </summary>
        public static FunctionCallNode<string> SqliteVersion() => new("sqlite_version");
        
        /// <summary>
        /// Returns the number of rows changed by the last statement.
        /// Renders as: changes()
        /// </summary>
        public static FunctionCallNode<int> Changes() => new("changes");
        
        /// <summary>
        /// Returns the total number of row changes since the connection was opened.
        /// Renders as: total_changes()
        /// </summary>
        public static FunctionCallNode<int> TotalChanges() => new("total_changes");
    }
}
