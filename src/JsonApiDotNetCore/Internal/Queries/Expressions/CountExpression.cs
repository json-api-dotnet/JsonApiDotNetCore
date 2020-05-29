using System;
using JsonApiDotNetCore.Internal.Queries.Parsing;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class CountExpression : FunctionExpression
    {
        public ResourceFieldChainExpression TargetCollection { get; }

        public CountExpression(ResourceFieldChainExpression targetCollection)
        {
            TargetCollection = targetCollection ?? throw new ArgumentNullException(nameof(targetCollection));
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitCount(this, argument);
        }

        public override string ToString()
        {
            return $"{Keywords.Count}({TargetCollection})";
        }
    }
}
