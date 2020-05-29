using System;
using JsonApiDotNetCore.Internal.Queries.Parsing;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class NotExpression : FilterExpression
    {
        public QueryExpression Child { get; }

        public NotExpression(QueryExpression child)
        {
            Child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitNot(this, argument);
        }

        public override string ToString()
        {
            return $"{Keywords.Not}({Child})";
        }
    }
}
