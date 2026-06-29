using Drizzle4Dotnet.Core.Query.Select;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Default.Query;

/// <summary>
/// Default/generic SELECT query builder using <see cref="DefaultSqlDialectImpl"/>.
/// Provides standard SQL SELECT functionality without dialect-specific features.
/// </summary>
public class DefaultSelectQuery<TReturn, TVirtualTable> : SelectQuery<TReturn, DefaultSqlDialectImpl, TVirtualTable, DefaultSelectQuery<TReturn, TVirtualTable>>
    where TVirtualTable : IVirtualTable<DefaultSqlDialectImpl>
{
    public DefaultSelectQuery(
        ISelectedColumns<TReturn, DefaultSqlDialectImpl, TVirtualTable> selectedColumns,
        IQueryExecutor<DefaultSqlDialectImpl> executor
    ) : base(selectedColumns, executor)
    {
    }
}
