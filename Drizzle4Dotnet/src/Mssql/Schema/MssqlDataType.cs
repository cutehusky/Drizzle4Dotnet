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
