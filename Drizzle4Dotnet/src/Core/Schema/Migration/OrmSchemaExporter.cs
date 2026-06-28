using System.Collections.Concurrent;
using System.Reflection;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Extracts <see cref="TableDefinition"/> objects from ORM table schema classes
/// (e.g., <c>UsersTable</c>) via reflection on the source-generated static properties.
/// Works with any dialect (PgSql, MySql, etc.).
/// </summary>
public static class OrmSchemaExporter
{
    private static readonly ConcurrentDictionary<Type, TableDefinition> _cache = new();

    /// <summary>
    /// Default CLR-to-SQL type mappings used for DDL generation.
    /// Override by passing a custom map to <see cref="GetTableDefinition{TTable}"/>.
    /// </summary>
    public static readonly Dictionary<Type, string> DefaultClrToSqlMap = new()
    {
        [typeof(int)] = "INTEGER",
        [typeof(long)] = "BIGINT",
        [typeof(short)] = "SMALLINT",
        [typeof(byte)] = "SMALLINT",
        [typeof(string)] = "TEXT",
        [typeof(bool)] = "BOOLEAN",
        [typeof(decimal)] = "NUMERIC(18,2)",
        [typeof(float)] = "REAL",
        [typeof(double)] = "DOUBLE PRECISION",
        [typeof(DateTime)] = "TIMESTAMP",
        [typeof(DateOnly)] = "DATE",
        [typeof(TimeOnly)] = "TIME",
        [typeof(Guid)] = "UUID",
        [typeof(byte[])] = "BYTEA",
        [typeof(char)] = "CHAR(1)",
    };

    /// <summary>
    /// PostgreSQL-specific CLR-to-SQL type mappings.
    /// </summary>
    public static readonly Dictionary<Type, string> PgSqlTypeMap = new(DefaultClrToSqlMap)
    {
        // PostgreSQL uses TEXT for strings, BIGSERIAL for auto-increment
    };

    /// <summary>
    /// SQLite-specific CLR-to-SQL type mappings.
    /// SQLite uses only 5 storage classes: INTEGER, REAL, TEXT, BLOB, NUMERIC.
    /// </summary>
    public static readonly Dictionary<Type, string> SqliteTypeMap = new()
    {
        [typeof(int)] = "INTEGER",
        [typeof(long)] = "INTEGER",
        [typeof(short)] = "INTEGER",
        [typeof(byte)] = "INTEGER",
        [typeof(string)] = "TEXT",
        [typeof(bool)] = "INTEGER",       // 0 or 1
        [typeof(decimal)] = "NUMERIC",
        [typeof(float)] = "REAL",
        [typeof(double)] = "REAL",
        [typeof(DateTime)] = "TEXT",      // ISO-8601 format
        [typeof(DateOnly)] = "TEXT",      // 'YYYY-MM-DD'
        [typeof(TimeOnly)] = "TEXT",      // 'HH:MM:SS'
        [typeof(Guid)] = "TEXT",          // hex string
        [typeof(byte[])] = "BLOB",
        [typeof(char)] = "TEXT",
    };

    /// <summary>
    /// MySQL-specific CLR-to-SQL type mappings.
    /// </summary>
    public static readonly Dictionary<Type, string> MySqlTypeMap = new()
    {
        [typeof(int)] = "INT",
        [typeof(long)] = "BIGINT",
        [typeof(short)] = "SMALLINT",
        [typeof(byte)] = "TINYINT",
        [typeof(string)] = "VARCHAR(255)",
        [typeof(bool)] = "TINYINT(1)",
        [typeof(decimal)] = "DECIMAL(18,2)",
        [typeof(float)] = "FLOAT",
        [typeof(double)] = "DOUBLE",
        [typeof(DateTime)] = "DATETIME(6)",
        [typeof(DateOnly)] = "DATE",
        [typeof(TimeOnly)] = "TIME",
        [typeof(Guid)] = "CHAR(36)",
        [typeof(byte[])] = "BLOB",
        [typeof(char)] = "CHAR(1)",
    };

    /// <summary>
    /// Gets a <see cref="TableDefinition"/> from an ORM table type via reflection.
    /// </summary>
    /// <typeparam name="TTable">The ORM table class (e.g., <c>UsersTable</c>).</typeparam>
    /// <param name="customTypeMap">Optional CLR-to-SQL type mapping overrides.</param>
    /// <param name="useCache">Whether to cache the result.</param>
    public static TableDefinition GetTableDefinition<TTable>(
        Dictionary<Type, string>? customTypeMap = null,
        bool useCache = true) where TTable : class
    {
        var type = typeof(TTable);
        if (useCache && _cache.TryGetValue(type, out var cached))
            return cached;

        var tableDef = ExtractTableDefinition(type, customTypeMap);

        if (useCache)
            _cache[type] = tableDef;

        return tableDef;
    }

    /// <summary>
    /// Gets <see cref="TableDefinition"/> from multiple ORM table types.
    /// </summary>
    public static List<TableDefinition> GetTableDefinitions(params Type[] tableTypes)
    {
        return tableTypes.Select(t => ExtractTableDefinition(t, null)).ToList();
    }

    /// <summary>
    /// Creates a <see cref="SchemaSnapshot"/> from an ORM table type.
    /// </summary>
    public static SchemaSnapshot CreateSchemaSnapshot<TTable>(string snapshotName)
        where TTable : class
    {
        var tableDef = GetTableDefinition<TTable>();
        return SchemaSnapshot.FromTableDefinitions(snapshotName, new[] { tableDef });
    }

    /// <summary>
    /// Creates a <see cref="SchemaSnapshot"/> from multiple table types.
    /// </summary>
    public static SchemaSnapshot CreateSchemaSnapshot(string snapshotName, params Type[] tableTypes)
    {
        var defs = tableTypes.Select(t => ExtractTableDefinition(t, null)).ToList();
        return SchemaSnapshot.FromTableDefinitions(snapshotName, defs);
    }

    /// <summary>
    /// Convenience method: creates a <see cref="CreateTableQuery"/> from an ORM table type.
    /// </summary>
    public static CreateTableQuery CreateTable<TTable>(
        Dictionary<Type, string>? customTypeMap = null)
        where TTable : class
    {
        var tableDef = GetTableDefinition<TTable>(customTypeMap);
        return new CreateTableQuery(tableDef);
    }

    private static TableDefinition ExtractTableDefinition(
        Type tableType,
        Dictionary<Type, string>? customTypeMap)
    {
        var typeMap = customTypeMap ?? DefaultClrToSqlMap;

        // Extract table name from static properties generated by TableGenerator
        var tableName = GetStaticPropertyValue<string>(tableType, "TableName") ?? tableType.Name;
        var schemaName = GetStaticPropertyValue<string>(tableType, "SchemaName") ?? "public";

        // Extract columns by finding static properties typed as DbColumn<,,> or subclasses
        var columns = new List<ColumnDefinition>();

        foreach (var prop in tableType.GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            var propType = prop.PropertyType;
            if (!IsColumnType(propType))
                continue;

            var columnName = GetColumnIdentifier(prop);
            if (string.IsNullOrEmpty(columnName))
                continue;

            var clrType = GetColumnClrType(propType);
            var sqlType = MapClrToSql(clrType, typeMap);
            var isNullable = IsClrNullable(clrType);

            columns.Add(new ColumnDefinition(columnName, sqlType)
            {
                IsNullable = isNullable
            });
        }

        return new TableDefinition(tableName, schemaName, columns);
    }

    private static bool IsColumnType(Type type)
    {
        if (type.IsGenericType)
        {
            // Check if it is or inherits from DbColumn<,,>
            var checkType = type;
            while (checkType != null)
            {
                if (checkType.IsGenericType)
                {
                    var genericDef = checkType.GetGenericTypeDefinition();
                    if (genericDef == typeof(DbColumn<,,>))
                        return true;
                }
                checkType = checkType.BaseType;
            }
        }
        return false;
    }

    private static string? GetColumnIdentifier(PropertyInfo prop)
    {
        // Get the static instance of the column
        var instance = prop.GetValue(null);
        if (instance == null)
            return null;

        // DbColumn has an "Identifier" property
        var identifierProp = prop.PropertyType.GetProperty("Identifier",
            BindingFlags.Public | BindingFlags.Instance);
        return identifierProp?.GetValue(instance)?.ToString();
    }

    private static Type GetColumnClrType(Type columnType)
    {
        // Traverse the type hierarchy to find DbColumn<T,,>
        var checkType = columnType;
        while (checkType != null)
        {
            if (checkType.IsGenericType &&
                checkType.GetGenericTypeDefinition() == typeof(DbColumn<,,>))
            {
                return checkType.GetGenericArguments()[0];
            }
            checkType = checkType.BaseType;
        }
        return typeof(string);
    }

    private static string MapClrToSql(Type clrType, Dictionary<Type, string> typeMap)
    {
        // Handle nullable types (Nullable<T>)
        var underlyingType = Nullable.GetUnderlyingType(clrType);
        if (underlyingType != null)
            return MapClrToSql(underlyingType, typeMap);

        // Try exact match
        if (typeMap.TryGetValue(clrType, out var sqlType))
            return sqlType;

        // Try by name for types that might be equivalent
        foreach (var (key, value) in typeMap)
        {
            if (key.IsAssignableFrom(clrType))
                return value;
        }

        return "TEXT";
    }

    private static bool IsClrNullable(Type type)
    {
        if (!type.IsValueType)
            return true; // Reference types (string, etc.) are nullable
        return Nullable.GetUnderlyingType(type) != null;
    }

    private static T? GetStaticPropertyValue<T>(Type type, string propertyName) where T : class
    {
        var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        return prop?.GetValue(null) as T;
    }

    /// <summary>
    /// Clears the internal type-to-definition cache.
    /// </summary>
    public static void ClearCache()
    {
        _cache.Clear();
    }
}
