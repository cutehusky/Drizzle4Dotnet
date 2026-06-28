using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.Oracle.Schema;

/// <summary>
/// Oracle-specific SQL data type.
/// </summary>
public readonly struct OracleDataType : ISqlDataType
{
    public string Sql { get; }

    private OracleDataType(string sql) => Sql = sql;

    public static readonly OracleDataType Number = new("NUMBER");
    public static readonly OracleDataType Integer = new("NUMBER(10)");
    public static readonly OracleDataType BigInt = new("NUMBER(19)");
    public static readonly OracleDataType SmallInt = new("NUMBER(5)");
    public static readonly OracleDataType TinyInt = new("NUMBER(3)");
    public static readonly OracleDataType Text = new("VARCHAR2(255)");
    public static readonly OracleDataType Boolean = new("NUMBER(1)");
    public static readonly OracleDataType Decimal = new("NUMBER(18,2)");
    public static readonly OracleDataType BinaryFloat = new("BINARY_FLOAT");
    public static readonly OracleDataType BinaryDouble = new("BINARY_DOUBLE");
    public static readonly OracleDataType Float = new("BINARY_FLOAT");
    public static readonly OracleDataType DoublePrecision = new("BINARY_DOUBLE");
    public static readonly OracleDataType Timestamp = new("TIMESTAMP");
    public static readonly OracleDataType Date = new("DATE");
    public static readonly OracleDataType Time = new("INTERVAL DAY TO SECOND");
    public static readonly OracleDataType Uuid = new("RAW(16)");
    public static readonly OracleDataType Raw = new("RAW(16)");
    public static readonly OracleDataType Blob = new("BLOB");
    public static readonly OracleDataType Bytea = new("BLOB");
    public static readonly OracleDataType Char = new("CHAR(1)");
    public static readonly OracleDataType VarChar2 = new("VARCHAR2(255)");

    /// <summary>Creates a NUMBER type with custom precision and scale.</summary>
    public static OracleDataType NumberFormat(int precision, int scale) => new($"NUMBER({precision},{scale})");
    /// <summary>Creates a VARCHAR2 type with custom length.</summary>
    public static OracleDataType VarChar2Of(int length) => new($"VARCHAR2({length})");
    /// <summary>Creates a custom/arbitrary SQL type.</summary>
    public static OracleDataType Custom(string sql) => new(sql);
}

// ============================================================================
// Oracle Data Type Attributes (subclasses of SqlTypeAttribute)
// ============================================================================

/// <summary>NUMBER</summary>
public sealed class OracleNumberAttribute : SqlTypeAttribute
{
    public override string SqlType => "NUMBER";
}

/// <summary>NUMBER(10)</summary>
public sealed class OracleIntegerAttribute : SqlTypeAttribute
{
    public override string SqlType => "NUMBER(10)";
}

/// <summary>NUMBER(19)</summary>
public sealed class OracleBigIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "NUMBER(19)";
}

/// <summary>NUMBER(5)</summary>
public sealed class OracleSmallIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "NUMBER(5)";
}

/// <summary>NUMBER(3)</summary>
public sealed class OracleTinyIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "NUMBER(3)";
}

/// <summary>VARCHAR2(255)</summary>
public sealed class OracleTextAttribute : SqlTypeAttribute
{
    public override string SqlType => "VARCHAR2(255)";
}

/// <summary>NUMBER(1)</summary>
public sealed class OracleBooleanAttribute : SqlTypeAttribute
{
    public override string SqlType => "NUMBER(1)";
}

/// <summary>NUMBER(precision, scale) — default NUMBER(18,2)</summary>
public sealed class OracleDecimalAttribute : SqlTypeAttribute
{
    public int Precision { get; }
    public int Scale { get; }

    public OracleDecimalAttribute(int precision = 18, int scale = 2)
    {
        Precision = precision;
        Scale = scale;
    }

    public override string SqlType => $"NUMBER({Precision},{Scale})";
}

/// <summary>BINARY_FLOAT</summary>
public sealed class OracleBinaryFloatAttribute : SqlTypeAttribute
{
    public override string SqlType => "BINARY_FLOAT";
}

/// <summary>BINARY_DOUBLE</summary>
public sealed class OracleBinaryDoubleAttribute : SqlTypeAttribute
{
    public override string SqlType => "BINARY_DOUBLE";
}

/// <summary>BINARY_FLOAT (alias)</summary>
public sealed class OracleFloatAttribute : SqlTypeAttribute
{
    public override string SqlType => "BINARY_FLOAT";
}

/// <summary>BINARY_DOUBLE (alias)</summary>
public sealed class OracleDoublePrecisionAttribute : SqlTypeAttribute
{
    public override string SqlType => "BINARY_DOUBLE";
}

/// <summary>TIMESTAMP</summary>
public sealed class OracleTimestampAttribute : SqlTypeAttribute
{
    public override string SqlType => "TIMESTAMP";
}

/// <summary>DATE</summary>
public sealed class OracleDateAttribute : SqlTypeAttribute
{
    public override string SqlType => "DATE";
}

/// <summary>INTERVAL DAY TO SECOND</summary>
public sealed class OracleTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTERVAL DAY TO SECOND";
}

/// <summary>RAW(16)</summary>
public sealed class OracleUuidAttribute : SqlTypeAttribute
{
    public override string SqlType => "RAW(16)";
}

/// <summary>RAW(16) (alias)</summary>
public sealed class OracleRawAttribute : SqlTypeAttribute
{
    public override string SqlType => "RAW(16)";
}

/// <summary>BLOB</summary>
public sealed class OracleBlobAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>BLOB (alias)</summary>
public sealed class OracleByteaAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>CHAR(1)</summary>
public sealed class OracleCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "CHAR(1)";
}

/// <summary>VARCHAR2(length) — default VARCHAR2(255)</summary>
public sealed class OracleVarChar2Attribute : SqlTypeAttribute
{
    public int? Length { get; }

    public OracleVarChar2Attribute() { }

    public OracleVarChar2Attribute(int length) => Length = length;

    public override string SqlType => Length.HasValue ? $"VARCHAR2({Length.Value})" : "VARCHAR2(255)";
}

/// <summary>NUMBER(precision, scale) with custom precision and scale.</summary>
public sealed class OracleNumberFormatAttribute : SqlTypeAttribute
{
    public int Precision { get; }
    public int Scale { get; }

    public OracleNumberFormatAttribute(int precision, int scale)
    {
        Precision = precision;
        Scale = scale;
    }

    public override string SqlType => $"NUMBER({Precision},{Scale})";
}

/// <summary>Custom raw SQL type.</summary>
public sealed class OracleCustomAttribute : SqlTypeAttribute
{
    public override string SqlType { get; }

    public OracleCustomAttribute(string sqlType) => SqlType = sqlType;
}
