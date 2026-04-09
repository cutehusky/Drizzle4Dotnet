using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Tables;

public interface IGenericTable<TDialect> where TDialect : ISqlDialect
{
    public string Sql { get; }
}

public interface ITable<TDialect>: IGenericTable<TDialect> where TDialect : ISqlDialect
{
    public static abstract string TableRefName { get; }
}

public interface IDbTable<TDialect>: ITable<TDialect> where TDialect : ISqlDialect
{
    public static abstract string TableName { get; }
    public static abstract string SchemaName { get; }
}

public interface ITableAlias<TDialect>: ITable<TDialect> where TDialect : ISqlDialect
{
    public static abstract string Alias { get; }
}