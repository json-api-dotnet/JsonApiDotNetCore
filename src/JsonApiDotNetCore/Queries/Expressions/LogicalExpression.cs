using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a logical filter function, resulting from text such as: and(equals(title,'Work'),has(articles))
    /// </summary>
    [PublicAPI]
    public class LogicalExpression : FilterExpression
    {
        public LogicalOperator Operator { get; }
        public IReadOnlyCollection<QueryExpression> Terms { get; }

        public LogicalExpression(LogicalOperator @operator, IReadOnlyCollection<QueryExpression> terms)
        {
            ArgumentGuard.NotNull(terms, nameof(terms));

            if (terms.Count < 2)
            {
                throw new ArgumentException("At least two terms are required.", nameof(terms));
            }

            Operator = @operator;
            Terms = terms;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitLogical(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(Operator.ToString().Camelize());
            builder.Append('(');
            builder.Append(string.Join(",", Terms.Select(term => term.ToString())));
            builder.Append(')');

            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (LogicalExpression)obj;

            return Operator == other.Operator && Terms.SequenceEqual(other.Terms);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(Operator);

            foreach (QueryExpression term in Terms)
            {
                hashCode.Add(term);
            }

            return hashCode.ToHashCode();
        }
    }
}
