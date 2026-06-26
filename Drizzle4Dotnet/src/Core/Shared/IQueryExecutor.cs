using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Tables;

namespace Drizzle4Dotnet.Core.Shared;

/// <summary>
/// Abstraction for executing queries against a database.
/// Decouples query building from execution, enabling unit testing
/// without a real database connection.
/// </summary>
public interface IQueryExecutor<TDialect> where TDialect : ISqlDialect
{
    /// <summary>
    /// Executes a returning query and maps results to a list of T.
    /// </summary>
    Task<List<T>> ExecuteGetListAsync<T>(IReturning<T, TDialect> query);
    
    /// <summary>
    /// Executes a returning query with virtual table support and maps results to a list of T.
    /// </summary>
    Task<List<T>> ExecuteGetListAsync<T, TVirtualTable>(IReturning<T, TDialect, TVirtualTable> query) 
        where TVirtualTable : IVirtualTable<TDialect>;
    
    /// <summary>
    /// Executes a non-query (INSERT/UPDATE/DELETE).
    /// </summary>
    Task ExecuteAsync(IGenericSql query);
    
    /// <summary>
    /// Executes a query and returns a single scalar value.
    /// </summary>
    Task<T?> ExecuteScalarAsync<T>(IGenericSql query);
    
    /// <summary>
    /// Executes a query and returns a DbDataReader.
    /// Caller is responsible for disposing the reader.
    /// </summary>
    Task<DbDataReader> ExecuteReaderAsync(IGenericSql query);
}
