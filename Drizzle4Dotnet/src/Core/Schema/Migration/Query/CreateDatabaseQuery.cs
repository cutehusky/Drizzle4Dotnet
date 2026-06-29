using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration.Query;

/// <summary>
/// Builds a CREATE DATABASE DDL statement.
/// Generates: CREATE DATABASE [IF NOT EXISTS] database_name
/// Used primarily by the MySQL dialect (where SCHEMA == DATABASE).
/// </summary>
public class CreateDatabaseQuery : ISql
{
    private readonly string _databaseName;
    private bool _ifNotExists = true;

    /// <summary>
    /// Creates a new CREATE DATABASE query for the given database name.
    /// </summary>
    /// <param name="databaseName">The database name to create.</param>
    public CreateDatabaseQuery(string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);
        _databaseName = databaseName;
    }

    /// <summary>
    /// Adds IF NOT EXISTS clause (default is true).
    /// </summary>
    public CreateDatabaseQuery IfNotExists(bool value = true)
    {
        _ifNotExists = value;
        return this;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("CREATE DATABASE ");
        if (_ifNotExists)
            sqlBuilder.Append("IF NOT EXISTS ");
        sqlBuilder.Append(_databaseName);

        // Add default character set for MySQL
        sqlBuilder.Append(" CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci");
    }
}
