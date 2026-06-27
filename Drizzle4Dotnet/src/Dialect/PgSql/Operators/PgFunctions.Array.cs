using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgFunctions
{
    // ======================================================================
    // Array Functions (PostgreSQL)
    // ======================================================================
    
    public static FunctionCallNode<T, T> ArrayAgg<T>(ISql<T> c1) => new("ARRAY_AGG", c1);
    public static FunctionCallNode<T, T> Unnest<T>(ISql<T> c1) => new("UNNEST", c1);
    
    public static FunctionCallNode<int> ArrayLength(IGenericSql c1, int dimension = 1) =>
        new("ARRAY_LENGTH", c1, new SqlValueNode<int>(dimension));
    
    public static BinaryNode<T, T[], bool> ArrayAny<T>(ISql<T> value, ISql<T[]> arrayCol)
        => new(value, arrayCol, " = ANY ");
    
    public static BinaryNode<T, T[], bool> ArrayAll<T>(ISql<T> value, ISql<T[]> arrayCol)
        => new(value, arrayCol, " = ALL ");
}

public static class PgFunctionsArrayExtensions
{
    public static FunctionCallNode<T, T> ArrayAgg<T>(this ISql<T> c1) =>
        new("ARRAY_AGG", c1);

    public static FunctionCallNode<T, T> Unnest<T>(this ISql<T> c1) =>
        new("UNNEST", c1);
}
