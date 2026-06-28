using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Migration.Query;

/// <summary>
/// Builds an ALTER TABLE DDL statement for adding, dropping, or modifying columns.
/// </summary>
public class AlterTableQuery : ISql
{
    private readonly string _tableName;
    private readonly string _schemaName;
    private readonly List<AlterTableAction> _actions = new();

    public AlterTableQuery(string tableName, string schemaName = "public")
    {
        _tableName = tableName;
        _schemaName = schemaName;
    }

    /// <summary>
    /// Adds a column to the table.
    /// </summary>
    public AlterTableQuery AddColumn(IColumnDefinition column)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.AddColumn, column));
        return this;
    }

    /// <summary>
    /// Drops a column from the table.
    /// </summary>
    public AlterTableQuery DropColumn(string columnName)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.DropColumn, columnName: columnName));
        return this;
    }

    /// <summary>
    /// Alters a column's data type.
    /// </summary>
    public AlterTableQuery AlterColumnType(string columnName, string newDataType, string? usingExpression = null)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.AlterColumnType, 
            columnName: columnName, newDataType: newDataType, usingExpression: usingExpression));
        return this;
    }

    /// <summary>
    /// Sets a column to NOT NULL.
    /// </summary>
    public AlterTableQuery SetNotNull(string columnName)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.SetNotNull, columnName: columnName));
        return this;
    }

    /// <summary>
    /// Drops a column's NOT NULL constraint.
    /// </summary>
    public AlterTableQuery DropNotNull(string columnName)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.DropNotNull, columnName: columnName));
        return this;
    }

    /// <summary>
    /// Sets a default value for a column.
    /// </summary>
    public AlterTableQuery SetDefault(string columnName, string defaultValue)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.SetDefault, 
            columnName: columnName, newDefault: defaultValue));
        return this;
    }

    /// <summary>
    /// Drops the default value of a column.
    /// </summary>
    public AlterTableQuery DropDefault(string columnName)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.DropDefault, columnName: columnName));
        return this;
    }

    /// <summary>
    /// Renames a column.
    /// </summary>
    public AlterTableQuery RenameColumn(string oldName, string newName)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.RenameColumn, 
            columnName: oldName, newName: newName));
        return this;
    }

    /// <summary>
    /// Adds a table constraint (e.g., FOREIGN KEY, UNIQUE).
    /// </summary>
    public AlterTableQuery AddConstraint(string constraintDefinition)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.AddConstraint, constraintSql: constraintDefinition));
        return this;
    }

    /// <summary>
    /// Drops a constraint by name.
    /// </summary>
    public AlterTableQuery DropConstraint(string constraintName)
    {
        _actions.Add(new AlterTableAction(AlterTableActionType.DropConstraint, columnName: constraintName));
        return this;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        var fullTableName = string.IsNullOrEmpty(_schemaName)
            ? _tableName
            : $"{_schemaName}.{_tableName}";

        foreach (var action in _actions)
        {
            sqlBuilder.Append("ALTER TABLE ");
            sqlBuilder.Append(fullTableName);
            sqlBuilder.Append(' ');

            switch (action.ActionType)
            {
                case AlterTableActionType.AddColumn:
                    sqlBuilder.Append("ADD COLUMN ");
                    AppendColumnDef(sqlBuilder, action.Column!);
                    break;

                case AlterTableActionType.DropColumn:
                    sqlBuilder.Append("DROP COLUMN ");
                    sqlBuilder.Append(action.ColumnName);
                    break;

                case AlterTableActionType.AlterColumnType:
                    sqlBuilder.Append("ALTER COLUMN ");
                    sqlBuilder.Append(action.ColumnName);
                    sqlBuilder.Append(" TYPE ");
                    sqlBuilder.Append(action.NewDataType);
                    if (action.UsingExpression != null)
                    {
                        sqlBuilder.Append(" USING ");
                        sqlBuilder.Append(action.UsingExpression);
                    }
                    break;

                case AlterTableActionType.SetNotNull:
                    sqlBuilder.Append("ALTER COLUMN ");
                    sqlBuilder.Append(action.ColumnName);
                    sqlBuilder.Append(" SET NOT NULL");
                    break;

                case AlterTableActionType.DropNotNull:
                    sqlBuilder.Append("ALTER COLUMN ");
                    sqlBuilder.Append(action.ColumnName);
                    sqlBuilder.Append(" DROP NOT NULL");
                    break;

                case AlterTableActionType.SetDefault:
                    sqlBuilder.Append("ALTER COLUMN ");
                    sqlBuilder.Append(action.ColumnName);
                    sqlBuilder.Append(" SET DEFAULT ");
                    sqlBuilder.Append(action.NewDefault);
                    break;

                case AlterTableActionType.DropDefault:
                    sqlBuilder.Append("ALTER COLUMN ");
                    sqlBuilder.Append(action.ColumnName);
                    sqlBuilder.Append(" DROP DEFAULT");
                    break;

                case AlterTableActionType.RenameColumn:
                    sqlBuilder.Append("RENAME COLUMN ");
                    sqlBuilder.Append(action.ColumnName);
                    sqlBuilder.Append(" TO ");
                    sqlBuilder.Append(action.NewName);
                    break;

                case AlterTableActionType.AddConstraint:
                    sqlBuilder.Append("ADD ");
                    sqlBuilder.Append(action.ConstraintSql);
                    break;

                case AlterTableActionType.DropConstraint:
                    sqlBuilder.Append("DROP CONSTRAINT ");
                    sqlBuilder.Append(action.ColumnName);
                    break;
            }

            sqlBuilder.Append(";\n");
        }
    }

    private static void AppendColumnDef(ISqlBuilder sqlBuilder, IColumnDefinition col)
    {
        sqlBuilder.Append(col.Name);
        sqlBuilder.Append(' ');
        sqlBuilder.Append(col.DataType);

        if (col.IsAutoIncrement)
        {
            sqlBuilder.Append(" GENERATED BY DEFAULT AS IDENTITY");
        }

        if (!col.IsNullable)
        {
            sqlBuilder.Append(" NOT NULL");
        }

        if (col.DefaultValue != null)
        {
            sqlBuilder.Append(" DEFAULT ");
            sqlBuilder.Append(col.DefaultValue);
        }

        if (col.IsPrimaryKey)
        {
            sqlBuilder.Append(" PRIMARY KEY");
        }
    }

    private enum AlterTableActionType
    {
        AddColumn,
        DropColumn,
        AlterColumnType,
        SetNotNull,
        DropNotNull,
        SetDefault,
        DropDefault,
        RenameColumn,
        AddConstraint,
        DropConstraint
    }

    private sealed class AlterTableAction
    {
        public AlterTableActionType ActionType { get; }
        public IColumnDefinition? Column { get; }
        public string? ColumnName { get; }
        public string? NewDataType { get; }
        public string? NewDefault { get; }
        public string? NewName { get; }
        public string? UsingExpression { get; }
        public string? ConstraintSql { get; }

        public AlterTableAction(
            AlterTableActionType actionType,
            IColumnDefinition? column = null,
            string? columnName = null,
            string? newDataType = null,
            string? newDefault = null,
            string? newName = null,
            string? usingExpression = null,
            string? constraintSql = null)
        {
            ActionType = actionType;
            Column = column;
            ColumnName = columnName;
            NewDataType = newDataType;
            NewDefault = newDefault;
            NewName = newName;
            UsingExpression = usingExpression;
            ConstraintSql = constraintSql;
        }
    }
}