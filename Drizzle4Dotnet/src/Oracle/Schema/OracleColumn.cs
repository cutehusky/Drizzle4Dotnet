using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Oracle.Schema;

/// <summary>
/// Oracle-specific column type. Alias for DbColumn{T, TTable, OracleSqlDialectImpl}.
/// </summary>
public class OracleColumn<T, TTable> : DbColumn<T, TTable, OracleSqlDialectImpl>
    where TTable : ITable<OracleSqlDialectImpl>
{
    public OracleColumn(string columnName) : base(columnName) { }
}

/// <summary>
/// Oracle-specific virtual column for subqueries and aliases.
/// </summary>
public class OracleVirtualColumn<T> : VirtualColumn<T, OracleSqlDialectImpl>
{
    public OracleVirtualColumn(string tableRefName, string columnName) 
        : base(tableRefName, columnName) { }
}
