using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;

public interface ISelectedColumns<TReturn, TDialect, TVirtualTable>: ISql where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    TReturn Mapper(DbDataReader r);
}

public interface IGetFieldByName
{
    IAliasedSql<T> Field<T>(string columnName);
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


public interface ISelection<TReturnModel, TReturnRecord, TDialect, TVirtualTable> where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect, TVirtualTable> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect, TVirtualTable> Mapping { get; }
}


public interface ISelectedColumns<TReturn, TDialect>: ISql where TDialect : ISqlDialect
{
    TReturn Mapper(DbDataReader r);
}


public interface ISelection<TReturnModel, TReturnRecord, TDialect> where TDialect : ISqlDialect 
{
    public static abstract ISelectedColumns<TReturnRecord, TDialect> Record { get; }
    public static abstract ISelectedColumns<TReturnModel, TDialect> Mapping { get; }
}


public class TypedTupleGeneratedSubqueryTable<TReturn, TDialect>: 
    IVirtualTable<TDialect>, ICteTable<TDialect>,
    IGetFieldByName
    where TDialect : ISqlDialect
{
    private readonly IGenericSql _baseSql;
    private readonly string _aliasName;
    private readonly bool _isCte = false;
    protected readonly ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> SelectedColumns;
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

    public ICteTable<TDialect> AsCte() {
        return new TypedTupleGeneratedSubqueryTable<TReturn, TDialect>(_aliasName, _baseSql, SelectedColumns, true);
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        return SelectedColumns.Field<T>(columnName);
    }

    public TypedTupleGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns,
        bool isCte = false
    ) {
        _baseSql = baseSql;
        _aliasName = aliasName;
        SelectedColumns = selectedColumns;
        _isCte = isCte;
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
    private readonly Func<IGetFieldByName, TShape> _shapeFunc;

    public TShape Shape => _shapeFunc(this);
    
    public TypedTupleAnonymousGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns,
        Func<IGetFieldByName, TShape> shapeFunc,
        bool isCte = false
    ) : base(aliasName, baseSql, selectedColumns, isCte)
    {
        _shapeFunc = shapeFunc;
    }
}

