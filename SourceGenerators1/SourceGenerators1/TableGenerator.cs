using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

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

        if (tableAttr == null &&  aliasAttr == null)
            return null;
        if  (tableAttr != null && aliasAttr != null)
            return null; // can't be both a table and an alias


        if (tableAttr != null)
        {
            var dbTableName = symbol.Name; // default to class name
            if (tableAttr.ConstructorArguments.Length > 0)
            {
                dbTableName = tableAttr.ConstructorArguments[0].Value?.ToString() ?? dbTableName;
            }

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
                        var colType = ((INamedTypeSymbol)subMember.Type).TypeArguments[0].ToDisplayString();
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
                IsAlias = false,
                
                Columns = columns,
                DbTableName = dbTableName
            };
        }
        
        if (aliasAttr != null)
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
                IsAlias = true,
                
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

            if (table.IsAlias)
            {
                var source = validTables.FirstOrDefault(m => 
                    !m.IsAlias && 
                    ($"{m.Namespace}.{m.ClassName}" == table.SourceTableFullName 
                     || m.ClassName == table.SourceTableFullName));
            
                if (source == null) continue;
                
                targetModel.Columns = source.Columns!;
                targetModel.DbTableName = source.DbTableName!;
            }
            
            sb.AppendLine($@"// <auto-generated/>");
            
            bool isGlobalNamespace = string.IsNullOrEmpty(table.Namespace) || table.Namespace == "<global namespace>";
        
            if (!isGlobalNamespace)
            {
                sb.AppendLine($"namespace {table.Namespace};");
                sb.AppendLine();
            }
            
            var refName = table.IsAlias ? table.AliasName : table.DbTableName;
            var tableSql = table.IsAlias ? $"{table.DbTableName} AS {table.AliasName}" : table.DbTableName;
            
            sb.AppendLine("using Drizzle_Like.Query.Select;");
            sb.AppendLine("using Drizzle_Like.Query.Insert;");
            sb.AppendLine("using Drizzle_Like.Query.Update;");
            sb.AppendLine("using Drizzle_Like.Shared;");
            sb.AppendLine("using Drizzle_Like.Schema.Columns;");
            sb.AppendLine("using Drizzle_Like.Schema.Tables;");
            
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Data.Common;");

            sb.AppendLine($@"
    partial class {table.ClassName} : ITable
    {{
        public static string TableName {{ get => ""{table.DbTableName}""; }}

        public static string Alias {{ get => ""{table.AliasName}""; }}

        public static string TableRefName {{ get => ""{refName}""; }}

        public string Sql => $""{tableSql}"";

        public string Identifier => $""\""{refName}\"""";
");
            var selectStructProperties = string.Join(", ", table.Columns!.Select(p => $"{p.Type} {p.PropName}"));
            var selectModelProperties = string.Join("\n        ",
                table.Columns!.Select(p => $"public {p.Type} {p.PropName} {{ get; set; }} = default!;"));
            var sqlFragments = string.Join(", ", table.Columns!.Select(p => $"{{{p.PropName}.Sql}}"));
            var selectStructMapper = string.Join(",\n                ",
                table.Columns!.Select((p, i) => $"r.IsDBNull({i}) ? default! : r.GetFieldValue<{p.Type}>({i})"));
            var selectModelMapper = string.Join(",\n                ",
                table.Columns!.Select((p, i) =>
                    $"{p.PropName} = r.IsDBNull({i}) ? default! : r.GetFieldValue<{p.Type}>({i})"));
            sb.AppendLine($@"
    public static ISelectedColumns<SelectResult> ResultAll {{ get; }} = new GeneratedResultSelection();
    public static ISelectedColumns<SelectModel> ModelAll {{ get; }} = new GeneratedModelSelection();

    public readonly record struct SelectResult({selectStructProperties});

    public class SelectModel {{
        {selectModelProperties}
    }}

    private sealed class GeneratedResultSelection : ISelectedColumns<SelectResult>
    {{
        public string Sql => $""{sqlFragments}"";

        public SelectResult Mapper(DbDataReader r)
        {{
            return new SelectResult(
                {selectStructMapper}
            );
        }}
    }}

    private sealed class GeneratedModelSelection : ISelectedColumns<SelectModel>
    {{
        public string Sql => $""{sqlFragments}"";

        public SelectModel Mapper(DbDataReader r)
        {{
            return new SelectModel {{
                {selectModelMapper}
            }};
        }}
    }}
");
            var insertModelProperties = string.Join("\n    ", table.Columns!.Select(c => 
                $"public {c.Type}? {c.PropName} {{ get; set; }}"));

            var insertWriter = string.Join("\n        ", table.Columns!.Select(c => 
                $@"if ({c.PropName} != null) values[""{c.DbColumnName}""] = {c.PropName};"));

            sb.AppendLine($@"
    public class InsertModel : IInsertRecord<{table.ClassName}>
    {{
        {insertModelProperties}

        public void Writer(Dictionary<string, object?> values)
        {{
            {insertWriter}
        }}
    }}
");

            var insertStructProperties = string.Join(", ", table.Columns!.Select(c => $"{c.Type}? {c.PropName} = null"));

            sb.AppendLine($@"
    public readonly record struct InsertRecord({insertStructProperties}) : IInsertRecord<{table.ClassName}>
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
                $@"if ({c.PropName}.HasValue) values[""{c.DbColumnName}""] = {c.PropName}.Value;"));

            sb.AppendLine($@"
    public class UpdateModel : IUpdateRecord<{table.ClassName}>
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
    public readonly record struct UpdateRecord({updateStructProperties}) : IUpdateRecord<{table.ClassName}>
    {{
        public void Writer(Dictionary<string, object?> values)
        {{
            {updateWriter}
        }}
    }}
");

            sb.AppendLine($@"
        static {table.ClassName}() {{
                {string.Join("\n        ", table.Columns!.Select(c =>  $"{c.PropName} = new DbColumn<{c.Type}, {table.ClassName}>(\"{c.DbColumnName}\");"))}
        }}");

            sb.AppendLine(string.Join("\n        ", table.Columns!.Select(c => $"public static DbColumn<{c.Type}, {table.ClassName}> {c.PropName} {{ get; set; }}")));
            
            sb.AppendLine(@"
        public static class ColumnNames
        {");
            foreach (var col in table.Columns!)
            {
                sb.AppendLine($@"        public const string {col.PropName} = ""{col.PropName}"";");
            }

            sb.AppendLine(@"    }
    }");
            context.AddSource($"{table.ClassName}_Columns.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }
    }

    private class TableModel
    {
        public string Namespace { get; set; } = "";
        public string ClassName { get; set; } = "";
        
        public string? DbTableName { get; set; }
        public List<(string PropName, string DbColumnName, string Type)>? Columns { get; set; }
        
        public bool IsAlias { get; set; }
        public string? AliasName { get; set; }
        public string? SourceTableFullName { get; set; } 
    }
}
