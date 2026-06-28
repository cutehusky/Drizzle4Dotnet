using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgFunctions
{
    // ======================================================================
    // JSON Functions (PostgreSQL)
    // ======================================================================
    
    public static BinaryNode<T, string, V> JsonExtract<T, V>(ISql<T> c1, ISql<string> path)
        => new(c1, path, " -> ");
    public static BinaryNode<T, string, string> JsonExtractText<T>(ISql<T> c1, ISql<string> path)
        => new(c1, path, " ->> ");
    
    public static FunctionCallNode<T, T> JsonAgg<T>(ISql<T> c1) => new("JSON_AGG", c1);
    
    public static FunctionCallNode<string> JsonBuildObject(params IGenericSql[] keyValuePairs) =>
        new("JSON_BUILD_OBJECT", keyValuePairs);
    
    public static FunctionCallNode<T, int> JsonArrayLength<T>(ISql<T> c1) => new("JSON_ARRAY_LENGTH", c1);
    
    public static FunctionCallNode<T, T> ToJson<T>(ISql<T> c1) => new("TO_JSON", c1);
    
    public static FunctionCallNode<T, T> RowToJson<T>(ISql<T> c1) => new("ROW_TO_JSON", c1);
}

public static class PgFunctionsJsonExtensions
{
    public static FunctionCallNode<T, T> JsonAgg<T>(this ISql<T> c1) =>
        new("JSON_AGG", c1);

    public static FunctionCallNode<T, int> JsonArrayLength<T>(this ISql<T> c1) =>
        new("JSON_ARRAY_LENGTH", c1);
}
