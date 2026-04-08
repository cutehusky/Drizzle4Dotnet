using Drizzle4Dotnet.Core.Shared.Operators.Nodes;

namespace Drizzle4Dotnet.Core.Shared.Operators;
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
    
    public static BinarySqlValueNode<T, bool> Eq<T>(ISql<T> c1, T value) => new(c1, value, _operatorEq);
    public static BinarySqlValueNode<T, bool> Lt<T>(ISql<T> c1, T value) => new(c1, value, _operatorLt);
    public static BinarySqlValueNode<T, bool> Gt<T>(ISql<T> c1, T value) => new(c1, value, _operatorGt);
    public static BinarySqlValueNode<T, bool> Ltq<T>(ISql<T> c1, T value) => new(c1, value, _operatorLtEq);
    public static BinarySqlValueNode<T, bool> Gtq<T>(ISql<T> c1, T value) => new(c1, value, _operatorGtEq);
    public static BinarySqlValueNode<T, bool> Ne<T>(ISql<T> c1, T value) => new(c1, value, _operatorNe);
    
    public static BinaryNode<T1, T2, bool> Eq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorEq);
    

    public static BinaryNode<bool> And(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, _operatorAnd);
    public static BinaryNode<bool> Or(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, _operatorOr);
    public static BinaryNode<bool> Xor(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, _operatorXor);
    
    public static BinarySqlValueNode<string, bool> Like(ISql<string> c1, string value) => new(c1, value, _operatorLike);
    public static BinarySqlValueNode<string, bool> NotLike(ISql<string> c1, string value) => new(c1, value, _operatorNotLike);
    public static BinarySqlValueNode<string, string> Contains(ISql<string> c1, string value) 
        => new(c1, $"%{value}%", _operatorLike);
    public static BinarySqlValueNode<string, bool> StartsWith(ISql<string> c1, string value) 
        => new(c1, $"{value}%", _operatorLike);
    public static BinarySqlValueNode<string, bool> EndsWith(ISql<string> c1, string value) 
        => new(c1, $"%{value}", _operatorLike);

    public static UnaryNode<T, bool> IsNull<T>(ISql<T> c1) => new(c1, _operatorIsNull);
    public static UnaryNode<T, bool> IsNotNull<T>(ISql<T> c1) => new(c1, _operatorIsNotNull);
   
    public static BinarySqlListValueNode<T, bool> In<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, _operatorIn);
    public static BinarySqlListValueNode<T, bool> NotIn<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, _operatorNotIn);
    
    public static BinaryNode<T1, T2, bool> In<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorIn, true);
    public static BinaryNode<T1, T2, bool> NotIn<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorNotIn, true);

    public static UnaryNode<bool> Not(ISql<bool> condition) => new(condition, _operatorNot, prefix: true);
    
    public static BinaryNode<T> Add<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "+");
    public static BinaryNode<T> Sub<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "-");
    public static BinaryNode<T> Mul<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "*");
    public static BinaryNode<T> Div<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "/");
    public static BinaryNode<T> Mod<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, "%");
    
    public static BinaryNode<string> Concat(ISql<string> c1, ISql<string> c2) => new(c1, c2, "||");
    public static BinarySqlValueNode<string, string> Concat(ISql<string> c1, string value) => new(c1, value, "||");
    
    public static UnaryNode<T, bool> Exists<T>(ISql<T> subquery)  => new(subquery, "EXISTS", prefix: true);
}