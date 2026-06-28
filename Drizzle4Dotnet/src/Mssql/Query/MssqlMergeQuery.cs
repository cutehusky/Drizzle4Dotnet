using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Query;

/// <summary>
/// MSSQL MERGE (upsert) query builder.
/// MERGE combines INSERT, UPDATE, and DELETE into a single statement.
/// Supported actions: WHEN MATCHED THEN UPDATE, WHEN NOT MATCHED THEN INSERT,
/// WHEN NOT MATCHED BY SOURCE THEN DELETE/UPDATE.
/// </summary>
public class MssqlMergeQuery<TTable> : QueryBase<MssqlSqlDialectImpl>,
    IJoin<MssqlMergeQuery<TTable>, MssqlSqlDialectImpl>,
    IAwaitableQuery
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private readonly TTable _table;
    private IGenericSql? _source;
    private IGenericSql? _onCondition;
    private IGenericSql? _whenMatchedUpdate;
    private IGenericSql? _whenNotMatchedInsert;
    private bool _whenNotMatchedBySourceDelete;
    private IGenericSql? _whenNotMatchedBySourceUpdate;
    private readonly List<(IGenericTable<MssqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private readonly List<string> _outputInsertedColumns = new();
    private readonly List<string> _outputDeletedColumns = new();

    public MssqlMergeQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
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
    public MssqlMergeQuery<TTable> Using(IGenericSql source)
    {
        _source = source;
        return this;
    }

    // ======================================================================
    // ON — match condition
    // ======================================================================

    /// <summary>Sets the ON condition for matching.</summary>
    public MssqlMergeQuery<TTable> On(IGenericSql condition)
    {
        _onCondition = condition;
        return this;
    }

    // ======================================================================
    // WHEN MATCHED THEN UPDATE
    // ======================================================================

    /// <summary>WHEN MATCHED THEN UPDATE SET ...</summary>
    public MssqlMergeQuery<TTable> WhenMatchedThenUpdate(Dictionary<string, object?> setValues)
    {
        _whenMatchedUpdate = new RawSqlSetClause(setValues);
        return this;
    }

    // ======================================================================
    // WHEN NOT MATCHED THEN INSERT
    // ======================================================================

    /// <summary>WHEN NOT MATCHED THEN INSERT (cols) VALUES (vals)</summary>
    public MssqlMergeQuery<TTable> WhenNotMatchedThenInsert(List<string> columns, List<object?> values)
    {
        _whenNotMatchedInsert = new RawSqlInsertClause(columns, values);
        return this;
    }

    // ======================================================================
    // WHEN NOT MATCHED BY SOURCE THEN DELETE
    // ======================================================================

    /// <summary>WHEN NOT MATCHED BY SOURCE THEN DELETE</summary>
    public MssqlMergeQuery<TTable> WhenNotMatchedBySourceThenDelete()
    {
        _whenNotMatchedBySourceDelete = true;
        return this;
    }

    // ======================================================================
    // OUTPUT clause
    // ======================================================================

    /// <summary>OUTPUT INSERTED.column for the specified columns.</summary>
    public MssqlMergeQuery<TTable> OutputInserted(params string[] columns)
    {
        _outputInsertedColumns.AddRange(columns);
        return this;
    }

    /// <summary>OUTPUT DELETED.column for the specified columns.</summary>
    public MssqlMergeQuery<TTable> OutputDeleted(params string[] columns)
    {
        _outputDeletedColumns.AddRange(columns);
        return this;
    }

    // ======================================================================
    // JOIN support (for USING clause)
    // ======================================================================

    private MssqlMergeQuery<TTable> JoinInternal(
        IGenericTable<MssqlSqlDialectImpl> table,
        IGenericSql? on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public MssqlMergeQuery<TTable> InnerJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");
    public MssqlMergeQuery<TTable> LeftJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");
    public MssqlMergeQuery<TTable> RightJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");
    public MssqlMergeQuery<TTable> CrossJoin(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS");

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("MERGE ");
        _table.BuildRefSql(sqlBuilder);
        sqlBuilder.Append(" AS target");

        // USING clause
        sqlBuilder.Append(" USING ");
        _source?.BuildSql(sqlBuilder);
        sqlBuilder.Append(" AS source");

        // JOINs on source
        SqlStatics.BuildSqlJoins<MssqlSqlDialectImpl>(sqlBuilder, _joins);

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

        // OUTPUT clause
        BuildOutputClause(sqlBuilder);
    }

    private void BuildOutputClause(ISqlBuilder sqlBuilder)
    {
        var allColumns = new List<string>();
        allColumns.AddRange(_outputInsertedColumns.Select(c => $"INSERTED.{c}"));
        allColumns.AddRange(_outputDeletedColumns.Select(c => $"DELETED.{c}"));

        if (allColumns.Count == 0) return;

        sqlBuilder.Append(" OUTPUT ");
        for (int i = 0; i < allColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(allColumns[i]);
        }
    }

    protected override void ValidateQuery()
    {
        if (_source == null)
            throw new InvalidOperationException("MERGE requires a source. Call Using().");
        if (_onCondition == null)
            throw new InvalidOperationException("MERGE requires an ON condition. Call On().");
    }
}

/// <summary>
/// Internal helper to build a SET clause from a dictionary for MERGE.
/// </summary>
internal class RawSqlSetClause : IGenericSql
{
    private readonly Dictionary<string, object?> _setValues;

    public RawSqlSetClause(Dictionary<string, object?> setValues)
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
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(kv.Key));
            sqlBuilder.Append(" = source.");
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(kv.Key));
        }
    }
}

/// <summary>
/// Internal helper to build an INSERT clause for MERGE.
/// </summary>
internal class RawSqlInsertClause : IGenericSql
{
    private readonly List<string> _columns;
    private readonly List<object?> _values;

    public RawSqlInsertClause(List<string> columns, List<object?> values)
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
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_columns[i]));
        }
        sqlBuilder.Append(") VALUES (");
        for (int i = 0; i < _values.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append("source.");
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_columns[i]));
        }
        sqlBuilder.Append(')');
    }
}
