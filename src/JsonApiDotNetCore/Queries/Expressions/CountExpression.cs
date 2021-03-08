using JetBrains.Annotations;
using JsonApiDotNetCore.Queries.Internal.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the "count" function, resulting from text such as: count(articles)
    /// </summary>
    [PublicAPI]
    public class CountExpression : FunctionExpression
    {
        public ResourceFieldChainExpression TargetCollection { get; }

        public CountExpression(ResourceFieldChainExpression targetCollection)
        {
            ArgumentGuard.NotNull(targetCollection, nameof(targetCollection));

            TargetCollection = targetCollection;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitCount(this, argument);
        }

        public override string ToString()
        {
            return $"{Keywords.Count}({TargetCollection})";
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

            var other = (CountExpression)obj;

            return TargetCollection.Equals(other.TargetCollection);
        }

        public override int GetHashCode()
        {
            return TargetCollection.GetHashCode();
        }
    }
}
