using Drizzle4Dotnet.Core.Schema.Migration;

namespace Drizzle4Dotnet.Sqlite.Schema;

/// <summary>
/// SQLite-specific SQL data type.
/// SQLite uses flexible typing with only 5 storage classes.
/// </summary>
public readonly struct SqliteDataType : ISqlDataType
{
    public string Sql { get; }

    private SqliteDataType(string sql) => Sql = sql;

    public static readonly SqliteDataType Integer = new("INTEGER");
    public static readonly SqliteDataType BigInt = new("INTEGER");
    public static readonly SqliteDataType SmallInt = new("INTEGER");
    public static readonly SqliteDataType TinyInt = new("INTEGER");
    public static readonly SqliteDataType Text = new("TEXT");
    public static readonly SqliteDataType Boolean = new("INTEGER");
    public static readonly SqliteDataType Numeric = new("NUMERIC");
    public static readonly SqliteDataType Real = new("REAL");
    public static readonly SqliteDataType DoublePrecision = new("REAL");
    public static readonly SqliteDataType Float = new("REAL");
    public static readonly SqliteDataType Timestamp = new("TEXT");
    public static readonly SqliteDataType DateTime = new("TEXT");
    public static readonly SqliteDataType Date = new("TEXT");
    public static readonly SqliteDataType Time = new("TEXT");
    public static readonly SqliteDataType Uuid = new("TEXT");
    public static readonly SqliteDataType Blob = new("BLOB");
    public static readonly SqliteDataType Bytea = new("BLOB");
    public static readonly SqliteDataType Char = new("TEXT");
    public static readonly SqliteDataType VarChar = new("TEXT");
    public static readonly SqliteDataType IntegerPrimaryKey = new("INTEGER PRIMARY KEY");

    /// <summary>Creates a custom/arbitrary SQL type.</summary>
    public static SqliteDataType Custom(string sql) => new(sql);
}
