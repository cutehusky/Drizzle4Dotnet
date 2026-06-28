using System.Runtime.CompilerServices;
using Drizzle4Dotnet.Core.Query;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.Oracle.Query;

/// <summary>
/// Oracle MERGE (upsert) query builder.
/// Oracle MERGE syntax (9i+):
///   MERGE INTO target_table t
///   USING source_table s
///   ON (t.id = s.id)
///   WHEN MATCHED THEN UPDATE SET t.name = s.name
///   WHEN NOT MATCHED THEN INSERT (id, name) VALUES (s.id, s.name)
/// </summary>
public class OracleMergeQuery<TTable> : QueryBase<OracleSqlDialectImpl>,
    IAwaitableQuery
    where TTable : ITable<OracleSqlDialectImpl>
{
    private readonly TTable _table;
    private IGenericSql? _source;
    private IGenericSql? _onCondition;
    private IGenericSql? _whenMatchedUpdate;
    private IGenericSql? _whenNotMatchedInsert;
    private readonly List<string> _returningColumns = new();

    public OracleMergeQuery(TTable table, IQueryExecutor<OracleSqlDialectImpl> executor) 
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
    public OracleMergeQuery<TTable> Using(IGenericSql source)
    {
        _source = source;
        return this;
    }

    // ======================================================================
    // ON — match condition
    // ======================================================================

    /// <summary>Sets the ON condition for matching.</summary>
    public OracleMergeQuery<TTable> On(IGenericSql condition)
    {
        _onCondition = condition;
        return this;
    }

    // ======================================================================
    // WHEN MATCHED THEN UPDATE
    // ======================================================================

    /// <summary>WHEN MATCHED THEN UPDATE SET ...</summary>
    public OracleMergeQuery<TTable> WhenMatchedThenUpdate(Dictionary<string, object?> setValues)
    {
        _whenMatchedUpdate = new OracleMergeSetClause(setValues);
        return this;
    }

    // ======================================================================
    // WHEN NOT MATCHED THEN INSERT
    // ======================================================================

    /// <summary>WHEN NOT MATCHED THEN INSERT (cols) VALUES (vals)</summary>
    public OracleMergeQuery<TTable> WhenNotMatchedThenInsert(List<string> columns, List<object?> values)
    {
        _whenNotMatchedInsert = new OracleMergeInsertClause(columns, values);
        return this;
    }

    // ======================================================================
    // RETURNING ... INTO clause
    // ======================================================================

    /// <summary>
    /// Adds RETURNING col1, col2 INTO :outP0, :outP1 to capture merged values.
    /// </summary>
    public OracleMergeQuery<TTable> ReturningInto(params string[] columnNames)
    {
        _returningColumns.AddRange(columnNames);
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("MERGE INTO ");
        _table.BuildRefSql(sqlBuilder);

        // USING clause
        sqlBuilder.Append(" USING ");
        _source?.BuildSql(sqlBuilder);

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

        // RETURNING ... INTO
        BuildReturningClause(sqlBuilder);
    }

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

    protected override void ValidateQuery()
    {
        if (_source == null)
            throw new InvalidOperationException("MERGE requires a source. Call Using().");
        if (_onCondition == null)
            throw new InvalidOperationException("MERGE requires an ON condition. Call On().");
    }
}

/// <summary>
/// Internal helper to build a SET clause from a dictionary for Oracle MERGE.
/// </summary>
internal class OracleMergeSetClause : IGenericSql
{
    private readonly Dictionary<string, object?> _setValues;

    public OracleMergeSetClause(Dictionary<string, object?> setValues)
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
            sqlBuilder.Append(OracleSqlDialectImpl.BuildIdentifier(kv.Key));
            sqlBuilder.Append(" = source.");
            sqlBuilder.Append(OracleSqlDialectImpl.BuildIdentifier(kv.Key));
        }
    }
}

/// <summary>
/// Internal helper to build an INSERT clause for Oracle MERGE.
/// </summary>
internal class OracleMergeInsertClause : IGenericSql
{
    private readonly List<string> _columns;
    private readonly List<object?> _values;

    public OracleMergeInsertClause(List<string> columns, List<object?> values)
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
            sqlBuilder.Append(OracleSqlDialectImpl.BuildIdentifier(_columns[i]));
        }
        sqlBuilder.Append(") VALUES (");
        for (int i = 0; i < _values.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append("source.");
            sqlBuilder.Append(OracleSqlDialectImpl.BuildIdentifier(_columns[i]));
        }
        sqlBuilder.Append(')');
    }
}
