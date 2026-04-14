using System.Text.RegularExpressions;

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

}