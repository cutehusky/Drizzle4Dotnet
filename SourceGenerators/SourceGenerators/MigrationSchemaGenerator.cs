using System;
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
/// [Table("Users", Schema = "public", Dialect = typeof(PgSqlSqlDialectImpl))]
/// [UniqueConstraint(new[] { "Email" })]
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

                // Check for SqlTypeAttribute subclass (e.g., [PgSqlBigInt], [MssqlInt])
                string? attributeSqlType = null;
                foreach (var attr in subMember.GetAttributes())
                {
                    if (attr.AttributeClass == null)
                        continue;
                    if (IsSqlTypeAttribute(attr.AttributeClass))
                    {
                        // Extract the SQL type from the attribute
                        // For attributes like [PgSqlBigInt] (no args) or [PgSqlVarChar(100)]
                        if (attr.ConstructorArguments.Length > 0)
                        {
                            var attrClassName = attr.AttributeClass.Name;
                            attributeSqlType = ResolveSqlTypeFromAttribute(attrClassName, attr.ConstructorArguments.ToArray());
                        }
                        else
                        {
                            // Parameterless attribute: map attribute class name to SQL type
                            attributeSqlType = ResolveSqlTypeFromAttribute(attr.AttributeClass.Name, null);
                        }
                        break;
                    }
                }

                // Determine final nullability
                bool isNullable;
                if (notNullOverride.HasValue)
                    isNullable = !notNullOverride.Value;
                else
                    isNullable = isNullableByDefault;

                columns.Add(new ColumnSchemaModel
                {
                    DbColumnName = dbColumnName,
                    ClrType = typeName,
                    IsNullable = isNullable,
                    AttributeSqlType = attributeSqlType,
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

        // Parse typed constraint and index attributes from the table class
        var foreignKeyConstraints = new List<ForeignKeyConstraintModel>();
        var uniqueConstraints = new List<UniqueConstraintModel>();
        var primaryKeyTableConstraints = new List<PrimaryKeyTableConstraintModel>();
        var checkTableConstraints = new List<CheckTableConstraintModel>();
        var indexes = new List<TableIndexModel>();

        foreach (var attr in symbol.GetAttributes())
        {
            var attrName = attr.AttributeClass?.Name;
            if (attrName == null)
                continue;

            if (attrName == "ForeignKeyConstraintAttribute" && attr.ConstructorArguments.Length >= 4)
            {
                var constraintName = attr.ConstructorArguments[0].Value?.ToString();
                var fkColumns = attr.ConstructorArguments[1].Values
                    .Select(v => v.Value?.ToString())
                    .Where(v => v != null)
                    .Cast<string>()
                    .ToArray();
                var foreignTableType = attr.ConstructorArguments[2].Value as INamedTypeSymbol;
                var foreignColumns = attr.ConstructorArguments[3].Values
                    .Select(v => v.Value?.ToString())
                    .Where(v => v != null)
                    .Cast<string>()
                    .ToArray();

                var foreignTableName = ResolveDbTableName(foreignTableType) ?? foreignTableType?.Name ?? "?";
                foreignKeyConstraints.Add(new ForeignKeyConstraintModel
                {
                    ConstraintName = constraintName,
                    Columns = fkColumns,
                    ForeignTable = foreignTableName,
                    ForeignColumns = foreignColumns
                });
            }
            else if (attrName == "UniqueConstraintAttribute" && attr.ConstructorArguments.Length >= 1)
            {
                var uqColumns = attr.ConstructorArguments[0].Values
                    .Select(v => v.Value?.ToString())
                    .Where(v => v != null)
                    .Cast<string>()
                    .ToArray();
                uniqueConstraints.Add(new UniqueConstraintModel { Columns = uqColumns });
            }
            else if (attrName == "PrimaryKeyTableConstraintAttribute" && attr.ConstructorArguments.Length >= 1)
            {
                var pkColumns = attr.ConstructorArguments[0].Values
                    .Select(v => v.Value?.ToString())
                    .Where(v => v != null)
                    .Cast<string>()
                    .ToArray();
                primaryKeyTableConstraints.Add(new PrimaryKeyTableConstraintModel { Columns = pkColumns });
            }
            else if (attrName == "CheckTableConstraintAttribute" && attr.ConstructorArguments.Length >= 1)
            {
                var expression = attr.ConstructorArguments[0].Value?.ToString();
                if (expression != null)
                {
                    checkTableConstraints.Add(new CheckTableConstraintModel { Expression = expression });
                }
            }
            else if (attrName == "IndexAttribute" && attr.ConstructorArguments.Length >= 2)
            {
                var indexName = attr.ConstructorArguments[0].Value?.ToString() ?? "";
                var idxColumns = attr.ConstructorArguments[1].Values
                    .Select(v => v.Value?.ToString())
                    .Where(v => v != null)
                    .Cast<string>()
                    .ToArray();

                bool isUnique = false;
                string? indexType = null;
                string? where = null;
                foreach (var namedArg in attr.NamedArguments)
                {
                    switch (namedArg.Key)
                    {
                        case "IsUnique":
                            isUnique = namedArg.Value.Value?.Equals(true) ?? false;
                            break;
                        case "IndexType":
                            indexType = namedArg.Value.Value?.ToString();
                            break;
                        case "Where":
                            where = namedArg.Value.Value?.ToString();
                            break;
                    }
                }

                indexes.Add(new TableIndexModel
                {
                    IndexName = indexName,
                    Columns = idxColumns,
                    IsUnique = isUnique,
                    IndexType = indexType,
                    Where = where
                });
            }
        }

        return new TableSchemaModel
        {
            Namespace = (symbol.ContainingNamespace.IsGlobalNamespace
                || symbol.ContainingNamespace.ToDisplayString() == "<global namespace>")
                ? "" : symbol.ContainingNamespace.ToDisplayString(),
            ClassName = symbol.Name,
            DbTableName = dbTableName!,
            DbSchemaName = schemaName!,
            Dialect = dialect,
            Columns = columns,
            ForeignKeyConstraints = foreignKeyConstraints,
            UniqueConstraints = uniqueConstraints,
            PrimaryKeyTableConstraints = primaryKeyTableConstraints,
            CheckTableConstraints = checkTableConstraints,
            Indexes = indexes
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
            sb.AppendLine("using Drizzle4Dotnet.Core.Schema.Migration.Query;");
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
                string sqlType;
                if (col.AttributeSqlType != null)
                {
                    // SqlTypeAttribute subclass (e.g., [PgSqlBigInt]): use the resolved SQL string
                    sqlType = $"\"{EscapeString(col.AttributeSqlType)}\"";
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
            bool hasConstraints = table.ForeignKeyConstraints.Count > 0
                || table.UniqueConstraints.Count > 0
                || table.PrimaryKeyTableConstraints.Count > 0
                || table.CheckTableConstraints.Count > 0;

            if (hasConstraints)
            {
                sb.AppendLine("            var tableConstraints = new TableConstraint[]");
                sb.AppendLine("            {");

                foreach (var fk in table.ForeignKeyConstraints)
                {
                    var constraintName = fk.ConstraintName != null ? $"\"{EscapeString(fk.ConstraintName)}\"" : "null";
                    var columns = string.Join(", ", fk.Columns.Select(c => $"\"{EscapeString(c)}\""));
                    var foreignColumns = string.Join(", ", fk.ForeignColumns.Select(c => $"\"{EscapeString(c)}\""));
                    sb.AppendLine($"                new ForeignKeyConstraint({constraintName}, new[] {{ {columns} }}, \"{EscapeString(fk.ForeignTable)}\", new[] {{ {foreignColumns} }}),");
                }

                foreach (var uq in table.UniqueConstraints)
                {
                    var columns = string.Join(", ", uq.Columns.Select(c => $"\"{EscapeString(c)}\""));
                    sb.AppendLine($"                new UniqueConstraint(new[] {{ {columns} }}),");
                }

                foreach (var pk in table.PrimaryKeyTableConstraints)
                {
                    var columns = string.Join(", ", pk.Columns.Select(c => $"\"{EscapeString(c)}\""));
                    sb.AppendLine($"                new PrimaryKeyTableConstraint(new[] {{ {columns} }}),");
                }

                foreach (var ck in table.CheckTableConstraints)
                {
                    sb.AppendLine($"                new CheckTableConstraint(\"{EscapeString(ck.Expression)}\"),");
                }

                sb.AppendLine("            };");
                sb.AppendLine();
            }

            // Build indexes
            bool hasIndexes = table.Indexes.Count > 0;

            if (hasIndexes)
            {
                sb.AppendLine("            var indexes = new TableIndex[]");
                sb.AppendLine("            {");
                foreach (var idx in table.Indexes)
                {
                    var columns = string.Join(", ", idx.Columns.Select(c => $"\"{EscapeString(c)}\""));
                    var isUnique = idx.IsUnique ? "true" : "false";
                    var indexType = idx.IndexType != null ? $"\"{EscapeString(idx.IndexType)}\"" : "null";
                    var where = idx.Where != null ? $"\"{EscapeString(idx.Where)}\"" : "null";
                    sb.AppendLine($"                new TableIndex(\"{EscapeString(idx.IndexName)}\", \"{table.DbSchemaName}\", \"{table.DbTableName}\", new[] {{ {columns} }}, isUnique: {isUnique}, indexType: {indexType}, where: {where}),");
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
                sb.AppendLine("                , tableConstraints: tableConstraints");
            }
            if (hasIndexes)
            {
                sb.AppendLine("                , indexes: indexes");
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

    /// <summary>
    /// Resolves the database table name from a type symbol by looking for the [Table] attribute.
    /// Uses the first constructor argument (table name) or falls back to the type name.
    /// </summary>
    private static string? ResolveDbTableName(INamedTypeSymbol? typeSymbol)
    {
        if (typeSymbol == null)
            return null;

        var tableAttr = typeSymbol.GetAttributes().FirstOrDefault(a =>
            a.AttributeClass?.Name == "TableAttribute");

        if (tableAttr != null && tableAttr.ConstructorArguments.Length > 0)
        {
            var name = tableAttr.ConstructorArguments[0].Value?.ToString();
            if (!string.IsNullOrEmpty(name))
                return name;
        }

        return typeSymbol.Name;
    }

    private static string EscapeString(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
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

    /// <summary>
    /// Checks if an attribute type derives from SqlTypeAttribute by walking the base type chain.
    /// </summary>
    private static bool IsSqlTypeAttribute(INamedTypeSymbol attrClass)
    {
        var current = attrClass.BaseType;
        while (current != null)
        {
            if (current.Name == "SqlTypeAttribute")
                return true;
            current = current.BaseType;
        }
        return false;
    }

    /// <summary>
    /// Resolves the SQL type string from a SqlTypeAttribute subclass by attribute class name.
    /// This is used at source-generation time when the attribute's abstract SqlType property
    /// is not directly available via constructor arguments alone.
    /// </summary>
    private static string? ResolveSqlTypeFromAttribute(string attributeName, TypedConstant[]? args)
    {
        // Map attribute class name patterns to SQL type strings.
        // This handles the common-case parameterless attributes like [PgSqlBigInt], [MssqlInt].
        // For parameterized attributes like [PgSqlVarChar(100)], the type is reconstructed here.

        // --- PgSql ---
        if (attributeName == "PgSqlCustomAttribute")
        {
            if (args != null && args.Length >= 1)
                return args[0].Value?.ToString();
            throw new InvalidOperationException("PgSqlCustomAttribute requires a SQL type string argument.");
        }
        if (attributeName == "MySqlCustomAttribute")
        {
            if (args != null && args.Length >= 1)
                return args[0].Value?.ToString();
            throw new InvalidOperationException("MySqlCustomAttribute requires a SQL type string argument.");
        }
        if (attributeName == "SqliteCustomAttribute")
        {
            if (args != null && args.Length >= 1)
                return args[0].Value?.ToString();
            throw new InvalidOperationException("SqliteCustomAttribute requires a SQL type string argument.");
        }
        if (attributeName == "MssqlCustomAttribute")
        {
            if (args != null && args.Length >= 1)
                return args[0].Value?.ToString();
            throw new InvalidOperationException("MssqlCustomAttribute requires a SQL type string argument.");
        }
        if (attributeName == "OracleCustomAttribute")
        {
            if (args != null && args.Length >= 1)
                return args[0].Value?.ToString();
            throw new InvalidOperationException("OracleCustomAttribute requires a SQL type string argument.");
        }
        
        if (attributeName == "PgSqlIntegerAttribute") return "INTEGER";
        if (attributeName == "PgSqlBigIntAttribute") return "BIGINT";
        if (attributeName == "PgSqlSmallIntAttribute") return "SMALLINT";
        if (attributeName == "PgSqlTextAttribute") return "TEXT";
        if (attributeName == "PgSqlBooleanAttribute") return "BOOLEAN";
        if (attributeName == "PgSqlNumericAttribute")
        {
            if (args != null && args.Length >= 2)
                return $"NUMERIC({args[0].Value},{args[1].Value})";
            return "NUMERIC(18,2)";
        }
        if (attributeName == "PgSqlRealAttribute") return "REAL";
        if (attributeName == "PgSqlDoublePrecisionAttribute") return "DOUBLE PRECISION";
        if (attributeName == "PgSqlTimestampAttribute") return "TIMESTAMP";
        if (attributeName == "PgSqlDateAttribute") return "DATE";
        if (attributeName == "PgSqlTimeAttribute") return "TIME";
        if (attributeName == "PgSqlUuidAttribute") return "UUID";
        if (attributeName == "PgSqlByteaAttribute") return "BYTEA";
        if (attributeName == "PgSqlBlobAttribute") return "BYTEA";
        if (attributeName == "PgSqlCharAttribute") return "CHAR(1)";
        if (attributeName == "PgSqlVarCharAttribute")
        {
            if (args != null && args.Length >= 1)
                return $"VARCHAR({args[0].Value})";
            return "VARCHAR";
        }
        if (attributeName == "PgSqlBigSerialAttribute") return "BIGSERIAL";
        if (attributeName == "PgSqlSerialAttribute") return "SERIAL";

        // --- Mssql ---
        if (attributeName == "MssqlIntAttribute") return "INT";
        if (attributeName == "MssqlBigIntAttribute") return "BIGINT";
        if (attributeName == "MssqlSmallIntAttribute") return "SMALLINT";
        if (attributeName == "MssqlTinyIntAttribute") return "TINYINT";
        if (attributeName == "MssqlTextAttribute") return "NVARCHAR(MAX)";
        if (attributeName == "MssqlBooleanAttribute") return "BIT";
        if (attributeName == "MssqlDecimalAttribute")
        {
            if (args != null && args.Length >= 2)
                return $"DECIMAL({args[0].Value},{args[1].Value})";
            return "DECIMAL(18,2)";
        }
        if (attributeName == "MssqlRealAttribute") return "REAL";
        if (attributeName == "MssqlFloatAttribute") return "FLOAT";
        if (attributeName == "MssqlDateTime2Attribute") return "DATETIME2";
        if (attributeName == "MssqlDateAttribute") return "DATE";
        if (attributeName == "MssqlTimeAttribute") return "TIME";
        if (attributeName == "MssqlUniqueIdentifierAttribute") return "UNIQUEIDENTIFIER";
        if (attributeName == "MssqlVarBinaryAttribute") return "VARBINARY(MAX)";
        if (attributeName == "MssqlBlobAttribute") return "VARBINARY(MAX)";
        if (attributeName == "MssqlByteaAttribute") return "VARBINARY(MAX)";
        if (attributeName == "MssqlNCharAttribute") return "NCHAR(1)";
        if (attributeName == "MssqlCharAttribute") return "CHAR(1)";
        if (attributeName == "MssqlNVarCharAttribute")
        {
            if (args != null && args.Length >= 1)
                return $"NVARCHAR({args[0].Value})";
            return "NVARCHAR(MAX)";
        }
        if (attributeName == "MssqlVarCharAttribute")
        {
            if (args != null && args.Length >= 1)
                return $"VARCHAR({args[0].Value})";
            return "VARCHAR(MAX)";
        }
        if (attributeName == "MssqlBigSerialAttribute") return "BIGINT IDENTITY(1,1)";
        if (attributeName == "MssqlSerialAttribute") return "INT IDENTITY(1,1)";

        // --- MySql ---
        if (attributeName == "MySqlIntAttribute") return "INT";
        if (attributeName == "MySqlBigIntAttribute") return "BIGINT";
        if (attributeName == "MySqlSmallIntAttribute") return "SMALLINT";
        if (attributeName == "MySqlTinyIntAttribute") return "TINYINT";
        if (attributeName == "MySqlTextAttribute") return "VARCHAR(255)";
        if (attributeName == "MySqlBooleanAttribute") return "TINYINT(1)";
        if (attributeName == "MySqlDecimalAttribute")
        {
            if (args != null && args.Length >= 2)
                return $"DECIMAL({args[0].Value},{args[1].Value})";
            return "DECIMAL(18,2)";
        }
        if (attributeName == "MySqlFloatAttribute") return "FLOAT";
        if (attributeName == "MySqlDoubleAttribute") return "DOUBLE";
        if (attributeName == "MySqlDateTimeAttribute") return "DATETIME(6)";
        if (attributeName == "MySqlDateAttribute") return "DATE";
        if (attributeName == "MySqlTimeAttribute") return "TIME";
        if (attributeName == "MySqlUuidAttribute") return "CHAR(36)";
        if (attributeName == "MySqlBlobAttribute") return "BLOB";
        if (attributeName == "MySqlByteaAttribute") return "BLOB";
        if (attributeName == "MySqlCharAttribute") return "CHAR(1)";
        if (attributeName == "MySqlVarCharAttribute")
        {
            if (args != null && args.Length >= 1)
                return $"VARCHAR({args[0].Value})";
            return "VARCHAR";
        }
        if (attributeName == "MySqlBigSerialAttribute") return "BIGINT AUTO_INCREMENT";
        if (attributeName == "MySqlSerialAttribute") return "INT AUTO_INCREMENT";

        // --- Oracle ---
        if (attributeName == "OracleNumberAttribute") return "NUMBER";
        if (attributeName == "OracleIntegerAttribute") return "NUMBER(10)";
        if (attributeName == "OracleBigIntAttribute") return "NUMBER(19)";
        if (attributeName == "OracleSmallIntAttribute") return "NUMBER(5)";
        if (attributeName == "OracleTinyIntAttribute") return "NUMBER(3)";
        if (attributeName == "OracleTextAttribute") return "VARCHAR2(255)";
        if (attributeName == "OracleBooleanAttribute") return "NUMBER(1)";
        if (attributeName == "OracleDecimalAttribute")
        {
            if (args != null && args.Length >= 2)
                return $"NUMBER({args[0].Value},{args[1].Value})";
            return "NUMBER(18,2)";
        }
        if (attributeName == "OracleBinaryFloatAttribute") return "BINARY_FLOAT";
        if (attributeName == "OracleBinaryDoubleAttribute") return "BINARY_DOUBLE";
        if (attributeName == "OracleFloatAttribute") return "BINARY_FLOAT";
        if (attributeName == "OracleDoublePrecisionAttribute") return "BINARY_DOUBLE";
        if (attributeName == "OracleTimestampAttribute") return "TIMESTAMP";
        if (attributeName == "OracleDateAttribute") return "DATE";
        if (attributeName == "OracleTimeAttribute") return "INTERVAL DAY TO SECOND";
        if (attributeName == "OracleUuidAttribute") return "RAW(16)";
        if (attributeName == "OracleRawAttribute") return "RAW(16)";
        if (attributeName == "OracleBlobAttribute") return "BLOB";
        if (attributeName == "OracleByteaAttribute") return "BLOB";
        if (attributeName == "OracleCharAttribute") return "CHAR(1)";
        if (attributeName == "OracleVarChar2Attribute")
        {
            if (args != null && args.Length >= 1)
                return $"VARCHAR2({args[0].Value})";
            return "VARCHAR2(255)";
        }
        if (attributeName == "OracleNumberFormatAttribute")
        {
            if (args != null && args.Length >= 2)
                return $"NUMBER({args[0].Value},{args[1].Value})";
            return "NUMBER(18,2)";
        }

        // --- Sqlite ---
        if (attributeName == "SqliteIntegerAttribute" || attributeName == "SqliteBigIntAttribute" || attributeName == "SqliteSmallIntAttribute" || attributeName == "SqliteTinyIntAttribute") return "INTEGER";
        if (attributeName == "SqliteBooleanAttribute") return "INTEGER";
        if (attributeName == "SqliteNumericAttribute") return "NUMERIC";
        if (attributeName == "SqliteTextAttribute" || attributeName == "SqliteTimestampAttribute" || attributeName == "SqliteDateTimeAttribute" || attributeName == "SqliteDateAttribute" || attributeName == "SqliteTimeAttribute" || attributeName == "SqliteUuidAttribute" || attributeName == "SqliteCharAttribute" || attributeName == "SqliteVarCharAttribute") return "TEXT";
        if (attributeName == "SqliteRealAttribute" || attributeName == "SqliteDoublePrecisionAttribute" || attributeName == "SqliteFloatAttribute") return "REAL";
        if (attributeName == "SqliteBlobAttribute" || attributeName == "SqliteByteaAttribute") return "BLOB";
        if (attributeName == "SqliteIntegerPrimaryKeyAttribute") return "INTEGER PRIMARY KEY";

        return null;
    }

    private class ColumnSchemaModel
    {
        public string DbColumnName { get; set; } = "";
        public string ClrType { get; set; } = "";
        public bool IsNullable { get; set; }

        // Extended column configuration
        public string? AttributeSqlType { get; set; }
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
        public DialectInfo Dialect { get; set; } = null!;
        public List<ColumnSchemaModel> Columns { get; set; } = new();
        public List<ForeignKeyConstraintModel> ForeignKeyConstraints { get; set; } = new();
        public List<UniqueConstraintModel> UniqueConstraints { get; set; } = new();
        public List<PrimaryKeyTableConstraintModel> PrimaryKeyTableConstraints { get; set; } = new();
        public List<CheckTableConstraintModel> CheckTableConstraints { get; set; } = new();
        public List<TableIndexModel> Indexes { get; set; } = new();
    }

    private class ForeignKeyConstraintModel
    {
        public string? ConstraintName { get; set; }
        public string[] Columns { get; set; } = [];
        public string ForeignTable { get; set; } = "";
        public string[] ForeignColumns { get; set; } = [];
    }

    private class UniqueConstraintModel
    {
        public string[] Columns { get; set; } = [];
    }

    private class PrimaryKeyTableConstraintModel
    {
        public string[] Columns { get; set; } = [];
    }

    private class CheckTableConstraintModel
    {
        public string Expression { get; set; } = "";
    }

    private class TableIndexModel
    {
        public string IndexName { get; set; } = "";
        public string[] Columns { get; set; } = [];
        public bool IsUnique { get; set; }
        public string? IndexType { get; set; }
        public string? Where { get; set; }
    }
}
