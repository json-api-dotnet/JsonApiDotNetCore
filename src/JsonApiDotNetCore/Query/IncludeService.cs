using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;

namespace JsonApiDotNetCore.Query
{
    public class IncludeService : IIncludeService, IQueryParameterService
    {
        private readonly List<List<RelationshipAttribute>> _includedChains;
        private readonly ICurrentRequest _currentRequest;
        private readonly IContextEntityProvider _provider;

        public IncludeService(ICurrentRequest currentRequest,
                                   IContextEntityProvider provider)
        {
            _currentRequest = currentRequest;
            _provider = provider;
            _includedChains = new List<List<RelationshipAttribute>>();
        }

        /// <summary>
        /// This constructor is used internally for testing.
        /// </summary>
        internal IncludeService() : this(null, null) { }

        public string Name => QueryConstants.INCLUDE;

        /// <inheritdoc/>
        public List<List<RelationshipAttribute>> Get()
        {
            return _includedChains;
        }

        /// <inheritdoc/>
        public void Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonApiException(400, "Include parameter must not be empty if provided");

            var chains = value.Split(QueryConstants.COMMA).ToList();
            foreach (var chain in chains)
                ParseChain(chain);
        }

        private void ParseChain(string chain)
        {
            var parsedChain = new List<RelationshipAttribute>();
            var resourceContext = _currentRequest.GetRequestResource();
            var chainParts = chain.Split(QueryConstants.DOT);
            foreach (var relationshipName in chainParts)
            {
                var relationship = resourceContext.Relationships.Single(r => r.PublicRelationshipName == relationshipName);
                if (relationship == null)
                    ThrowInvalidRelationshipError(resourceContext, relationshipName);

                if (relationship.CanInclude == false)
                    ThrowCannotIncludeError(resourceContext, relationshipName);

                parsedChain.Add(relationship);
                resourceContext = _provider.GetContextEntity(relationship.PrincipalType);
            }
            _includedChains.Add(parsedChain);
        }

        private void ThrowCannotIncludeError(ContextEntity resourceContext, string requestedRelationship)
        {
            throw new JsonApiException(400, $"Including the relationship {requestedRelationship} on {resourceContext.EntityName} is not allowed");
        }

        private void ThrowInvalidRelationshipError(ContextEntity resourceContext, string requestedRelationship)
        {
            throw new JsonApiException(400, $"Invalid relationship {requestedRelationship} on {resourceContext.EntityName}",
                $"{resourceContext.EntityName} does not have a relationship named {requestedRelationship}");
        }
    }
}
