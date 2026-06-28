using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.MySql;

public static partial class MySqlFunctions
{
    // ======================================================================
    // JSON Functions (MySQL 8.0+)
    // ======================================================================
    
    public static FunctionCallNode<T, string, V> JsonExtract<T, V>(ISql<T> c1, string path)
        => new("JSON_EXTRACT", c1, new SqlValueNode<string>(path));
    
    public static FunctionCallNode<string> JsonExtractText<T>(ISql<T> c1, string path)
        => new("JSON_UNQUOTE", 
            new FunctionCallNode<string>("JSON_EXTRACT", c1, new SqlValueNode<string>(path)));
    
    public static BinaryNode<T, string, V> JsonArrow<T, V>(ISql<T> c1, string path)
        => new(c1, new SqlValueNode<string>(path), " -> ");
    
    public static BinaryNode<T, string, string> JsonArrowText<T>(ISql<T> c1, string path)
        => new(c1, new SqlValueNode<string>(path), " ->> ");
    
    public static FunctionCallNode<T, T> JsonArrayAgg<T>(ISql<T> c1)
        => new("JSON_ARRAYAGG", c1);
    
    public static FunctionCallNode<string> JsonObjectAgg(IGenericSql key, IGenericSql value)
        => new("JSON_OBJECTAGG", key, value);
    
    public static FunctionCallNode<string> JsonArray(params IGenericSql[] values)
        => new("JSON_ARRAY", values);
    
    public static FunctionCallNode<string> JsonObject(params IGenericSql[] keyValuePairs)
        => new("JSON_OBJECT", keyValuePairs);
    
    public static FunctionCallNode<bool> JsonContains(IGenericSql c1, IGenericSql value)
        => new("JSON_CONTAINS", c1, value);
    
    public static FunctionCallNode<T, int> JsonLength<T>(ISql<T> c1)
        => new("JSON_LENGTH", c1);
    
    public static FunctionCallNode<T, T> JsonKeys<T>(ISql<T> c1)
        => new("JSON_KEYS", c1);
}
