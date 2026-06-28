using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Mssql.Schema;

// ======================================================================
// MSSQL-specific table interface aliases
// These bind the MssqlSqlDialectImpl to the generic table interfaces
// for cleaner usage in MSSQL-specific code.
// ======================================================================

/// <summary>
/// MSSQL-specific generic table. Alias for IGenericTable<MssqlSqlDialectImpl>.
/// </summary>
public interface IMssqlGenericTable : IGenericTable<MssqlSqlDialectImpl> { }

/// <summary>
/// MSSQL-specific table. Alias for ITable<MssqlSqlDialectImpl>.
/// </summary>
public interface IMssqlTable : ITable<MssqlSqlDialectImpl> { }

/// <summary>
/// MSSQL-specific CTE table. Alias for ICteTable<MssqlSqlDialectImpl>.
/// </summary>
public interface IMssqlCteTable : ICteTable<MssqlSqlDialectImpl> { }

/// <summary>
/// MSSQL-specific virtual table. Alias for IVirtualTable<MssqlSqlDialectImpl>.
/// </summary>
public interface IMssqlVirtualTable : IVirtualTable<MssqlSqlDialectImpl> { }

/// <summary>
/// MSSQL-specific database table. Alias for IDbTable<MssqlSqlDialectImpl>.
/// </summary>
public interface IMssqlDbTable : IDbTable<MssqlSqlDialectImpl> { }

/// <summary>
/// MSSQL-specific table alias. Alias for ITableAlias<MssqlSqlDialectImpl>.
/// </summary>
public interface IMssqlTableAlias : ITableAlias<MssqlSqlDialectImpl> { }
