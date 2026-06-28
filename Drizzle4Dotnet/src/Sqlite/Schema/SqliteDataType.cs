using Drizzle4Dotnet.Core.Schema.Columns;
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

// ============================================================================
// Sqlite Data Type Attributes (subclasses of SqlTypeAttribute)
// ============================================================================

/// <summary>INTEGER</summary>
public sealed class SqliteIntegerAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER";
}

/// <summary>INTEGER (alias for BigInt/SmallInt/TinyInt)</summary>
public sealed class SqliteBigIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER";
}

/// <summary>INTEGER (alias)</summary>
public sealed class SqliteSmallIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER";
}

/// <summary>INTEGER (alias)</summary>
public sealed class SqliteTinyIntAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER";
}

/// <summary>TEXT</summary>
public sealed class SqliteTextAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>INTEGER (boolean)</summary>
public sealed class SqliteBooleanAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER";
}

/// <summary>NUMERIC</summary>
public sealed class SqliteNumericAttribute : SqlTypeAttribute
{
    public override string SqlType => "NUMERIC";
}

/// <summary>REAL</summary>
public sealed class SqliteRealAttribute : SqlTypeAttribute
{
    public override string SqlType => "REAL";
}

/// <summary>REAL (alias)</summary>
public sealed class SqliteDoublePrecisionAttribute : SqlTypeAttribute
{
    public override string SqlType => "REAL";
}

/// <summary>REAL (alias)</summary>
public sealed class SqliteFloatAttribute : SqlTypeAttribute
{
    public override string SqlType => "REAL";
}

/// <summary>TEXT (timestamp/datetime)</summary>
public sealed class SqliteTimestampAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>TEXT (datetime)</summary>
public sealed class SqliteDateTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>TEXT (date)</summary>
public sealed class SqliteDateAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>TEXT (time)</summary>
public sealed class SqliteTimeAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>TEXT (uuid)</summary>
public sealed class SqliteUuidAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>BLOB</summary>
public sealed class SqliteBlobAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>BLOB (alias)</summary>
public sealed class SqliteByteaAttribute : SqlTypeAttribute
{
    public override string SqlType => "BLOB";
}

/// <summary>TEXT (char)</summary>
public sealed class SqliteCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>TEXT (varchar)</summary>
public sealed class SqliteVarCharAttribute : SqlTypeAttribute
{
    public override string SqlType => "TEXT";
}

/// <summary>INTEGER PRIMARY KEY</summary>
public sealed class SqliteIntegerPrimaryKeyAttribute : SqlTypeAttribute
{
    public override string SqlType => "INTEGER PRIMARY KEY";
}

/// <summary>Custom raw SQL type.</summary>
public sealed class SqliteCustomAttribute : SqlTypeAttribute
{
    public override string SqlType { get; }

    public SqliteCustomAttribute(string sqlType) => SqlType = sqlType;
}
