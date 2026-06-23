using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query;

/// <summary>
/// Extension methods providing typed .Returning() support on DML queries (INSERT/UPDATE/DELETE),
/// matching the pattern used by .Select() with TypedTupleSelectedColumns.
/// </summary>
public static class ReturningExtensions
{
    // ====== 1 column ======

    public static ReturningQuery<T1, TDialect, TypedTupleGeneratedSubqueryTable<T1, TDialect>> Returning<T1, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<T1, TDialect, TypedTupleGeneratedSubqueryTable<T1, TDialect>>(
            query, new TypedTupleSelectedColumns<T1, TDialect>(col1));
    }

    // ====== 2 columns ======

    public static ReturningQuery<(T1, T2), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2), TDialect>> Returning<T1, T2, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, TDialect>(col1, col2));
    }

    // ====== 3 columns ======

    public static ReturningQuery<(T1, T2, T3), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3), TDialect>> Returning<T1, T2, T3, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, TDialect>(col1, col2, col3));
    }

    // ====== 4 columns ======

    public static ReturningQuery<(T1, T2, T3, T4), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4), TDialect>> Returning<T1, T2, T3, T4, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, TDialect>(col1, col2, col3, col4));
    }

    // ====== 5 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5), TDialect>> Returning<T1, T2, T3, T4, T5, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, TDialect>(col1, col2, col3, col4, col5));
    }

    // ====== 6 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6), TDialect>> Returning<T1, T2, T3, T4, T5, T6, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, TDialect>(col1, col2, col3, col4, col5, col6));
    }

    // ====== 7 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, TDialect>(col1, col2, col3, col4, col5, col6, col7));
    }

    // ====== 8 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8));
    }

    // ====== 9 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9));
    }

    // ====== 10 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9, IAliasedSql<T10> col10)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9, col10));
    }

    // ====== 11 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9, IAliasedSql<T10> col10, IAliasedSql<T11> col11)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11));
    }

    // ====== 12 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9, IAliasedSql<T10> col10, IAliasedSql<T11> col11, IAliasedSql<T12> col12)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12));
    }

    // ====== 13 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9, IAliasedSql<T10> col10, IAliasedSql<T11> col11, IAliasedSql<T12> col12, IAliasedSql<T13> col13)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13));
    }

    // ====== 14 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9, IAliasedSql<T10> col10, IAliasedSql<T11> col11, IAliasedSql<T12> col12, IAliasedSql<T13> col13, IAliasedSql<T14> col14)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14));
    }

    // ====== 15 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9, IAliasedSql<T10> col10, IAliasedSql<T11> col11, IAliasedSql<T12> col12, IAliasedSql<T13> col13, IAliasedSql<T14> col14, IAliasedSql<T15> col15)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15));
    }

    // ====== 16 columns ======

    public static ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16), TDialect>> Returning<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDialect>(
        this Query<TDialect> query, IAliasedSql<T1> col1, IAliasedSql<T2> col2, IAliasedSql<T3> col3, IAliasedSql<T4> col4, IAliasedSql<T5> col5, IAliasedSql<T6> col6, IAliasedSql<T7> col7, IAliasedSql<T8> col8, IAliasedSql<T9> col9, IAliasedSql<T10> col10, IAliasedSql<T11> col11, IAliasedSql<T12> col12, IAliasedSql<T13> col13, IAliasedSql<T14> col14, IAliasedSql<T15> col15, IAliasedSql<T16> col16)
        where TDialect : ISqlDialect
    {
        return new ReturningQuery<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16), TDialect, TypedTupleGeneratedSubqueryTable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16), TDialect>>(
            query, new TypedTupleSelectedColumns<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TDialect>(col1, col2, col3, col4, col5, col6, col7, col8, col9, col10, col11, col12, col13, col14, col15, col16));
    }
}
