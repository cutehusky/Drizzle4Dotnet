using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;
using Drizzle4Dotnet.Oracle.Nodes;

namespace Drizzle4Dotnet.Oracle.Query;

/// <summary>
/// Oracle-specific INSERT query builder.
/// Extends InsertQuery with:
/// - RETURNING ... INTO clause (Oracle's mechanism for returning values from DML)
/// - INSERT ALL (Oracle's multi-table insert)
/// - Throws NotSupportedException for DEFAULT VALUES (not supported by Oracle)
/// </summary>
public class OracleInsertQuery<TTable> : InsertQuery<TTable, OracleSqlDialectImpl, OracleInsertQuery<TTable>>
    where TTable : ITable<OracleSqlDialectImpl>
{
    private readonly List<string> _returningColumns = new();
    private bool _isInsertAll;
    private readonly List<InsertAllTarget> _insertAllTargets = new();

    public OracleInsertQuery(TTable table, IQueryExecutor<OracleSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // RETURNING ... INTO clause
    // ======================================================================

    /// <summary>
    /// Adds RETURNING col1, col2 INTO :outP0, :outP1 to capture inserted values.
    /// Oracle syntax: INSERT INTO table (cols) VALUES (vals) RETURNING col1 INTO :outP0
    /// </summary>
    public OracleInsertQuery<TTable> ReturningInto(params string[] columnNames)
    {
        _returningColumns.AddRange(columnNames);
        return this;
    }

    // ======================================================================
    // INSERT ALL (multi-table insert)
    // ======================================================================

    /// <summary>
    /// Begins an INSERT ALL statement.
    /// Oracle syntax:
    ///   INSERT ALL
    ///     INTO table1 (cols) VALUES (vals)
    ///     INTO table2 (cols) VALUES (vals)
    ///   SELECT * FROM DUAL
    /// </summary>
    public OracleInsertQuery<TTable> InsertAll()
    {
        _isInsertAll = true;
        return this;
    }

    /// <summary>
    /// Adds a target table for INSERT ALL.
    /// </summary>
    public OracleInsertQuery<TTable> Into(TTable targetTable, Dictionary<string, object?> values)
    {
        _insertAllTargets.Add(new InsertAllTarget(targetTable, values, null));
        return this;
    }

    /// <summary>
    /// Adds a conditional target for INSERT ALL with WHEN clause.
    /// </summary>
    public OracleInsertQuery<TTable> IntoWhen(TTable targetTable, IGenericSql condition, Dictionary<string, object?> values)
    {
        _insertAllTargets.Add(new InsertAllTarget(targetTable, values, condition));
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        if (_isInsertAll)
        {
            BuildInsertAll(sqlBuilder);
            return;
        }

        BuildInsertKeywords(sqlBuilder);
        Table.BuildRefSql(sqlBuilder);

        if (UseDefaultValues)
        {
            throw new NotSupportedException(
                "Oracle does not support INSERT ... DEFAULT VALUES. " +
                "Use explicit column values with DEFAULT keyword instead.");
        }
        else if (FromQuery != null)
        {
            sqlBuilder.Append(' ');
            FromQuery.BuildSql(sqlBuilder);
            BuildReturningClause(sqlBuilder);
        }
        else if (ValuesToInsert.Count > 0)
        {
            var allColumns = ValuesToInsert.SelectMany(d => d.Keys).Distinct().ToList();
            SqlStatics.BuildInsertColumnList<OracleSqlDialectImpl>(sqlBuilder, allColumns);
            BuildReturningClause(sqlBuilder);
            SqlStatics.BuildInsertRowValues(sqlBuilder, ValuesToInsert, allColumns);
        }
    }

    /// <summary>
    /// Builds the RETURNING ... INTO clause.
    /// Oracle syntax: RETURNING col1, col2 INTO :outP0, :outP1
    /// </summary>
    private void BuildReturningClause(ISqlBuilder sqlBuilder)
    {
        if (_returningColumns.Count == 0) return;

        sqlBuilder.Append(" RETURNING ");
        for (int i = 0; i < _returningColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(OracleSqlDialectImpl.BuildIdentifier(_returningColumns[i]));
        }

        sqlBuilder.Append(" INTO ");
        for (int i = 0; i < _returningColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append($":outP{i}");
        }
    }

    /// <summary>
    /// Builds INSERT ALL statement.
    /// Oracle syntax:
    ///   INSERT ALL
    ///     [WHEN condition THEN]
    ///     INTO table (cols) VALUES (vals)
    ///   SELECT * FROM DUAL
    /// </summary>
    private void BuildInsertAll(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("INSERT ALL");

        foreach (var target in _insertAllTargets)
        {
            if (target.Condition != null)
            {
                sqlBuilder.Append(" WHEN ");
                target.Condition.BuildSql(sqlBuilder);
                sqlBuilder.Append(" THEN");
            }

            sqlBuilder.Append(" INTO ");
            target.Table.BuildRefSql(sqlBuilder);

            var columns = target.Values.Keys.ToList();
            if (columns.Count > 0)
            {
                SqlStatics.BuildInsertColumnList<OracleSqlDialectImpl>(sqlBuilder, columns);
                sqlBuilder.Append(" VALUES (");
                for (int i = 0; i < columns.Count; i++)
                {
                    if (i > 0) sqlBuilder.Append(", ");
                    var val = target.Values[columns[i]];
                    if (val is IGenericSql sqlVal)
                    {
                        sqlVal.BuildSql(sqlBuilder);
                    }
                    else
                    {
                        sqlBuilder.Append(sqlBuilder.AddParameter(val));
                    }
                }
                sqlBuilder.Append(')');
            }
        }

        // INSERT ALL requires a SELECT to source the rows
        sqlBuilder.Append(" SELECT * FROM DUAL");
    }

    protected override void BuildDefaultValues(ISqlBuilder sqlBuilder)
    {
        throw new NotSupportedException(
            "Oracle does not support INSERT ... DEFAULT VALUES. " +
            "Use explicit column values with DEFAULT keyword instead.");
    }
}

/// <summary>
/// Internal helper for INSERT ALL target tables.
/// </summary>
internal class InsertAllTarget
{
    public ITable<OracleSqlDialectImpl> Table { get; }
    public Dictionary<string, object?> Values { get; }
    public IGenericSql? Condition { get; }

    public InsertAllTarget(ITable<OracleSqlDialectImpl> table, Dictionary<string, object?> values, IGenericSql? condition)
    {
        Table = table;
        Values = values;
        Condition = condition;
    }
}
