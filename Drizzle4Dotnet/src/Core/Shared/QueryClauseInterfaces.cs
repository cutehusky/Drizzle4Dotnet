using Drizzle4Dotnet.Core.Schema.Tables;

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
public interface ISupportLimit<TQuery>
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
/// Marks a query as supporting standard JOIN clauses.
/// </summary>
public interface IJoin<TQuery, TDialect> where TDialect : ISqlDialect
{
    /// <summary>INNER JOIN with ON condition.</summary>
    TQuery InnerJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>LEFT JOIN with ON condition.</summary>
    TQuery LeftJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>RIGHT JOIN with ON condition.</summary>
    TQuery RightJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>FULL JOIN with ON condition.</summary>
    TQuery FullJoin(IGenericTable<TDialect> table, IGenericSql on);

    /// <summary>CROSS JOIN (no ON condition).</summary>
    TQuery CrossJoin(IGenericTable<TDialect> table);
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
