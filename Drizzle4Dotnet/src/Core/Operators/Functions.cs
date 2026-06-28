using Drizzle4Dotnet.MySql.Operators;
using Drizzle4Dotnet.PgSql.Operators;

namespace Drizzle4Dotnet.Core.Operators;

/// <summary>
/// Standard SQL functions organized into partial class files by category.
/// Extension methods on ISql provide `column.Count()` syntax.
/// Static methods on Functions provide `Functions.Count(column)` syntax.
/// For dialect-specific functions, see <see cref="MySqlFunctions"/> or <see cref="PgFunctions"/>.
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
