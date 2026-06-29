using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Mssql.Query;

/// <summary>
/// MSSQL-specific INSERT query builder.
/// Extends InsertQuery with:
/// - OUTPUT INSERTED clause (MSSQL's equivalent of RETURNING)
/// </summary>
public class MssqlInsertQuery<TTable> : InsertQuery<TTable, MssqlSqlDialectImpl, MssqlInsertQuery<TTable>>
    where TTable : ITable<MssqlSqlDialectImpl>
{
    private readonly List<string> _outputColumns = new();

    public MssqlInsertQuery(TTable table, IQueryExecutor<MssqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    // ======================================================================
    // OUTPUT clause (MSSQL's equivalent of RETURNING)
    // ======================================================================

    /// <summary>
    /// Adds OUTPUT INSERTED.column to capture inserted values.
    /// MSSQL syntax: INSERT INTO [table] (cols) OUTPUT INSERTED.col VALUES (...)
    /// </summary>
    public MssqlInsertQuery<TTable> OutputInserted(params IColumnOfTable<TTable>[] columns)
    {
        foreach (var col in columns)
            _outputColumns.Add(col.Identifier);
        return this;
    }

    /// <summary>
    /// Adds OUTPUT INSERTED.column to capture inserted values (by column name).
    /// </summary>
    public MssqlInsertQuery<TTable> OutputInserted(params string[] columnNames)
    {
        _outputColumns.AddRange(columnNames);
        return this;
    }

    // ======================================================================
    // BuildSql
    // ======================================================================

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        ValidateQuery();

        SqlStatics.BuildSqlCte(sqlBuilder, CteTables, Recursive);

        BuildInsertKeywords(sqlBuilder);
        Table.BuildRefSql(sqlBuilder);

        if (UseDefaultValues)
        {
            // OUTPUT clause before DEFAULT VALUES
            BuildOutputClause(sqlBuilder);
            BuildDefaultValues(sqlBuilder);
        }
        else if (FromQuery != null)
        {
            sqlBuilder.Append(' ');
            // OUTPUT clause between column list and SELECT
            // Need to get columns from the FROM query
            BuildOutputClause(sqlBuilder);
            FromQuery.BuildSql(sqlBuilder);
        }
        else if (ValuesToInsert.Count > 0)
        {
            var allColumns = ValuesToInsert.SelectMany(d => d.Keys).Distinct().ToList();
            // Column list
            SqlStatics.BuildInsertColumnList<MssqlSqlDialectImpl>(sqlBuilder, allColumns);
            // OUTPUT clause goes between column list and VALUES
            BuildOutputClause(sqlBuilder);
            SqlStatics.BuildInsertRowValues(sqlBuilder, ValuesToInsert, allColumns);
        }
    }

    /// <summary>
    /// Builds the OUTPUT INSERTED clause.
    /// MSSQL syntax: OUTPUT INSERTED.[col1], INSERTED.[col2]
    /// </summary>
    private void BuildOutputClause(ISqlBuilder sqlBuilder)
    {
        if (_outputColumns.Count == 0) return;

        sqlBuilder.Append(" OUTPUT ");
        for (int i = 0; i < _outputColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append("INSERTED.");
            sqlBuilder.Append(MssqlSqlDialectImpl.BuildIdentifier(_outputColumns[i]));
        }
    }
}
