using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerators;

[Generator]
public class TableGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: static (ctx, _) => GetTableInfo(ctx))
            .Where(static m => m is not null);

        context.RegisterSourceOutput(provider.Collect(), GenerateCode);
    }

    private static TableModel? GetTableInfo(GeneratorSyntaxContext ctx)
    {
        var classDecl = (ClassDeclarationSyntax)ctx.Node;
        var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        if (symbol == null)
            return null;

        var tableAttr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "TableAttribute");
        var aliasAttr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "AliasAttribute");
        var virtualTableAttr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "VirtualAttribute");
        
        if (tableAttr != null && aliasAttr == null && virtualTableAttr == null)
        {
            string? dbTableName = "", schemaName = "";
            if (tableAttr.ConstructorArguments.Length > 0)
            {
                dbTableName = tableAttr.ConstructorArguments[0].Value?.ToString();
                if (tableAttr.ConstructorArguments.Length > 1)
                {
                    schemaName = tableAttr.ConstructorArguments[1].Value?.ToString();
                }
            }
            if (string.IsNullOrEmpty(dbTableName))
                dbTableName =  symbol.Name;
            if (string.IsNullOrEmpty(schemaName))
                schemaName = "public";

            var columns = new List<(string PropName, string DbColumnName, string Type)>();
            foreach (var member in symbol.GetMembers().OfType<INamedTypeSymbol>())
            {
                if (member == null)
                    continue;
                if (member.Name != "Columns")
                    continue;
                foreach (var subMember in member.GetMembers().OfType<IPropertySymbol>())
                {
                    var colAttr = subMember.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "ColumnAttribute");
                    if (colAttr != null && colAttr.ConstructorArguments.Length > 0)
                    {
                        var dbColumnName = colAttr.ConstructorArguments[0].Value?.ToString() ?? subMember.Name;
                        var colType = subMember.Type.ToDisplayString();
                        columns.Add((subMember.Name, dbColumnName, colType));
                    }
                }
                break;
            }

            return new TableModel
            {
                Namespace = (symbol.ContainingNamespace == null 
                             || symbol.ContainingNamespace.IsGlobalNamespace
                             ||  symbol.ContainingNamespace.ToDisplayString() == "<global namespace>"
                             ) 
                    ? "" : symbol.ContainingNamespace.ToDisplayString(),
                ClassName = symbol.Name,
                TableType = ETableType.DbTable,
                
                Columns = columns,
                DbTableName = dbTableName,
                DbSchemaName = schemaName
            };
        }
        
        if (aliasAttr != null && tableAttr == null && virtualTableAttr == null)
        {
            if (aliasAttr.ConstructorArguments.Length == 0)
                return null; // must have at least the source table type
            
            var sourceTableType = aliasAttr.ConstructorArguments[0].Value as INamedTypeSymbol;
            var aliasName = symbol.Name; // default to class name
            if (aliasAttr.ConstructorArguments.Length > 1)
            {
                aliasName = aliasAttr.ConstructorArguments[1].Value?.ToString() ?? symbol.Name;
            }

            var sourceTableFullName = sourceTableType?.ToDisplayString();

            return new TableModel
            {
                Namespace = (symbol.ContainingNamespace == null 
                             || symbol.ContainingNamespace.IsGlobalNamespace
                             ||  symbol.ContainingNamespace.ToDisplayString() == "<global namespace>"
                    ) 
                        ? "" : symbol.ContainingNamespace.ToDisplayString(),
                ClassName = symbol.Name,
                TableType = ETableType.Alias,
                
                AliasName = aliasName,
                SourceTableFullName = sourceTableFullName
            };
        }
        return null;
    }

    private static void GenerateCode(
        SourceProductionContext context, 
        ImmutableArray<TableModel?> tables)
    {
        if (tables.IsDefaultOrEmpty) 
            return;
        var validTables = tables.Where(m => m != null).Cast<TableModel>().ToList();

        foreach (var table in validTables)
        {
            if (table == null)
                continue;
            var sb = new StringBuilder();
            var targetModel = table;

            if (table.TableType == ETableType.Alias)
            {
                var source = validTables.FirstOrDefault(m => 
                    m.TableType ==  ETableType.DbTable && 
                    ($"{m.Namespace}.{m.ClassName}" == table.SourceTableFullName 
                     || m.ClassName == table.SourceTableFullName));
            
                if (source == null) continue;
                
                targetModel.Columns = source.Columns!;
                targetModel.DbSchemaName = source.DbSchemaName;
                targetModel.DbTableName = source.DbTableName;
            }
            
            sb.AppendLine("// <auto-generated/>");
            
            bool isGlobalNamespace = string.IsNullOrEmpty(table.Namespace) || table.Namespace == "<global namespace>";
        
            if (!isGlobalNamespace)
            {
                sb.AppendLine($"namespace {table.Namespace};");
                sb.AppendLine();
            }
            
            
            sb.AppendLine("using Drizzle4Dotnet.Core.Query.Select;");
            sb.AppendLine("using Drizzle4Dotnet.Core.Query.Insert;");
            sb.AppendLine("using Drizzle4Dotnet.Core.Query.Update;");
            sb.AppendLine("using Drizzle4Dotnet.Core.Shared;");
            sb.AppendLine("using Drizzle4Dotnet.Core.Schema.Columns;");
            sb.AppendLine("using Drizzle4Dotnet.Core.Schema.Tables;");
            sb.AppendLine("using Drizzle4Dotnet.Dialect;");
            sb.AppendLine("using System.Text;");
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data.Common;");
            
            var refName = table.TableType == ETableType.DbTable ? table.DbTableName! : table.AliasName!;
            var baseInterface = table.TableType == ETableType.DbTable
                ? "IDbTable<PgSqlSqlDialectImpl>"
                : "ITableAlias<PgSqlSqlDialectImpl>";
            var tableProperties = table.TableType == ETableType.DbTable
                ? $@"   public static string TableName {{ get => ""{table.DbTableName!}""; }}
    public static string SchemaName {{ get => ""{table.DbSchemaName!}""; }}" 
                : $@"   public static string Alias {{ get => ""{table.AliasName!}""; }}";
            var selectSqlFragments = string.Join(", ", table.Columns!.Select(p => $"{{{p.PropName}.Sql}}"));
            
            sb.AppendLine($@"
    partial class {table.ClassName} : {baseInterface}
    {{
        {tableProperties}

        public static string TableRefName {{ get => ""{refName}""; }}

        private static readonly string _sql;

        public string BuildSql(Dictionary<string, object?> parameters) => _sql;
        public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb) => sb.Append(_sql);
");
            var tableSql = table.TableType == ETableType.DbTable
                ? $"_sql = PgSqlSqlDialectImpl.BuildTableName(\"{table.DbSchemaName}\", \"{table.DbTableName}\");"
                : $"_sql = $\"{{PgSqlSqlDialectImpl.BuildTableName(\"{table.DbSchemaName}\", \"{table.DbTableName}\")}} AS {{ PgSqlSqlDialectImpl.BuildIdentifier(\"{table.AliasName}\")}}\";";

            sb.AppendLine($@"
        static {table.ClassName}() {{
                {string.Join("\n        ", table.Columns!.Select(c =>  $"{c.PropName} = new DbColumn<{c.Type}, {table.ClassName}, PgSqlSqlDialectImpl>(\"{c.DbColumnName}\");"))}
                {tableSql}
                GeneratedModelSelection._sql = $""{selectSqlFragments}"";
                GeneratedResultSelection._sql = $""{selectSqlFragments}"";
        }}");

            sb.AppendLine(string.Join("\n        ", table.Columns!.Select(c => $"public static DbColumn<{c.Type}, {table.ClassName}, PgSqlSqlDialectImpl> {c.PropName} {{ get; set; }}")));
            
            sb.AppendLine(@"
        public static class ColumnNames
        {");
            foreach (var col in table.Columns!)
            {
                sb.AppendLine($@"        public const string {col.PropName} = ""{col.PropName}"";");
            }

            sb.AppendLine("    }");
            
            var selectStructProperties = string.Join(", ", table.Columns!.Select(p => $"{p.Type} {p.PropName}"));
            var selectModelProperties = string.Join("\n        ",
                table.Columns!.Select(p => $"public {p.Type} {p.PropName} {{ get; set; }} = default!;"));
            var selectStructMapper = string.Join(",\n                ",
                table.Columns!.Select((p, i) => $"r.IsDBNull({i}) ? default! : r.GetFieldValue<{p.Type}>({i})"));
            var selectModelMapper = string.Join(",\n                ",
                table.Columns!.Select((p, i) =>
                    $"{p.PropName} = r.IsDBNull({i}) ? default! : r.GetFieldValue<{p.Type}>({i})"));
            sb.AppendLine($@"
    public static ISelectedColumns<SelectResult, PgSqlSqlDialectImpl, GeneratedSubqueryTable> ResultAll {{ get; }} = new GeneratedResultSelection();
    public static ISelectedColumns<SelectModel, PgSqlSqlDialectImpl, GeneratedSubqueryTable> ModelAll {{ get; }} = new GeneratedModelSelection();

    public readonly record struct SelectResult({selectStructProperties});

    public class SelectModel {{
        {selectModelProperties}
    }}

    public class GeneratedSubqueryTable: IVirtualTable<PgSqlSqlDialectImpl>
    {{
        private readonly IGenericSql _baseSql;
        private readonly string _aliasName;
        public string BuildSql(Dictionary<string, object?> parameters) => $""({{_baseSql.BuildSql(parameters)}}) AS {{_aliasName}}"";
        public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb) {{
            sb.Append('(');
            _baseSql.BuildSql(parameters, sb);
            sb.Append($"") AS "");
            sb.Append(_aliasName);
        }}

        {string.Join("\n        ", table.Columns!.Select(p => $"public VirtualColumn<{p.Type}, PgSqlSqlDialectImpl> {p.PropName} {{ get; set; }}"))}

        public GeneratedSubqueryTable(
            string aliasName,
            IGenericSql baseSql
        ) {{
            _baseSql = baseSql;
            _aliasName = aliasName;
            {string.Join("\n            ", table.Columns!.Select(p => $"this.{p.PropName} = new VirtualColumn<{p.Type}, PgSqlSqlDialectImpl>(aliasName, \"{p.DbColumnName}\");"))}
        }}

        public static IVirtualTable<PgSqlSqlDialectImpl> Create(IGenericSql baseSql, string aliasName) => new GeneratedSubqueryTable(aliasName, baseSql);
    }}

    private sealed class GeneratedResultSelection : ISelectedColumns<SelectResult, PgSqlSqlDialectImpl, GeneratedSubqueryTable>
    {{
        public static string _sql;
        public string BuildSql(Dictionary<string, object?> parameters) => _sql;

        public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
        {{
            sb.Append(_sql);
        }}

        public SelectResult Mapper(DbDataReader r)
        {{
            return new SelectResult(
                {selectStructMapper}
            );
        }}
    }}

    private sealed class GeneratedModelSelection : ISelectedColumns<SelectModel, PgSqlSqlDialectImpl, GeneratedSubqueryTable>
    {{
        public static string _sql;

        public string BuildSql(Dictionary<string, object?> parameters) => _sql;
        public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
        {{
            sb.Append(_sql);
        }}

        public SelectModel Mapper(DbDataReader r)
        {{
            return new SelectModel {{
                {selectModelMapper}
            }};
        }}
    }}
");
            var insertModelProperties = string.Join("\n    ", table.Columns!.Select(c => 
                $"public {c.Type} {c.PropName} {{ get; set; }}"));

            var insertWriter = string.Join("\n        ", table.Columns!.Select(c => 
                $@"if ({c.PropName} != null) values[PgSqlSqlDialectImpl.BuildIdentifier(""{c.DbColumnName}"")] = {c.PropName};"));

            sb.AppendLine($@"
    public class InsertModel : IInsertRecord<{table.ClassName}, PgSqlSqlDialectImpl>
    {{
        {insertModelProperties}

        public void Writer(Dictionary<string, object?> values)
        {{
            {insertWriter}
        }}
    }}
");

            var insertStructProperties = string.Join(", ", table.Columns!.Select(c => $"{c.Type} {c.PropName} = default"));

            sb.AppendLine($@"
    public readonly record struct InsertRecord({insertStructProperties}) : IInsertRecord<{table.ClassName}, PgSqlSqlDialectImpl>
    {{
        public void Writer(Dictionary<string, object?> values)
        {{
            {insertWriter}
        }}
    }}
");

            var updateModelProperties = string.Join("\n    ", table.Columns!.Select(c => 
                $"public Optional<{c.Type}> {c.PropName} {{ get; set; }} = default;"));

            var updateWriter = string.Join("\n        ", table.Columns!.Select(c => 
                $@"if ({c.PropName}.HasValue) values[PgSqlSqlDialectImpl.BuildIdentifier(""{c.DbColumnName}"")] = {c.PropName}.Value;"));

            sb.AppendLine($@"
    public class UpdateModel : IUpdateRecord<{table.ClassName}, PgSqlSqlDialectImpl>
    {{
        {updateModelProperties}

        public void Writer(Dictionary<string, object?> values)
        {{
            {updateWriter}
        }}
    }}
");

            var updateStructProperties = string.Join(", ", table.Columns!.Select(c => 
                $"Optional<{c.Type}> {c.PropName} = default"));


            sb.AppendLine($@"
    public readonly record struct UpdateRecord({updateStructProperties}) : IUpdateRecord<{table.ClassName}, PgSqlSqlDialectImpl>
    {{
        public void Writer(Dictionary<string, object?> values)
        {{
            {updateWriter}
        }}
    }}
");
            sb.AppendLine(@"
        }");
            context.AddSource($"{table.ClassName}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    enum ETableType
    {
        DbTable,
        Alias,
    }

    private class TableModel
    {
        public string Namespace { get; set; } = "";
        public string ClassName { get; set; } = "";

        public ETableType  TableType { get; set; }
        
        // For Table
        public string? DbTableName { get; set; }
        public string? DbSchemaName { get; set; }
        public List<(string PropName, string DbColumnName, string Type)>? Columns { get; set; }
        
        // For Alias
        public string? AliasName { get; set; }
        public string? SourceTableFullName { get; set; } 
    }
}
