namespace Drizzle4Dotnet.Core.Operators;

/// <summary>
/// Standard SQL operators organized into partial class files by category.
/// Extension methods on ISql provide `column.Eq(value)` syntax.
/// Static methods on ISql provide `Operators.Eq(column, value)` syntax.
/// For PostgreSQL-specific operators, see <see cref="PgSql.PgOperators"/>.
/// </summary>
public static partial class Operators
{
    // Operator string constants — shared across all partial files
    internal const string EqOp = "=";
    internal const string LtOp = "<";
    internal const string GtOp = ">";
    internal const string LtEqOp = "<=";
    internal const string GtEqOp = ">=";
    internal const string NeOp = "<>";
    internal const string AndOp = " AND ";
    internal const string OrOp = " OR ";
    internal const string XorOp = " XOR ";
    internal const string LikeOp = " LIKE ";
    internal const string NotLikeOp = " NOT LIKE ";
    internal const string InOp = " IN ";
    internal const string NotInOp = " NOT IN ";
    internal const string IsNullOp = " IS NULL ";
    internal const string IsNotNullOp = " IS NOT NULL ";
    internal const string BetweenOp = " BETWEEN ";
    internal const string NotBetweenOp = " NOT BETWEEN ";
    internal const string NotOp = " NOT ";
    internal const string ConcatOp = " || ";
    internal const string AddOp = " + ";
    internal const string SubOp = " - ";
    internal const string MulOp = " * ";
    internal const string DivOp = " / ";
    internal const string ModOp = " % ";
    internal const string ExistsOp = " EXISTS ";
}