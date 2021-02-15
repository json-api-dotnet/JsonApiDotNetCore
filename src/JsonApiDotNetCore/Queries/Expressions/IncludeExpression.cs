using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Queries.Expressions
{
    /// <summary>
    /// Represents an inclusion tree, resulting from text such as: owner,articles.revisions
    /// </summary>
    public class IncludeExpression : QueryExpression
    {
        public IReadOnlyCollection<IncludeElementExpression> Elements { get; }

        public static readonly IncludeExpression Empty = new IncludeExpression();

        private IncludeExpression()
        {
            Elements = Array.Empty<IncludeElementExpression>();
        }

        public IncludeExpression(IReadOnlyCollection<IncludeElementExpression> elements)
        {
            ArgumentGuard.NotNull(elements, nameof(elements));

            if (!elements.Any())
            {
                throw new ArgumentException("Must have one or more elements.", nameof(elements));
            }

            Elements = elements;
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitInclude(this, argument);
        }

        public override string ToString()
        {
            var chains = IncludeChainConverter.GetRelationshipChains(this);
            return string.Join(",", chains.Select(child => child.ToString()));
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

            var other = (IncludeExpression) obj;

            return Elements.SequenceEqual(other.Elements);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var element in Elements)
            {
                hashCode.Add(element);
            }

            return hashCode.ToHashCode();
        }
    }
}
