using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a sorting, resulting from text such as: lastName,-lastModifiedAt
    /// </summary>
    public class SortExpression : QueryExpression
    {
        public IReadOnlyCollection<SortElementExpression> Elements { get; }

        public SortExpression(IReadOnlyCollection<SortElementExpression> elements)
        {
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));

            if (!elements.Any())
            {
                throw new ArgumentException("Must have one or more elements.", nameof(elements));
            }
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSort(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Elements.Select(child => child.ToString()));
        }
    }
}
