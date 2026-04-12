namespace Drizzle4Dotnet.Core.Schema.Tables;


[AttributeUsage(AttributeTargets.Class)]
public class TableAttribute(string name, string schema = "") : Attribute
{
}

[AttributeUsage(AttributeTargets.Class)]
public class AliasAttribute(Type table, string alias) : Attribute
{
}

