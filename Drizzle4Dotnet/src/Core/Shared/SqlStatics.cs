using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;

/// <summary>
/// Pure static utility methods for building SQL fragments.
/// These methods have no side effects beyond writing to the ISqlBuilder.
/// </summary>
public static class SqlStatics
{
    /// <summary>
    /// Appends a clause with a header, separator between items, and optional parenthesis wrapping.
    /// Example: AppendClause(builder, " WHERE ", " AND ", conditions, wrapInParentheses: true)
    /// Output: WHERE (cond1) AND (cond2)
    /// </summary>
    public static void BuildClause(
        ISqlBuilder sqlBuilder,
        string header,
        string separator,
        IReadOnlyList<IGenericSql> items,
        bool wrapInParentheses = false)
    {
        if (items.Count == 0) return;

        sqlBuilder.Append(header);
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(separator);
            if (wrapInParentheses) sqlBuilder.Append('(');
            items[i].BuildSql(sqlBuilder);
            if (wrapInParentheses) sqlBuilder.Append(')');
        }
    }

    /// <summary>
    /// Builds a SET clause for UPDATE queries from a dictionary of column → value pairs.
    /// Values can be raw scalars (added as parameters) or IGenericSql expressions (rendered inline).
    /// </summary>
    public static void BuildSqlSet<TDialect>(
        ISqlBuilder sqlBuilder,
        Dictionary<string, object?> setValues)
        where TDialect : ISqlDialect
    {
        if (setValues.Count == 0) return;

        sqlBuilder.Append(" SET ");
        BuildSetValues<TDialect>(sqlBuilder, setValues);
    }

    /// <summary>
    /// Builds SET column = value pairs from a dictionary of column → value/expression.
    /// Shared by both PgSql (ON CONFLICT DO UPDATE SET) and MySql (ON DUPLICATE KEY UPDATE).
    /// Values can be raw scalars (added as parameters) or IGenericSql expressions (rendered inline).
    /// No prefix or separator header is added — the caller prepends the appropriate clause keyword.
    /// </summary>
    public static void BuildSetValues<TDialect>(
        ISqlBuilder sqlBuilder,
        Dictionary<string, object?> updates)
        where TDialect : ISqlDialect
    {
        if (updates.Count == 0) return;

        bool first = true;
        foreach (var kv in updates)
        {
            if (!first) sqlBuilder.Append(", ");
            else first = false;

            sqlBuilder.Append(TDialect.BuildIdentifier(kv.Key));
            sqlBuilder.Append(" = ");
            if (kv.Value is IGenericSql op)
            {
                sqlBuilder.Append('(');
                op.BuildSql(sqlBuilder);
                sqlBuilder.Append(')');
            }
            else
            {
                sqlBuilder.Append(sqlBuilder.AddParameter(kv.Value));
            }
        }
    }

    /// <summary>
    /// Builds a column list: (col1, col2, col3)
    /// Uses the dialect's identifier builder for proper quoting.
    /// </summary>
    public static void BuildInsertColumnList<TDialect>(
        ISqlBuilder sqlBuilder,
        List<string> allColumns)
        where TDialect : ISqlDialect
    {
        if (allColumns.Count == 0) return;

        sqlBuilder.Append(" (");
        for (int i = 0; i < allColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(TDialect.BuildIdentifier(allColumns[i]));
        }
        sqlBuilder.Append(')');
    }

    /// <summary>
    /// Builds the VALUES clause for INSERT:
    /// VALUES (p0, p1, ...), (p2, p3, ...)
    /// Each row in newValues corresponds to a value tuple; allColumns defines the column order.
    /// Missing columns in a row are rendered as defaultValue ("NULL" by default, "DEFAULT" for REPLACE).
    /// </summary>
    public static void BuildInsertRowValues(
        ISqlBuilder sqlBuilder,
        List<Dictionary<string, object?>> newValues,
        List<string> allColumns,
        string defaultValue = "NULL")
    {
        if (newValues.Count == 0) return;

        sqlBuilder.Append(" VALUES ");
        for (int rowIndex = 0; rowIndex < newValues.Count; rowIndex++)
        {
            if (rowIndex > 0) sqlBuilder.Append(", ");

            sqlBuilder.Append('(');
            var row = newValues[rowIndex];

            for (int colIndex = 0; colIndex < allColumns.Count; colIndex++)
            {
                if (colIndex > 0) sqlBuilder.Append(", ");

                if (row.TryGetValue(allColumns[colIndex], out var val))
                {
                    if (val is IGenericSql op)
                    {
                        sqlBuilder.Append('(');
                        op.BuildSql(sqlBuilder);
                        sqlBuilder.Append(')');
                    }
                    else
                    {
                        sqlBuilder.Append(sqlBuilder.AddParameter(val));
                    }
                }
                else
                {
                    sqlBuilder.Append(defaultValue);
                }
            }
            sqlBuilder.Append(')');
        }
    }

    /// <summary>
    /// Builds JOIN clauses from a list of (table, joinType, onCondition) tuples.
    /// Example: INNER JOIN other ON (t1.id = other.t1_id)
    /// </summary>
    public static void BuildSqlJoins<TDialect>(
        ISqlBuilder sqlBuilder,
        IReadOnlyList<(IGenericTable<TDialect> table, string type, IGenericSql? on)> joins)
        where TDialect : ISqlDialect
    {
        if (joins.Count == 0) return;

        foreach (var (table, type, on) in joins)
        {
            sqlBuilder.Append(' ').Append(type).Append(" JOIN ");
            table.BuildRefSql(sqlBuilder);
            if (on != null)
            {
                sqlBuilder.Append(" ON (");
                on.BuildSql(sqlBuilder);
                sqlBuilder.Append(')');
            }
        }
    }

    /// <summary>
    /// Builds the WITH / WITH RECURSIVE clause if CTEs are registered.
    /// Call this at the very beginning of BuildSql() in any query subclass
    /// to render the CTE prefix before the main statement.
    /// </summary>
    public static void BuildSqlCte<TDialect>(
        ISqlBuilder sqlBuilder,
        IReadOnlyList<ICteTable<TDialect>> cteTables,
        bool recursive = false)
        where TDialect : ISqlDialect
    {
        if (cteTables.Count == 0) return;

        sqlBuilder.Append("WITH");
        if (recursive) sqlBuilder.Append(" RECURSIVE");
        sqlBuilder.Append(' ');
        for (int i = 0; i < cteTables.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            cteTables[i].BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(' ');
    }

    /// <summary>
    /// Builds an ORDER BY clause from a list of (expression, ascending) pairs.
    /// Example: ORDER BY col1 ASC, col2 DESC
    /// </summary>
    public static void BuildSqlOrderBy(
        ISqlBuilder sqlBuilder,
        IReadOnlyList<(IGenericSql expr, bool asc)> orderBys)
    {
        if (orderBys.Count == 0) return;

        sqlBuilder.Append(" ORDER BY ");
        for (int i = 0; i < orderBys.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            var (expr, isAsc) = orderBys[i];
            expr.BuildSql(sqlBuilder);
            sqlBuilder.Append(isAsc ? " ASC" : " DESC");
        }
    }
}
