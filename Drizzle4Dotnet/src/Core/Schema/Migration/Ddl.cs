namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Standalone DDL (Data Definition Language) factory methods.
/// Provides builders for CREATE TABLE, DROP TABLE, ALTER TABLE, CREATE INDEX, DROP INDEX.
/// Extracted from DbClient to allow use without a database connection instance.
/// </summary>
public static class Ddl
{
    /// <summary>
    /// Creates a builder for a CREATE TABLE statement.
    /// </summary>
    public static CreateTableQuery CreateTable(TableDefinition table) => new(table);

    /// <summary>
    /// Creates a builder for a DROP TABLE statement.
    /// </summary>
    public static DropTableQuery DropTable(string tableName, string schemaName = "public") =>
        new(tableName, schemaName);

    /// <summary>
    /// Creates a builder for an ALTER TABLE statement.
    /// </summary>
    public static AlterTableQuery AlterTable(string tableName, string schemaName = "public") =>
        new(tableName, schemaName);

    /// <summary>
    /// Creates a builder for a CREATE INDEX statement.
    /// </summary>
    public static CreateIndexQuery CreateIndex(string indexName, string tableName, string schemaName = "public") =>
        new(indexName, tableName, schemaName);

    /// <summary>
    /// Creates a builder for a DROP INDEX statement.
    /// </summary>
    public static DropIndexQuery DropIndex(string indexName, string? tableName = null) =>
        new(indexName, tableName);
}
