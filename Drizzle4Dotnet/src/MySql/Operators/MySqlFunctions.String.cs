using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.MySql.Operators.Nodes;

namespace Drizzle4Dotnet.MySql.Operators;

public static partial class MySqlFunctions
{
    // ======================================================================
    // String Functions (MySQL-specific)
    // ======================================================================
    
    public static FunctionCallNode<string> Concat(params IGenericSql[] args)
        => new("CONCAT", args);
    
    public static FunctionCallNode<string> Concat(params string[] args)
        => new("CONCAT", 
            args.Select(a => new SqlValueNode<string>(a)).Cast<IGenericSql>().ToArray());
    
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
    
    public static FunctionCallNode<T, long> CharLength<T>(ISql<T> c1) 
        => new("CHAR_LENGTH", c1);
    
    public static FunctionCallNode<long> Locate(IGenericSql substr, IGenericSql str)
        => new("LOCATE", substr, str);
    
    public static FunctionCallNode<long> Locate(IGenericSql substr, IGenericSql str, int pos)
        => new("LOCATE", substr, str, new SqlValueNode<int>(pos));
    
    public static FunctionCallNode<string> SubstringIndex(IGenericSql str, IGenericSql delim, int count)
        => new("SUBSTRING_INDEX", str, delim, new SqlValueNode<int>(count));
    
    public static MySqlPositionNode Position(IGenericSql substring, ISql<string> c1)
        => new(substring, c1);
    
    public static MySqlPositionNode Position(ISql<string> c1, string substring)
        => new(new SqlValueNode<string>(substring), c1);
}

public static class MySqlFunctionsStringExtensions
{
    public static FunctionCallNode<T, long> CharLength<T>(this ISql<T> c1)
        => new("CHAR_LENGTH", c1);
    
    public static MySqlPositionNode Position(this ISql<string> c1, string substring) 
        => new(new SqlValueNode<string>(substring), c1);
}
