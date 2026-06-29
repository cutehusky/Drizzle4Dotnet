using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Query;

/// <summary>
/// MSSQL-specific DELETE query builder.
/// Extends DeleteQuery with:
/// - DELETE with FROM/JOIN (MSSQL syntax: DELETE t FROM t JOIN ... ON ... WHERE ...)
/// - TOP(n) on DELETE
/// - OUTPUT DELETED clause
/// </summary>
public class MssqlDeleteQuery<TTable> : DeleteQuery<TTable, MssqlSqlDialectImpl, MssqlDeleteQuery<TTable>>,
    IJoin<MssqlDeleteQuery<TTable>, MssqlSqlDialectImpl>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private readonly List<(IGenericTable<MssqlSqlDialectImpl>, string, IGenericSql?)> _joins = new();
    private int? _topCount;
    private readonly List<string> _outputColumns = new();

    public MssqlDeleteQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // DELETE with JOIN
    // ======================================================================

    private MssqlDeleteQuery<TTable> JoinInternal(
        IGenericTable<MssqlSqlDialectImpl> table,
        IGenericSql? on,
        string type)
    {
        _joins.Add((table, type, on));
        return this;
    }

    public MssqlDeleteQuery<TTable> InnerJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "INNER");
    public MssqlDeleteQuery<TTable> LeftJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "LEFT");
    public MssqlDeleteQuery<TTable> RightJoin(IGenericTable<MssqlSqlDialectImpl> table, IGenericSql on)
        => JoinInternal(table, on, "RIGHT");
    public MssqlDeleteQuery<TTable> CrossJoin(IGenericTable<MssqlSqlDialectImpl> table)
        => JoinInternal(table, null, "CROSS");

    // ======================================================================
    // TOP on DELETE
    // ======================================================================

    /// <summary>Limits the number of rows to delete using TOP (n).</summary>
    public MssqlDeleteQuery<TTable> Top(int count)
    {
        _topCount = count;
        return this;
    }

    // ======================================================================
    // OUTPUT clause
    // ======================================================================

    /// <summary>OUTPUT DELETED.column — returns the values of deleted rows.</summary>
    public MssqlDeleteQuery<TTable> OutputDeleted(params string[] columns)
    {
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

        if (_joins.Count > 0)
        {
            // DELETE with JOIN: DELETE [table] OUTPUT DELETED.* FROM [table] JOIN ... WHERE ...
            sqlBuilder.Append("DELETE");
            if (_topCount.HasValue)
            {
                sqlBuilder.Append(" TOP (");
                sqlBuilder.Append(sqlBuilder.AddParameter(_topCount.Value));
                sqlBuilder.Append(')');
            }
            sqlBuilder.Append(' ');
            Table.BuildRefSql(sqlBuilder);
            BuildOutputClause(sqlBuilder);
            sqlBuilder.Append(" FROM ");
            Table.BuildRefSql(sqlBuilder);
            SqlStatics.BuildSqlJoins<MssqlSqlDialectImpl>(sqlBuilder, _joins);
        }
        else
        {
            // Standard DELETE
            sqlBuilder.Append("DELETE");
            if (_topCount.HasValue)
            {
                sqlBuilder.Append(" TOP (");
                sqlBuilder.Append(sqlBuilder.AddParameter(_topCount.Value));
                sqlBuilder.Append(')');
            }
            sqlBuilder.Append(" FROM ");
            Table.BuildRefSql(sqlBuilder);
            BuildOutputClause(sqlBuilder);
        }

        SqlStatics.BuildClause(sqlBuilder, " WHERE ", " AND ", Wheres, wrapInParentheses: true);
    }

    private void BuildOutputClause(ISqlBuilder sqlBuilder)
    {
        if (_outputColumns.Count == 0) return;

        sqlBuilder.Append(" OUTPUT ");
        for (int i = 0; i < _outputColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append("DELETED.");
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_outputColumns[i]));
        }
    }
}
