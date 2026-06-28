using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration.Query;

/// <summary>
/// Builds a DROP INDEX DDL statement.
/// </summary>
public class DropIndexQuery : ISql
{
    private readonly string _indexName;
    private readonly string? _tableName; // Needed for PostgreSQL: DROP INDEX IF EXISTS table.index
    private bool _ifExists;
    private bool _cascade;

    public DropIndexQuery(string indexName, string? tableName = null)
    {
        _indexName = indexName;
        _tableName = tableName;
    }

    public DropIndexQuery IfExists()
    {
        _ifExists = true;
        return this;
    }

    public DropIndexQuery Cascade()
    {
        _cascade = true;
        return this;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("DROP INDEX ");
        if (_ifExists)
        {
            sqlBuilder.Append("IF EXISTS ");
        }

        if (_tableName != null)
        {
            sqlBuilder.Append(_tableName);
            sqlBuilder.Append('.');
        }
        sqlBuilder.Append(_indexName);

        if (_cascade)
        {
            sqlBuilder.Append(" CASCADE");
        }
    }
}