using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.PgSql.Schema;

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