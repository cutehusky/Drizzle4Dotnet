using Drizzle4Dotnet.Core.Shared.Operators.Nodes;
using Drizzle4Dotnet.Core.Query;

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
    
    public static BinarySqlValueNode<T> Eq<T>(ISql<T> c1, T value) => new(c1, value, _operatorEq);
    public static BinarySqlValueNode<T> Lt<T>(ISql<T> c1, T value) => new(c1, value, _operatorLt);
    public static BinarySqlValueNode<T> Gt<T>(ISql<T> c1, T value) => new(c1, value, _operatorGt);
    public static BinarySqlValueNode<T> Ltq<T>(ISql<T> c1, T value) => new(c1, value, _operatorLtEq);
    public static BinarySqlValueNode<T> Gtq<T>(ISql<T> c1, T value) => new(c1, value, _operatorGtEq);
    public static BinarySqlValueNode<T> Ne<T>(ISql<T> c1, T value) => new(c1, value, _operatorNe);
    
    public static BinaryNode Eq(ISql c1, ISql c2) => new(c1, c2, _operatorEq);
    public static BinaryNode Lt(ISql c1, ISql c2) => new(c1, c2, _operatorLt);
    public static BinaryNode Gt(ISql c1, ISql c2) => new(c1, c2, _operatorGt);
    public static BinaryNode Ltq(ISql c1, ISql c2) => new(c1, c2, _operatorLtEq);
    public static BinaryNode Gtq(ISql c1, ISql c2) => new(c1, c2, _operatorGtEq);
    public static BinaryNode Ne(ISql c1, ISql c2) => new(c1, c2, _operatorNe);

    public static BinaryNode And(ISql c1, ISql c2) => new(c1, c2, _operatorAnd);
    public static BinaryNode Or(ISql c1, ISql c2) => new(c1, c2, _operatorOr);
    public static BinaryNode Xor(ISql c1, ISql c2) => new(c1, c2, _operatorXor);
    
    public static BinarySqlValueNode<string> Like(ISql<string> c1, string value) => new(c1, value, _operatorLike);
    public static BinarySqlValueNode<string> NotLike(ISql<string> c1, string value) => new(c1, value, _operatorNotLike);
    public static BinarySqlValueNode<string> Contains(ISql<string> c1, string value) 
        => new(c1, $"%{value}%", _operatorLike);
    public static BinarySqlValueNode<string> StartsWith(ISql<string> c1, string value) 
        => new(c1, $"{value}%", _operatorLike);
    public static BinarySqlValueNode<string> EndsWith(ISql<string> c1, string value) 
        => new(c1, $"%{value}", _operatorLike);
    
    public static UnaryNode IsNull(ISql c1) => new(c1, _operatorIsNull);
    public static UnaryNode IsNotNull(ISql c1) => new(c1, _operatorIsNotNull);
   
    public static BinarySqlListValueNode<T> In<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, _operatorIn);
    public static BinarySqlListValueNode<T> NotIn<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, _operatorNotIn);
    
    public static BinaryNode In(ISql c1, ISql c2) => new(c1, c2, _operatorIn);
    public static BinaryNode NotIn(ISql c1, ISql c2) => new(c1, c2, _operatorNotIn);

    public static UnaryNode Not(ISql condition) => new(condition, _operatorNot, prefix: true);
    
    public static BinaryNode Add(ISql c1, ISql c2) => new(c1, c2, "+");
    public static BinaryNode Sub(ISql c1, ISql c2) => new(c1, c2, "-");
    public static BinaryNode Mul(ISql c1, ISql c2) => new(c1, c2, "*");
    public static BinaryNode Div(ISql c1, ISql c2) => new(c1, c2, "/");
    public static BinaryNode Mod(ISql c1, ISql c2) => new(c1, c2, "%");
    
    public static BinaryNode Concat(ISql c1, ISql c2) => new(c1, c2, "||");
    public static BinarySqlValueNode<string> Concat(ISql<string> c1, string value) => new(c1, value, "||");
    public static UnaryNode Exists<TDialect>(Query<TDialect> subquery) where TDialect: ISqlDialect => new(subquery, "EXISTS", prefix: true);
}