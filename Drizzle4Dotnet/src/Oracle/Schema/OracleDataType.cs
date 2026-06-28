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
