using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonApiDotNetCore.Internal.Queries.Expressions
{
    public class IncludeExpression : QueryExpression
    {
        // TODO: Unfold into a tree of child relationships, so it can be used from ResourceDefinitions.

        public IReadOnlyCollection<ResourceFieldChainExpression> Chains { get; }

        public static readonly IncludeExpression Empty = new IncludeExpression();

        private IncludeExpression()
        {
            Chains = Array.Empty<ResourceFieldChainExpression>();
        }

        public IncludeExpression(IReadOnlyCollection<ResourceFieldChainExpression> chains)
        {
            Chains = chains ?? throw new ArgumentNullException(nameof(chains));

            if (!chains.Any())
            {
                throw new ArgumentException("Must have one or more chains.", nameof(chains));
            }
        }

        public override TResult Accept<TArgument, TResult>(QueryExpressionVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitInclude(this, argument);
        }

        public override string ToString()
        {
            return string.Join(",", Chains.Select(child => child.ToString()));
        }
    }
}
