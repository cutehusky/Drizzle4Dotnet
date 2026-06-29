using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Sqlite.Query;

/// <summary>
/// SQLite-specific SELECT query builder with virtual table support.
/// SQLite supports standard SELECT with DISTINCT, GROUP BY, HAVING, WINDOW,
/// ORDER BY, LIMIT/OFFSET, NATURAL JOIN, and compound queries.
/// SQLite does NOT support:
/// - LATERAL joins
/// - FULL OUTER JOIN (before 3.39)
/// - DISTINCT ON
/// - Lock clauses (FOR UPDATE, etc.)
/// </summary>
public class SqliteSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, SqliteSqlDialectImpl, TVirtualTable, SqliteSelectQuery<TReturn, TVirtualTable>>,
    INaturalJoin<SqliteSelectQuery<TReturn, TVirtualTable>, SqliteSqlDialectImpl>
    where TVirtualTable : IVirtualTable<SqliteSqlDialectImpl>
{
    public SqliteSelectQuery(
        ISelectedColumns<TReturn, SqliteSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<SqliteSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }

    // ====== NATURAL JOINS (supported by SQLite) ======

    public SqliteSelectQuery<TReturn, TVirtualTable> NaturalJoin(IGenericTable<SqliteSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL");

    public SqliteSelectQuery<TReturn, TVirtualTable> NaturalLeftJoin(IGenericTable<SqliteSqlDialectImpl> table)
        => JoinInternal(table, null, "NATURAL LEFT");

    // SQLite does not support:
    // - LATERAL joins
    // - FULL OUTER JOIN (before 3.39)
    // - DISTINCT ON
    // - Lock clauses (FOR UPDATE, etc.)
    // These are not overridden, so attempting to use them will result in
    // compile-time errors (interface not implemented) rather than runtime errors.
}
