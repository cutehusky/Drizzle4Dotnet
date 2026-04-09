using System.Text;

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


    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"{_col1.BuildSql(parameters)} {_operator1} {_col2.BuildSql(parameters)} {_operator2} {_col3.BuildSql(parameters)}";
    }

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        _col1.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator1).Append(' ');
        _col2.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator2).Append(' ');
        _col3.BuildSql(parameters, sb);
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


    public string BuildSql(Dictionary<string, object?> parameters)
    {
        return $"{_col1.BuildSql(parameters)} {_operator1} {_col2.BuildSql(parameters)} {_operator2} {_col3.BuildSql(parameters)}";
    }

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        _col1.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator1).Append(' ');
        _col2.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator2).Append(' ');
        _col3.BuildSql(parameters, sb);
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


    public string BuildSql(Dictionary<string, object?> parameters)
    {
        var paramName = $"@p{parameters.Count}";
        parameters[paramName] = _col2;
        
        return $"{_col1.BuildSql(parameters)} {_operator1} {paramName} {_operator2} {_col3.BuildSql(parameters)}";
    }

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        var paramName = $"@p{parameters.Count}";
        parameters[paramName] = _col2;
        
        _col1.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator1).Append(' ');
        sb.Append(paramName);
        sb.Append(' ').Append(_operator2).Append(' ');
        _col3.BuildSql(parameters, sb);
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

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        var p3 = $"@p{parameters.Count}";
        parameters[p3] = _col3;
        return $"{_col1.BuildSql(parameters)} {_operator1} {_col2.BuildSql(parameters)} {_operator2} {p3}";
    }

    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        var p3 = $"@p{parameters.Count}";
        parameters[p3] = _col3;
        
        _col1.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator1).Append(' ');
        _col2.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator2).Append(' ');
        sb.Append(p3);
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

    public string BuildSql(Dictionary<string, object?> parameters)
    {
        var p2 = $"@p{parameters.Count}";
        parameters[p2] = _col2;
        var p3 = $"@p{parameters.Count}";
        parameters[p3] = _col3;
        return $"{_col1.BuildSql(parameters)} {_operator1} {p2} {_operator2} {p3}";
    }
    
    public void BuildSql(Dictionary<string, object?> parameters, StringBuilder sb)
    {
        var p2 = $"@p{parameters.Count}";
        parameters[p2] = _col2;
        var p3 = $"@p{parameters.Count}";
        parameters[p3] = _col3;
        
        _col1.BuildSql(parameters, sb);
        sb.Append(' ').Append(_operator1).Append(' ');
        sb.Append(p2);
        sb.Append(' ').Append(_operator2).Append(' ');
        sb.Append(p3);
    }
}
