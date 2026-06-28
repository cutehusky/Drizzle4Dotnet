using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.PgSql.Query;

/// <summary>
/// PostgreSQL MERGE (upsert) query builder (PG 15+).
/// MERGE combines INSERT, UPDATE, and DELETE into a single statement.
/// Supported actions: WHEN MATCHED THEN UPDATE, WHEN NOT MATCHED THEN INSERT,
/// WHEN NOT MATCHED BY SOURCE THEN DELETE/UPDATE.
/// PostgreSQL syntax: MERGE INTO target USING source ON condition ...
/// </summary>
public class PgMergeQuery<TTable> : QueryBase<PgSqlSqlDialectImpl>,
    IJoin<PgMergeQuery<TTable>, PgSqlSqlDialectImpl>,
    ISupportCte<PgMergeQuery<TTable>, PgSqlSqlDialectImpl>,
    IAwaitableQuery
    where TTable : ITable<PgSqlSqlDialectImpl>
{
    private readonly TTable _table;
    private IGenericSql? _source;
    private IGenericSql? _onCondition;
    private IGenericSql? _whenMatchedUpdate;
    private IGenericSql? _whenNotMatchedInsert;
    private bool _whenNotMatchedBySourceDelete;
    private IGenericSql? _whenNotMatchedBySourceUpdate;
    private readonly List<(IGenericTable<PgSqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private readonly List<string> _returningColumns = new();

    public PgMergeQuery(TTable table, IQueryExecutor<PgSqlSqlDialectImpl> executor) 
        : base(executor)
    {
        _table = table;
    }

    public TaskAwaiter GetAwaiter()
    {
        return Executor.ExecuteAsync(this).GetAwaiter();
    }

    // ======================================================================
    // USING — source table/subquery
    // ======================================================================

    /// <summary>Sets the source table/subquery for MERGE.</summary>
    public PgMergeQuery<TTable> Using(IGenericSql source)
    {
        _source = source;
        return this;
    }

    // ======================================================================
    // ON — match condition
    // ======================================================================

    /// <summary>Sets the ON condition for matching.</summary>
    public PgMergeQuery<TTable> On(IGenericSql condition)
    {
        _onCondition = condition;
        return this;
    }

    // ======================================================================
    // WHEN MATCHED THEN UPDATE
    // ======================================================================

    /// <summary>WHEN MATCHED THEN UPDATE SET ...</summary>
    public PgMergeQuery<TTable> WhenMatchedThenUpdate(Dictionary<string, object?> setValues)
    {
        _whenMatchedUpdate = new PgMergeSetClause(setValues);
        return this;
    }

    // ======================================================================
    // WHEN NOT MATCHED THEN INSERT
    // ======================================================================

    /// <summary>WHEN NOT MATCHED THEN INSERT (cols) VALUES (vals)</summary>
    public PgMergeQuery<TTable> WhenNotMatchedThenInsert(List<string> columns, List<object?> values)
    {
        _whenNotMatchedInsert = new PgMergeInsertClause(columns, values);
        return this;
    }

    // ======================================================================
    // WHEN NOT MATCHED BY SOURCE THEN DELETE
    // ======================================================================

    /// <summary>WHEN NOT MATCHED BY SOURCE THEN DELETE</summary>
    public PgMergeQuery<TTable> WhenNotMatchedBySourceThenDelete()
    {
        _whenNotMatchedBySourceDelete = true;
        return this;
    }

    // ======================================================================
    // RETURNING clause (PostgreSQL-specific)
    // ======================================================================

    /// <summary>RETURNING the specified columns.</summary>
    public PgMergeQuery<TTable> Returning(params string[] columns)
    {
        _returningColumns.AddRange(columns);
        return this;
    }

    // ======================================================================
    // CTE support (WITH clause)
    // ======================================================================

    public PgMergeQuery<TTable> With(params ICteTable<PgSqlSqlDialectImpl>[] cteTables)
    {
        CteTables.AddRange(cteTables);
        return this;
    }

    public PgMergeQuery<TTable> WithRecursive(params ICteTable<PgSqlSqlDialectImpl>[] cteTables)
    {
        CteTables.AddRange(cteTables);
        Recursive = true;
        return this;
    }

    // ======================================================================
    // JOIN support (for USING clause)
    // ======================================================================

    private PgMergeQuery<TTable> JoinInternal(
        IGenericTable<PgSqlSqlDialectImpl> table,
        IGenericSql? on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public PgMergeQuery<TTable> InnerJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");
    public PgMergeQuery<TTable> LeftJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");
    public PgMergeQuery<TTable> RightJoin(IGenericTable<PgSqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");
    public PgMergeQuery<TTable> CrossJoin(IGenericTable<PgSqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS");

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        // PostgreSQL uses MERGE INTO (unlike MSSQL which uses MERGE without INTO)
        sqlBuilder.Append("MERGE INTO ");
        _table.BuildRefSql(sqlBuilder);
        sqlBuilder.Append(" AS target");

        // USING clause
        sqlBuilder.Append(" USING ");
        _source?.BuildSql(sqlBuilder);
        sqlBuilder.Append(" AS source");

        // JOINs on source
        SqlStatics.BuildSqlJoins<PgSqlSqlDialectImpl>(sqlBuilder, _joins);

        // ON clause
        sqlBuilder.Append(" ON (");
        _onCondition?.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');

        // WHEN MATCHED THEN UPDATE
        if (_whenMatchedUpdate != null)
        {
            sqlBuilder.Append(" WHEN MATCHED THEN UPDATE SET ");
            _whenMatchedUpdate.BuildSql(sqlBuilder);
        }

        // WHEN NOT MATCHED THEN INSERT
        if (_whenNotMatchedInsert != null)
        {
            sqlBuilder.Append(" WHEN NOT MATCHED THEN INSERT ");
            _whenNotMatchedInsert.BuildSql(sqlBuilder);
        }

        // WHEN NOT MATCHED BY SOURCE THEN DELETE
        if (_whenNotMatchedBySourceDelete)
        {
            sqlBuilder.Append(" WHEN NOT MATCHED BY SOURCE THEN DELETE");
        }

        // WHEN NOT MATCHED BY SOURCE THEN UPDATE
        if (_whenNotMatchedBySourceUpdate != null)
        {
            sqlBuilder.Append(" WHEN NOT MATCHED BY SOURCE THEN UPDATE SET ");
            _whenNotMatchedBySourceUpdate.BuildSql(sqlBuilder);
        }

        // RETURNING clause (PostgreSQL-specific)
        BuildReturningClause(sqlBuilder);
    }

    private void BuildReturningClause(ISqlBuilder sqlBuilder)
    {
        if (_returningColumns.Count == 0) return;

        sqlBuilder.Append(" RETURNING ");
        for (int i = 0; i < _returningColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(_returningColumns[i]);
        }
    }

    protected override void ValidateQuery()
    {
        base.ValidateQuery();
        if (_source == null)
            throw new InvalidOperationException("MERGE requires a source. Call Using().");
        if (_onCondition == null)
            throw new InvalidOperationException("MERGE requires an ON condition. Call On().");
    }
}

/// <summary>
/// Internal helper to build a SET clause from a dictionary for PostgreSQL MERGE.
/// Renders as: target."col" = source."col"
/// </summary>
internal class PgMergeSetClause : IGenericSql
{
    private readonly Dictionary<string, object?> _setValues;

    public PgMergeSetClause(Dictionary<string, object?> setValues)
    {
        _setValues = setValues;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        bool first = true;
        foreach (var kv in _setValues)
        {
            if (!first) sqlBuilder.Append(", ");
            else first = false;

            sqlBuilder.Append("target.");
            sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(kv.Key));
            sqlBuilder.Append(" = source.");
            sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(kv.Key));
        }
    }
}

/// <summary>
/// Internal helper to build an INSERT clause for PostgreSQL MERGE.
/// Renders as: (col1, col2) VALUES (source.col1, source.col2)
/// </summary>
internal class PgMergeInsertClause : IGenericSql
{
    private readonly List<string> _columns;
    private readonly List<object?> _values;

    public PgMergeInsertClause(List<string> columns, List<object?> values)
    {
        _columns = columns;
        _values = values;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append('(');
        for (int i = 0; i < _columns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(_columns[i]));
        }
        sqlBuilder.Append(") VALUES (");
        for (int i = 0; i < _values.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append("source.");
            sqlBuilder.Append(PgSqlSqlDialectImpl.BuildIdentifier(_columns[i]));
        }
        sqlBuilder.Append(')');
    }
}
