using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql;

// ======================================================================
// MSSQL JSON Functions (SQL Server 2016+)
// ======================================================================
public static partial class MssqlFunctions
{
    /// <summary>
    /// MSSQL JSON functions (SQL Server 2016+).
    /// MSSQL uses different JSON function names than PostgreSQL.
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// JSON_VALUE(expr, path) — extracts a scalar value from a JSON string.
        /// Path syntax: '$.key' or 'lax $.key' (default: lax mode).
        /// </summary>
        public static FunctionCallNode<string> JsonValue(IGenericSql expr, string path)
            => new FunctionCallNode<string>("JSON_VALUE", expr, new SqlValueNode<string>(path));

        /// <summary>
        /// JSON_QUERY(expr, path) — extracts an object or array from a JSON string.
        /// Returns a JSON fragment (not a scalar value).
        /// </summary>
        public static FunctionCallNode<string> JsonQuery(IGenericSql expr, string path)
            => new FunctionCallNode<string>("JSON_QUERY", expr, new SqlValueNode<string>(path));

        /// <summary>
        /// JSON_MODIFY(expr, path, value) — updates a property in a JSON string.
        /// Returns the updated JSON string.
        /// </summary>
        public static FunctionCallNode<string> JsonModify(IGenericSql expr, string path, IGenericSql value)
            => new FunctionCallNode<string>("JSON_MODIFY", expr, new SqlValueNode<string>(path), value);

        /// <summary>
        /// ISJSON(expr) — checks if a string contains valid JSON (returns 1/0).
        /// </summary>
        public static UnaryNode<string, int> IsJson(ISql<string> expr)
            => new(expr, "ISJSON", true);

        /// <summary>
        /// JSON_PATH_EXISTS(expr, path) — checks if a path exists in a JSON string (SQL Server 2022+).
        /// </summary>
        public static FunctionCallNode<int> JsonPathExists(IGenericSql expr, string path)
            => new FunctionCallNode<int>("JSON_PATH_EXISTS", expr, new SqlValueNode<string>(path));
    }
}
