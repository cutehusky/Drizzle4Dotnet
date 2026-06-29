using Drizzle4Dotnet.Core.Query.Delete;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Default.Query;

/// <summary>
/// Default/generic DELETE query builder using <see cref="DefaultSqlDialectImpl"/>.
/// Provides standard SQL DELETE functionality without dialect-specific features.
/// </summary>
public class DefaultDeleteQuery<TTable> : DeleteQuery<TTable, DefaultSqlDialectImpl, DefaultDeleteQuery<TTable>>
    where TTable : ITable<DefaultSqlDialectImpl>
{
    public DefaultDeleteQuery(TTable table, IQueryExecutor<DefaultSqlDialectImpl> executor)
        : base(table, executor)
    {
    }
}
