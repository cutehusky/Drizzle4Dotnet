using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Query;

/// <summary>
/// MSSQL-specific UPDATE query builder.
/// Extends UpdateQuery with:
/// - UPDATE with FROM/JOIN (MSSQL syntax: UPDATE t SET ... FROM t JOIN ... ON ...)
/// - TOP(n) on UPDATE
/// - OUTPUT INSERTED/DELETED clause
/// </summary>
public class MssqlUpdateQuery<TTable> : UpdateQuery<TTable, MssqlSqlDialectImpl, MssqlUpdateQuery<TTable>>,
    IJoin<MssqlUpdateQuery<TTable>, MssqlSqlDialectImpl>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MssqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private int? _topCount;
    private readonly List<string> _outputColumns = new();
    private bool _outputInserted; // true = OUTPUT INSERTED.*, false = OUTPUT DELETED.*

    public MssqlUpdateQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // UPDATE with JOIN (MSSQL syntax: FROM clause)
    // ======================================================================

    private MssqlUpdateQuery<TTable> JoinInternal(
        IGenericTable<MssqlSqlDialectImpl> table,
        IGenericSql? on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public MssqlUpdateQuery<TTable> InnerJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");
    public MssqlUpdateQuery<TTable> LeftJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");
    public MssqlUpdateQuery<TTable> RightJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");
    public MssqlUpdateQuery<TTable> CrossJoin(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS");

    // ======================================================================
    // TOP on UPDATE
    // ======================================================================

    /// <summary>Limits the number of rows to update using TOP (n).</summary>
    public MssqlUpdateQuery<TTable> Top(int count)
    {
        _topCount = count;
        return this;
    }

    // ======================================================================
    // OUTPUT clause
    // ======================================================================

    /// <summary>OUTPUT INSERTED.column — returns the new values after update.</summary>
    public MssqlUpdateQuery<TTable> OutputInserted(params string[] columns)
    {
        _outputInserted = true;
        _outputColumns.AddRange(columns);
        return this;
    }

    /// <summary>OUTPUT DELETED.column — returns the old values before update.</summary>
    public MssqlUpdateQuery<TTable> OutputDeleted(params string[] columns)
    {
        _outputInserted = false;
        _outputColumns.AddRange(columns);
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("UPDATE");
        if (_topCount.HasValue)
        {
            sqlBuilder.Append(" TOP (");
            sqlBuilder.Append(sqlBuilder.AddParameter(_topCount.Value));
            sqlBuilder.Append(')');
        }
        sqlBuilder.Append(' ');
        Table.BuildRefSql(sqlBuilder);

        // OUTPUT clause (after table, before SET — MSSQL-specific position)
        BuildOutputClause(sqlBuilder);

        // SET clause
        SqlStatics.BuildSqlSetClause<MssqlSqlDialectImpl>(sqlBuilder, SetValues);

        // FROM clause for JOINs
        if (_joins.Count > 0)
        {
            sqlBuilder.Append(" FROM ");
            Table.BuildRefSql(sqlBuilder);
            SqlStatics.BuildSqlJoins<MssqlSqlDialectImpl>(sqlBuilder, _joins);
        }

        // WHERE clause (includes join conditions if combined)
        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }

    private void BuildOutputClause(ISqlBuilder sqlBuilder)
    {
        if (_outputColumns.Count == 0) return;

        sqlBuilder.Append(" OUTPUT ");
        string prefix = _outputInserted ? "INSERTED" : "DELETED";
        for (int i = 0; i < _outputColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(prefix).Append('.');
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_outputColumns[i]));
        }
    }
}
