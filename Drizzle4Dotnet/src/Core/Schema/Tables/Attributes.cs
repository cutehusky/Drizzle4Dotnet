namespace Drizzle4Dotnet.Core.Schema.Tables;


[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name) : Attribute { public string Name { get; } = name; }

[AttributeUsage(AttributeTargets.Class)]
public class AliasAttribute( Type table, string alias) : Attribute
{
    public string Alias { get; } = alias;
    public Type Table { get; } = table;
}
