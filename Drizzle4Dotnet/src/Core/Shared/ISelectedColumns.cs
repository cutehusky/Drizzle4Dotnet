using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;


public interface ISelectedColumns<TReturn, TDialect>: ISql where TDialect : ISqlDialect
{
    TReturn Mapper(DbDataReader r);
}

public interface ISelectedColumns<TReturn, TDialect, TVirtualTable>: ISql where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    TReturn Mapper(DbDataReader r);
}

public interface IGetFieldByName
{
    IAliasedSql<T> Field<T>(string columnName);
    IAliasedSql<T> Field<T>(IAliasedSql<T> column);
}

public interface ITypedTupleSelectedColumns<
    TReturn, 
    TDialect, 
    TVirtualTable
>: ISelectedColumns<TReturn, TDialect, TVirtualTable>,
    IGetFieldByName
    where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
}


public interface ISelection<TReturnModel, TReturnRecord, TDialect> where TDialect : ISqlDialect 
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect> Mapping { get; }
}

public interface ISelection<TReturnModel, TReturnRecord, TDialect, TVirtualTable> where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect, TVirtualTable> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect, TVirtualTable> Mapping { get; }
}


public class TypedTupleGeneratedSubqueryTable<TReturn, TDialect>: 
    IVirtualTable<TDialect>,
    IGetFieldByName
    where TDialect : ISqlDialect
{
    protected readonly IGenericSql BaseSql;
    protected readonly string AliasName;
    protected readonly ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> SelectedColumns;
    public virtual void BuildSql(ISqlBuilder sqlBuilder) {
        sqlBuilder.Append('(');
        BaseSql.BuildSql(sqlBuilder);
        sqlBuilder.Append(") AS ");
        sqlBuilder.Append(TDialect.BuildIdentifier(AliasName));
    }

    public virtual void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        BuildSql(sqlBuilder);
    }

    public TypedTupleGeneratedCteTable<TReturn, TDialect> AsCte() {
        return new TypedTupleGeneratedCteTable<TReturn, TDialect>(AliasName, BaseSql, SelectedColumns);
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        var col = SelectedColumns.Field<T>(columnName);
        if (col == null) 
            throw new ArgumentException($"Invalid data type requested for column {columnName} in subquery {AliasName}");
        return new VirtualColumn<T,TDialect>(AliasName, col.Identifier);
    }
    
    public IAliasedSql<T> Field<T>(IAliasedSql<T> column)
    {
        var col = SelectedColumns.Field(column);
        if (col == null) 
            throw new ArgumentException($"Invalid data type requested for column {column} in subquery {AliasName}");
        return new VirtualColumn<T,TDialect>(AliasName, col.Identifier);
    }


    public TypedTupleGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns
    ) {
        BaseSql = baseSql;
        AliasName = aliasName;
        SelectedColumns = selectedColumns;
    }

    public static IVirtualTable<TDialect> Create(
        IGenericSql baseSql, 
        string aliasName,
        object selectedColumns
    ) => new TypedTupleGeneratedSubqueryTable<TReturn, TDialect>(
        aliasName, 
        baseSql, 
        ( ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>>) selectedColumns);
}


public class TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, TDialect>: 
    TypedTupleGeneratedSubqueryTable<TReturn, TDialect>
    where TDialect : ISqlDialect
{
    public TShape Shape { get; }

    public TypedTupleAnonymousGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns,
        Func<IGetFieldByName, TShape> shapeFunc
    ) : base(aliasName, baseSql, selectedColumns)
    {
        Shape = shapeFunc(this);
    }
    
    public TypedTupleAnonymousGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns,
        TShape shape
    ) : base(aliasName, baseSql, selectedColumns)
    {
        Shape = shape;
    }
    
    public new TypedTupleAnonymousGeneratedCteTable<TShape, TReturn, TDialect> AsCte() {
        return new TypedTupleAnonymousGeneratedCteTable<TShape, TReturn, TDialect>(
            AliasName,
            BaseSql,
            SelectedColumns,
            Shape
        );
    }
}


public class TypedTupleGeneratedCteTable<TReturn, TDialect>: 
    TypedTupleGeneratedSubqueryTable<TReturn, TDialect>, ICteTable<TDialect>
    where TDialect : ISqlDialect
{
    public TypedTupleGeneratedCteTable(string aliasName, IGenericSql baseSql, ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns) : base(aliasName, baseSql, selectedColumns)
    {
    }

    public override void BuildSql(ISqlBuilder sqlBuilder) {
        sqlBuilder.Append(TDialect.BuildIdentifier(AliasName));
        sqlBuilder.Append(" AS (");
        BaseSql.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }

    public override void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(TDialect.BuildIdentifier(AliasName));
    }
}


public class TypedTupleAnonymousGeneratedCteTable<TShape, TReturn, TDialect>: 
    TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, TDialect>,
    ICteTable<TDialect> where TDialect : ISqlDialect
{
    public TypedTupleAnonymousGeneratedCteTable(string aliasName, IGenericSql baseSql, ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns, TShape shape) : base(aliasName, baseSql, selectedColumns, shape)
    {
    }

    public override void BuildSql(ISqlBuilder sqlBuilder) {
        sqlBuilder.Append(TDialect.BuildIdentifier(AliasName));
        sqlBuilder.Append(" AS (");
        BaseSql.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }

    public override void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(TDialect.BuildIdentifier(AliasName));
    }
}


