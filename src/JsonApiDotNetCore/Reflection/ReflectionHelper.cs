using JsonApiDotNetCore.Models.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Reflection
{
    public static class ReflectionHelper
    {        
        public static Member GetMember<TModel, TReturn>(Expression<Func<TModel, TReturn>> expression)
        {
            return GetMember(expression.Body);
        }

        private static bool IsMethodExpression(Expression expression)
        {
            return expression is MethodCallExpression || (expression is UnaryExpression && IsMethodExpression((expression as UnaryExpression).Operand));
        }

        private static Member GetMember(Expression expression)
        {            
            var memberExpression = GetMemberExpression(expression);

            return memberExpression.Member.ToMember();
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            return GetMemberExpression(expression, true);
        }

        private static MemberExpression GetMemberExpression(Expression expression, bool enforceCheck)
        {
            MemberExpression memberExpression = null;

            if (expression.NodeType == ExpressionType.Convert)
            {
                var body = (UnaryExpression)expression;
                memberExpression = body.Operand as MemberExpression;
            }
            else if (expression.NodeType == ExpressionType.MemberAccess)
            {
                memberExpression = expression as MemberExpression;
            }

            if (enforceCheck && memberExpression == null)
            {
                throw new ArgumentException("Not a member access", "expression");
            }

            return memberExpression;
        }        
    }
}
