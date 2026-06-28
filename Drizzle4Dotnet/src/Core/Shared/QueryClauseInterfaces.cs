using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Query.Update;

namespace Drizzle4Dotnet.Core.Shared;

/// <summary>
/// Marks a query as supporting WHERE clause conditions.
/// Provides a consistent API for filtering across all query types.
/// </summary>
public interface ISupportWhere<TQuery>
{
    /// <summary>
    /// Adds one or more WHERE conditions (combined with AND).
    /// </summary>
    TQuery Where(params IGenericSql[] conditions);
    
    /// <summary>
    /// Adds a single WHERE condition.
    /// </summary>
    TQuery Where(IGenericSql condition);
}

/// <summary>
/// Marks a query as supporting ORDER BY clause.
/// </summary>
public interface ISupportOrderBy<TQuery>
{
    /// <summary>
    /// Adds an ORDER BY column with optional direction.
    /// </summary>
    TQuery OrderBy(IGenericSql col, bool asc = true);
}

/// <summary>
/// Marks a query as supporting LIMIT clause.
/// </summary>
public interface ISupportOffsetLimit<TQuery>
{
    /// <summary>
    /// Limits the number of rows returned.
    /// </summary>
    TQuery Limit(int limit);
    
    /// <summary>
    /// Skips a number of rows before returning results.
    /// </summary>
    TQuery Offset(int offset);
}

/// <summary>
/// Marks a query as supporting CTE (WITH clause).
/// </summary>
public interface ISupportCte<TQuery, TDialect> where TDialect : ISqlDialect
{
    /// <summary>
    /// Attaches a CTE table to this query.
    /// </summary>
    TQuery With(ICteTable<TDialect> cteTable);
}

/// <summary>
/// Marks a query as supporting standard JOIN clauses universally supported by all major databases
/// (INNER, LEFT, RIGHT, CROSS).
/// Supported by: PostgreSQL, MySQL, SQLite, SQL Server, Oracle.
/// FULL OUTER JOIN is not included here — see <see cref="IFullOuterJoin{TQuery, TDialect}"/>.
/// </summary>
public interface IJoin<TQuery, TDialect> where TDialect : ISqlDialect
{
    /// <summary>INNER JOIN with ON condition.</summary>
    TQuery InnerJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>LEFT JOIN with ON condition.</summary>
    TQuery LeftJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>RIGHT JOIN with ON condition.</summary>
    TQuery RightJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>CROSS JOIN (no ON condition).</summary>
    TQuery CrossJoin(IGenericTable<TDialect> table);
}

/// <summary>
/// Marks a query as supporting FULL OUTER JOIN.
/// Not supported by MySQL; supported by PostgreSQL, SQLite (3.39+), SQL Server, Oracle.
/// </summary>
public interface IFullOuterJoin<TQuery, TDialect> where TDialect : ISqlDialect
{
    /// <summary>FULL OUTER JOIN with ON condition.</summary>
    TQuery FullJoin(IGenericTable<TDialect> table, IGenericSql on);
}

/// <summary>
/// Marks a query as supporting NATURAL JOIN clauses.
/// Natural joins automatically join on columns with the same name — no ON condition needed.
/// Supported by: PostgreSQL, MySQL, SQLite, Oracle.
/// Not supported by: SQL Server.
/// NATURAL RIGHT JOIN is PostgreSQL-only and is provided as a direct method
/// on PgSelectQuery, not through this interface.
/// </summary>
public interface INaturalJoin<TQuery, TDialect> where TDialect : ISqlDialect
{
    /// <summary>NATURAL JOIN (automatically joins on matching column names, no ON condition).</summary>
    TQuery NaturalJoin(IGenericTable<TDialect> table);

    /// <summary>NATURAL LEFT JOIN (automatically joins on matching column names, no ON condition).</summary>
    TQuery NaturalLeftJoin(IGenericTable<TDialect> table);
}

/// <summary>
/// Marks a query as supporting PostgreSQL-style DISTINCT ON (column-level distinct).
/// Only applicable to PostgreSQL; renders as DISTINCT ON (col1, col2, ...).
/// </summary>
public interface ISupportDistinctOn<TQuery>
{
    /// <summary>
    /// Adds DISTINCT ON (columns) to the SELECT clause.
    /// Only the first column(s) specified determine uniqueness; ORDER BY is typically needed.
    /// </summary>
    TQuery DistinctOn(params IGenericSql[] columns);
}

/// <summary>
/// Marks a query as supporting DISTINCT (deduplication of result rows).
/// </summary>
public interface ISupportDistinct<TQuery>
{
    /// <summary>
    /// Adds DISTINCT to the SELECT clause to eliminate duplicate rows.
    /// </summary>
    TQuery Distinct();
}

/// <summary>
/// Marks a query as supporting PostgreSQL-style LATERAL joins.
/// </summary>
public interface ILateralJoin<TQuery, TDialect> where TDialect : ISqlDialect
{
    /// <summary>INNER LATERAL JOIN with ON condition.</summary>
    TQuery InnerLateralJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>LEFT LATERAL JOIN with ON condition.</summary>
    TQuery LeftLateralJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>CROSS LATERAL JOIN (no ON condition).</summary>
    TQuery CrossLateralJoin(IGenericTable<TDialect> table);
}

/// <summary>
/// Marks a query as supporting setting values for INSERT operations.
/// Provides methods for specifying rows/values to insert.
/// </summary>
public interface ISupportInsertValue<TQuery, TTable, TDialect>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    /// <summary>
    /// Adds a single insert row from a typed record.
    /// </summary>
    TQuery Value(IInsertRecord<TTable, TDialect> record);

    /// <summary>
    /// Adds multiple insert rows from typed records.
    /// </summary>
    TQuery Values(params IInsertRecord<TTable, TDialect>[] records);

    /// <summary>
    /// Adds a single insert row from a column-value dictionary.
    /// </summary>
    TQuery Value(Dictionary<IColumnOfTable<TTable>, object?> columnValuePairs);

    /// <summary>
    /// Adds multiple insert rows from column-value dictionaries.
    /// </summary>
    TQuery Values(params Dictionary<IColumnOfTable<TTable>, object?>[] columnValuePairsArray);
}

/// <summary>
/// Marks a query as supporting SET clauses for UPDATE operations.
/// Provides methods for specifying column-value pairs to update.
/// </summary>
public interface ISupportUpdateSet<TQuery, TTable, TDialect>
    where TTable : ITable<TDialect>
    where TDialect : ISqlDialect
{
    /// <summary>
    /// Sets a column to a scalar value.
    /// </summary>
    TQuery Set<T>(DbColumn<T, TTable, TDialect> column, T value);

    /// <summary>
    /// Sets multiple columns from a typed record.
    /// </summary>
    TQuery Set(IUpdateRecord<TTable, TDialect> record);

    /// <summary>
    /// Sets a column to a dynamic SQL expression value.
    /// </summary>
    TQuery Set<T>(DbColumn<T, TTable, TDialect> column, ISql<T> value);

    /// <summary>
    /// Sets multiple columns from a column-value dictionary.
    /// </summary>
    TQuery Set(Dictionary<IColumnOfTable<TTable>, object> columnValuePairs);
}
