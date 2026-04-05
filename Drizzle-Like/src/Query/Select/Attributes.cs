namespace Drizzle_Like.Query.Select;

[AttributeUsage(AttributeTargets.Class)] public class DbSelectAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property)]
public class MapWithAttribute(
    Type table,
    string col
) : Attribute
{
    public Type Table { get; } = table;
    public string Col { get; } = col;
}

[AttributeUsage(AttributeTargets.Property)]
public class MapWithAliasAttribute(
    Type table,
    string alias,
    string col
) : Attribute
{
    public Type Table { get; } = table;
    public string Alias { get; } = alias;
    public string Col { get; } = col;
}

[AttributeUsage(AttributeTargets.Property)]
public class MapWithRaw(
    string raw,
    string alias
) : Attribute
{
    public string Raw { get; } = raw;
}