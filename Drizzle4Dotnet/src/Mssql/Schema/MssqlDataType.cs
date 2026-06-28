using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.Mssql.Schema;

/// <summary>
/// MSSQL-specific SQL data type.
/// </summary>
public readonly struct MssqlDataType : ISqlDataType
{
    public string Sql { get; }

    private MssqlDataType(string sql) => Sql = sql;

    public static readonly MssqlDataType Int = new("INT");
    public static readonly MssqlDataType BigInt = new("BIGINT");
    public static readonly MssqlDataType SmallInt = new("SMALLINT");
    public static readonly MssqlDataType TinyInt = new("TINYINT");
    public static readonly MssqlDataType Text = new("NVARCHAR(MAX)");
    public static readonly MssqlDataType Boolean = new("BIT");
    public static readonly MssqlDataType Decimal = new("DECIMAL(18,2)");
    public static readonly MssqlDataType Real = new("REAL");
    public static readonly MssqlDataType Float = new("FLOAT");
    public static readonly MssqlDataType DateTime2 = new("DATETIME2");
    public static readonly MssqlDataType Date = new("DATE");
    public static readonly MssqlDataType Time = new("TIME");
    public static readonly MssqlDataType UniqueIdentifier = new("UNIQUEIDENTIFIER");
    public static readonly MssqlDataType VarBinary = new("VARBINARY(MAX)");
    public static readonly MssqlDataType Blob = new("VARBINARY(MAX)");
    public static readonly MssqlDataType Bytea = new("VARBINARY(MAX)");
    public static readonly MssqlDataType NChar = new("NCHAR(1)");
    public static readonly MssqlDataType Char = new("CHAR(1)");
    public static readonly MssqlDataType NVarChar = new("NVARCHAR(MAX)");
    public static readonly MssqlDataType VarChar = new("VARCHAR(MAX)");
    public static readonly MssqlDataType BigSerial = new("BIGINT IDENTITY(1,1)");
    public static readonly MssqlDataType Serial = new("INT IDENTITY(1,1)");

    /// <summary>Creates a DECIMAL type with custom precision and scale.</summary>
    public static MssqlDataType DecimalFormat(int precision, int scale) => new($"DECIMAL({precision},{scale})");
    /// <summary>Creates an NVARCHAR type with custom length.</summary>
    public static MssqlDataType NVarCharOf(int length) => new($"NVARCHAR({length})");
    /// <summary>Creates a custom/arbitrary SQL type.</summary>
    public static MssqlDataType Custom(string sql) => new(sql);
}

// ============================================================================
// Mssql Data Type Attributes (subclasses of SqlTypeAttribute)
// ============================================================================

/// <summary>INT</summary>
public sealed class MssqlIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "INT";
}

/// <summary>BIGINT</summary>
public sealed class MssqlBigIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIGINT";
}

/// <summary>SMALLINT</summary>
public sealed class MssqlSmallIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "SMALLINT";
}

/// <summary>TINYINT</summary>
public sealed class MssqlTinyIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "TINYINT";
}

/// <summary>NVARCHAR(MAX)</summary>
public sealed class MssqlTextAttribute : SqlTypeAttribute
{
    public override string SqlType => "NVARCHAR(MAX)";
}

/// <summary>BIT</summary>
public sealed class MssqlBooleanAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIT";
}

/// <summary>DECIMAL(precision, scale) — default DECIMAL(18,2)</summary>
public sealed class MssqlDecimalAttribute : SqlTypeAttribute
{
    public int Precision { get; }
    public int Scale { get; }

    public MssqlDecimalAttribute(int precision = 18, int scale = 2)
    {
        Precision = precision;
        Scale = scale;
    }

    public override string SqlType => $"DECIMAL({Precision},{Scale})";
}

/// <summary>REAL</summary>
public sealed class MssqlRealAttribute : SqlTypeAttribute
{
    public override string SqlType => "REAL";
}

/// <summary>FLOAT</summary>
public sealed class MssqlFloatAttribute : SqlTypeAttribute
{
    public override string SqlType => "FLOAT";
}

/// <summary>DATETIME2</summary>
public sealed class MssqlDateTime2Attribute : SqlTypeAttribute
{
    public override string SqlType => "DATETIME2";
}

/// <summary>DATE</summary>
public sealed class MssqlDateAttribute : SqlTypeAttribute
{
    public override string SqlType => "DATE";
}

/// <summary>TIME</summary>
public sealed class MssqlTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "TIME";
}

/// <summary>UNIQUEIDENTIFIER</summary>
public sealed class MssqlUniqueIdentifierAttribute : SqlTypeAttribute
{
    public override string SqlType => "UNIQUEIDENTIFIER";
}

/// <summary>VARBINARY(MAX)</summary>
public sealed class MssqlVarBinaryAttribute : SqlTypeAttribute
{
    public override string SqlType => "VARBINARY(MAX)";
}

/// <summary>VARBINARY(MAX) (alias)</summary>
public sealed class MssqlBlobAttribute : SqlTypeAttribute
{
    public override string SqlType => "VARBINARY(MAX)";
}

/// <summary>VARBINARY(MAX) (alias)</summary>
public sealed class MssqlByteaAttribute : SqlTypeAttribute
{
    public override string SqlType => "VARBINARY(MAX)";
}

/// <summary>NCHAR(1)</summary>
public sealed class MssqlNCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "NCHAR(1)";
}

/// <summary>CHAR(1)</summary>
public sealed class MssqlCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "CHAR(1)";
}

/// <summary>NVARCHAR(length) — default NVARCHAR(MAX)</summary>
public sealed class MssqlNVarCharAttribute : SqlTypeAttribute
{
    public int? Length { get; }

    public MssqlNVarCharAttribute() { }

    public MssqlNVarCharAttribute(int length) => Length = length;

    public override string SqlType => Length.HasValue ? $"NVARCHAR({Length.Value})" : "NVARCHAR(MAX)";
}

/// <summary>VARCHAR(length) — default VARCHAR(MAX)</summary>
public sealed class MssqlVarCharAttribute : SqlTypeAttribute
{
    public int? Length { get; }

    public MssqlVarCharAttribute() { }

    public MssqlVarCharAttribute(int length) => Length = length;

    public override string SqlType => Length.HasValue ? $"VARCHAR({Length.Value})" : "VARCHAR(MAX)";
}

/// <summary>BIGINT IDENTITY(1,1)</summary>
public sealed class MssqlBigSerialAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIGINT IDENTITY(1,1)";
}

/// <summary>INT IDENTITY(1,1)</summary>
public sealed class MssqlSerialAttribute : SqlTypeAttribute
{
    public override string SqlType => "INT IDENTITY(1,1)";
}

/// <summary>Custom raw SQL type.</summary>
public sealed class MssqlCustomAttribute : SqlTypeAttribute
{
    public override string SqlType { get; }

    public MssqlCustomAttribute(string sqlType) => SqlType = sqlType;
}
