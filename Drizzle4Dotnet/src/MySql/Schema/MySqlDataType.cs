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
