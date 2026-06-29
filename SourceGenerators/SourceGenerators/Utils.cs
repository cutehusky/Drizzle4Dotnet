using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace SourceGenerators;

public static class Utils
{
    
    private static readonly Regex NullableRegex = new Regex(@"^System\.Nullable<.*>|^Nullable<.*>|\?$", RegexOptions.Compiled);

    public static bool IsNullable(string typeName) => NullableRegex.IsMatch(typeName);

    public static string GetDataReaderMethod(string typeName) {
        var baseType = typeName
            .Replace("?", "")
            .Replace("System.Nullable<", "")
            .Replace("Nullable<", "")
            .Replace(">", "");
        
        return baseType switch
        {
            "int" or "Int32" => "Int32",
            "long" or "Int64" => "Int64",
            "short" or "Int16" => "Int16",
            "string" or "String" => "String",
            "bool" or "Boolean" => "Boolean",
            "System.Guid" or "Guid" => "Guid",
            "float" or "Single" => "Float",
            "double" or "Double" => "Double",
            "decimal" or "Decimal" => "Decimal",
            "System.DateTime" or "DateTime" => "DateTime",
            "byte[]" => "FieldValue<byte[]>",
            _ => $"FieldValue<{baseType}>" 
        };
    }

    /// <summary>
    /// Extracts the dialect type name from named arguments of an attribute.
    /// Supports: [Table("name", Dialect = typeof(Xxx))], [DbSelect(Dialect = typeof(Xxx))]
    /// Returns the fully qualified type name (e.g., "Drizzle4Dotnet.Dialect.PgSqlSqlDialectImpl").
    /// </summary>
    public static string? GetDialectFromAttribute(AttributeData attr)
    {
        foreach (var namedArg in attr.NamedArguments)
        {
            if (namedArg.Key == "Dialect" && namedArg.Value.Value is INamedTypeSymbol typeSymbol)
            {
                return typeSymbol.ToDisplayString();
            }
        }
        return null;
    }
}

/// <summary>
/// Holds dialect-specific type name mappings for source generation.
/// Each known dialect has a corresponding set of type names used when generating code.
/// </summary>
public class DialectInfo
{
    public string DialectImplType { get; }
    public string DialectNamespace { get; }
    public string ColumnType { get; }
    public string TableInterface { get; }
    public string AliasInterface { get; }
    public string VirtualTableInterface { get; }
    public string CteTableInterface { get; }
    
    private DialectInfo(
        string dialectImplType,
        string dialectNamespace,
        string columnType,
        string tableInterface,
        string aliasInterface,
        string virtualTableInterface,
        string cteTableInterface)
    {
        DialectImplType = dialectImplType;
        DialectNamespace = dialectNamespace;
        ColumnType = columnType;
        TableInterface = tableInterface;
        AliasInterface = aliasInterface;
        VirtualTableInterface = virtualTableInterface;
        CteTableInterface = cteTableInterface;
    }

    public static readonly DialectInfo PgSql = new DialectInfo(
        dialectImplType: "PgSqlSqlDialectImpl",
        dialectNamespace: "Drizzle4Dotnet.PgSql",
        columnType: "PgColumn",
        tableInterface: "IPgDbTable",
        aliasInterface: "IPgTableAlias",
        virtualTableInterface: "IPgVirtualTable",
        cteTableInterface: "IPgCteTable"
    );

    public static readonly DialectInfo MySql = new DialectInfo(
        dialectImplType: "MySqlSqlDialectImpl",
        dialectNamespace: "Drizzle4Dotnet.MySql",
        columnType: "MySqlColumn",
        tableInterface: "IMySqlDbTable",
        aliasInterface: "IMySqlTableAlias",
        virtualTableInterface: "IMySqlVirtualTable",
        cteTableInterface: "IMySqlCteTable"
    );

    public static readonly DialectInfo Sqlite = new DialectInfo(
        dialectImplType: "SqliteSqlDialectImpl",
        dialectNamespace: "Drizzle4Dotnet.Sqlite",
        columnType: "SqliteColumn",
        tableInterface: "ISqliteDbTable",
        aliasInterface: "ISqliteTableAlias",
        virtualTableInterface: "ISqliteVirtualTable",
        cteTableInterface: "ISqliteCteTable"
    );

    public static readonly DialectInfo Mssql = new DialectInfo(
        dialectImplType: "MssqlSqlDialectImpl",
        dialectNamespace: "Drizzle4Dotnet.Mssql",
        columnType: "MssqlColumn",
        tableInterface: "IMssqlDbTable",
        aliasInterface: "IMssqlTableAlias",
        virtualTableInterface: "IMssqlVirtualTable",
        cteTableInterface: "IMssqlCteTable"
    );

    public static readonly DialectInfo Oracle = new DialectInfo(
        dialectImplType: "OracleSqlDialectImpl",
        dialectNamespace: "Drizzle4Dotnet.Oracle",
        columnType: "OracleColumn",
        tableInterface: "IOracleDbTable",
        aliasInterface: "IOracleTableAlias",
        virtualTableInterface: "IOracleVirtualTable",
        cteTableInterface: "IOracleCteTable"
    );

    private static readonly Dictionary<string, DialectInfo> _byTypeName = new()
    {
        { "Drizzle4Dotnet.PgSql.PgSqlSqlDialectImpl", PgSql },
        { "Drizzle4Dotnet.MySql.MySqlSqlDialectImpl", MySql },
        { "Drizzle4Dotnet.Sqlite.SqliteSqlDialectImpl", Sqlite },
        { "Drizzle4Dotnet.Mssql.MssqlSqlDialectImpl", Mssql },
        { "Drizzle4Dotnet.Oracle.OracleSqlDialectImpl", Oracle },
    };

    /// <summary>
    /// Resolves the DialectInfo from a fully qualified dialect type name.
    /// Defaults to PgSql if the type name is unknown or null.
    /// </summary>
    public static DialectInfo Resolve(string? dialectTypeName)
    {
        if (dialectTypeName != null && _byTypeName.TryGetValue(dialectTypeName, out var info))
            return info;
        return PgSql; // default for backward compatibility
    }
}
