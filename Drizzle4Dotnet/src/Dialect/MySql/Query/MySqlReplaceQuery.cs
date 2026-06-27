using Drizzle4Dotnet.Core;
using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;
using Drizzle4Dotnet.Dialect;

namespace Drizzle4Dotnet.MySql;

/// <summary>
/// MySQL REPLACE query builder.
/// REPLACE INTO works like INSERT but replaces rows on duplicate key.
/// MySQL-specific — not supported in other dialects.
/// Generates: REPLACE INTO `table` (`col1`, `col2`) VALUES (@p0, @p1)
/// </summary>
public class MySqlReplaceQuery<TTable> : InsertQuery<TTable, MySqlSqlDialectImpl, MySqlReplaceQuery<TTable>>
    where TTable : ITable<MySqlSqlDialectImpl>
{
    public MySqlReplaceQuery(TTable table, IQueryExecutor<MySqlSqlDialectImpl> executor) 
        : base(table, executor)
    {
    }

    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (NewValues.Count == 0)
            throw new InvalidOperationException("No values provided for replace.");

        var allColumns = NewValues.SelectMany(d => d.Keys).Distinct().ToList();

        BuildSqlCte(sqlBuilder);

        sqlBuilder.Append("REPLACE INTO ");
        Table.BuildRefSql(sqlBuilder);
        sqlBuilder.Append(" (");

        for (int i = 0; i < allColumns.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(MySqlSqlDialectImpl.BuildIdentifier(allColumns[i]));
        }
        sqlBuilder.Append(") VALUES ");

        for (int rowIndex = 0; rowIndex < NewValues.Count; rowIndex++)
        {
            if (rowIndex > 0) sqlBuilder.Append(", ");

            sqlBuilder.Append('(');
            var row = NewValues[rowIndex];

            for (int colIndex = 0; colIndex < allColumns.Count; colIndex++)
            {
                if (colIndex > 0) sqlBuilder.Append(", ");

                if (row.TryGetValue(allColumns[colIndex], out var val))
                {
                    sqlBuilder.Append(sqlBuilder.AddParameter(val));
                }
                else
                {
                    sqlBuilder.Append("DEFAULT");
                }
            }
            sqlBuilder.Append(')');
        }
    }
}
