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
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));

            if (!elements.Any())
            {
                throw new ArgumentException("Must have one or more elements.", nameof(elements));
            }
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
    }
}
