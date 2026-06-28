using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.MySql.Query;

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

    protected override void ValidateQuery()
    {
        base.ValidateQuery();
        if (Recursive || CteTables.Count > 0)
        {
            throw new InvalidOperationException("REPLACE query does not support CTEs.");
        }
    }

    protected override void BuildInsertKeywords(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("REPLACE INTO ");
    }

    protected override void BuildDefaultValues(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(" () VALUES ()");
    }
}
