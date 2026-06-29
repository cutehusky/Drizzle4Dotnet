using Drizzle4Dotnet.Core.Query.Update;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Default.Query;

/// <summary>
/// Default/generic UPDATE query builder using <see cref="DefaultSqlDialectImpl"/>.
/// Provides standard SQL UPDATE functionality without dialect-specific features.
/// </summary>
public class DefaultUpdateQuery<TTable> : UpdateQuery<TTable, DefaultSqlDialectImpl, DefaultUpdateQuery<TTable>>
    where TTable : ITable<DefaultSqlDialectImpl>
{
    public DefaultUpdateQuery(TTable table, IQueryExecutor<DefaultSqlDialectImpl> executor)
        : base(table, executor)
    {
    }
}
