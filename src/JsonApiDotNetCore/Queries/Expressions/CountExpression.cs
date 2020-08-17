using System;
using JsonApiDotNetCore.Internal.Queries.Parsing;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents the "count" function, resulting from text such as: count(articles)
    /// </summary>
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
