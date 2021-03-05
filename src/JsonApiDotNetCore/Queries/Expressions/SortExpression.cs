using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents a sorting, resulting from text such as: lastName,-lastModifiedAt
    /// </summary>
    [PublicAPI]
    public class SortExpression : QueryExpression
    {
        public IReadOnlyCollection<SortElementExpression> Elements { get; }

        public SortExpression(IReadOnlyCollection<SortElementExpression> elements)
        {
            ArgumentGuard.NotNullNorEmpty(elements, nameof(elements));

            Elements = elements;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitSort(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Elements.Select(child => child.ToString()));
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

            var other = (SortExpression)obj;

            return Elements.SequenceEqual(other.Elements);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (SortElementExpression element in Elements)
            {
                hashCode.Add(element);
            }

            return hashCode.ToHashCode();
        }
    }
}
