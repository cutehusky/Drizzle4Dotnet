using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Sqlite;

public static partial class SqliteFunctions
{
    /// <summary>
    /// SQLite numeric functions.
    /// </summary>
    public static class Numeric
    {
        /// <summary>
        /// Returns a random integer between -2^63 and 2^63-1.
        /// Renders as: random()
        /// </summary>
        public static FunctionCallNode<long> Random() => new("random");
        
        /// <summary>
        /// Returns the absolute value of a number.
        /// Renders as: abs(value)
        /// </summary>
        public static FunctionCallNode<long> Abs(ISql<long> value) => new("abs", value);
        
        /// <summary>
        /// Rounds a value to a specified number of decimal places.
        /// Renders as: round(value, digits)
        /// </summary>
        public static ISql<double> Round(ISql<double> value, ISql<int>? digits = null)
        {
            if (digits != null)
                return new FunctionCallNode<double>("round", value, digits);
            return new FunctionCallNode<double>("round", value);
        }
        
        /// <summary>
        /// Returns the total number of row changes made since the database connection was opened.
        /// Renders as: total_changes()
        /// </summary>
        public static FunctionCallNode<int> TotalChanges() => new("total_changes");
    }
}
