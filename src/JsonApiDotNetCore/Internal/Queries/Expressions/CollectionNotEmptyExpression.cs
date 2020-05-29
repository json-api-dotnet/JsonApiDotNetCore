using System;
using JsonApiDotNetCore.Internal.Queries.Parsing;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class CollectionNotEmptyExpression : FilterExpression
    {
        public ResourceFieldChainExpression TargetCollection { get; }

        public CollectionNotEmptyExpression(ResourceFieldChainExpression targetCollection)
        {
            TargetCollection = targetCollection ?? throw new ArgumentNullException(nameof(targetCollection));
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitCollectionNotEmpty(this, argument);
        }

        public override string ToString()
        {
            return $"{Keywords.Has}({TargetCollection})";
        }
    }
}
