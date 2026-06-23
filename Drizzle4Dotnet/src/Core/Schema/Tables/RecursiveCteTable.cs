using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Schema.Tables;

/// <summary>
/// Represents a recursive CTE table (WITH RECURSIVE ... AS (...)).
/// Wraps a compound query (anchor UNION ALL recursive) as an ICteTable.
/// </summary>
public class RecursiveCteTable<TReturn, TDialect> : ICteTable<TDialect>, IGetFieldByName
    where TDialect : ISqlDialect
{
    private readonly IGenericSql _body;
    private readonly string _alias;
    private readonly ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> _selectedColumns;

    public RecursiveCteTable(
        string alias,
        IGenericSql body,
        ITypedTupleSelectedColumns<TReturn, TDialect, TypedTupleGeneratedSubqueryTable<TReturn, TDialect>> selectedColumns)
    {
        _alias = alias;
        _body = body;
        _selectedColumns = selectedColumns;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
        sqlBuilder.Append(" AS (");
        _body.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }

    public void BuildRefSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(TDialect.BuildIdentifier(_alias));
    }

    public IAliasedSql<T> Field<T>(string columnName)
    {
        var col = _selectedColumns.Field<T>(columnName);
        if (col == null)
            throw new ArgumentException($"Invalid data type requested for column {columnName} in recursive CTE {_alias}");
        return new VirtualColumn<T, TDialect>(_alias, col.Identifier);
    }

    public IAliasedSql<T> Field<T>(IAliasedSql<T> column)
    {
        var col = _selectedColumns.Field(column);
        if (col == null)
            throw new ArgumentException($"Invalid data type requested for column {column} in recursive CTE {_alias}");
        return new VirtualColumn<T, TDialect>(_alias, col.Identifier);
    }
}
