using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgFunctions
{
    // ======================================================================
    // Window Functions (PostgreSQL)
    // ======================================================================

    public static FunctionCallNode<long> RowNumber() => new("ROW_NUMBER");
    public static FunctionCallNode<long> Rank() => new("RANK");
    public static FunctionCallNode<long> DenseRank() => new("DENSE_RANK");

    public static FunctionCallNode<int, long> Ntile(int n)
        => new("NTILE", new SqlValueNode<int>(n));

    public static FunctionCallNode<T> Lead<T>(ISql<T> c1, int offset, IGenericSql defaultValue)
        => new("LEAD", c1, new SqlValueNode<int>(offset), defaultValue);
    public static FunctionCallNode<T, int, T> Lead<T>(ISql<T> c1, int offset) =>
        new("LEAD", c1, new SqlValueNode<int>(offset));
    public static FunctionCallNode<T, T> Lead<T>(ISql<T> c1) => new("LEAD", c1);

    public static FunctionCallNode<T> Lag<T>(ISql<T> c1, int offset, IGenericSql defaultValue)
        => new("LAG", c1, new SqlValueNode<int>(offset), defaultValue);
    public static FunctionCallNode<T, int, T> Lag<T>(ISql<T> c1, int offset) =>
        new("LAG", c1, new SqlValueNode<int>(offset));
    public static FunctionCallNode<T, T> Lag<T>(ISql<T> c1) => new("LAG", c1);

    public static FunctionCallNode<T, T> FirstValue<T>(ISql<T> c1) => new("FIRST_VALUE", c1);
    public static FunctionCallNode<T, T> LastValue<T>(ISql<T> c1) => new("LAST_VALUE", c1);
    public static FunctionCallNode<T, int, T> NthValue<T>(ISql<T> c1, int n)
        => new("NTH_VALUE", c1, new SqlValueNode<int>(n));
}
