
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration.Query;

/// <summary>
/// Builds a CREATE INDEX DDL statement.
/// </summary>
public class CreateIndexQuery : ISql
{
    private readonly string _indexName;
    private readonly string _tableName;
    private readonly string _schemaName;
    private readonly List<string> _columns = new();
    private bool _unique;
    private bool _ifNotExists;
    private string? _indexType; // e.g., "BTREE", "HASH", "GIN", "GiST"
    private string? _where; // Partial index

    public CreateIndexQuery(string indexName, string tableName, string schemaName = "public")
    {
        _indexName = indexName;
        _tableName = tableName;
        _schemaName = schemaName;
    }

    /// <summary>
    /// Specifies the column(s) for the index.
    /// </summary>
    public CreateIndexQuery On(params string[] columns)
    {
        _columns.AddRange(columns);
        return this;
    }

    /// <summary>
    /// Makes the index UNIQUE.
    /// </summary>
    public CreateIndexQuery Unique()
    {
        _unique = true;
        return this;
    }

    /// <summary>
    /// Adds IF NOT EXISTS clause.
    /// </summary>
    public CreateIndexQuery IfNotExists()
    {
        _ifNotExists = true;
        return this;
    }

    /// <summary>
    /// Sets the index type (e.g., "BTREE", "HASH", "GIN", "GiST").
    /// </summary>
    public CreateIndexQuery Using(string indexType)
    {
        _indexType = indexType;
        return this;
    }

    /// <summary>
    /// Adds a partial index WHERE condition.
    /// </summary>
    public CreateIndexQuery Where(string condition)
    {
        _where = condition;
        return this;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("CREATE ");
        if (_unique)
        {
            sqlBuilder.Append("UNIQUE ");
        }
        sqlBuilder.Append("INDEX ");
        if (_ifNotExists)
        {
            sqlBuilder.Append("IF NOT EXISTS ");
        }
        sqlBuilder.Append(_indexName);
        sqlBuilder.Append(" ON ");

        var fullTableName = string.IsNullOrEmpty(_schemaName)
            ? _tableName
            : $"{_schemaName}.{_tableName}";
        sqlBuilder.Append(fullTableName);

        if (_indexType != null)
        {
            sqlBuilder.Append(" USING ");
            sqlBuilder.Append(_indexType);
        }

        sqlBuilder.Append(" (");
        sqlBuilder.Append(string.Join(", ", _columns));
        sqlBuilder.Append(')');

        if (_where != null)
        {
            sqlBuilder.Append(" WHERE ");
            sqlBuilder.Append(_where);
        }
    }
}