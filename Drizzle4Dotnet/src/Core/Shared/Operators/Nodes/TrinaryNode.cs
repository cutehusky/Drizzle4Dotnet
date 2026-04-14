namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;


public readonly struct TrinaryNode<T>: IOperator<T>
{
    private readonly ISql<T> _col1;
    private readonly ISql<T> _col2;
    private readonly ISql<T> _col3;
    private readonly string _operator1;
    private readonly string _operator2;
    
    public TrinaryNode(ISql<T> col1, ISql<T> col2, ISql<T> col3, string operator1, string operator2)
    {
        _col1 = col1;
        _col2 = col2;
        _col3 = col3;
        _operator1 = operator1;
        _operator2 = operator2;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _col1.BuildSql(sqlBuilder);
        sqlBuilder.Append(' ').Append(_operator1).Append(' ');
        _col2.BuildSql(sqlBuilder);
        sqlBuilder.Append(' ').Append(_operator2).Append(' ');
        _col3.BuildSql(sqlBuilder);
    }
}

public readonly struct TrinaryNode<T, TReturn>: IOperator<TReturn>
{
    private readonly ISql<T> _col1;
    private readonly ISql<T> _col2;
    private readonly ISql<T> _col3;
    private readonly string _operator1;
    private readonly string _operator2;
    
    public TrinaryNode(ISql<T> col1, ISql<T> col2, ISql<T> col3, string operator1, string operator2)
    {
        _col1 = col1;
        _col2 = col2;
        _col3 = col3;
        _operator1 = operator1;
        _operator2 = operator2;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _col1.BuildSql(sqlBuilder);
        sqlBuilder.Append(' ').Append(_operator1).Append(' ');
        _col2.BuildSql(sqlBuilder);
        sqlBuilder.Append(' ').Append(_operator2).Append(' ');
        _col3.BuildSql(sqlBuilder);
    }
}


public readonly struct TrinaryNodeV2Val<T, TReturn>: IOperator<TReturn>
{
    private readonly ISql<T> _col1;
    private readonly T _col2;
    private readonly ISql<T> _col3;
    private readonly string _operator1;
    private readonly string _operator2;
    
    public TrinaryNodeV2Val(ISql<T> col1, T col2, ISql<T> col3, string operator1, string operator2)
    {
        _col1 = col1;
        _col2 = col2;
        _col3 = col3;
        _operator1 = operator1;
        _operator2 = operator2;
    }
    
    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _col1.BuildSql(sqlBuilder);
        
        sqlBuilder.Append(' ').Append(_operator1).Append(' ');
        
        var paramName = sqlBuilder.AddParameter(_col2);
        sqlBuilder.Append(paramName);
        
        sqlBuilder.Append(' ').Append(_operator2).Append(' ');
        
        _col3.BuildSql(sqlBuilder);
    }
}

public readonly struct TrinaryNodeV3Val<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _col1;
    private readonly ISql<T> _col2;
    private readonly T _col3;
    private readonly string _operator1;
    private readonly string _operator2;

    public TrinaryNodeV3Val(ISql<T> col1, ISql<T> col2, T col3, string operator1, string operator2)
    {
        _col1 = col1;
        _col2 = col2;
        _col3 = col3;
        _operator1 = operator1;
        _operator2 = operator2;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _col1.BuildSql(sqlBuilder);
        
        sqlBuilder.Append(' ').Append(_operator1).Append(' ');
        
        _col2.BuildSql(sqlBuilder);
        
        sqlBuilder.Append(' ').Append(_operator2).Append(' ');
        
        var paramName = sqlBuilder.AddParameter(_col3);
        sqlBuilder.Append(paramName);
    }
}

public readonly struct TrinaryNodeV23Val<T, TReturn> : IOperator<TReturn>
{
    private readonly ISql<T> _col1;
    private readonly T _col2;
    private readonly T _col3;
    private readonly string _operator1;
    private readonly string _operator2;

    public TrinaryNodeV23Val(ISql<T> col1, T col2, T col3, string operator1, string operator2)
    {
        _col1 = col1;
        _col2 = col2;
        _col3 = col3;
        _operator1 = operator1;
        _operator2 = operator2;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        _col1.BuildSql(sqlBuilder);
        
        sqlBuilder.Append(' ').Append(_operator1).Append(' ');
        
        var paramName2 = sqlBuilder.AddParameter(_col2);
        sqlBuilder.Append(paramName2);
        
        sqlBuilder.Append(' ').Append(_operator2).Append(' ');
        
        var paramName3 = sqlBuilder.AddParameter(_col3);
        sqlBuilder.Append(paramName3);
    }
}
