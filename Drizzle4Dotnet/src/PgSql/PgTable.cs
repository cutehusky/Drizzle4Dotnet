using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql;

// ======================================================================
// PostgreSQL-specific table interface aliases
// These bind the PgSqlSqlDialectImpl to the generic table interfaces
// for cleaner usage in PostgreSQL-specific code.
// ======================================================================

/// <summary>
/// PostgreSQL-specific generic table. Alias for IGenericTable&lt;PgSqlSqlDialectImpl&gt;.
/// </summary>
public interface IPgGenericTable : IGenericTable<PgSqlSqlDialectImpl> { }

/// <summary>
/// PostgreSQL-specific table. Alias for ITable&lt;PgSqlSqlDialectImpl&gt;.
/// </summary>
public interface IPgTable : ITable<PgSqlSqlDialectImpl> { }

/// <summary>
/// PostgreSQL-specific CTE table. Alias for ICteTable&lt;PgSqlSqlDialectImpl&gt;.
/// </summary>
public interface IPgCteTable : ICteTable<PgSqlSqlDialectImpl> { }

/// <summary>
/// PostgreSQL-specific virtual table. Alias for IVirtualTable&lt;PgSqlSqlDialectImpl&gt;.
/// </summary>
public interface IPgVirtualTable : IVirtualTable<PgSqlSqlDialectImpl> { }

/// <summary>
/// PostgreSQL-specific database table. Alias for IDbTable&lt;PgSqlSqlDialectImpl&gt;.
/// </summary>
public interface IPgDbTable : IDbTable<PgSqlSqlDialectImpl> { }

/// <summary>
/// PostgreSQL-specific table alias. Alias for ITableAlias&lt;PgSqlSqlDialectImpl&gt;.
/// </summary>
public interface IPgTableAlias : ITableAlias<PgSqlSqlDialectImpl> { }


/// <summary>
/// PostgreSQL-specific virtual table for subqueries.
/// Alias for RawSubqueryTableSql&lt;PgSqlSqlDialectImpl&gt;.
/// Implements IPgGenericTable, IPgCteTable, IPgVirtualTable.
/// </summary>
public class PgSubqueryTable : RawSubqueryTableSql<PgSqlSqlDialectImpl>,
    IPgGenericTable, IPgCteTable, IPgVirtualTable
{
    public PgSubqueryTable(RawSql<PgSqlSqlDialectImpl> sql, string alias, bool isCte = false) 
        : base(sql, alias, isCte) { }
}