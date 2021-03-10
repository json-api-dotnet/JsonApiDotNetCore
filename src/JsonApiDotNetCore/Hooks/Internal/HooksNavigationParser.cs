using System;
using System.Linq.Expressions;
using System.Reflection;

namespace JsonApiDotNetCore.Hooks.Internal
{
    internal sealed class HooksNavigationParser
    {
        /// <summary>
        /// Gets the property info that is referenced in the NavigationAction expression. Credits: https://stackoverflow.com/a/17116267/4441216
        /// </summary>
        public static PropertyInfo ParseNavigationExpression<TResource>(Expression<Func<TResource, object>> navigationExpression)
        {
            ArgumentGuard.NotNull(navigationExpression, nameof(navigationExpression));

            MemberExpression exp;

            // this line is necessary, because sometimes the expression comes in as Convert(originalExpression)
            if (navigationExpression.Body is UnaryExpression unaryExpression)
            {
                if (unaryExpression.Operand is MemberExpression memberExpression)
                {
                    exp = memberExpression;
                }
                else
                {
                    throw new ArgumentException("Invalid expression.", nameof(navigationExpression));
                }
            }
            else if (navigationExpression.Body is MemberExpression memberExpression)
            {
                exp = memberExpression;
            }
            else
            {
                throw new ArgumentException("Invalid expression.", nameof(navigationExpression));
            }

            return (PropertyInfo)exp.Member;
        }
    }
}
