using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.MySql.Schema;

/// <summary>
/// MySQL-specific SQL data type.
/// </summary>
public readonly struct MySqlDataType : ISqlDataType
{
    public string Sql { get; }

    private MySqlDataType(string sql) => Sql = sql;

    public static readonly MySqlDataType Int = new("INT");
    public static readonly MySqlDataType BigInt = new("BIGINT");
    public static readonly MySqlDataType SmallInt = new("SMALLINT");
    public static readonly MySqlDataType TinyInt = new("TINYINT");
    public static readonly MySqlDataType Text = new("VARCHAR(255)");
    public static readonly MySqlDataType Boolean = new("TINYINT(1)");
    public static readonly MySqlDataType Decimal = new("DECIMAL(18,2)");
    public static readonly MySqlDataType Float = new("FLOAT");
    public static readonly MySqlDataType Double = new("DOUBLE");
    public static readonly MySqlDataType DateTime = new("DATETIME(6)");
    public static readonly MySqlDataType Date = new("DATE");
    public static readonly MySqlDataType Time = new("TIME");
    public static readonly MySqlDataType Uuid = new("CHAR(36)");
    public static readonly MySqlDataType Blob = new("BLOB");
    public static readonly MySqlDataType Bytea = new("BLOB");
    public static readonly MySqlDataType Char = new("CHAR(1)");
    public static readonly MySqlDataType VarChar = new("VARCHAR");
    public static readonly MySqlDataType BigSerial = new("BIGINT AUTO_INCREMENT");
    public static readonly MySqlDataType Serial = new("INT AUTO_INCREMENT");

    /// <summary>Creates a VARCHAR type with custom length.</summary>
    public static MySqlDataType VarCharOf(int length) => new($"VARCHAR({length})");
    /// <summary>Creates a DECIMAL type with custom precision and scale.</summary>
    public static MySqlDataType DecimalFormat(int precision, int scale) => new($"DECIMAL({precision},{scale})");
    /// <summary>Creates a custom/arbitrary SQL type.</summary>
    public static MySqlDataType Custom(string sql) => new(sql);
}

// ============================================================================
// MySql Data Type Attributes (subclasses of SqlTypeAttribute)
// ============================================================================

/// <summary>INT</summary>
public sealed class MySqlIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "INT";
}

/// <summary>BIGINT</summary>
public sealed class MySqlBigIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIGINT";
}

/// <summary>SMALLINT</summary>
public sealed class MySqlSmallIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "SMALLINT";
}

/// <summary>TINYINT</summary>
public sealed class MySqlTinyIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "TINYINT";
}

/// <summary>VARCHAR(255)</summary>
public sealed class MySqlTextAttribute : SqlTypeAttribute
{
    public override string SqlType => "VARCHAR(255)";
}

/// <summary>TINYINT(1)</summary>
public sealed class MySqlBooleanAttribute : SqlTypeAttribute
{
    public override string SqlType => "TINYINT(1)";
}

/// <summary>DECIMAL(precision, scale) — default DECIMAL(18,2)</summary>
public sealed class MySqlDecimalAttribute : SqlTypeAttribute
{
    public int Precision { get; }
    public int Scale { get; }

    public MySqlDecimalAttribute(int precision = 18, int scale = 2)
    {
        Precision = precision;
        Scale = scale;
    }

    public override string SqlType => $"DECIMAL({Precision},{Scale})";
}

/// <summary>FLOAT</summary>
public sealed class MySqlFloatAttribute : SqlTypeAttribute
{
    public override string SqlType => "FLOAT";
}

/// <summary>DOUBLE</summary>
public sealed class MySqlDoubleAttribute : SqlTypeAttribute
{
    public override string SqlType => "DOUBLE";
}

/// <summary>DATETIME(6)</summary>
public sealed class MySqlDateTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "DATETIME(6)";
}

/// <summary>DATE</summary>
public sealed class MySqlDateAttribute : SqlTypeAttribute
{
    public override string SqlType => "DATE";
}

/// <summary>TIME</summary>
public sealed class MySqlTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "TIME";
}

/// <summary>CHAR(36)</summary>
public sealed class MySqlUuidAttribute : SqlTypeAttribute
{
    public override string SqlType => "CHAR(36)";
}

/// <summary>BLOB</summary>
public sealed class MySqlBlobAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>BLOB (alias)</summary>
public sealed class MySqlByteaAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>CHAR(1)</summary>
public sealed class MySqlCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "CHAR(1)";
}

/// <summary>VARCHAR(length) — default VARCHAR</summary>
public sealed class MySqlVarCharAttribute : SqlTypeAttribute
{
    public int? Length { get; }

    public MySqlVarCharAttribute() { }

    public MySqlVarCharAttribute(int length) => Length = length;

    public override string SqlType => Length.HasValue ? $"VARCHAR({Length.Value})" : "VARCHAR";
}

/// <summary>BIGINT AUTO_INCREMENT</summary>
public sealed class MySqlBigSerialAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIGINT AUTO_INCREMENT";
}

/// <summary>INT AUTO_INCREMENT</summary>
public sealed class MySqlSerialAttribute : SqlTypeAttribute
{
    public override string SqlType => "INT AUTO_INCREMENT";
}

/// <summary>Custom raw SQL type.</summary>
public sealed class MySqlCustomAttribute : SqlTypeAttribute
{
    public override string SqlType { get; }

    public MySqlCustomAttribute(string sqlType) => SqlType = sqlType;
}
