using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Tables;

public interface IGenericTable<TDialect> where TDialect : ISqlDialect
{
    public void BuildSql(ISqlBuilder sqlBuilder);
    
    public void BuildRefSql(ISqlBuilder sqlBuilder);
}

public interface ITable<TDialect>: IGenericTable<TDialect> where TDialect : ISqlDialect
{
    public static abstract string TableRefName { get; }
}

public interface ICteTable<TDialect>: IGenericTable<TDialect> where TDialect : ISqlDialect
{
}

public interface IVirtualTable<TDialect>: IGenericTable<TDialect> where TDialect : ISqlDialect
{
    public ICteTable<TDialect> AsCte();
    public static abstract IVirtualTable<TDialect> Create(IGenericSql baseQuery, string alias, object selectedColumns);
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