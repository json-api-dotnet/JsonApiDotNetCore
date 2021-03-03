using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries.Internal.Parsing
{
    [PublicAPI]
    public class SparseFieldTypeParser : QueryExpressionParser
    {
        private readonly IResourceContextProvider _resourceContextProvider;

        public SparseFieldTypeParser(IResourceContextProvider resourceContextProvider)
            : base(resourceContextProvider)
        {
            _resourceContextProvider = resourceContextProvider;
        }

        public ResourceContext Parse(string source)
        {
            Tokenize(source);

            ResourceContext resourceContext = ParseSparseFieldTarget();

            AssertTokenStackIsEmpty();

            return resourceContext;
        }

        private ResourceContext ParseSparseFieldTarget()
        {
            if (!TokenStack.TryPop(out Token token) || token.Kind != TokenKind.Text)
            {
                throw new QueryParseException("Parameter name expected.");
            }

            EatSingleCharacterToken(TokenKind.OpenBracket);

            ResourceContext resourceContext = ParseResourceName();

            EatSingleCharacterToken(TokenKind.CloseBracket);

            return resourceContext;
        }

        private ResourceContext ParseResourceName()
        {
            if (TokenStack.TryPop(out Token token) && token.Kind == TokenKind.Text)
            {
                return GetResourceContext(token.Value);
            }

            throw new QueryParseException("Resource type expected.");
        }

        private ResourceContext GetResourceContext(string resourceName)
        {
            ResourceContext resourceContext = _resourceContextProvider.GetResourceContext(resourceName);

            if (resourceContext == null)
            {
                throw new QueryParseException($"Resource type '{resourceName}' does not exist.");
            }

            return resourceContext;
        }

        protected override IReadOnlyCollection<ResourceFieldAttribute> OnResolveFieldChain(string path, FieldChainRequirements chainRequirements)
        {
            throw new NotSupportedException();
        }
    }
}
