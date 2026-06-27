using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

// ======================================================================
// Date/Time Functions (Standard SQL): Now, CurrentTimestamp, CurrentDate
// ======================================================================
public static partial class Functions
{
    public static FunctionCallNode<DateTime> Now() => new("NOW");
    public static FunctionCallNode<DateTime> CurrentTimestamp() => new("CURRENT_TIMESTAMP");
    public static FunctionCallNode<DateTime> CurrentDate() => new("CURRENT_DATE");
}
