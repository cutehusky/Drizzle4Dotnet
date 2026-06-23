namespace Drizzle4Dotnet.Core.Shared.Operators.Nodes;

public interface IFunctionCallNode<TReturn> : IOperator<TReturn>
{
    string FunctionName { get; }
    IGenericSql[] Arguments { get; }
}

/// <summary>
/// Represents a function call: FUNC_NAME(arg1, arg2, ...)
/// For unary functions like UPPER(col), use FunctionCallNode with 1 typed argument.
/// For functions with multiple arguments, use this node.
/// </summary>
public readonly struct FunctionCallNode<TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql[] _arguments;
    private readonly string _separator;

    /// <summary>
    /// The function/operator name (e.g., "ROW_NUMBER", "LEAD", "CONCAT_WS").
    /// </summary>
    public string FunctionName => _functionName;

    /// <summary>
    /// The argument expressions passed to this function call.
    /// </summary>
    public IGenericSql[] Arguments => _arguments;

    /// <summary>
    /// The separator between arguments (default is ", ").
    /// </summary>
    public string Separator => _separator;

    public FunctionCallNode(string functionName, params IGenericSql[] arguments)
        : this(functionName, ", ", arguments)
    {
    }

    public FunctionCallNode(string functionName, string separator, params IGenericSql[] arguments)
    {
        _functionName = functionName;
        _separator = separator;
        _arguments = (IGenericSql[])arguments.Clone();
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        for (int i = 0; i < _arguments.Length; i++)
        {
            if (i > 0) sqlBuilder.Append(_separator);
            _arguments[i].BuildSql(sqlBuilder);
        }
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 1 typed argument: FUNC_NAME(arg1)
/// </summary>
public readonly struct FunctionCallNode<T1, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql[] Arguments => [_arg1];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1)
        : this(functionName, ", ", arg1)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 2 typed arguments: FUNC_NAME(arg1, arg2)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql[] Arguments => [_arg1, _arg2];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2)
        : this(functionName, ", ", arg1, arg2)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
        _arg2 = arg2;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 3 typed arguments: FUNC_NAME(arg1, arg2, arg3)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3)
        : this(functionName, ", ", arg1, arg2, arg3)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 4 typed arguments: FUNC_NAME(arg1, arg2, arg3, arg4)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4)
        : this(functionName, ", ", arg1, arg2, arg3, arg4)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
        _arg4 = arg4;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 5 typed arguments: FUNC_NAME(arg1, arg2, arg3, arg4, arg5)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
        _arg4 = arg4;
        _arg5 = arg5;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 6 typed arguments: FUNC_NAME(arg1, arg2, arg3, arg4, arg5, arg6)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
        _arg4 = arg4;
        _arg5 = arg5;
        _arg6 = arg6;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 7 typed arguments: FUNC_NAME(arg1, arg2, arg3, arg4, arg5, arg6, arg7)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
        _arg4 = arg4;
        _arg5 = arg5;
        _arg6 = arg6;
        _arg7 = arg7;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 8 typed arguments: FUNC_NAME(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1;
        _arg2 = arg2;
        _arg3 = arg3;
        _arg4 = arg4;
        _arg5 = arg5;
        _arg6 = arg6;
        _arg7 = arg7;
        _arg8 = arg8;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator);
        _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 9 typed arguments: FUNC_NAME(arg1, ..., arg9)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 10 typed arguments: FUNC_NAME(arg1, ..., arg10)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly IGenericSql _arg10;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql Arg10 => _arg10;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9, _arg10];
    public string Separator => _separator;

    public FunctionCallNode(string functionName, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10)
    {
    }

    public FunctionCallNode(string functionName, string separator, ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5, ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
        _arg10 = arg10;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg10.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 11 typed arguments: FUNC_NAME(arg1, ..., arg11)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly IGenericSql _arg10;
    private readonly IGenericSql _arg11;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql Arg10 => _arg10;
    public IGenericSql Arg11 => _arg11;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9, _arg10, _arg11];
    public string Separator => _separator;

    public FunctionCallNode(string functionName,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11)
    {
    }

    public FunctionCallNode(string functionName, string separator,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
        _arg10 = arg10; _arg11 = arg11;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg10.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg11.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 12 typed arguments: FUNC_NAME(arg1, ..., arg12)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly IGenericSql _arg10;
    private readonly IGenericSql _arg11;
    private readonly IGenericSql _arg12;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql Arg10 => _arg10;
    public IGenericSql Arg11 => _arg11;
    public IGenericSql Arg12 => _arg12;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9, _arg10, _arg11, _arg12];
    public string Separator => _separator;

    public FunctionCallNode(string functionName,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12)
    {
    }

    public FunctionCallNode(string functionName, string separator,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
        _arg10 = arg10; _arg11 = arg11; _arg12 = arg12;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg10.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg11.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg12.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 13 typed arguments: FUNC_NAME(arg1, ..., arg13)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly IGenericSql _arg10;
    private readonly IGenericSql _arg11;
    private readonly IGenericSql _arg12;
    private readonly IGenericSql _arg13;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql Arg10 => _arg10;
    public IGenericSql Arg11 => _arg11;
    public IGenericSql Arg12 => _arg12;
    public IGenericSql Arg13 => _arg13;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9, _arg10, _arg11, _arg12, _arg13];
    public string Separator => _separator;

    public FunctionCallNode(string functionName,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13)
    {
    }

    public FunctionCallNode(string functionName, string separator,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
        _arg10 = arg10; _arg11 = arg11; _arg12 = arg12;
        _arg13 = arg13;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg10.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg11.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg12.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg13.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 14 typed arguments: FUNC_NAME(arg1, ..., arg14)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly IGenericSql _arg10;
    private readonly IGenericSql _arg11;
    private readonly IGenericSql _arg12;
    private readonly IGenericSql _arg13;
    private readonly IGenericSql _arg14;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql Arg10 => _arg10;
    public IGenericSql Arg11 => _arg11;
    public IGenericSql Arg12 => _arg12;
    public IGenericSql Arg13 => _arg13;
    public IGenericSql Arg14 => _arg14;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9, _arg10, _arg11, _arg12, _arg13, _arg14];
    public string Separator => _separator;

    public FunctionCallNode(string functionName,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13, ISql<T14> arg14)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14)
    {
    }

    public FunctionCallNode(string functionName, string separator,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13, ISql<T14> arg14)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
        _arg10 = arg10; _arg11 = arg11; _arg12 = arg12;
        _arg13 = arg13; _arg14 = arg14;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg10.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg11.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg12.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg13.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg14.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 15 typed arguments: FUNC_NAME(arg1, ..., arg15)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly IGenericSql _arg10;
    private readonly IGenericSql _arg11;
    private readonly IGenericSql _arg12;
    private readonly IGenericSql _arg13;
    private readonly IGenericSql _arg14;
    private readonly IGenericSql _arg15;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql Arg10 => _arg10;
    public IGenericSql Arg11 => _arg11;
    public IGenericSql Arg12 => _arg12;
    public IGenericSql Arg13 => _arg13;
    public IGenericSql Arg14 => _arg14;
    public IGenericSql Arg15 => _arg15;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9, _arg10, _arg11, _arg12, _arg13, _arg14, _arg15];
    public string Separator => _separator;

    public FunctionCallNode(string functionName,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13, ISql<T14> arg14, ISql<T15> arg15)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15)
    {
    }

    public FunctionCallNode(string functionName, string separator,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13, ISql<T14> arg14, ISql<T15> arg15)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
        _arg10 = arg10; _arg11 = arg11; _arg12 = arg12;
        _arg13 = arg13; _arg14 = arg14; _arg15 = arg15;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg10.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg11.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg12.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg13.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg14.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg15.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Function call with 16 typed arguments: FUNC_NAME(arg1, ..., arg16)
/// </summary>
public readonly struct FunctionCallNode<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TReturn> : IFunctionCallNode<TReturn>
{
    private readonly string _functionName;
    private readonly IGenericSql _arg1;
    private readonly IGenericSql _arg2;
    private readonly IGenericSql _arg3;
    private readonly IGenericSql _arg4;
    private readonly IGenericSql _arg5;
    private readonly IGenericSql _arg6;
    private readonly IGenericSql _arg7;
    private readonly IGenericSql _arg8;
    private readonly IGenericSql _arg9;
    private readonly IGenericSql _arg10;
    private readonly IGenericSql _arg11;
    private readonly IGenericSql _arg12;
    private readonly IGenericSql _arg13;
    private readonly IGenericSql _arg14;
    private readonly IGenericSql _arg15;
    private readonly IGenericSql _arg16;
    private readonly string _separator;

    public string FunctionName => _functionName;
    public IGenericSql Arg1 => _arg1;
    public IGenericSql Arg2 => _arg2;
    public IGenericSql Arg3 => _arg3;
    public IGenericSql Arg4 => _arg4;
    public IGenericSql Arg5 => _arg5;
    public IGenericSql Arg6 => _arg6;
    public IGenericSql Arg7 => _arg7;
    public IGenericSql Arg8 => _arg8;
    public IGenericSql Arg9 => _arg9;
    public IGenericSql Arg10 => _arg10;
    public IGenericSql Arg11 => _arg11;
    public IGenericSql Arg12 => _arg12;
    public IGenericSql Arg13 => _arg13;
    public IGenericSql Arg14 => _arg14;
    public IGenericSql Arg15 => _arg15;
    public IGenericSql Arg16 => _arg16;
    public IGenericSql[] Arguments => [_arg1, _arg2, _arg3, _arg4, _arg5, _arg6, _arg7, _arg8, _arg9, _arg10, _arg11, _arg12, _arg13, _arg14, _arg15, _arg16];
    public string Separator => _separator;

    public FunctionCallNode(string functionName,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13, ISql<T14> arg14, ISql<T15> arg15,
        ISql<T16> arg16)
        : this(functionName, ", ", arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16)
    {
    }

    public FunctionCallNode(string functionName, string separator,
        ISql<T1> arg1, ISql<T2> arg2, ISql<T3> arg3, ISql<T4> arg4, ISql<T5> arg5,
        ISql<T6> arg6, ISql<T7> arg7, ISql<T8> arg8, ISql<T9> arg9, ISql<T10> arg10,
        ISql<T11> arg11, ISql<T12> arg12, ISql<T13> arg13, ISql<T14> arg14, ISql<T15> arg15,
        ISql<T16> arg16)
    {
        _functionName = functionName;
        _separator = separator;
        _arg1 = arg1; _arg2 = arg2; _arg3 = arg3;
        _arg4 = arg4; _arg5 = arg5; _arg6 = arg6;
        _arg7 = arg7; _arg8 = arg8; _arg9 = arg9;
        _arg10 = arg10; _arg11 = arg11; _arg12 = arg12;
        _arg13 = arg13; _arg14 = arg14; _arg15 = arg15;
        _arg16 = arg16;
    }

    public void BuildSql(ISqlBuilder sqlBuilder)
    {
        sqlBuilder.Append(_functionName).Append('(');
        _arg1.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg2.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg3.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg4.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg5.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg6.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg7.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg8.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg9.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg10.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg11.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg12.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg13.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg14.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg15.BuildSql(sqlBuilder);
        sqlBuilder.Append(_separator); _arg16.BuildSql(sqlBuilder);
        sqlBuilder.Append(')');
    }
}

/// <summary>
/// Extension methods for creating function calls with mixed ISql and value arguments.
/// </summary>
public static class FunctionCallExtensions
{
    public static FunctionCallNode<TReturn> ToFunction<TReturn>(this IGenericSql sql, string functionName) =>
        new(functionName, sql);
}
