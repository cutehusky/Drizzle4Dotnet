using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;

/// <summary>
/// Standard SQL functions organized into partial class files by category.
/// Extension methods on ISql provide `column.Count()` syntax.
/// Static methods on Functions provide `Functions.Count(column)` syntax.
/// For dialect-specific functions, see <see cref="MySql.MySqlFunctions"/> or <see cref="PgSql.PgFunctions"/>.
/// </summary>
public static partial class Functions
{
}

// ======================================================================
// Extension methods for ISql — provide `column.Count()` syntax
// ======================================================================
public static partial class FunctionsExtensions
{
}
