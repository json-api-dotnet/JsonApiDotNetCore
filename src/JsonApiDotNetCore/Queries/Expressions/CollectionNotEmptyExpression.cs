using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the "has" filter function, resulting from text such as: has(articles)
    /// </summary>
    [PublicAPI]
    public class CollectionNotEmptyExpression : FilterExpression
    {
        public ResourceFieldChainExpression TargetCollection { get; }

        public CollectionNotEmptyExpression(ResourceFieldChainExpression targetCollection)
        {
            ArgumentGuard.NotNull(targetCollection, nameof(targetCollection));

            TargetCollection = targetCollection;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitCollectionNotEmpty(this, argument);
        }

        public override string ToString()
        {
            return $"{Keywords.Has}({TargetCollection})";
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

            var other = (CollectionNotEmptyExpression)obj;

            return TargetCollection.Equals(other.TargetCollection);
        }

        public override int GetHashCode()
        {
            return TargetCollection.GetHashCode();
        }
    }
}
