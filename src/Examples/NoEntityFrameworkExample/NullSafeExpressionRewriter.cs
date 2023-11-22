using System.Linq.Expressions;
using System.Reflection;

namespace NoEntityFrameworkExample;

/// <summary>
/// Inserts a null check on member dereference and extension method invocation, to prevent a <see cref="NullReferenceException" /> from being thrown when
/// the expression is compiled and executed.
/// </summary>
/// For example,
/// <code><![CDATA[
/// Database.TodoItems.Where(todoItem => todoItem.Assignee.Id == todoItem.Owner.Id)
/// ]]> </code>
/// would throw if the database contains a
/// TodoItem that doesn't have an assignee.
/// <example></example>
public sealed class NullSafeExpressionRewriter : ExpressionVisitor
{
    private const string MinValueName = nameof(long.MinValue);
    private static readonly ConstantExpression Int32MinValueConstant = Expression.Constant(int.MinValue, typeof(int));

    private static readonly ExpressionType[] ComparisonExpressionTypes =
    [
        ExpressionType.LessThan,
        ExpressionType.LessThanOrEqual,
        ExpressionType.GreaterThan,
        ExpressionType.GreaterThanOrEqual,
        ExpressionType.Equal
        // ExpressionType.NotEqual is excluded because WhereClauseBuilder never produces that.
    ];

    private readonly Stack<MethodType> _callStack = new();

    public TExpression Rewrite<TExpression>(TExpression expression)
        where TExpression : Expression
    {
        _callStack.Clear();

        return (TExpression)Visit(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.Name == "Where")
        {
            _callStack.Push(MethodType.Where);
            Expression expression = base.VisitMethodCall(node);
            _callStack.Pop();
            return expression;
        }

        if (node.Method.Name is "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending")
        {
            // Ordering can be improved by expanding into multiple OrderBy/ThenBy() calls, as described at
            // https://stackoverflow.com/questions/26186527/linq-order-by-descending-with-null-values-on-bottom/26186585#26186585.
            // For example:
            //   .OrderBy(element => element.First.Second.CharValue)
            // Could be translated to:
            //   .OrderBy(element => element.First != null)
            //   .ThenBy(element => element.First == null ? false : element.First.Second != null)
            //   .ThenBy(element => element.First == null ? '\0' : element.First.Second == null ? '\0' : element.First.Second.CharValue)
            // Which correctly orders 'element.First == null' before 'element.First.Second == null'.
            // The current implementation translates to:
            //   .OrderBy(element => element.First == null ? '\0' : element.First.Second == null ? '\0' : element.First.Second.CharValue)
            // in which the order of these two rows is undeterministic.

            _callStack.Push(MethodType.Ordering);
            Expression expression = base.VisitMethodCall(node);
            _callStack.Pop();
            return expression;
        }

        if (_callStack.Count > 0)
        {
            MethodType outerMethodType = _callStack.Peek();

            if (outerMethodType == MethodType.Ordering && node.Method.Name == "Count")
            {
                return ToNullSafeCountInvocationInOrderBy(node);
            }

            if (outerMethodType == MethodType.Where && node.Method.Name == "Any")
            {
                return ToNullSafeAnyInvocationInWhere(node);
            }
        }

        return base.VisitMethodCall(node);
    }

    private static Expression ToNullSafeCountInvocationInOrderBy(MethodCallExpression countMethodCall)
    {
        Expression thisArgument = countMethodCall.Arguments.Single();

        if (thisArgument is MemberExpression memberArgument)
        {
            // OrderClauseBuilder never produces nested Count() calls.

            // SRC: some.Other.Children.Count()
            // DST: some.Other == null ? int.MinValue : some.Other.Children == null ? int.MinValue : some.Other.Children.Count()
            return ToConditionalMemberAccessInOrderBy(countMethodCall, memberArgument, Int32MinValueConstant);
        }

        return countMethodCall;
    }

    private static Expression ToConditionalMemberAccessInOrderBy(Expression outer, MemberExpression innerMember, ConstantExpression defaultValue)
    {
        MemberExpression? currentMember = innerMember;
        Expression result = outer;

        do
        {
            // Static property/field invocations can never be null (though unlikely we'll ever encounter those).
            if (!IsStaticMemberAccess(currentMember))
            {
                // SRC: first.Second.StringValue
                // DST: first.Second == null ? null : first.Second.StringValue
                ConstantExpression nullConstant = Expression.Constant(null, currentMember.Type);
                BinaryExpression isNull = Expression.Equal(currentMember, nullConstant);
                result = Expression.Condition(isNull, defaultValue, result);
            }

            currentMember = currentMember.Expression as MemberExpression;
        }
        while (currentMember != null);

        return result;
    }

    private static bool IsStaticMemberAccess(MemberExpression member)
    {
        if (member.Member is FieldInfo field)
        {
            return field.IsStatic;
        }

        if (member.Member is PropertyInfo property)
        {
            MethodInfo? getter = property.GetGetMethod();
            return getter != null && getter.IsStatic;
        }

        return false;
    }

    private Expression ToNullSafeAnyInvocationInWhere(MethodCallExpression anyMethodCall)
    {
        Expression thisArgument = anyMethodCall.Arguments.First();

        if (thisArgument is MemberExpression memberArgument)
        {
            MethodCallExpression newAnyMethodCall = anyMethodCall;

            if (anyMethodCall.Arguments.Count > 1)
            {
                // SRC: .Any(first => first.Second.Value == 1)
                // DST: .Any(first => first != null && first.Second != null && first.Second.Value == 1)
                List<Expression> newArguments = anyMethodCall.Arguments.Skip(1).Select(Visit).Cast<Expression>().ToList();
                newArguments.Insert(0, thisArgument);

                newAnyMethodCall = anyMethodCall.Update(anyMethodCall.Object, newArguments);
            }

            // SRC: some.Other.Any()
            // DST: some != null && some.Other != null && some.Other.Any()
            return ToConditionalMemberAccessInBooleanExpression(newAnyMethodCall, memberArgument, false);
        }

        return anyMethodCall;
    }

    private static Expression ToConditionalMemberAccessInBooleanExpression(Expression outer, MemberExpression innerMember, bool skipNullCheckOnLastAccess)
    {
        MemberExpression? currentMember = innerMember;
        Expression result = outer;

        do
        {
            // Null-check the last member access in the chain on extension method invocation. For example: a.b.c.Count() requires a null-check on 'c'.
            // This is unneeded for boolean comparisons. For example: a.b.c == d does not require a null-check on 'c'.
            if (!skipNullCheckOnLastAccess || currentMember != innerMember)
            {
                // Static property/field invocations can never be null (though unlikely we'll ever encounter those).
                if (!IsStaticMemberAccess(currentMember))
                {
                    // SRC: first.Second.Value == 1
                    // DST: first.Second != null && first.Second.Value == 1
                    ConstantExpression nullConstant = Expression.Constant(null, currentMember.Type);
                    BinaryExpression isNotNull = Expression.NotEqual(currentMember, nullConstant);
                    result = Expression.AndAlso(isNotNull, result);
                }
            }

            // Do not null-check the first member access in the chain, because that's the lambda parameter itself.
            // For example, in: item => item.First.Second, 'item' does not require a null-check.
            currentMember = currentMember.Expression as MemberExpression;
        }
        while (currentMember != null);

        return result;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
        if (_callStack.Count > 0 && _callStack.Peek() == MethodType.Where)
        {
            if (ComparisonExpressionTypes.Contains(node.NodeType))
            {
                Expression result = node;

                result = ToNullSafeTermInBinary(node.Right, result);
                result = ToNullSafeTermInBinary(node.Left, result);

                return result;
            }
        }

        return base.VisitBinary(node);
    }

    private static Expression ToNullSafeTermInBinary(Expression binaryTerm, Expression result)
    {
        if (binaryTerm is MemberExpression rightMember)
        {
            // SRC: some.Other.Value == 1
            // DST: some != null && some.Other != null && some.Other.Value == 1
            return ToConditionalMemberAccessInBooleanExpression(result, rightMember, true);
        }

        if (binaryTerm is MethodCallExpression { Method.Name: "Count" } countMethodCall)
        {
            Expression thisArgument = countMethodCall.Arguments.Single();

            if (thisArgument is MemberExpression memberArgument)
            {
                // SRC: some.Other.Count() == 1
                // DST: some != null && some.Other != null && some.Other.Count() == 1
                return ToConditionalMemberAccessInBooleanExpression(result, memberArgument, false);
            }
        }

        return result;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (_callStack.Count > 0 && _callStack.Peek() == MethodType.Ordering)
        {
            if (node.Expression is MemberExpression innerMember)
            {
                ConstantExpression defaultValue = CreateConstantForMemberIsNull(node.Type);
                return ToConditionalMemberAccessInOrderBy(node, innerMember, defaultValue);
            }

            return node;
        }

        return base.VisitMember(node);
    }

    private static ConstantExpression CreateConstantForMemberIsNull(Type type)
    {
        bool canContainNull = !type.IsValueType || Nullable.GetUnderlyingType(type) != null;

        if (canContainNull)
        {
            return Expression.Constant(null, type);
        }

        Type innerType = Nullable.GetUnderlyingType(type) ?? type;
        ConstantExpression? constant = TryCreateConstantForStaticMinValue(innerType);

        if (constant != null)
        {
            return constant;
        }

        object? defaultValue = Activator.CreateInstance(type);
        return Expression.Constant(defaultValue, type);
    }

    private static ConstantExpression? TryCreateConstantForStaticMinValue(Type type)
    {
        // Int32.MinValue is a field, while Int128.MinValue is a property.

        FieldInfo? field = type.GetField(MinValueName, BindingFlags.Public | BindingFlags.Static);

        if (field != null)
        {
            object? value = field.GetValue(null);
            return Expression.Constant(value, type);
        }

        PropertyInfo? property = type.GetProperty(MinValueName, BindingFlags.Public | BindingFlags.Static);

        if (property != null)
        {
            MethodInfo? getter = property.GetGetMethod();

            if (getter != null)
            {
                object? value = getter.Invoke(null, Array.Empty<object>());
                return Expression.Constant(value, type);
            }
        }

        return null;
    }

    private enum MethodType
    {
        Where,
        Ordering
    }
}
