using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.MySql;

public static partial class MySqlOperators
{
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2>(ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, OpsNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T>(ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), OpsNullSafeEqual);
}

public static class MySqlOperatorsNullSafeExtensions
{
    public static BinaryNode<T1, T2, bool> NullSafeEqual<T1, T2>(
        this ISql<T1> c1, ISql<T2> c2)
        => new(c1, c2, MySqlOperators.OpsNullSafeEqual);
    public static BinaryNode<T, T, bool> NullSafeEqual<T>(
        this ISql<T> c1, T value)
        => new(c1, new SqlValueNode<T>(value), MySqlOperators.OpsNullSafeEqual);
}
