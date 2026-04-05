namespace Drizzle_Like.Query.Select;


public class ReturningQuery<TReturn> : Query<TReturn>
{
    private readonly IParameterizedSql<object> _baseQuery;

    public ReturningQuery(
        IParameterizedSql<object> baseQuery,
        ISelectedColumns<TReturn> selectedColumns,
        DbClient dbClient) 
        : base(selectedColumns, baseQuery.Parameters, dbClient)
    {
        _baseQuery = baseQuery;
    }

    public override string Sql => $"{_baseQuery.Sql} RETURNING {SelectedColumns!.Sql}";
}
