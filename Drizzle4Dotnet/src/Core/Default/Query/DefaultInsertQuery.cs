using Drizzle4Dotnet.Core.Query.Insert;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Default.Query;

/// <summary>
/// Default/generic INSERT query builder using <see cref="DefaultSqlDialectImpl"/>.
/// Provides standard SQL INSERT functionality without dialect-specific features.
/// </summary>
public class DefaultInsertQuery<TTable> : InsertQuery<TTable, DefaultSqlDialectImpl, DefaultInsertQuery<TTable>>
    where TTable : ITable<DefaultSqlDialectImpl>
{
    public DefaultInsertQuery(TTable table, IQueryExecutor<DefaultSqlDialectImpl> executor)
        : base(table, executor)
    {
    }
}
