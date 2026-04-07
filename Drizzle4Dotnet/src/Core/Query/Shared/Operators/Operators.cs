using Drizzle4Dotnet.Core.Query.Shared.Operators.Nodes;
using Drizzle4Dotnet.Core.Shared;

namespace Drizzle4Dotnet.Core.Query.Shared.Operators;

public static class Operators
{
    const string _operatorEq = "=";
    const string _operatorLt = "<";
    const string _operatorGt = ">";
    const string _operatorLtEq = "<=";
    const string _operatorGtEq = ">=";
    const string _operatorNe = "<>";
    const string _operatorAnd = "AND";
    const string _operatorOr = "OR";
    const string _operatorXor = "XOR";
    const string _operatorLike = "LIKE";
    const string _operatorNotLike = "NOT LIKE";
    
    const string _operatorIn = "IN";
    const string _operatorNotIn = "NOT IN";
    const string _operatorIsNull = "IS NULL";
    const string _operatorIsNotNull = "IS NOT NULL";
    const string _operatorBetween = "BETWEEN";
    const string _operatorNot = "NOT";
    
    public static BinaryValueNode<T> Eq<T>(ISql<T> col, T value) => new(col, value, _operatorEq);
    public static BinaryValueNode<T> Lt<T>(ISql<T> col, T value) => new(col, value, _operatorLt);
    public static BinaryValueNode<T> Gt<T>(ISql<T> col, T value) => new(col, value, _operatorGt);
    public static BinaryValueNode<T> Ltq<T>(ISql<T> col, T value) => new(col, value, _operatorLtEq);
    public static BinaryValueNode<T> Gtq<T>(ISql<T> col, T value) => new(col, value, _operatorGtEq);
    public static BinaryValueNode<T> Ne<T>(ISql<T> col, T value) => new(col, value, _operatorNe);

    public static BinarySqlNode<T1, T2> Eq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorEq);
    public static BinarySqlNode<T1, T2> Lt<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorLt);
    public static BinarySqlNode<T1, T2> Gt<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorGt);
    public static BinarySqlNode<T1, T2> Ltq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorLtEq);
    public static BinarySqlNode<T1, T2> Gtq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorGtEq);
    public static BinarySqlNode<T1, T2> Ne<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorNe);

    public static BinaryNode Eq(IOperator c1, IOperator c2) => new(c1, c2, _operatorEq);
    public static BinaryNode Lt(IOperator c1, IOperator c2) => new(c1, c2, _operatorLt);
    public static BinaryNode Gt(IOperator c1, IOperator c2) => new(c1, c2, _operatorGt);
    public static BinaryNode Ltq(IOperator c1, IOperator c2) => new(c1, c2, _operatorLtEq);
    public static BinaryNode Gtq(IOperator c1, IOperator c2) => new(c1, c2, _operatorGtEq);
    public static BinaryNode Ne(IOperator c1, IOperator c2) => new(c1, c2, _operatorNe);

    public static BinaryNode And(IOperator c1, IOperator c2) => new(c1, c2, _operatorAnd);
    public static BinaryNode Or(IOperator c1, IOperator c2) => new(c1, c2, _operatorOr);
    public static BinaryNode Xor(IOperator c1, IOperator c2) => new(c1, c2, _operatorXor);

    public static BinarySqlNode<T1, T2> And<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorAnd);
    public static BinarySqlNode<T1, T2> Or<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorOr);
    public static BinarySqlNode<T1, T2> Xor<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorXor);
    
    public static BinaryValueNode<string> Like(ISql<string> col, string value) => new(col, value, _operatorLike);
    public static BinaryValueNode<string> NotLike(ISql<string> col, string value) => new(col, value, _operatorNotLike);
    public static BinaryValueNode<string> Contains(ISql<string> col, string value) 
        => new(col, $"%{value}%", _operatorLike);
    public static BinaryValueNode<string> StartsWith(ISql<string> col, string value) 
        => new(col, $"{value}%", _operatorLike);
    public static BinaryValueNode<string> EndsWith(ISql<string> col, string value) 
        => new(col, $"%{value}", _operatorLike);
    
    public static UnarySqlNode<T> IsNull<T>(ISql<T> col) => new(col, _operatorIsNull);
    public static UnarySqlNode<T> IsNotNull<T>(ISql<T> col) => new(col, _operatorIsNotNull);
    public static UnaryNode IsNull(IOperator condition) => new(condition, _operatorIsNull, prefix: true);
    public static UnaryNode IsNotNull(IOperator condition) => new(condition, _operatorIsNotNull, prefix: true);

    public static BinaryListValueNode<T> In<T>(ISql<T> col, IEnumerable<T> values) => new(col, values, _operatorIn);
    public static BinaryListValueNode<T> NotIn<T>(ISql<T> col, IEnumerable<T> values) => new(col, values, _operatorNotIn);

    public static UnaryNode Not(IOperator condition) => new(condition, _operatorNot, prefix: true);
    public static UnarySqlNode<T> Not<T>(ISql<T> condition) => new(condition, _operatorNot, prefix: true);
    
    public static BinarySqlNode<T, T> Add<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "+");
    public static BinarySqlNode<T, T> Sub<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "-");
    public static BinarySqlNode<T, T> Mul<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "*");
    public static BinarySqlNode<T, T> Div<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "/");
    
    public static UnarySqlNode<T> Exists<T>(ISql<T> subquery) => new(subquery, "EXISTS", prefix: true);
}