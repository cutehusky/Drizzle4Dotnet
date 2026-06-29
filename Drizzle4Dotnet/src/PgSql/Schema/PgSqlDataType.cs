using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.PgSql.Schema;

/// <summary>
/// PostgreSQL-specific SQL data type.
/// Use static fields like <see cref="BigInt"/> instead of raw strings.
/// </summary>
public readonly struct PgSqlDataType : ISqlDataType
{
    public string Sql { get; }

    private PgSqlDataType(string sql) => Sql = sql;

    public static readonly PgSqlDataType Integer = new("INTEGER");
    public static readonly PgSqlDataType BigInt = new("BIGINT");
    public static readonly PgSqlDataType SmallInt = new("SMALLINT");
    public static readonly PgSqlDataType Text = new("TEXT");
    public static readonly PgSqlDataType Boolean = new("BOOLEAN");
    public static readonly PgSqlDataType Numeric = new("NUMERIC(18,2)");
    public static readonly PgSqlDataType Real = new("REAL");
    public static readonly PgSqlDataType DoublePrecision = new("DOUBLE PRECISION");
    public static readonly PgSqlDataType Timestamp = new("TIMESTAMP");
    public static readonly PgSqlDataType Date = new("DATE");
    public static readonly PgSqlDataType Time = new("TIME");
    public static readonly PgSqlDataType Uuid = new("UUID");
    public static readonly PgSqlDataType Bytea = new("BYTEA");
    public static readonly PgSqlDataType Blob = new("BYTEA");
    public static readonly PgSqlDataType Char = new("CHAR(1)");
    public static readonly PgSqlDataType VarChar = new("VARCHAR");
    public static readonly PgSqlDataType BigSerial = new("BIGSERIAL");
    public static readonly PgSqlDataType Serial = new("SERIAL");

    public static PgSqlDataType NumericFormat(int precision, int scale) => new($"NUMERIC({precision},{scale})");
    public static PgSqlDataType VarCharOf(int length) => new($"VARCHAR({length})");
    public static PgSqlDataType Custom(string sql) => new(sql);
}

// ============================================================================
// PgSql Data Type Attributes (subclasses of SqlTypeAttribute)
// Use these on properties alongside [Column] to specify the exact SQL type.
// ============================================================================

/// <summary>INTEGER</summary>
public sealed class PgSqlIntegerAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER";
}

/// <summary>BIGINT</summary>
public sealed class PgSqlBigIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIGINT";
}

/// <summary>SMALLINT</summary>
public sealed class PgSqlSmallIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "SMALLINT";
}

/// <summary>TEXT</summary>
public sealed class PgSqlTextAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>BOOLEAN</summary>
public sealed class PgSqlBooleanAttribute : SqlTypeAttribute
{
    public override string SqlType => "BOOLEAN";
}

/// <summary>NUMERIC(precision, scale) — default NUMERIC(18,2)</summary>
public sealed class PgSqlNumericAttribute : SqlTypeAttribute
{
    public int Precision { get; }
    public int Scale { get; }

    public PgSqlNumericAttribute(int precision = 18, int scale = 2)
    {
        Precision = precision;
        Scale = scale;
    }

    public override string SqlType => $"NUMERIC({Precision},{Scale})";
}

/// <summary>REAL</summary>
public sealed class PgSqlRealAttribute : SqlTypeAttribute
{
    public override string SqlType => "REAL";
}

/// <summary>DOUBLE PRECISION</summary>
public sealed class PgSqlDoublePrecisionAttribute : SqlTypeAttribute
{
    public override string SqlType => "DOUBLE PRECISION";
}

/// <summary>TIMESTAMP</summary>
public sealed class PgSqlTimestampAttribute : SqlTypeAttribute
{
    public override string SqlType => "TIMESTAMP";
}

/// <summary>DATE</summary>
public sealed class PgSqlDateAttribute : SqlTypeAttribute
{
    public override string SqlType => "DATE";
}

/// <summary>TIME</summary>
public sealed class PgSqlTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "TIME";
}

/// <summary>UUID</summary>
public sealed class PgSqlUuidAttribute : SqlTypeAttribute
{
    public override string SqlType => "UUID";
}

/// <summary>BYTEA</summary>
public sealed class PgSqlByteaAttribute : SqlTypeAttribute
{
    public override string SqlType => "BYTEA";
}

/// <summary>BYTEA (alias for bytea)</summary>
public sealed class PgSqlBlobAttribute : SqlTypeAttribute
{
    public override string SqlType => "BYTEA";
}

/// <summary>CHAR(1)</summary>
public sealed class PgSqlCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "CHAR(1)";
}

/// <summary>VARCHAR(length) — default VARCHAR</summary>
public sealed class PgSqlVarCharAttribute : SqlTypeAttribute
{
    public int? Length { get; }

    public PgSqlVarCharAttribute() { }

    public PgSqlVarCharAttribute(int length) => Length = length;

    public override string SqlType => Length.HasValue ? $"VARCHAR({Length.Value})" : "VARCHAR";
}

/// <summary>BIGSERIAL</summary>
public sealed class PgSqlBigSerialAttribute : SqlTypeAttribute
{
    public override string SqlType => "BIGSERIAL";
}

/// <summary>SERIAL</summary>
public sealed class PgSqlSerialAttribute : SqlTypeAttribute
{
    public override string SqlType => "SERIAL";
}

/// <summary>Custom raw SQL type.</summary>
public sealed class PgSqlCustomAttribute : SqlTypeAttribute
{
    public override string SqlType { get; }

    public PgSqlCustomAttribute(string sqlType) => SqlType = sqlType;
}
