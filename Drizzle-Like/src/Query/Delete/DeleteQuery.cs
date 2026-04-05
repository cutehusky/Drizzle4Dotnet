using System.Text;
using Drizzle_Like.Query.Select;
using Drizzle_Like.Query.Shared.Operators;
using Drizzle_Like.Schema.Tables;

namespace Drizzle_Like.Query.Delete;


public class DeleteQuery<TTable> : Query<object> where TTable : ITable
{
    private readonly TTable _table;
    private readonly List<string> _wheres = new();

    public DeleteQuery(TTable table, DbClient dbClient) : base(null, dbClient)
    {
        _table = table;
    }

    public DeleteQuery<TTable> Where(IOperator condition)
    {
        _wheres.Add(condition.BuildSql(Parameters));
        return this;
    }

    public override string Sql
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append("DELETE FROM ");
            sb.Append(_table.Sql);

            if (_wheres.Count > 0)
            {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", _wheres));
            }

            return sb.ToString();
        }
    }

    public ReturningQuery<TReturn> Returning<TReturn>(
        ISelectedColumns<TReturn> selectedColumns)
    {
        return new ReturningQuery<TReturn>(this, selectedColumns, DbClient);
    }
}
