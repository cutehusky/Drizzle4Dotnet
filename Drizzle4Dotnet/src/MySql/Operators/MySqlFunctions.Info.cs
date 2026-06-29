using Drizzle4Dotnet.Core.Operators.Nodes;

namespace Drizzle4Dotnet.MySql.Operators;

public static partial class MySqlFunctions
{
    // ======================================================================
    // Info / Utility Functions
    // ======================================================================
    
    public static FunctionCallNode<long> LastInsertId()
        => new("LAST_INSERT_ID");
    
    public static FunctionCallNode<string> Database()
        => new("DATABASE");
    
    public static FunctionCallNode<string> User()
        => new("USER");
    
    public static FunctionCallNode<string> Version()
        => new("VERSION");
    
    public static FunctionCallNode<long> RowCount()
        => new("ROW_COUNT");
    
    public static FunctionCallNode<long> FoundRows()
        => new("FOUND_ROWS");
}
