using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;

public interface ISelectedColumns<TReturn, TDialect, TVirtualTable>: ISql where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    TReturn Mapper(DbDataReader r);
    IAliasedSql<T> Field<T>(string columnName);
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
    IVirtualTable<TDialect>, ICteTable<TDialect> where TDialect : ISqlDialect
{
    private readonly IGenericSql _baseSql;
    private readonly string _aliasName;
    private readonly bool _isCte = false;
    private readonly ISelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> _selectedColumns;
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
        return new TypedTupleGeneratedSubqueryTable<TReturn, TDialect>(_aliasName, _baseSql, _selectedColumns, true);
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        return _selectedColumns.Field<T>(columnName);
    }

    public TypedTupleGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        ISelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns,
        bool isCte = false
    ) {
        _baseSql = baseSql;
        _aliasName = aliasName;
        _selectedColumns = selectedColumns;
        _isCte = isCte;
    }

    public static IVirtualTable<TDialect> Create(
        IGenericSql baseSql, 
        string aliasName,
        object selectedColumns
    ) => new TypedTupleGeneratedSubqueryTable<TReturn, TDialect>(
        aliasName, 
        baseSql, 
        ( ISelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>>) selectedColumns);
}