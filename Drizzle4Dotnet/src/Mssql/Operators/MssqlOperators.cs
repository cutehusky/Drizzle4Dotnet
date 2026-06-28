using Drizzle4Dotnet.Core.Operators;
using Drizzle4Dotnet.Core.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql;

/// <summary>
/// MSSQL-specific operators.
/// Most standard operators (comparison, arithmetic, logical, string) are already
/// in Core/Shared/Operators/ and work with MSSQL unchanged.
/// This class provides MSSQL-specific operator extensions and overrides.
/// </summary>
public static class MssqlOperators
{
    // ======================================================================
    // String Concatenation
    // MSSQL uses + for string concatenation (unlike PostgreSQL which uses ||)
    // ======================================================================

    /// <summary>
    /// String concatenation using + operator (MSSQL style).
    /// MSSQL uses + for concatenation, unlike PostgreSQL's || operator.
    /// </summary>
    public static BinaryNode<T> Concat<T>(ISql<T> left, ISql<T> right)
        => Operators.Add(left, right);
}
