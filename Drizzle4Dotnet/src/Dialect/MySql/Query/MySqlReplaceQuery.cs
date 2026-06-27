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

        SqlStatics.BuildSqlCte<MySqlSqlDialectImpl>(sqlBuilder, CteTables, Recursive);

        sqlBuilder.Append("REPLACE INTO ");
        Table.BuildRefSql(sqlBuilder);

        SqlStatics.BuildInsertColumnList<MySqlSqlDialectImpl>(sqlBuilder, allColumns);
        SqlStatics.BuildInsertRowValues(sqlBuilder, NewValues, allColumns, defaultValue: "DEFAULT");
    }
}
