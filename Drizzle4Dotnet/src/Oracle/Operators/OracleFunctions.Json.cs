using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Oracle;

public static partial class OracleFunctions
{
    /// <summary>
    /// Oracle JSON functions (12c+).
    /// </summary>
    public static class Json
    {
        /// <summary>Extract scalar JSON value. Renders: JSON_VALUE(expr, '$.path')</summary>
        public static FunctionCallNode<string> JsonValue(ISql<string> expr, string path)
            => new("JSON_VALUE", expr, Sql.Value(path));

        /// <summary>Extract JSON object/array. Renders: JSON_QUERY(expr, '$.path')</summary>
        public static FunctionCallNode<string> JsonQuery(ISql<string> expr, string path)
            => new("JSON_QUERY", expr, Sql.Value(path));

        /// <summary>Check JSON path existence. Renders: JSON_EXISTS(expr, '$.path')</summary>
        public static FunctionCallNode<bool> JsonExists(ISql<string> expr, string path)
            => new("JSON_EXISTS", expr, Sql.Value(path));

        /// <summary>Create JSON object. Renders: JSON_OBJECT(key VALUE val, ...)</summary>
        public static FunctionCallNode<string> JsonObject(params (string key, IGenericSql value)[] entries)
        {
            var args = new List<IGenericSql>();
            foreach (var (key, value) in entries)
            {
                args.Add(Sql.Value(key));
                args.Add(new RawSql("VALUE"));
                args.Add(value);
            }
            return new FunctionCallNode<string>("JSON_OBJECT", args.ToArray());
        }

        /// <summary>Create JSON array. Renders: JSON_ARRAY(val1, val2, ...)</summary>
        public static FunctionCallNode<string> JsonArray(params IGenericSql[] values)
            => new("JSON_ARRAY", values);

        /// <summary>Aggregate to JSON array. Renders: JSON_ARRAYAGG(expr)</summary>
        public static FunctionCallNode<string> JsonArrayagg(ISql<string> expr)
            => new("JSON_ARRAYAGG", expr);

        /// <summary>Aggregate to JSON object. Renders: JSON_OBJECTAGG(key VALUE val)</summary>
        public static FunctionCallNode<string> JsonObjectagg(ISql<string> key, IGenericSql value)
            => new("JSON_OBJECTAGG", key, new RawSql("VALUE"), value);

        /// <summary>JSON merge (18c+). Renders: JSON_MERGEPATCH(target, patch)</summary>
        public static FunctionCallNode<string> JsonMergepatch(ISql<string> target, ISql<string> patch)
            => new("JSON_MERGEPATCH", target, patch);
    }
}
