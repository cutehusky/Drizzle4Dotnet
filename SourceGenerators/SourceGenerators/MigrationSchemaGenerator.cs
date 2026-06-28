using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators;

/// <summary>
/// Source generator that reads [Table] and [Column] attributes from partial table classes
/// and generates a GetTableDefinition() method returning a pre-built TableDefinition.
/// Supports rich column configuration: DataType, DefaultValue, AutoIncrement, 
/// NotNull/Nullable, Check, Comment, PrimaryKey, and table-level Constraints.
/// 
/// Example input:
/// <code>
/// [Table("Users", Schema = "public", Dialect = typeof(PgSqlSqlDialectImpl),
///     Constraints = new[] { "UNIQUE(Email)" })]
/// public partial class UsersTable
/// {
///     public static class Columns
///     {
///         [Column("Id", DataType = "BIGSERIAL", PrimaryKey = true, AutoIncrement = true)]
///         public static long Id { get; set; }
///         
///         [Column("Name", DataType = "VARCHAR(100)", NotNull = true)]
///         public static string Name { get; set; }
///         
///         [Column("Salary", DataType = "NUMERIC(18,2)", DefaultValue = "0")]
///         public static decimal Salary { get; set; }
///         
///         [Column("CreatedAt", DataType = "TIMESTAMP", DefaultValue = "NOW()", NotNull = true)]
///         public static DateTime CreatedAt { get; set; }
///     }
/// }
/// </code>
/// </summary>
[Generator]
public class MigrationSchemaGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => GetTableModel(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(provider.Collect(), GenerateCode);
    }

    private static TableSchemaModel? GetTableModel(GeneratorSyntaxContext ctx)
    {
        var classDecl = (ClassDeclarationSyntax)ctx.Node;
        var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        if (symbol == null)
            return null;

        var tableAttr = symbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "TableAttribute");

        if (tableAttr == null)
            return null;

        // Read table name and schema from attribute
        string? dbTableName = null;
        string? schemaName = null;
        string? dialectTypeName = null;
        var tableConstraints = new List<string>();

        if (tableAttr.ConstructorArguments.Length > 0)
            dbTableName = tableAttr.ConstructorArguments[0].Value?.ToString();
        if (tableAttr.ConstructorArguments.Length > 1)
            schemaName = tableAttr.ConstructorArguments[1].Value?.ToString();

        foreach (var namedArg in tableAttr.NamedArguments)
        {
            switch (namedArg.Key)
            {
                case "Schema":
                    if (namedArg.Value.Kind == TypedConstantKind.Primitive)
                        schemaName = namedArg.Value.Value?.ToString();
                    break;
                case "Dialect":
                    if (namedArg.Value.Value is INamedTypeSymbol typeSymbol)
                        dialectTypeName = typeSymbol.ToDisplayString();
                    break;
                case "Constraints":
                    if (namedArg.Value.Kind == TypedConstantKind.Array)
                    {
                        foreach (var val in namedArg.Value.Values)
                        {
                            if (val.Value?.ToString() is { } s)
                                tableConstraints.Add(s);
                        }
                    }
                    break;
            }
        }

        if (string.IsNullOrEmpty(dbTableName))
            dbTableName = symbol.Name;
        if (string.IsNullOrEmpty(schemaName))
            schemaName = "public";

        var dialect = DialectInfo.Resolve(dialectTypeName);

        // Read columns from nested Columns class
        var columns = new List<ColumnSchemaModel>();

        foreach (var member in symbol.GetMembers().OfType<INamedTypeSymbol>())
        {
            if (member.Name != "Columns")
                continue;

            foreach (var subMember in member.GetMembers().OfType<IPropertySymbol>())
            {
                var colAttr = subMember.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass?.Name == "ColumnAttribute");

                if (colAttr == null)
                    continue;

                var dbColumnName = colAttr.ConstructorArguments.Length > 0
                    ? colAttr.ConstructorArguments[0].Value?.ToString() ?? subMember.Name
                    : subMember.Name;

                var clrType = subMember.Type;
                var typeName = clrType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                var isNullableByDefault = clrType.NullableAnnotation == NullableAnnotation.Annotated
                    || (clrType is INamedTypeSymbol namedType && namedType.IsGenericType &&
                        namedType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T);

                // Read column attribute properties
                string? explicitDataType = null;
                string? defaultValue = null;
                bool autoIncrement = false;
                bool? notNullOverride = null;
                string? checkExpression = null;
                string? comment = null;
                bool primaryKey = false;

                foreach (var namedArg in colAttr.NamedArguments)
                {
                    switch (namedArg.Key)
                    {
                        case "DataType":
                            explicitDataType = namedArg.Value.Value?.ToString();
                            break;
                        case "DefaultValue":
                            defaultValue = namedArg.Value.Value?.ToString();
                            break;
                        case "AutoIncrement":
                            autoIncrement = namedArg.Value.Value?.Equals(true) ?? false;
                            break;
                        case "NotNull":
                            notNullOverride = namedArg.Value.Value?.Equals(true) ?? false;
                            break;
                        case "Nullable":
                            if (notNullOverride == null)
                                notNullOverride = !(namedArg.Value.Value?.Equals(true) ?? false);
                            break;
                        case "Check":
                            checkExpression = namedArg.Value.Value?.ToString();
                            break;
                        case "Comment":
                            comment = namedArg.Value.Value?.ToString();
                            break;
                        case "PrimaryKey":
                            primaryKey = namedArg.Value.Value?.Equals(true) ?? false;
                            break;
                    }
                }

                // Also check for [PrimaryKey] attribute
                if (!primaryKey)
                {
                    primaryKey = subMember.GetAttributes().Any(a =>
                        a.AttributeClass?.Name == "PrimaryKeyAttribute");
                }

                // Determine final nullability
                bool isNullable;
                if (notNullOverride.HasValue)
                    isNullable = !notNullOverride.Value;
                else
                    isNullable = isNullableByDefault;

                columns.Add(new ColumnSchemaModel
                {
                    PropName = subMember.Name,
                    DbColumnName = dbColumnName,
                    ClrType = typeName,
                    IsNullable = isNullable,
                    ExplicitDataType = explicitDataType,
                    DefaultValue = defaultValue,
                    AutoIncrement = autoIncrement,
                    PrimaryKey = primaryKey,
                    CheckExpression = checkExpression,
                    Comment = comment
                });
            }
            break;
        }

        if (columns.Count == 0)
            return null;

        var hasExistingInterface = symbol.AllInterfaces.Any(i =>
            i.Name == dialect.TableInterface.Replace("I", ""));

        return new TableSchemaModel
        {
            Namespace = (symbol.ContainingNamespace.IsGlobalNamespace
                || symbol.ContainingNamespace.ToDisplayString() == "<global namespace>")
                ? "" : symbol.ContainingNamespace.ToDisplayString(),
            ClassName = symbol.Name,
            DbTableName = dbTableName,
            DbSchemaName = schemaName,
            Dialect = dialect,
            Columns = columns,
            TableConstraints = tableConstraints,
            HasExistingInterface = hasExistingInterface
        };
    }

    private static void GenerateCode(
        SourceProductionContext context,
        ImmutableArray<TableSchemaModel?> tables)
    {
        if (tables.IsDefaultOrEmpty)
            return;

        foreach (var table in tables)
        {
            if (table == null)
                continue;

            var sb = new StringBuilder();
            var dialect = table.Dialect;

            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("#pragma warning disable CS0108, CS0114, CS8600, CS8601, CS8602, CS8603, CS8604, CS8618");

            bool isGlobalNamespace = string.IsNullOrEmpty(table.Namespace);

            if (!isGlobalNamespace)
            {
                sb.AppendLine($"namespace {table.Namespace};");
                sb.AppendLine();
            }

            sb.AppendLine("using Drizzle4Dotnet.Core.Schema.Migration;");
            sb.AppendLine("using Drizzle4Dotnet.Core.Shared;");
            sb.AppendLine($"using {dialect.DialectNamespace};");
            sb.AppendLine($"using {dialect.DialectNamespace}.Schema;");
            sb.AppendLine();

            sb.AppendLine($"partial class {table.ClassName}");
            sb.AppendLine("{");

            sb.AppendLine("    private static TableDefinition? _schemaDefinition;");
            sb.AppendLine();

            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Gets a TableDefinition for this table, usable with the Migration system.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static TableDefinition SchemaDefinition");
            sb.AppendLine("    {");
            sb.AppendLine("        get");
            sb.AppendLine("        {");
            sb.AppendLine("            if (_schemaDefinition != null)");
            sb.AppendLine("                return _schemaDefinition;");
            sb.AppendLine();

            // Build column definitions
            var columnTypeName = $"ColumnDefinition<{dialect.DialectImplType}>";
            sb.AppendLine("            IColumnDefinition[] columns = new IColumnDefinition[]");
            sb.AppendLine("            {");
            foreach (var col in table.Columns)
            {
                bool isExplicitType = col.ExplicitDataType != null;
                string sqlType;
                if (isExplicitType)
                {
                    // Explicit DataType from attribute: use raw string
                    sqlType = $"\"{EscapeString(col.ExplicitDataType)}\"";
                }
                else
                {
                    // Auto-mapped type: use dialect-specific const string reference
                    sqlType = MapClrToSqlTypeRef(col.ClrType, dialect.DialectImplType);
                }

                var isNullable = col.IsNullable ? "true" : "false";
                var autoIncrement = col.AutoIncrement ? "true" : "false";
                var primaryKey = col.PrimaryKey ? "true" : "false";

                sb.AppendLine($"                new {columnTypeName}(\"{col.DbColumnName}\", {sqlType})");
                sb.AppendLine("                {");

                if (!col.IsNullable)
                    sb.AppendLine($"                    IsNullable = {isNullable},");
                else
                    sb.AppendLine($"                    IsNullable = {isNullable},");

                if (col.PrimaryKey)
                    sb.AppendLine($"                    IsPrimaryKey = {primaryKey},");

                if (col.AutoIncrement)
                    sb.AppendLine($"                    IsAutoIncrement = {autoIncrement},");

                if (col.DefaultValue != null)
                    sb.AppendLine($"                    DefaultValue = \"{EscapeString(col.DefaultValue)}\",");

                if (col.CheckExpression != null)
                    sb.AppendLine($"                    CheckExpression = \"{EscapeString(col.CheckExpression)}\",");

                if (col.Comment != null)
                    sb.AppendLine($"                    Comment = \"{EscapeString(col.Comment)}\",");

                sb.AppendLine("                },");
            }
            sb.AppendLine("            };");
            sb.AppendLine();

            // Build table constraints
            bool hasConstraints = table.TableConstraints.Count > 0;

            if (hasConstraints)
            {
                sb.AppendLine("            var tableConstraints = new string[]");
                sb.AppendLine("            {");
                foreach (var constraint in table.TableConstraints)
                {
                    sb.AppendLine($"                \"{EscapeString(constraint)}\",");
                }
                sb.AppendLine("            };");
                sb.AppendLine();
            }

            sb.AppendLine($"            _schemaDefinition = new TableDefinition(");
            sb.AppendLine($"                \"{table.DbTableName}\",");
            sb.AppendLine($"                \"{table.DbSchemaName}\",");
            sb.AppendLine("                columns");
            if (hasConstraints)
            {
                sb.AppendLine("                , tableConstraints");
            }
            sb.AppendLine("            );");
            sb.AppendLine();
            sb.AppendLine("            return _schemaDefinition;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Generate convenience method
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Creates a CREATE TABLE query for this table.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static CreateTableQuery CreateTableQuery(bool ifNotExists = false)");
            sb.AppendLine("    {");
            sb.AppendLine("        var q = new CreateTableQuery(SchemaDefinition);");
            sb.AppendLine("        if (ifNotExists)");
            sb.AppendLine("            q = q.IfNotExists();");
            sb.AppendLine("        return q;");
            sb.AppendLine("    }");
            sb.AppendLine();

            // Generate convenience method for building CREATE TABLE SQL
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Builds the CREATE TABLE SQL for this table.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine(
                $"    public static string ToCreateTableSql{GetDialectSuffix(dialect.DialectImplType)}(bool ifNotExists = false)");
            sb.AppendLine("    {");
            sb.AppendLine("        var q = CreateTableQuery(ifNotExists);");
            sb.AppendLine(
                $"        var builder = new SqlBuilder<{dialect.DialectImplType}>();");
            sb.AppendLine("        q.BuildSql(builder);");
            sb.AppendLine("        return builder.Build().Item1;");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            var nsPrefix = string.IsNullOrEmpty(table.Namespace)
                ? "" : $"{table.Namespace.Replace(".", "_")}_";
            context.AddSource($"{nsPrefix}{table.ClassName}_Migration.g.cs",
                SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private static string EscapeString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string MapClrToSqlType(string fullyQualifiedTypeName, string dialectType)
    {
        var simplified = fullyQualifiedTypeName
            .Replace("global::", "")
            .Replace("System.", "")
            .Replace("?", "");

        var isSqlite = dialectType.Contains("Sqlite");
        var isMySql = dialectType.Contains("MySql");
        var isPgSql = dialectType.Contains("PgSql");
        var isMssql = dialectType.Contains("Mssql");
        var isOracle = dialectType.Contains("Oracle");

        if (isSqlite)
        {
            return simplified switch
            {
                "int" or "Int32" or "long" or "Int64" or "short" or "Int16" or "byte" or "Byte" => "INTEGER",
                "string" or "String" or "DateTime" or "DateOnly" or "TimeOnly" or "Guid" or "char" or "Char" => "TEXT",
                "bool" or "Boolean" => "INTEGER",
                "decimal" or "Decimal" => "NUMERIC",
                "float" or "Single" or "double" or "Double" => "REAL",
                "byte[]" or "Byte[]" => "BLOB",
                _ => "TEXT"
            };
        }

        if (isMssql)
        {
            return simplified switch
            {
                "int" or "Int32" => "INT",
                "long" or "Int64" => "BIGINT",
                "short" or "Int16" => "SMALLINT",
                "byte" or "Byte" => "TINYINT",
                "string" or "String" => "NVARCHAR(MAX)",
                "bool" or "Boolean" => "BIT",
                "decimal" or "Decimal" => "DECIMAL(18,2)",
                "float" or "Single" => "REAL",
                "double" or "Double" => "FLOAT",
                "DateTime" => "DATETIME2",
                "DateOnly" => "DATE",
                "TimeOnly" => "TIME",
                "Guid" => "UNIQUEIDENTIFIER",
                "byte[]" or "Byte[]" => "VARBINARY(MAX)",
                "char" or "Char" => "NCHAR(1)",
                _ => "NVARCHAR(MAX)"
            };
        }

        if (isOracle)
        {
            return simplified switch
            {
                "int" or "Int32" => "NUMBER(10)",
                "long" or "Int64" => "NUMBER(19)",
                "short" or "Int16" => "NUMBER(5)",
                "byte" or "Byte" => "NUMBER(3)",
                "string" or "String" => "VARCHAR2(255)",
                "bool" or "Boolean" => "NUMBER(1)",
                "decimal" or "Decimal" => "NUMBER(18,2)",
                "float" or "Single" => "BINARY_FLOAT",
                "double" or "Double" => "BINARY_DOUBLE",
                "DateTime" => "TIMESTAMP",
                "DateOnly" => "DATE",
                "TimeOnly" => "INTERVAL DAY TO SECOND",
                "Guid" => "RAW(16)",
                "byte[]" or "Byte[]" => "BLOB",
                "char" or "Char" => "CHAR(1)",
                _ => "VARCHAR2(255)"
            };
        }

        return simplified switch
        {
            "int" or "Int32" => "INTEGER",
            "long" or "Int64" => "BIGINT",
            "short" or "Int16" => "SMALLINT",
            "byte" or "Byte" => isMySql ? "TINYINT" : "SMALLINT",
            "string" or "String" => isMySql ? "VARCHAR(255)" : "TEXT",
            "bool" or "Boolean" => isMySql ? "TINYINT(1)" : "BOOLEAN",
            "decimal" or "Decimal" => isMySql ? "DECIMAL(18,2)" : "NUMERIC(18,2)",
            "float" or "Single" => "REAL",
            "double" or "Double" => isMySql ? "DOUBLE" : "DOUBLE PRECISION",
            "DateTime" => isMySql ? "DATETIME(6)" : "TIMESTAMP",
            "DateOnly" => "DATE",
            "TimeOnly" => "TIME",
            "Guid" => isMySql ? "CHAR(36)" : "UUID",
            "byte[]" or "Byte[]" => isMySql ? "BLOB" : "BYTEA",
            "char" or "Char" => "CHAR(1)",
            _ => "TEXT"
        };
    }

    /// <summary>
    /// Returns the dialect type const class name (e.g., "PgSqlType", "MySqlType", "SqliteType").
    /// </summary>
    private static string GetTypeConstClassName(string dialectType)
    {
        if (dialectType.Contains("MySql"))
            return "MySqlType";
        if (dialectType.Contains("Sqlite"))
            return "SqliteType";
        if (dialectType.Contains("Mssql"))
            return "MssqlType";
        if (dialectType.Contains("Oracle"))
            return "OracleType";
        return "PgSqlType";
    }

    /// <summary>
    /// Maps a CLR type to a dialect-specific const string reference for use in generated code.
    /// Returns a code fragment like "PgSqlType.BigInt" instead of a raw SQL string literal.
    /// </summary>
    private static string MapClrToSqlTypeRef(string fullyQualifiedTypeName, string dialectType)
    {
        var simplified = fullyQualifiedTypeName
            .Replace("global::", "")
            .Replace("System.", "")
            .Replace("?", "");

        var typeClass = GetTypeConstClassName(dialectType);
        var isMySql = dialectType.Contains("MySql");

        var propName = simplified switch
        {
            "int" or "Int32" => "Integer",
            "long" or "Int64" => "BigInt",
            "short" or "Int16" => "SmallInt",
            "byte" or "Byte" => "TinyInt",
            "string" or "String" => "Text",
            "bool" or "Boolean" => "Boolean",
            "decimal" or "Decimal" => "Numeric",
            "float" or "Single" => "Float",
            "double" or "Double" => "DoublePrecision",
            "DateTime" => "Timestamp",
            "DateOnly" => "Date",
            "TimeOnly" => "Time",
            "Guid" => "Uuid",
            "byte[]" or "Byte[]" => "Bytea",
            "char" or "Char" => "Char",
            _ => "Text",
        };

        // Handle dialect-specific naming differences
        if (isMySql)
        {
            if (propName is "Float") propName = "Float";
            if (propName is "DoublePrecision") propName = "Double";
            if (propName is "Timestamp") propName = "DateTime";
            if (propName is "Numeric") propName = "Decimal";
            if (propName is "Integer") propName = "Int";
            if (propName is "Bytea") propName = "Blob";
        }
        else if (typeClass == "SqliteType")
        {
            // SQLite uses generic types, many map to same const
            if (propName is "BigInt" or "SmallInt" or "TinyInt") propName = "Integer";
            if (propName is "DoublePrecision" or "Float") propName = "Real";
            if (propName is "Timestamp") propName = "Text";
            if (propName is "Uuid" or "Char") propName = "Text";
            if (propName is "Date" or "Time") propName = "Text";
            if (propName is "Bytea") propName = "Blob";
        }
        else if (typeClass == "OracleType")
        {
            if (propName is "Integer") propName = "Integer";
            if (propName is "Numeric") propName = "Decimal";
            if (propName is "Float") propName = "BinaryFloat";
            if (propName is "DoublePrecision") propName = "BinaryDouble";
            if (propName is "Time") propName = "Time";
            if (propName is "Bytea") propName = "Blob";
            if (propName is "Uuid") propName = "Uuid";
        }
        else if (typeClass == "MssqlType")
        {
            if (propName is "Integer") propName = "Int";
            if (propName is "Numeric") propName = "Decimal";
            if (propName is "Timestamp") propName = "DateTime2";
            if (propName is "DoublePrecision") propName = "Float";
            if (propName is "Uuid") propName = "UniqueIdentifier";
            if (propName is "Bytea") propName = "VarBinary";
            if (propName is "Char") propName = "NChar";
        }
        else
        {
            // PgSqlType
            if (propName is "TinyInt") propName = "SmallInt";
            if (propName is "Float") propName = "Real";
        }

        return $"{typeClass}.{propName}";
    }

    private static string GetDialectSuffix(string dialectType)
    {
        if (dialectType.Contains("MySql"))
            return "MySql";
        if (dialectType.Contains("Sqlite"))
            return "Sqlite";
        if (dialectType.Contains("Mssql"))
            return "Mssql";
        if (dialectType.Contains("Oracle"))
            return "Oracle";
        return "PgSql";
    }

    private class ColumnSchemaModel
    {
        public string PropName { get; set; } = "";
        public string DbColumnName { get; set; } = "";
        public string ClrType { get; set; } = "";
        public bool IsNullable { get; set; }

        // Extended column configuration
        public string? ExplicitDataType { get; set; }
        public string? DefaultValue { get; set; }
        public bool AutoIncrement { get; set; }
        public bool PrimaryKey { get; set; }
        public string? CheckExpression { get; set; }
        public string? Comment { get; set; }
    }

    private class TableSchemaModel
    {
        public string Namespace { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string DbTableName { get; set; } = "";
        public string DbSchemaName { get; set; } = "public";
        public DialectInfo Dialect { get; set; } = DialectInfo.PgSql;
        public List<ColumnSchemaModel> Columns { get; set; } = new();
        public List<string> TableConstraints { get; set; } = new();
        public bool HasExistingInterface { get; set; }
    }
}
