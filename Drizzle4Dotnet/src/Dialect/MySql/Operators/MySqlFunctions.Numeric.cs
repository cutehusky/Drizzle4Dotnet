using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Core.Shared.Operators;
using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.MySql;

public static partial class MySqlFunctions
{
    // ======================================================================
    // Numeric / Math Functions (MySQL-specific)
    // ======================================================================
    
    public static FunctionCallNode<T> Rand<T>()
        => new("RAND");
    
    public static FunctionCallNode<T, int, T> Truncate<T>(ISql<T> c1, int decimals)
        => new("TRUNCATE", c1, new SqlValueNode<int>(decimals));
    
    public static FunctionCallNode<T, long> BitCount<T>(ISql<T> c1)
        => new("BIT_COUNT", c1);
    
    public static FunctionCallNode<T, long> Crc32<T>(ISql<T> c1)
        => new("CRC32", c1);
}
