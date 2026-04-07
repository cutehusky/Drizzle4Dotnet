namespace Drizzle4Dotnet.Core.Schema.Columns;


[AttributeUsage(AttributeTargets.Property)]
public class ColumnAttribute(string name) : Attribute { public string Name { get; } = name; }

[AttributeUsage(AttributeTargets.Property)]
public class PrimaryKeyAttribute : Attribute { }
