using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.Oracle.Query;

namespace Drizzle4Dotnet.Oracle;

/// <summary>
/// Convenience extension methods for OracleQueryBuilder.
/// These provide overloads for Select/SelectDistinct with typed columns.
/// Following the pattern of MssqlQueryBuilderExtensions / PgSqlQueryBuilderExtensions.
/// </summary>
public static class OracleQueryBuilderExtensions
{
    // ======================================================================
    // Select overloads (1-8 columns)
    // ======================================================================

    /// <summary>SELECT with 1 column.</summary>
    public static OracleSelectQuery<T1, TypedTupleGeneratedSubqueryTable<T1, OracleSqlDialectImpl>> 
        Select<T1>(this OracleQueryBuilder builder, IAliasedSql<T1> col1)
        => builder.Select(new TypedTupleSelectedColumns<T1, OracleSqlDialectImpl>(col1));

    /// <summary>SELECT DISTINCT with 1 column.</summary>
    public static OracleSelectQuery<T1, TypedTupleGeneratedSubqueryTable<T1, OracleSqlDialectImpl>> 
        SelectDistinct<T1>(this OracleQueryBuilder builder, IAliasedSql<T1> col1)
        => builder.SelectDistinct(new TypedTupleSelectedColumns<T1, OracleSqlDialectImpl>(col1));
}
