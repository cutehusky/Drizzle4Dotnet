using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.PgSql;

public static partial class PgFunctions
{
    // ======================================================================
    // Other PostgreSQL-specific Functions
    // ======================================================================
    
    public static FunctionCallNode<T> Random<T>() => new("RANDOM");
    
    public static CastNode<T> CastPg<T>(IGenericSql expression, string targetType) =>
        new(expression, targetType, usePostgresSyntax: true);
    
    public static FunctionCallNode<string> ConcatWs(IGenericSql separator, params IGenericSql[] columns)
    {
        var args = new List<IGenericSql> { separator };
        args.AddRange(columns);
        return new FunctionCallNode<string>("CONCAT_WS", args.ToArray());
    }
    public static FunctionCallNode<string> ConcatWs(string separator, params IGenericSql[] columns)
    {
        var args = new List<IGenericSql> { new SqlValueNode<string>(separator) };
        args.AddRange(columns);
        return new FunctionCallNode<string>("CONCAT_WS", args.ToArray());
    }
}
