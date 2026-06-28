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
