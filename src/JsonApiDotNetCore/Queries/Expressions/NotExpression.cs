using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the "not" filter function, resulting from text such as: not(equals(title,'Work'))
    /// </summary>
    [PublicAPI]
    public class NotExpression : FilterExpression
    {
        public QueryExpression Child { get; }

        public NotExpression(QueryExpression child)
        {
            ArgumentGuard.NotNull(child, nameof(child));

            Child = child;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitNot(this, argument);
        }

        public override string ToString()
        {
            return $"{Keywords.Not}({Child})";
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

            var other = (NotExpression)obj;

            return Child.Equals(other.Child);
        }

        public override int GetHashCode()
        {
            return Child.GetHashCode();
        }
    }
}
