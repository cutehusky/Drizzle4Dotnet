using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;

public interface ISelectedColumns<TReturn, TDialect, TVirtualTable>: ISql where TDialect : ISqlDialect where TVirtualTable : IVirtualTable<TDialect>
{
    TReturn Mapper(DbDataReader r);
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


public class TypedTupleGeneratedSubqueryTable<TDialect>: 
    IVirtualTable<TDialect>, ICteTable<TDialect> where TDialect : ISqlDialect
{
    private readonly IGenericSql _baseSql;
    private readonly string _aliasName;
    private readonly bool _isCte = false;
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
        return new TypedTupleGeneratedSubqueryTable<TDialect>(_aliasName, _baseSql, true);
    }

    public VirtualColumn<T, TDialect> Field<T>(string columnName)
    {
        return new VirtualColumn<T, TDialect>(_aliasName, columnName);
    }

    public TypedTupleGeneratedSubqueryTable(
        string aliasName,
        IGenericSql baseSql,
        bool isCte = false
    ) {
        _baseSql = baseSql;
        _aliasName = aliasName;
        _isCte = isCte;
    }

    public static IVirtualTable<TDialect> Create(IGenericSql baseSql, string aliasName) => new TypedTupleGeneratedSubqueryTable<TDialect>(aliasName, baseSql);
}