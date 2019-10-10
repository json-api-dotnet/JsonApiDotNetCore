using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
{

    public class IncludeService : QueryParameterService, IIncludeService
    {
        /// todo: use read-only lists.
        private readonly List<List<RelationshipAttribute>> _includedChains;
        private readonly ICurrentRequest _currentRequest;
        private readonly IContextEntityProvider _provider;
        private ContextEntity _primaryResourceContext;
        public IncludeService(ICurrentRequest currentRequest, IContextEntityProvider provider)
        {
            _currentRequest = currentRequest;
            _provider = provider;
            _includedChains = new List<List<RelationshipAttribute>>();
        }

        /// <summary>
        /// This constructor is used internally for testing.
        /// </summary>
        internal IncludeService(ContextEntity primaryResourceContext, IContextEntityProvider provider) : this(currentRequest: null, provider: provider)
        {
            _primaryResourceContext = primaryResourceContext;
        }

        /// <inheritdoc/>
        public List<List<RelationshipAttribute>> Get()
        {
            return _includedChains.Select(chain => chain.ToList()).ToList();
        }

        /// <inheritdoc/>
        public override void Parse(string _, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonApiException(400, "Include parameter must not be empty if provided");

            var chains = value.Split(QueryConstants.COMMA).ToList();
            foreach (var chain in chains)
                ParseChain(chain);
        }

        private void ParseChain(string chain)
        {
            _primaryResourceContext = _primaryResourceContext ?? _currentRequest.GetRequestResource();

            var parsedChain = new List<RelationshipAttribute>();
            var chainParts = chain.Split(QueryConstants.DOT);
            var resourceContext = _primaryResourceContext;
            foreach (var relationshipName in chainParts)
            {
                var relationship = resourceContext.Relationships.SingleOrDefault(r => r.PublicRelationshipName == relationshipName);
                if (relationship == null)
                    throw InvalidRelationshipError(resourceContext, relationshipName);

                if (relationship.CanInclude == false)
                    throw CannotIncludeError(resourceContext, relationshipName);

                parsedChain.Add(relationship);
                resourceContext = _provider.GetContextEntity(relationship.DependentType);
            }
            _includedChains.Add(parsedChain);
        }

        private JsonApiException CannotIncludeError(ContextEntity resourceContext, string requestedRelationship)
        {
           return new JsonApiException(400, $"Including the relationship {requestedRelationship} on {resourceContext.EntityName} is not allowed");
        }

        private JsonApiException InvalidRelationshipError(ContextEntity resourceContext, string requestedRelationship)
        {
           return new JsonApiException(400, $"Invalid relationship {requestedRelationship} on {resourceContext.EntityName}",
                $"{resourceContext.EntityName} does not have a relationship named {requestedRelationship}");
        }
    }
}
