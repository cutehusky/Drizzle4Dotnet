using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Sqlite;

// ======================================================================
// SQLite-specific table interface aliases
// These bind the SqliteSqlDialectImpl to the generic table interfaces
// for cleaner usage in SQLite-specific code.
// ======================================================================

/// <summary>
/// SQLite-specific generic table. Alias for IGenericTable<SqliteSqlDialectImpl>.
/// </summary>
public interface ISqliteGenericTable : IGenericTable<SqliteSqlDialectImpl> { }

/// <summary>
/// SQLite-specific table. Alias for ITable<SqliteSqlDialectImpl>.
/// </summary>
public interface ISqliteTable : ITable<SqliteSqlDialectImpl> { }

/// <summary>
/// SQLite-specific CTE table. Alias for ICteTable<SqliteSqlDialectImpl>.
/// </summary>
public interface ISqliteCteTable : ICteTable<SqliteSqlDialectImpl> { }

/// <summary>
/// SQLite-specific virtual table. Alias for IVirtualTable<SqliteSqlDialectImpl>.
/// </summary>
public interface ISqliteVirtualTable : IVirtualTable<SqliteSqlDialectImpl> { }

/// <summary>
/// SQLite-specific database table. Alias for IDbTable<SqliteSqlDialectImpl>.
/// </summary>
public interface ISqliteDbTable : IDbTable<SqliteSqlDialectImpl> { }

/// <summary>
/// SQLite-specific table alias. Alias for ITableAlias<SqliteSqlDialectImpl>.
/// </summary>
public interface ISqliteTableAlias : ITableAlias<SqliteSqlDialectImpl> { }
