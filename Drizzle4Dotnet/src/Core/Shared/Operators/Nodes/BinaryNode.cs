namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;


public readonly struct BinarySqlValueNode<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _col;
    private readonly T _value;
    private readonly string _operator;

    public BinarySqlValueNode(ISql<T> col, T value,  string @operator)
    {
        _col = col;
        _value = value;
        _operator = @operator;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _col.BuildSql(sqlBuilder);
        string paramName = sqlBuilder.AddParameter(_value);
        sqlBuilder.Append(' ').Append(_operator).Append(' ').Append(paramName);
    }
}

public readonly struct BinarySqlListValueNode<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _col;
    private readonly IEnumerable<T> _value;
    private readonly string _operator;

    public BinarySqlListValueNode(ISql<T> col, IEnumerable<T> value,  string @operator)
    {
        _col = col;
        _value = value;
        _operator = @operator;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _col.BuildSql(sqlBuilder);
        var paramNames = new List<string>();

        foreach (var val in _value)
        {
            paramNames.Add(sqlBuilder.AddParameter(val));
        }

        if (paramNames.Count == 0)
        {
            sqlBuilder.Append(_operator.Contains("NOT") ? " 1=1 " : " 1=0 ");
            return;
        }

        sqlBuilder.Append(' ').Append(_operator).Append(" (");
        for (int i = 0; i < paramNames.Count; i++)
        {
            if (i > 0) sqlBuilder.Append(", ");
            sqlBuilder.Append(paramNames[i]);
        }
        sqlBuilder.Append(')');
    }
}

public readonly struct BinaryNode<T> : IOperator<T>
{
    private readonly ISql<T> _c1;
    private readonly ISql<T> _c2;
    private readonly string _operator;
    private readonly bool _wrapInParentheses;

    public BinaryNode(ISql<T> c1, ISql<T> c2, string @operator, bool wrapInParentheses = false)
    {
        _c1 = c1;
        _c2 = c2;
        _operator = @operator;
        _wrapInParentheses = wrapInParentheses;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_wrapInParentheses) sqlBuilder.Append('(');
        _c1.BuildSql(sqlBuilder);
        if (_wrapInParentheses) sqlBuilder.Append(')');
        sqlBuilder.Append(' ').Append(_operator).Append(' ');
        if (_wrapInParentheses) sqlBuilder.Append('(');
        _c2.BuildSql(sqlBuilder);
        if (_wrapInParentheses) sqlBuilder.Append(')');
    }
}

public readonly struct BinaryNode<T1, T2, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T1> _c1;
    private readonly ISql<T2> _c2;
    private readonly string _operator;
    private readonly bool _wrapInParentheses;

    public BinaryNode(ISql<T1> c1, ISql<T2> c2, string @operator, bool wrapInParentheses = false)
    {
        _c1 = c1;
        _c2 = c2;
        _operator = @operator;
        _wrapInParentheses = wrapInParentheses;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        if (_wrapInParentheses) sqlBuilder.Append('(');
        _c1.BuildSql(sqlBuilder);
        if (_wrapInParentheses) sqlBuilder.Append(')');
        sqlBuilder.Append(' ').Append(_operator).Append(' ');
        if (_wrapInParentheses) sqlBuilder.Append('(');
        _c2.BuildSql(sqlBuilder);
        if (_wrapInParentheses) sqlBuilder.Append(')');
    }
}