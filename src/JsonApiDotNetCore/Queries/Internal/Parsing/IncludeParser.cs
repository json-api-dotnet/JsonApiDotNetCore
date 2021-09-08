using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    [PublicAPI]
    public class IncludeParser : QueryExpressionParser
    {
        private static readonly IncludeChainConverter IncludeChainConverter = new();

        private readonly Action<RelationshipAttribute, ResourceContext, string> _validateSingleRelationshipCallback;
        private ResourceContext _resourceContextInScope;

        public IncludeParser(IResourceGraph resourceGraph, Action<RelationshipAttribute, ResourceContext, string> validateSingleRelationshipCallback = null)
            : base(resourceGraph)
        {
            _validateSingleRelationshipCallback = validateSingleRelationshipCallback;
        }

        public IncludeExpression Parse(string source, ResourceContext resourceContextInScope, int? maximumDepth)
        {
            ArgumentGuard.NotNull(resourceContextInScope, nameof(resourceContextInScope));

            _resourceContextInScope = resourceContextInScope;

            Tokenize(source);

            IncludeExpression expression = ParseInclude(maximumDepth);

            AssertTokenStackIsEmpty();

            return expression;
        }

        protected IncludeExpression ParseInclude(int? maximumDepth)
        {
            ResourceFieldChainExpression firstChain = ParseFieldChain(FieldChainRequirements.IsRelationship, "Relationship name expected.");

            List<ResourceFieldChainExpression> chains = firstChain.AsList();

            while (TokenStack.Any())
            {
                EatSingleCharacterToken(TokenKind.Comma);

                ResourceFieldChainExpression nextChain = ParseFieldChain(FieldChainRequirements.IsRelationship, "Relationship name expected.");
                chains.Add(nextChain);
            }

            ValidateMaximumIncludeDepth(maximumDepth, chains);

            return IncludeChainConverter.FromRelationshipChains(chains);
        }

        private static void ValidateMaximumIncludeDepth(int? maximumDepth, IEnumerable<ResourceFieldChainExpression> chains)
        {
            if (maximumDepth != null)
            {
                foreach (ResourceFieldChainExpression chain in chains)
                {
                    if (chain.Fields.Count > maximumDepth)
                    {
                        string path = string.Join('.', chain.Fields.Select(field => field.PublicName));
                        throw new QueryParseException($"Including '{path}' exceeds the maximum inclusion depth of {maximumDepth}.");
                    }
                }
            }
        }

        protected override IImmutableList<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            return ChainResolver.ResolveRelationshipChain(_resourceContextInScope, path, _validateSingleRelationshipCallback);
        }
    }
}
