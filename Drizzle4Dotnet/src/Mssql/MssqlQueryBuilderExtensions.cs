using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Mssql.Query;

namespace Drizzle4Dotnet.Mssql;

/// <summary>
/// Convenience extension methods for MssqlQueryBuilder.
/// These provide overloads for Select/SelectDistinct with typed columns.
/// Following the pattern of PgSqlQueryBuilderExtensions / SqliteQueryBuilderExtensions.
/// </summary>
public static class MssqlQueryBuilderExtensions
{
    // ======================================================================
    // Select overloads (1-8 columns)
    // ======================================================================

    /// <summary>SELECT with 1 column.</summary>
    public static MssqlSelectQuery<T1, TypedTupleGeneratedSubqueryTable<T1, MssqlSqlDialectImpl>> 
        Select<T1>(this MssqlQueryBuilder builder, IAliasedSql<T1> col1)
        => builder.Select(new TypedTupleSelectedColumns<T1, MssqlSqlDialectImpl>(col1));

    /// <summary>SELECT DISTINCT with 1 column.</summary>
    public static MssqlSelectQuery<T1, TypedTupleGeneratedSubqueryTable<T1, MssqlSqlDialectImpl>> 
        SelectDistinct<T1>(this MssqlQueryBuilder builder, IAliasedSql<T1> col1)
        => builder.SelectDistinct(new TypedTupleSelectedColumns<T1, MssqlSqlDialectImpl>(col1));
}
