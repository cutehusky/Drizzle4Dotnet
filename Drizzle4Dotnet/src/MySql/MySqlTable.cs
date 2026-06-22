using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

// ======================================================================
// MySQL-specific table interface aliases
// These bind the MySqlSqlDialectImpl to the generic table interfaces
// for cleaner usage in MySQL-specific code.
// ======================================================================

/// <summary>
/// MySQL-specific generic table. Alias for IGenericTable<MySqlSqlDialectImpl>.
/// </summary>
public interface IMySqlGenericTable : IGenericTable<MySqlSqlDialectImpl> { }

/// <summary>
/// MySQL-specific table. Alias for ITable<MySqlSqlDialectImpl>.
/// </summary>
public interface IMySqlTable : ITable<MySqlSqlDialectImpl> { }

/// <summary>
/// MySQL-specific CTE table. Alias for ICteTable<MySqlSqlDialectImpl>.
/// </summary>
public interface IMySqlCteTable : ICteTable<MySqlSqlDialectImpl> { }

/// <summary>
/// MySQL-specific virtual table. Alias for IVirtualTable<MySqlSqlDialectImpl>.
/// </summary>
public interface IMySqlVirtualTable : IVirtualTable<MySqlSqlDialectImpl> { }

/// <summary>
/// MySQL-specific database table. Alias for IDbTable<MySqlSqlDialectImpl>.
/// </summary>
public interface IMySqlDbTable : IDbTable<MySqlSqlDialectImpl> { }

/// <summary>
/// MySQL-specific table alias. Alias for ITableAlias<MySqlSqlDialectImpl>.
/// </summary>
public interface IMySqlTableAlias : ITableAlias<MySqlSqlDialectImpl> { }


/// <summary>
/// MySQL-specific virtual table for subqueries.
/// Alias for RawSubqueryTableSql<MySqlSqlDialectImpl>.
/// Implements IMySqlGenericTable, IMySqlCteTable, IMySqlVirtualTable.
/// </summary>
public class MySqlSubqueryTable : RawSubqueryTableSql<MySqlSqlDialectImpl>,
    IMySqlGenericTable, IMySqlCteTable, IMySqlVirtualTable
{
    public MySqlSubqueryTable(RawSql<MySqlSqlDialectImpl> sql, string alias, bool isCte = false) 
        : base(sql, alias, isCte) { }
}
