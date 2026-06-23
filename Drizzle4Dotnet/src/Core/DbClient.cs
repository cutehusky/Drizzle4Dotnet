using System.Data.Common;
using Drizzle4Dotnet.Core.Schema.Migration;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core;


public abstract class DbClient<TDialect>: IAsyncDisposable where TDialect : ISqlDialect
{
    // ======================================================================
    // DDL / Schema / Migration Factory Methods
    // ======================================================================

    /// <summary>
    /// Creates a builder for a CREATE TABLE statement.
    /// </summary>
    protected static CreateTableQuery CreateTable(TableDefinition table) =>
        new(table);

    /// <summary>
    /// Creates a builder for a DROP TABLE statement.
    /// </summary>
    protected static DropTableQuery DropTable(string tableName, string schemaName = "public") =>
        new(tableName, schemaName);

    /// <summary>
    /// Creates a builder for an ALTER TABLE statement.
    /// </summary>
    protected static AlterTableQuery AlterTable(string tableName, string schemaName = "public") =>
        new(tableName, schemaName);

    /// <summary>
    /// Creates a builder for a CREATE INDEX statement.
    /// </summary>
    protected static CreateIndexQuery CreateIndex(string indexName, string tableName, string schemaName = "public") =>
        new(indexName, tableName, schemaName);

    /// <summary>
    /// Creates a builder for a DROP INDEX statement.
    /// </summary>
    protected static DropIndexQuery DropIndex(string indexName, string? tableName = null) =>
        new(indexName, tableName);

    /// <summary>
    /// Creates a <see cref="TableDefinition"/> from a table type implementing <see cref="ITable{TDialect}"/>.
    /// This uses reflection to extract column metadata from the table's Columns class.
    /// </summary>
    protected static TableDefinition GetTableDefinition<TTable>() where TTable : ITable<TDialect>
    {
        // Use reflection to extract table name and columns from the generated table class
        var tableType = typeof(TTable);
        
        // Get table name from static properties
        var tableNameProp = tableType.GetProperty("TableName", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var tableName = tableNameProp?.GetValue(null)?.ToString() ?? tableType.Name;
        
        var schemaNameProp = tableType.GetProperty("SchemaName",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var schemaName = schemaNameProp?.GetValue(null)?.ToString() ?? "public";

        // Get columns from ColumnNames static class
        var columnNamesType = tableType.GetNestedType("ColumnNames");
        if (columnNamesType == null)
        {
            // Fall back to the Columns static class property types
            var columnsClass = tableType.GetNestedType("Columns");
            if (columnsClass == null)
                throw new InvalidOperationException($"Table {tableType.Name} has no Columns or ColumnNames definition.");

            var columnNames = columnsClass.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Select(f => f.Name)
                .ToList();

            var columnDefs = columnNames.Select(name => new ColumnDefinition(name, MapClrTypeToSql(name, tableType))).ToList();
            return new TableDefinition(tableName, schemaName, columnDefs);
        }

        var colNames = columnNamesType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Select(f => f.GetValue(null)?.ToString() ?? f.Name)
            .ToList();

        var colDefs = colNames.Select(name => new ColumnDefinition(name, "BIGINT") // Default data type
        {
            IsNullable = false // Default to NOT NULL
        }).ToList();

        return new TableDefinition(tableName, schemaName, colDefs);
    }

    /// <summary>
    /// Maps a C# CLR type name to a SQL data type for DDL generation.
    /// Override this in dialect-specific subclasses for type mapping customization.
    /// </summary>
    protected static string MapClrTypeToSql(string columnName, System.Type tableType)
    {
        // Default type mapping — override in dialect-specific subclasses
        var columnsClass = tableType.GetNestedType("Columns");
        if (columnsClass == null) return "BIGINT";

        var prop = columnsClass.GetField(columnName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (prop == null) return "BIGINT";

        var clrType = prop.FieldType;
        return ClrTypeToSqlType(clrType);
    }

    /// <summary>
    /// Maps a CLR type to a generic SQL data type.
    /// </summary>
    protected static string ClrTypeToSqlType(System.Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type);
        if (underlyingType != null)
            return ClrTypeToSqlType(underlyingType);

        if (type == typeof(int) || type == typeof(long) || type == typeof(short))
            return "BIGINT";
        if (type == typeof(string))
            return "TEXT";
        if (type == typeof(bool))
            return "BOOLEAN";
        if (type == typeof(decimal) || type == typeof(float) || type == typeof(double))
            return "NUMERIC(18,2)";
        if (type == typeof(DateTime))
            return "TIMESTAMP";
        if (type == typeof(Guid))
            return "UUID";
        if (type == typeof(byte[]))
            return "BYTEA";

        return "TEXT";
    }
    protected readonly DbConnection _conn;
    protected readonly DbTransaction? _transaction;

    protected DbClient(DbConnection conn,  DbTransaction? transaction = null)
    {
        _conn = conn;
        _transaction = transaction;
    }

    public async Task<List<T>> ExecuteGetListAsync<T, TVirtualTable>(IReturning<T, TDialect, TVirtualTable> query) where TVirtualTable : IVirtualTable<TDialect>
    {
        await using var cmd = _conn.CreateCommand();
        cmd.Transaction = _transaction;

        var sqlBuilder = new SqlBuilder<TDialect>();
        query.BuildSql(sqlBuilder);
        var (sql, parameters) = sqlBuilder.Build();
        cmd.CommandText = sql;

        foreach (var entry in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = entry.Key;
            p.Value = entry.Value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<T>();
        var mapper = query.Mapper;
        while (await reader.ReadAsync())
        {
            result.Add(mapper(reader));
        }
        return result;
    }
    
    public async Task<List<T>> ExecuteGetListAsync<T>(IReturning<T, TDialect> query)
    {
        await using var cmd = _conn.CreateCommand();
        cmd.Transaction = _transaction;

        var sqlBuilder = new SqlBuilder<TDialect>();
        query.BuildSql(sqlBuilder);
        var (sql, parameters) = sqlBuilder.Build();
        cmd.CommandText = sql;

        foreach (var entry in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = entry.Key;
            p.Value = entry.Value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        var result = new List<T>();
        var mapper = query.Mapper;
        while (await reader.ReadAsync())
        {
            result.Add(mapper(reader));
        }
        return result;
    }
    
    public async Task ExecuteAsync(ISql query)
    {
        await using var cmd = _conn.CreateCommand();
        cmd.Transaction = _transaction;

        var sqlBuilder = new SqlBuilder<TDialect>();
        query.BuildSql(sqlBuilder);
        var (sql, parameters) = sqlBuilder.Build();
        cmd.CommandText = sql;

        foreach (var entry in parameters)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = entry.Key;
            p.Value = entry.Value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        await cmd.ExecuteNonQueryAsync();
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
    }
}


public abstract class DbClientWithTransaction<TInstance, TDialect>: DbClient<TDialect> 
where TDialect : ISqlDialect
where TInstance : DbClientWithTransaction<TInstance, TDialect>
{
    protected DbClientWithTransaction(DbConnection conn, DbTransaction? transaction = null) : base(conn, transaction)
    {
    }

    /// <summary>
    /// Factory method for subclasses to create a new instance of themselves with a transaction.
    /// Each concrete subclass must implement this to return its own type.
    /// </summary>
    protected abstract TInstance CreateInstance(DbConnection conn, DbTransaction? transaction);

    
    public async Task CommitAsync() => await (_transaction?.CommitAsync() ?? Task.CompletedTask);
    public async Task RollbackAsync() => await (_transaction?.RollbackAsync() ?? Task.CompletedTask);

    public async Task<TInstance> BeginTransactionAsync()
    {
        var transaction = await _conn.BeginTransactionAsync();
        return CreateInstance(_conn, transaction);
    }
    
    public async Task RunInTransactionAsync(Func<TInstance, Task> action)
    {
        await using var txClient = await BeginTransactionAsync();
        try
        {
            await action(txClient);
            await txClient.CommitAsync();
        }
        catch
        {
            await txClient.RollbackAsync();
            throw;
        }
    }
}
