using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Sqlite;

public static partial class SqliteFunctions
{
    /// <summary>
    /// SQLite JSON functions (built-in since SQLite 3.38+).
    /// </summary>
    public static class Json
    {
        /// <summary>
        /// Validates and minifies a JSON string.
        /// Renders as: json(value)
        /// </summary>
        public static FunctionCallNode<string> JsonMinify(ISql<string> value)
            => new("json", value);
        
        /// <summary>
        /// Extracts a JSON value at the specified path.
        /// Renders as: json_extract(value, path)
        /// </summary>
        public static FunctionCallNode<string> Extract(ISql<string> value, ISql<string> path)
            => new("json_extract", value, path);
        
        /// <summary>
        /// Creates a JSON object from key-value pairs.
        /// Renders as: json_object(key1, value1, key2, value2, ...)
        /// </summary>
        public static FunctionCallNode<string> Object(params ISql<object>[] args)
            => new("json_object", args);
        
        /// <summary>
        /// Creates a JSON array from values.
        /// Renders as: json_array(value1, value2, ...)
        /// </summary>
        public static FunctionCallNode<string> Array(params ISql<object>[] args)
            => new("json_array", args);
        
        /// <summary>
        /// Sets a JSON value at the specified path.
        /// Renders as: json_set(value, path, newValue)
        /// </summary>
        public static FunctionCallNode<string> Set(ISql<string> value, ISql<string> path, ISql<string> newValue)
            => new("json_set", value, path, newValue);
        
        /// <summary>
        /// Removes a key from a JSON object at the specified path.
        /// Renders as: json_remove(value, path)
        /// </summary>
        public static FunctionCallNode<string> Remove(ISql<string> value, ISql<string> path)
            => new("json_remove", value, path);
        
        /// <summary>
        /// Returns the type of a JSON value.
        /// Renders as: json_type(value, path?)
        /// </summary>
        public static FunctionCallNode<string> Type(ISql<string> value, ISql<string>? path = null)
        {
            if (path != null)
                return new FunctionCallNode<string>("json_type", value, path);
            return new FunctionCallNode<string>("json_type", value);
        }
        
        /// <summary>
        /// Checks if a string is valid JSON.
        /// Renders as: json_valid(value)
        /// </summary>
        public static ISql<bool> Valid(ISql<string> value)
            => new RawSql<bool>("json_valid(...)");
        
        /// <summary>
        /// Aggregate function that returns a JSON array of all values.
        /// Renders as: json_group_array(value)
        /// </summary>
        public static FunctionCallNode<string> GroupArray(ISql<object> value)
            => new("json_group_array", value);
        
        /// <summary>
        /// Aggregate function that returns a JSON object of key-value pairs.
        /// Renders as: json_group_object(key, value)
        /// </summary>
        public static FunctionCallNode<string> GroupObject(ISql<string> key, ISql<object> value)
            => new("json_group_object", key, value);
    }
}
