using Drizzle4Dotnet.Core.Schema.Columns;

namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Interface for SQL data type representations.
/// Each dialect implements its own set of data types as readonly structs
/// with static readonly instances for common types.
/// </summary>
public interface ISqlDataType
{
    /// <summary>The SQL type name string (e.g., "BIGINT", "VARCHAR(255)").</summary>
    string Sql { get; }
}

// ============================================================================
// Custom / Raw Data Type
// ============================================================================

/// <summary>
/// A raw/custom SQL data type defined by an arbitrary string.
/// Used when a dialect-specific type is not available or for ad-hoc types.
/// </summary>
public readonly struct RawSqlDataType : ISqlDataType
{
    public string Sql { get; }

    public RawSqlDataType(string sql) => Sql = sql;
}

/// <summary>
/// Default/generic ANSI SQL standard data type.
/// Used for unsupported dialects or as a fallback when no dialect-specific type is available.
/// Follows standard SQL type naming (INTEGER, BIGINT, VARCHAR, etc.).
/// </summary>
public readonly struct SqlDataType : ISqlDataType
{
    public string Sql { get; }

    private SqlDataType(string sql) => Sql = sql;

    /// <summary>INTEGER</summary>
    public static readonly SqlDataType Integer = new("INTEGER");
    /// <summary>BIGINT</summary>
    public static readonly SqlDataType BigInt = new("BIGINT");
    /// <summary>SMALLINT</summary>
    public static readonly SqlDataType SmallInt = new("SMALLINT");
    /// <summary>TINYINT</summary>
    public static readonly SqlDataType TinyInt = new("TINYINT");
    /// <summary>VARCHAR(255) — generic string type</summary>
    public static readonly SqlDataType Text = new("VARCHAR(255)");
    /// <summary>BOOLEAN</summary>
    public static readonly SqlDataType Boolean = new("BOOLEAN");
    /// <summary>DECIMAL(18,2)</summary>
    public static readonly SqlDataType Decimal = new("DECIMAL(18,2)");
    /// <summary>REAL</summary>
    public static readonly SqlDataType Real = new("REAL");
    /// <summary>FLOAT</summary>
    public static readonly SqlDataType Float = new("FLOAT");
    /// <summary>DOUBLE PRECISION</summary>
    public static readonly SqlDataType DoublePrecision = new("DOUBLE PRECISION");
    /// <summary>TIMESTAMP</summary>
    public static readonly SqlDataType Timestamp = new("TIMESTAMP");
    /// <summary>DATE</summary>
    public static readonly SqlDataType Date = new("DATE");
    /// <summary>TIME</summary>
    public static readonly SqlDataType Time = new("TIME");
    /// <summary>UUID — generic CHAR(36)</summary>
    public static readonly SqlDataType Uuid = new("CHAR(36)");
    /// <summary>BLOB</summary>
    public static readonly SqlDataType Blob = new("BLOB");
    /// <summary>BLOB (alias)</summary>
    public static readonly SqlDataType Bytea = new("BLOB");
    /// <summary>CHAR(1)</summary>
    public static readonly SqlDataType Char = new("CHAR(1)");
    /// <summary>VARCHAR</summary>
    public static readonly SqlDataType VarChar = new("VARCHAR");
    /// <summary>BIGINT — auto-increment equivalent (generic)</summary>
    public static readonly SqlDataType BigSerial = new("BIGINT");
    /// <summary>INTEGER — auto-increment equivalent (generic)</summary>
    public static readonly SqlDataType Serial = new("INTEGER");

    /// <summary>Creates a VARCHAR type with custom length.</summary>
    public static SqlDataType VarCharOf(int length) => new($"VARCHAR({length})");
    /// <summary>Creates a DECIMAL type with custom precision and scale.</summary>
    public static SqlDataType DecimalFormat(int precision, int scale) => new($"DECIMAL({precision},{scale})");
    /// <summary>Creates a custom/arbitrary SQL type.</summary>
    public static SqlDataType Custom(string sql) => new(sql);
}

// ============================================================================
// Default Data Type Attributes (subclasses of SqlTypeAttribute)
// Use these on properties alongside [Column] to specify the exact SQL type
// when using the default/generic dialect.
// ============================================================================

/// <summary>INTEGER</summary>
public sealed class SqlIntegerAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER";
}

/// <summary>BIGINT</summary>
public sealed class SqlBigIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIGINT";
}

/// <summary>SMALLINT</summary>
public sealed class SqlSmallIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "SMALLINT";
}

/// <summary>TINYINT</summary>
public sealed class SqlTinyIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "TINYINT";
}

/// <summary>VARCHAR(255)</summary>
public sealed class SqlTextAttribute : SqlTypeAttribute
{
    public override string SqlType => "VARCHAR(255)";
}

/// <summary>BOOLEAN</summary>
public sealed class SqlBooleanAttribute : SqlTypeAttribute
{
    public override string SqlType => "BOOLEAN";
}

/// <summary>DECIMAL(precision, scale) — default DECIMAL(18,2)</summary>
public sealed class SqlDecimalAttribute : SqlTypeAttribute
{
    public int Precision { get; }
    public int Scale { get; }

    public SqlDecimalAttribute(int precision = 18, int scale = 2)
    {
        Precision = precision;
        Scale = scale;
    }

    public override string SqlType => $"DECIMAL({Precision},{Scale})";
}

/// <summary>REAL</summary>
public sealed class SqlRealAttribute : SqlTypeAttribute
{
    public override string SqlType => "REAL";
}

/// <summary>FLOAT</summary>
public sealed class SqlFloatAttribute : SqlTypeAttribute
{
    public override string SqlType => "FLOAT";
}

/// <summary>DOUBLE PRECISION</summary>
public sealed class SqlDoublePrecisionAttribute : SqlTypeAttribute
{
    public override string SqlType => "DOUBLE PRECISION";
}

/// <summary>TIMESTAMP</summary>
public sealed class SqlTimestampAttribute : SqlTypeAttribute
{
    public override string SqlType => "TIMESTAMP";
}

/// <summary>DATE</summary>
public sealed class SqlDateAttribute : SqlTypeAttribute
{
    public override string SqlType => "DATE";
}

/// <summary>TIME</summary>
public sealed class SqlTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "TIME";
}

/// <summary>CHAR(36) — generic UUID representation</summary>
public sealed class SqlUuidAttribute : SqlTypeAttribute
{
    public override string SqlType => "CHAR(36)";
}

/// <summary>BLOB</summary>
public sealed class SqlBlobAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>BLOB (alias)</summary>
public sealed class SqlByteaAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>CHAR(1)</summary>
public sealed class SqlCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "CHAR(1)";
}

/// <summary>VARCHAR(length) — default VARCHAR</summary>
public sealed class SqlVarCharAttribute : SqlTypeAttribute
{
    public int? Length { get; }

    public SqlVarCharAttribute() { }

    public SqlVarCharAttribute(int length) => Length = length;

    public override string SqlType => Length.HasValue ? $"VARCHAR({Length.Value})" : "VARCHAR";
}

/// <summary>Custom raw SQL type.</summary>
public sealed class SqlCustomAttribute : SqlTypeAttribute
{
    public override string SqlType { get; }

    public SqlCustomAttribute(string sqlType) => SqlType = sqlType;
}
