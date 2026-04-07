using System.Text;
using Drizzle4Dotnet.Core.Query.Shared.Operators;
using Drizzle4Dotnet.Core.Schema.Tables;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Delete;


public class DeleteQuery<TTable, TDialect> : Query<TDialect> where TTable : ITable<TDialect> where TDialect : ISqlDialect
{
    private readonly TTable _table;
    private readonly List<IOperator> _wheres = new();

    public DeleteQuery(TTable table, DbClient<TDialect> dbClient) : base(dbClient)
    {
        _table = table;
    }

    public DeleteQuery<TTable, TDialect> Where(IOperator condition)
    {
        _wheres.Add(condition);
        return this;
    }

    public override string Sql
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append("DELETE FROM ");
            sb.Append(_table.Sql);

            var wheres = _wheres.Select(w => w.BuildSql(Parameters)).ToList();
            if (_wheres.Count > 0)
            {
                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", wheres));
            }

            return sb.ToString();
        }
    }

    public ReturningQuery<TReturn, TDialect> Returning<TReturn>(
        ISelectedColumns<TReturn, TDialect> selectedColumns)
    {
        return new ReturningQuery<TReturn, TDialect>(this, selectedColumns);
    }
}
