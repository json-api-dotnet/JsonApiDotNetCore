using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a custom filter function
    /// </summary>
    public class CustomExpression : FilterExpression
    {
        public string Operator { get; }
        public IReadOnlyCollection<QueryExpression> Arguments { get; }

        public CustomExpression(string @operator, IReadOnlyCollection<QueryExpression> arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            Operator = @operator;
            Arguments = arguments;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.DefaultVisit(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Operator.ToString().Camelize());
            builder.Append('(');
            builder.Append(string.Join(",", Arguments.Select(term => term.ToString())));
            builder.Append(')');

            return builder.ToString();
        }
    }
}
