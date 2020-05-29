using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class PaginationQueryStringValueExpression : QueryExpression
    {
        public IReadOnlyCollection<PaginationElementQueryStringValueExpression> Elements { get; }

        public PaginationQueryStringValueExpression(
            IReadOnlyCollection<PaginationElementQueryStringValueExpression> elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));

            if (!Elements.Any())
            {
                throw new ArgumentException("Must have one or more elements.", nameof(elements));
            }
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor,
            TArgument argument)
        {
            return visitor.PaginationQueryStringValue(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Elements.Select(constant => constant.ToString()));
        }
    }
}
