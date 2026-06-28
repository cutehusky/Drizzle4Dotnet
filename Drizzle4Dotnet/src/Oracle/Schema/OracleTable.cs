using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Oracle.Schema;

// ======================================================================
// Oracle-specific table interface aliases
// These bind the OracleSqlDialectImpl to the generic table interfaces
// for cleaner usage in Oracle-specific code.
// ======================================================================

/// <summary>
/// Oracle-specific generic table. Alias for IGenericTable&lt;OracleSqlDialectImpl&gt;.
/// </summary>
public interface IOracleGenericTable : IGenericTable<OracleSqlDialectImpl> { }

/// <summary>
/// Oracle-specific table. Alias for ITable&lt;OracleSqlDialectImpl&gt;.
/// </summary>
public interface IOracleTable : ITable<OracleSqlDialectImpl> { }

/// <summary>
/// Oracle-specific CTE table. Alias for ICteTable&lt;OracleSqlDialectImpl&gt;.
/// </summary>
public interface IOracleCteTable : ICteTable<OracleSqlDialectImpl> { }

/// <summary>
/// Oracle-specific virtual table. Alias for IVirtualTable&lt;OracleSqlDialectImpl&gt;.
/// </summary>
public interface IOracleVirtualTable : IVirtualTable<OracleSqlDialectImpl> { }

/// <summary>
/// Oracle-specific database table. Alias for IDbTable&lt;OracleSqlDialectImpl&gt;.
/// </summary>
public interface IOracleDbTable : IDbTable<OracleSqlDialectImpl> { }

/// <summary>
/// Oracle-specific table alias. Alias for ITableAlias&lt;OracleSqlDialectImpl&gt;.
/// </summary>
public interface IOracleTableAlias : ITableAlias<OracleSqlDialectImpl> { }
