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
    IVirtualTable<TDialect>, ICteTable<TDialect>,
    IGetFieldByName
    where TDialect : ISqlDialect
{
    private readonly IGenericSql _baseSql;
    private readonly string _aliasName;
    private readonly bool _isCte = false;
    private readonly ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> _selectedColumns;
    public void BuildSql(ISqlBuilder sqlBuilder) {
        if (_isCte) {
            sqlBuilder.Append(TDialect.BuildIdentifier(_aliasName));
            sqlBuilder.Append(" AS (");
            _baseSql.BuildSql(sqlBuilder);
            sqlBuilder.Append(')');
            return;
        }

        sqlBuilder.Append('(');
        _baseSql.BuildSql(sqlBuilder);
        sqlBuilder.Append(") AS ");
        sqlBuilder.Append(TDialect.BuildIdentifier(_aliasName));
    }

    public void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        if (_isCte) {
            sqlBuilder.Append(TDialect.BuildIdentifier(_aliasName));
            return;
        }
        BuildSql(sqlBuilder);
    }

    public TypedTupleGeneratedSubqueryTable<TReturn, TDialect> AsCte() {
        return new TypedTupleGeneratedSubqueryTable<TReturn, TDialect>(_aliasName, _baseSql, _selectedColumns, true);
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        var col = _selectedColumns.Field<T>(columnName);
        if (col == null) 
            throw new ArgumentException($"Invalid data type requested for column {columnName} in subquery {_aliasName}");
        return new VirtualColumn<T,TDialect>(_aliasName, col.Identifier);
    }
    
    public IAliasedSql<T> Field<T>(IAliasedSql<T> column)
    {
        var col = _selectedColumns.Field(column);
        if (col == null) 
            throw new ArgumentException($"Invalid data type requested for column {column} in subquery {_aliasName}");
        return new VirtualColumn<T,TDialect>(_aliasName, col.Identifier);
    }


    public TypedTupleGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns,
        bool isCte = false
    ) {
        _baseSql = baseSql;
        _aliasName = aliasName;
        _selectedColumns = selectedColumns;
        _isCte = isCte;
    }
    
    public TypedTupleGeneratedSubqueryTable(
        TypedTupleGeneratedSubqueryTable<TReturn, TDialect> baseTable,
        bool isCte = false
    ) : this(baseTable._aliasName, baseTable._baseSql, baseTable._selectedColumns, isCte)
    {
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
        Func<IGetFieldByName, TShape> shapeFunc,
        bool isCte = false
    ) : base(aliasName, baseSql, selectedColumns, isCte)
    {
        Shape = shapeFunc(this);
    }
    
    public TypedTupleAnonymousGeneratedSubqueryTable(
        TypedTupleGeneratedSubqueryTable<TReturn, TDialect> baseTable,
        TShape shape,
        bool isCte = false
    ) : base(baseTable, isCte)
    {
        Shape = shape;
    }
    
    public new TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, TDialect> AsCte() {
        return new TypedTupleAnonymousGeneratedSubqueryTable<TShape, TReturn, TDialect>(
            new TypedTupleGeneratedSubqueryTable<TReturn, TDialect>(this, true),
            Shape,
            true
        );
    }
}

