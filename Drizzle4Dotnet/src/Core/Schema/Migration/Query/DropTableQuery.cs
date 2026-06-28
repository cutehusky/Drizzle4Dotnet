using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration.Query;

/// <summary>
/// Builds a DROP TABLE DDL statement.
/// </summary>
public class DropTableQuery : ISql
{
    private readonly string _tableName;
    private readonly string _schemaName;
    private bool _ifExists;
    private bool _cascade;

    public DropTableQuery(string tableName, string schemaName = "public")
    {
        _tableName = tableName;
        _schemaName = schemaName;
    }

    /// <summary>
    /// Adds IF EXISTS clause.
    /// </summary>
    public DropTableQuery IfExists()
    {
        _ifExists = true;
        return this;
    }

    /// <summary>
    /// Adds CASCADE clause.
    /// </summary>
    public DropTableQuery Cascade()
    {
        _cascade = true;
        return this;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("DROP TABLE ");
        if (_ifExists)
        {
            sqlBuilder.Append("IF EXISTS ");
        }

        var fullTableName = string.IsNullOrEmpty(_schemaName)
            ? _tableName
            : $"{_schemaName}.{_tableName}";
        sqlBuilder.Append(fullTableName);

        if (_cascade)
        {
            sqlBuilder.Append(" CASCADE");
        }
    }
}