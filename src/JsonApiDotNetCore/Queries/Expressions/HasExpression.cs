using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the "has" filter function, resulting from text such as: has(articles) or has(articles,equals(isHidden,'false'))
    /// </summary>
    [PublicAPI]
    public class HasExpression : FilterExpression
    {
        public ResourceFieldChainExpression TargetCollection { get; }
        public FilterExpression? Filter { get; }

        public HasExpression(ResourceFieldChainExpression targetCollection, FilterExpression? filter)
        {
            ArgumentGuard.NotNull(targetCollection, nameof(targetCollection));

            TargetCollection = targetCollection;
            Filter = filter;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitHas(this, argument);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Keywords.Has);
            builder.Append('(');
            builder.Append(TargetCollection);

            if (Filter != null)
            {
                builder.Append(',');
                builder.Append(Filter);
            }

            builder.Append(')');

            return builder.ToString();
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (HasExpression)obj;

            return TargetCollection.Equals(other.TargetCollection) && Equals(Filter, other.Filter);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TargetCollection, Filter);
        }
    }
}
