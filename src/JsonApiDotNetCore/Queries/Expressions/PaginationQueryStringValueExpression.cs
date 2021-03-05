using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents pagination in a query string, resulting from text such as: 1,articles:2
    /// </summary>
    [PublicAPI]
    public class PaginationQueryStringValueExpression : QueryExpression
    {
        public IReadOnlyCollection<PaginationElementQueryStringValueExpression> Elements { get; }

        public PaginationQueryStringValueExpression(IReadOnlyCollection<PaginationElementQueryStringValueExpression> elements)
        {
            ArgumentGuard.NotNullNorEmpty(elements, nameof(elements));

            Elements = elements;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.PaginationQueryStringValue(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Elements.Select(constant => constant.ToString()));
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

            var other = (PaginationQueryStringValueExpression)obj;

            return Elements.SequenceEqual(other.Elements);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (PaginationElementQueryStringValueExpression element in Elements)
            {
                hashCode.Add(element);
            }

            return hashCode.ToHashCode();
        }
    }
}
