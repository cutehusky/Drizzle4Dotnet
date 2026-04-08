using Drizzle4Dotnet.Core.Schema.Columns;
using Drizzle4Dotnet.Core.Schema.Tables;
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
    const string _operatorAnd = " AND ";
    const string _operatorOr = " OR ";
    const string _operatorXor = " XOR ";
    const string _operatorLike = " LIKE ";
    const string _operatorNotLike = " NOT LIKE ";
    
    const string _operatorIn = " IN ";
    const string _operatorNotIn = " NOT IN ";
    const string _operatorIsNull = " IS NULL ";
    const string _operatorIsNotNull = " IS NOT NULL ";
    const string _operatorBetween = " BETWEEN ";
    const string _operatorNotBetween = " NOT BETWEEN ";
    const string _operatorNot = " NOT ";
    const string _operatorConcat = " || ";
    const  string _operatorAdd = " + ";
    const string _operatorSub = " - ";
    const string _operatorMul = " * ";
    const string _operatorDiv = " / ";
    const string  _operatorMod = " % ";
    const string _operatorExists = " EXISTS ";
    
    public static BinarySqlValueNode<T, bool> Eq<T>(ISql<T> c1, T value) => new(c1, value, _operatorEq);
    public static BinarySqlValueNode<T, bool> Lt<T>(ISql<T> c1, T value) => new(c1, value, _operatorLt);
    public static BinarySqlValueNode<T, bool> Gt<T>(ISql<T> c1, T value) => new(c1, value, _operatorGt);
    public static BinarySqlValueNode<T, bool> Ltq<T>(ISql<T> c1, T value) => new(c1, value, _operatorLtEq);
    public static BinarySqlValueNode<T, bool> Gtq<T>(ISql<T> c1, T value) => new(c1, value, _operatorGtEq);
    public static BinarySqlValueNode<T, bool> Ne<T>(ISql<T> c1, T value) => new(c1, value, _operatorNe);
    
    
    
    public static BinarySqlValueNode<T, bool> Eq<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, value, _operatorEq);
    public static BinarySqlValueNode<T, bool> Lt<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T value) where TDialect : ISqlDialect where TTable : ITable<TDialect> => new(c1, value, _operatorLt);
    public static BinarySqlValueNode<T, bool> Gt<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T value) where TDialect : ISqlDialect where TTable : ITable<TDialect> => new(c1, value, _operatorGt);
    public static BinarySqlValueNode<T, bool> Ltq<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, value, _operatorLtEq);
    public static BinarySqlValueNode<T, bool> Gtq<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T value) where TDialect : ISqlDialect where TTable : ITable<TDialect> => new(c1, value, _operatorGtEq);
    public static BinarySqlValueNode<T, bool> Ne<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, value, _operatorNe);

    
    
    public static BinaryNode<T1, T2, bool> Eq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorEq);
    public static BinaryNode<T1, T2, bool> Lt<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorLt);
    public static BinaryNode<T1, T2, bool> Gt<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorGt);
    public static BinaryNode<T1, T2, bool> Ltq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorLtEq);
    public static BinaryNode<T1, T2, bool> Gtq<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorGtEq);
    public static BinaryNode<T1, T2, bool> Ne<T1, T2>(ISql<T1> c1, ISql<T2> c2) => new(c1, c2, _operatorNe);
    
    
    
    public static BinaryNode<T1, T2, bool> Eq<T1, T2, TTable1, TDialect1, TTable2>(this DbColumn<T1, TTable1, TDialect1> c1, DbColumn<T2, TTable2, TDialect1> c2) where TTable1 : ITable<TDialect1> where TDialect1 : ISqlDialect where TTable2 : ITable<TDialect1> => new(c1, c2, _operatorEq);
    public static BinaryNode<T1, T2, bool> Lt<T1, T2, TTable1, TDialect1, TTable2>(this DbColumn<T1, TTable1, TDialect1> c1, DbColumn<T2, TTable2, TDialect1> c2) where TTable1 : ITable<TDialect1> where TDialect1 : ISqlDialect where TTable2 : ITable<TDialect1> => new(c1, c2, _operatorLt);
    public static BinaryNode<T1, T2, bool> Gt<T1, T2, TTable1, TDialect1, TTable2>(this DbColumn<T1, TTable1, TDialect1> c1, DbColumn<T2, TTable2, TDialect1> c2) where TTable1 : ITable<TDialect1> where TDialect1 : ISqlDialect where TTable2 : ITable<TDialect1> => new(c1, c2, _operatorGt);
    public static BinaryNode<T1, T2, bool> Ltq<T1, T2, TTable1, TDialect1, TTable2>(this DbColumn<T1, TTable1, TDialect1> c1, DbColumn<T2, TTable2, TDialect1> c2) where TTable1 : ITable<TDialect1> where TDialect1 : ISqlDialect where TTable2 : ITable<TDialect1> => new(c1, c2, _operatorLtEq);
    public static BinaryNode<T1, T2, bool> Gtq<T1, T2, TTable1, TDialect1, TTable2>(this DbColumn<T1, TTable1, TDialect1> c1, DbColumn<T2, TTable2, TDialect1> c2) where TTable1 : ITable<TDialect1> where TDialect1 : ISqlDialect where TTable2 : ITable<TDialect1> => new(c1, c2, _operatorGtEq);
    public static BinaryNode<T1, T2, bool> Ne<T1, T2, TTable1, TDialect1, TTable2>(this DbColumn<T1, TTable1, TDialect1> c1, DbColumn<T2, TTable2, TDialect1> c2) where TTable1 : ITable<TDialect1> where TDialect1 : ISqlDialect where TTable2 : ITable<TDialect1> => new(c1, c2, _operatorNe);

    
    
    public static BinaryNode<bool> And(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, _operatorAnd);
    public static BinaryNode<bool> Or(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, _operatorOr);
    public static BinaryNode<bool> Xor(ISql<bool> c1, ISql<bool> c2) => new(c1, c2, _operatorXor);
    public static NnaryNode<bool, bool> And<T>(params ISql<bool>[] conditions) => new(conditions,  _operatorAnd);
    public static NnaryNode<bool, bool> Or<T>(params ISql<bool>[] conditions) => new(conditions, _operatorOr);
    public static NnaryNode<bool, bool> Xor<T>(params ISql<bool>[] conditions) => new(conditions, _operatorXor);
    
    
    
    public static BinarySqlValueNode<string, bool> Like(ISql<string> c1, string value) => new(c1, value, _operatorLike);
    public static BinarySqlValueNode<string, bool> NotLike(ISql<string> c1, string value) => new(c1, value, _operatorNotLike);
    public static BinaryNode<string, string, bool> Like(ISql<string> c1, ISql<string> value) => new(c1, value, _operatorLike);
    public static BinaryNode<string, string, bool> NotLike(ISql<string> c1, ISql<string> value) => new(c1, value, _operatorNotLike);
    public static BinarySqlValueNode<string, string> Contains(ISql<string> c1, string value) 
        => new(c1, $"%{value}%", _operatorLike);
    public static BinarySqlValueNode<string, bool> StartsWith(ISql<string> c1, string value) 
        => new(c1, $"{value}%", _operatorLike);
    public static BinarySqlValueNode<string, bool> EndsWith(ISql<string> c1, string value) 
        => new(c1, $"%{value}", _operatorLike);
    
    
    
    public static BinarySqlValueNode<string, bool> Like<TTable, TDialect>(this DbColumn<string, TTable, TDialect> c1, string value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, value, _operatorLike);
    public static BinarySqlValueNode<string, bool> NotLike<TTable, TDialect>(this DbColumn<string, TTable, TDialect> c1, string value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, value, _operatorNotLike);
    public static BinaryNode<string, string, bool> Like<TTable, TDialect>(this DbColumn<string, TTable, TDialect> c1, ISql<string> value) where TDialect : ISqlDialect where TTable : ITable<TDialect> => new(c1, value, _operatorLike);
    public static BinaryNode<string, string, bool> NotLike<TTable, TDialect>(this DbColumn<string, TTable, TDialect> c1, ISql<string> value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, value, _operatorNotLike);
    public static BinarySqlValueNode<string, string> Contains<TTable, TDialect>(this DbColumn<string, TTable, TDialect> c1, string value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, $"%{value}%", _operatorLike);
    public static BinarySqlValueNode<string, bool> StartsWith<TTable, TDialect>(this DbColumn<string, TTable, TDialect> c1, string value) where TDialect : ISqlDialect where TTable : ITable<TDialect> => new(c1, $"{value}%", _operatorLike);
    public static BinarySqlValueNode<string, bool> EndsWith<TTable, TDialect>(this DbColumn<string, TTable, TDialect> c1, string value) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, $"%{value}", _operatorLike);

    
    
    public static UnaryNode<T, bool> IsNull<T>(ISql<T> c1) => new(c1, _operatorIsNull);
    public static UnaryNode<T, bool> IsNotNull<T>(ISql<T> c1) => new(c1, _operatorIsNotNull);
    
    
    
    public static UnaryNode<T, bool> IsNull<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, _operatorIsNull);
    public static UnaryNode<T, bool> IsNotNull<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1) where TTable : ITable<TDialect> where TDialect : ISqlDialect => new(c1, _operatorIsNotNull);
    
    
    
    public static BinarySqlListValueNode<T, bool> In<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, _operatorIn);
    public static BinarySqlListValueNode<T, bool> NotIn<T>(ISql<T> c1, IEnumerable<T> values) => new(c1, values, _operatorNotIn);
    public static BinaryNode<T, T, bool> In<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, _operatorIn, true);
    public static BinaryNode<T, T, bool> NotIn<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, _operatorNotIn, true);
    public static NnaryAnyNode<T, bool, TDialect> In<T, TDialect>(ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, _operatorIn);
    public static NnaryAnyNode<T, bool, TDialect> NotIn<T, TDialect>(ISql<T> c1, params SqlValue<T, TDialect>[] node) where TDialect : ISqlDialect => new(c1, node, _operatorNotIn);
    
    
    
    public static BinarySqlListValueNode<T, bool> In<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, IEnumerable<T> values)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, values, _operatorIn);
    public static BinarySqlListValueNode<T, bool> NotIn<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, IEnumerable<T> values)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, values, _operatorNotIn);
    public static BinaryNode<T, T, bool> In<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, ISql<T> c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorIn, true);
    public static BinaryNode<T, T, bool> NotIn<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, ISql<T> c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorNotIn, true);
    public static NnaryAnyNode<T, bool, TDialect> In<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, params SqlValue<T, TDialect>[] node)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, node, _operatorIn);
    public static NnaryAnyNode<T, bool, TDialect> NotIn<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, params SqlValue<T, TDialect>[] node)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, node, _operatorNotIn);
    
    
    
    public static UnaryNode<bool> Not(ISql<bool> condition) => new(condition, _operatorNot, prefix: true);
    
    
    
    public static BinaryNode<T> Add<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, _operatorAdd);
    public static BinaryNode<T> Sub<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, _operatorSub);
    public static BinaryNode<T> Mul<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, _operatorMul);
    public static BinaryNode<T> Div<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, _operatorDiv);
    public static BinaryNode<T> Mod<T>(ISql<T> c1, ISql<T> c2) => new(c1, c2, _operatorMod);
    
    
    
    public static BinarySqlValueNode<T, T> Add<T>(ISql<T> c1, T c2) => new(c1, c2, _operatorAdd);
    public static BinarySqlValueNode<T, T> Sub<T>(ISql<T> c1, T c2) => new(c1, c2, _operatorSub);
    public static BinarySqlValueNode<T, T> Mul<T>(ISql<T> c1, T c2) => new(c1, c2, _operatorMul);
    public static BinarySqlValueNode<T, T> Div<T>(ISql<T> c1, T c2) => new(c1, c2, _operatorDiv);
    public static BinarySqlValueNode<T, T> Mod<T>(ISql<T> c1, T c2) => new(c1, c2, _operatorMod);
    
    
    
    public static BinaryNode<T> Add<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, ISql<T> c2)
    where TTable : ITable<TDialect> where TDialect : ISqlDialect
    => new(c1, c2, _operatorAdd);
    public static BinaryNode<T> Sub<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, ISql<T> c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorSub);
    public static BinaryNode<T> Mul<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, ISql<T> c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorMul);
    public static BinaryNode<T> Div<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, ISql<T> c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorDiv);
    public static BinaryNode<T> Mod<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, ISql<T> c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorMod);

    
    
    public static BinarySqlValueNode<T, T> Add<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorAdd);
    public static BinarySqlValueNode<T, T> Sub<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorSub);
    public static BinarySqlValueNode<T, T> Mul<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorMul);
    public static BinarySqlValueNode<T, T> Div<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorDiv);
    public static BinarySqlValueNode<T, T> Mod<T, TTable, TDialect>(this DbColumn<T, TTable, TDialect> c1, T c2)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, c2, _operatorMod);
    
    
    
    public static BinaryNode<string> Concat(ISql<string> c1, ISql<string> c2) => new(c1, c2, _operatorConcat);
    public static BinarySqlValueNode<string, string> Concat(ISql<string> c1, string value) => new(c1, value, _operatorConcat);
    
    
    
    public static BinaryNode<string> Concat<TTable, TDialect>(
        this DbColumn<string, TTable, TDialect> c1, 
        ISql<string> c2)
        where TTable : ITable<TDialect> 
        where TDialect : ISqlDialect
        => new(c1, c2, _operatorConcat);
    public static BinarySqlValueNode<string, string> Concat<TTable, TDialect>(
        this DbColumn<string, TTable, TDialect> c1, 
        string value)
        where TTable : ITable<TDialect> 
        where TDialect : ISqlDialect
        => new(c1, value, _operatorConcat);
    
    
    
    public static UnaryNode<T, bool> Exists<T>(ISql<T> subquery)  => new(subquery, _operatorExists, prefix: true);
    
    
    
    public static TrinaryNode<T, bool> Between<T>(ISql<T> c1, ISql<T> lower, ISql<T> upper) => new(c1, lower, upper, _operatorBetween, _operatorAnd);
    public static TrinaryNodeV2Val<T, bool> Between<T>(ISql<T> c1, T lower, ISql<T> upper) => new(c1, lower, upper, _operatorBetween,_operatorAnd);
    public static TrinaryNodeV3Val<T, bool> Between<T>(ISql<T> c1, ISql<T> lower, T upper) => new(c1, lower, upper, _operatorBetween,_operatorAnd);
    public static TrinaryNodeV23Val<T, bool> Between<T>(ISql<T> c1, T lower, T upper) => new(c1, lower, upper, _operatorBetween,_operatorAnd);
     
    
    
    public static TrinaryNode<T, bool> NotBetween<T>(ISql<T> c1, ISql<T> lower, ISql<T> upper) => new(c1, lower, upper, _operatorNotBetween, _operatorAnd);
    public static TrinaryNodeV2Val<T, bool> NotBetween<T>(ISql<T> c1, T lower, ISql<T> upper) => new(c1, lower, upper, _operatorNotBetween,_operatorAnd);
    public static TrinaryNodeV3Val<T, bool> NotBetween<T>(ISql<T> c1, ISql<T> lower, T upper) => new(c1, lower, upper, _operatorNotBetween,_operatorAnd);
    public static TrinaryNodeV23Val<T, bool> NotBetween<T>(ISql<T> c1, T lower, T upper) => new(c1, lower, upper, _operatorNotBetween,_operatorAnd);
    
    
    
    public static TrinaryNode<T, bool> Between<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, ISql<T> lower, ISql<T> upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorBetween, _operatorAnd);
    public static TrinaryNodeV2Val<T, bool> Between<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, T lower, ISql<T> upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorBetween, _operatorAnd);
    public static TrinaryNodeV3Val<T, bool> Between<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, ISql<T> lower, T upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorBetween, _operatorAnd);
    public static TrinaryNodeV23Val<T, bool> Between<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, T lower, T upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorBetween, _operatorAnd);
    
    
    
    public static TrinaryNode<T, bool> NotBetween<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, ISql<T> lower, ISql<T> upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorNotBetween, _operatorAnd);
    public static TrinaryNodeV2Val<T, bool> NotBetween<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, T lower, ISql<T> upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorNotBetween, _operatorAnd);
    public static TrinaryNodeV3Val<T, bool> NotBetween<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, ISql<T> lower, T upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorNotBetween, _operatorAnd);
    public static TrinaryNodeV23Val<T, bool> NotBetween<T, TTable, TDialect>(
        this DbColumn<T, TTable, TDialect> c1, T lower, T upper)
        where TTable : ITable<TDialect> where TDialect : ISqlDialect
        => new(c1, lower, upper, _operatorNotBetween, _operatorAnd);
}