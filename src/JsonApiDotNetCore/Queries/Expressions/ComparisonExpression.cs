using System;
using Humanizer;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a comparison filter function, resulting from text such as: equals(name,'Joe')
    /// </summary>
    [PublicAPI]
    public class ComparisonExpression : FilterExpression
    {
        public ComparisonOperator Operator { get; }
        public QueryExpression Left { get; }
        public QueryExpression Right { get; }

        public ComparisonExpression(ComparisonOperator @operator, QueryExpression left, QueryExpression right)
        {
            ArgumentGuard.NotNull(left, nameof(left));
            ArgumentGuard.NotNull(right, nameof(right));

            Operator = @operator;
            Left = left;
            Right = right;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitComparison(this, argument);
        }

        public override string ToString()
        {
            return $"{Operator.ToString().Camelize()}({Left},{Right})";
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

            var other = (ComparisonExpression)obj;

            return Operator == other.Operator && Left.Equals(other.Left) && Right.Equals(other.Right);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Operator, Left, Right);
        }
    }
}
