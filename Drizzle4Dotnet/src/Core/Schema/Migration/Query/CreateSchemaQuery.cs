using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration.Query;

/// <summary>
/// Builds a CREATE SCHEMA DDL statement.
/// Generates: CREATE SCHEMA [IF NOT EXISTS] schema_name
/// Used by PgSql, Mssql, and Oracle dialects to auto-create schemas during migration.
/// </summary>
public class CreateSchemaQuery : ISql
{
    private readonly string _schemaName;
    private bool _ifNotExists = true;

    /// <summary>
    /// Creates a new CREATE SCHEMA query for the given schema name.
    /// </summary>
    /// <param name="schemaName">The database schema name to create.</param>
    public CreateSchemaQuery(string schemaName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        _schemaName = schemaName;
    }

    /// <summary>
    /// Adds IF NOT EXISTS clause (default is true).
    /// </summary>
    public CreateSchemaQuery IfNotExists(bool value = true)
    {
        _ifNotExists = value;
        return this;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("CREATE SCHEMA ");
        if (_ifNotExists)
            sqlBuilder.Append("IF NOT EXISTS ");
        sqlBuilder.Append(_schemaName);
    }
}
