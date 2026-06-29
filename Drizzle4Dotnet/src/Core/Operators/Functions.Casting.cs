using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Operators;

// ======================================================================
// Type Casting: Cast, CastToString, CastToInt, CastToLong, CastToDouble, CastToDateTime
// ======================================================================
public static partial class Functions
{
    public static CastNode<T> Cast<T>(IGenericSql expression, string targetType) =>
        new(expression, targetType, usePostgresSyntax: false);
    
    public static CastNode<string> CastToString(IGenericSql expression) =>
        new(expression, "TEXT", usePostgresSyntax: false);
    public static CastNode<int> CastToInt(IGenericSql expression) =>
        new(expression, "INTEGER", usePostgresSyntax: false);
    public static CastNode<long> CastToLong(IGenericSql expression) =>
        new(expression, "BIGINT", usePostgresSyntax: false);
    public static CastNode<double> CastToDouble(IGenericSql expression) =>
        new(expression, "DOUBLE PRECISION", usePostgresSyntax: false);
    public static CastNode<DateTime> CastToDateTime(IGenericSql expression) =>
        new(expression, "TIMESTAMP", usePostgresSyntax: false);
}
