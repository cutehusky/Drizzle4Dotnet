using System.Text;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration;

/// <summary>
/// Abstract base for typed table-level constraints (e.g., FOREIGN KEY, UNIQUE, CHECK).
/// Each subclass knows how to build its SQL representation via <see cref="BuildSql"/>.
/// </summary>
public abstract class TableConstraint
{
    /// <summary>
    /// Appends the constraint SQL to the provided <see cref="ISqlBuilder"/>.
    /// </summary>
    public abstract void BuildSql(ISqlBuilder sqlBuilder);

    /// <summary>
    /// Returns the full constraint SQL string.
    /// </summary>
    public override string ToString()
    {
        var builder = new StringBuilderSqlBuilder();
        BuildSql(builder);
        return builder.ToString();
    }

}

/// <summary>
/// A raw SQL string constraint, for backward compatibility.
/// </summary>
public class RawTableConstraint : TableConstraint
{
    /// <summary>Optional constraint name (e.g., "FK_Users_Departments").</summary>
    public string? ConstraintName { get; }

    /// <summary>The raw constraint SQL.</summary>
    public string Sql { get; }

    /// <param name="sql">The raw constraint SQL string.</param>
    /// <param name="constraintName">Optional constraint name.</param>
    public RawTableConstraint(string sql, string? constraintName = null)
    {
        Sql = sql;
        ConstraintName = constraintName;
    }

    /// <inheritdoc />
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(Sql);
    }
}

/// <summary>
/// A FOREIGN KEY constraint: <c>FOREIGN KEY (cols) REFERENCES ForeignTable (foreignCols)</c>.
/// </summary>
public class ForeignKeyConstraint : TableConstraint
{
    /// <summary>Optional constraint name (e.g., "FK_Users_Departments").</summary>
    public string? ConstraintName { get; }

    /// <summary>Source column names (database column names).</summary>
    public string[] Columns { get; }

    /// <summary>The referenced table name (database table name).</summary>
    public string ForeignTable { get; }

    /// <summary>The referenced column names (database column names).</summary>
    public string[] ForeignColumns { get; }

    /// <param name="constraintName">Optional constraint name.</param>
    /// <param name="columns">Source column names (DB column names).</param>
    /// <param name="foreignTable">Referenced table name (DB table name).</param>
    /// <param name="foreignColumns">Referenced column names (DB column names).</param>
    public ForeignKeyConstraint(string? constraintName, string[] columns, string foreignTable, string[] foreignColumns)
    {
        ConstraintName = constraintName;
        Columns = columns;
        ForeignTable = foreignTable;
        ForeignColumns = foreignColumns;
    }

    /// <inheritdoc />
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (!string.IsNullOrEmpty(ConstraintName))
        {
            sqlBuilder.Append("CONSTRAINT ");
            sqlBuilder.Append(ConstraintName);
            sqlBuilder.Append(' ');
        }
        sqlBuilder.Append("FOREIGN KEY (");
        sqlBuilder.Append(string.Join(", ", Columns.Select(c => $"\"{c}\"")));
        sqlBuilder.Append(") REFERENCES ");
        sqlBuilder.Append(ForeignTable);
        sqlBuilder.Append(" (");
        sqlBuilder.Append(string.Join(", ", ForeignColumns.Select(c => $"\"{c}\"")));
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// A UNIQUE constraint: <c>UNIQUE (col1, col2, ...)</c>.
/// </summary>
public class UniqueConstraint : TableConstraint
{
    /// <summary>Optional constraint name (e.g., "UQ_Users_Email").</summary>
    public string? ConstraintName { get; }

    /// <summary>Column names that form the unique constraint.</summary>
    public string[] Columns { get; }

    /// <param name="columns">Column names (DB column names).</param>
    /// <param name="constraintName">Optional constraint name.</param>
    public UniqueConstraint(string[] columns, string? constraintName = null)
    {
        Columns = columns;
        ConstraintName = constraintName;
    }

    /// <inheritdoc />
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (!string.IsNullOrEmpty(ConstraintName))
        {
            sqlBuilder.Append("CONSTRAINT ");
            sqlBuilder.Append(ConstraintName);
            sqlBuilder.Append(' ');
        }
        sqlBuilder.Append("UNIQUE (");
        sqlBuilder.Append(string.Join(", ", Columns.Select(c => $"\"{c}\"")));
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// A PRIMARY KEY constraint: <c>PRIMARY KEY (col1, col2, ...)</c>.
/// </summary>
public class PrimaryKeyTableConstraint : TableConstraint
{
    /// <summary>Optional constraint name (e.g., "PK_Users").</summary>
    public string? ConstraintName { get; }

    /// <summary>Column names that form the primary key.</summary>
    public string[] Columns { get; }

    /// <param name="columns">Column names (DB column names).</param>
    /// <param name="constraintName">Optional constraint name.</param>
    public PrimaryKeyTableConstraint(string[] columns, string? constraintName = null)
    {
        Columns = columns;
        ConstraintName = constraintName;
    }

    /// <inheritdoc />
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (!string.IsNullOrEmpty(ConstraintName))
        {
            sqlBuilder.Append("CONSTRAINT ");
            sqlBuilder.Append(ConstraintName);
            sqlBuilder.Append(' ');
        }
        sqlBuilder.Append("PRIMARY KEY (");
        sqlBuilder.Append(string.Join(", ", Columns.Select(c => $"\"{c}\"")));
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// A CHECK constraint: <c>CHECK (expression)</c>.
/// </summary>
public class CheckTableConstraint : TableConstraint
{
    /// <summary>Optional constraint name (e.g., "CHK_Age_Positive").</summary>
    public string? ConstraintName { get; }

    /// <summary>The CHECK expression (e.g., "value > 0").</summary>
    public string Expression { get; }

    /// <param name="expression">The CHECK constraint expression.</param>
    /// <param name="constraintName">Optional constraint name.</param>
    public CheckTableConstraint(string expression, string? constraintName = null)
    {
        Expression = expression;
        ConstraintName = constraintName;
    }

    /// <inheritdoc />
    public override void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (!string.IsNullOrEmpty(ConstraintName))
        {
            sqlBuilder.Append("CONSTRAINT ");
            sqlBuilder.Append(ConstraintName);
            sqlBuilder.Append(' ');
        }
        sqlBuilder.Append("CHECK (");
        sqlBuilder.Append(Expression);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Represents a database index definition for DDL generation (CREATE INDEX).
/// Not part of CREATE TABLE; used for separate index creation.
/// </summary>
public class TableIndex
{
    /// <summary>The index name.</summary>
    public string IndexName { get; }

    /// <summary>Table schema.</summary>
    public string SchemaName { get; }

    /// <summary>Table name.</summary>
    public string TableName { get; }

    /// <summary>Indexed column names (database column names).</summary>
    public string[] Columns { get; }

    /// <summary>Whether this is a UNIQUE index.</summary>
    public bool IsUnique { get; }

    /// <summary>Index type (e.g., "BTREE", "HASH", "GIN"). Null for default.</summary>
    public string? IndexType { get; }

    /// <summary>Partial index WHERE condition. Null for full index.</summary>
    public string? Where { get; }

    public TableIndex(
        string indexName,
        string schemaName,
        string tableName,
        string[] columns,
        bool isUnique = false,
        string? indexType = null,
        string? where = null)
    {
        IndexName = indexName;
        SchemaName = schemaName;
        TableName = tableName;
        Columns = columns;
        IsUnique = isUnique;
        IndexType = indexType;
        Where = where;
    }

    /// <summary>
    /// Builds a CREATE INDEX statement.
    /// </summary>
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append("CREATE ");
        if (IsUnique)
            sqlBuilder.Append("UNIQUE ");
        sqlBuilder.Append("INDEX ");
        sqlBuilder.Append(IndexName);
        sqlBuilder.Append(" ON ");

        var fullTableName = string.IsNullOrEmpty(SchemaName)
            ? TableName
            : $"{SchemaName}.{TableName}";
        sqlBuilder.Append(fullTableName);

        if (IndexType != null)
        {
            sqlBuilder.Append(" USING ");
            sqlBuilder.Append(IndexType);
        }

        sqlBuilder.Append(" (");
        sqlBuilder.Append(string.Join(", ", Columns));
        sqlBuilder.Append(')');

        if (Where != null)
        {
            sqlBuilder.Append(" WHERE ");
            sqlBuilder.Append(Where);
        }
    }

    public override string ToString()
    {
        var builder = new StringBuilderSqlBuilder();
        BuildSql(builder);
        return builder.ToString();
    }
}

/// <summary>
/// Minimal <see cref="ISqlBuilder"/> implementation using <see cref="StringBuilder"/>,
/// used by <see cref="TableConstraint.ToString"/> and <see cref="TableIndex.ToString"/>.
/// </summary>
internal sealed class StringBuilderSqlBuilder : ISqlBuilder
{
    private readonly StringBuilder _sb = new();

    public string AddParameter(object? value) => throw new NotSupportedException();

    public ISqlBuilder Append(string sql) { _sb.Append(sql); return this; }
    public ISqlBuilder Append(char sql) { _sb.Append(sql); return this; }

    public (string, Dictionary<string, object?>) Build() => (_sb.ToString(), new Dictionary<string, object?>());

    public override string ToString() => _sb.ToString();
}
