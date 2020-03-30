using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    public class IncludeService : QueryParameterService, IIncludeService
    {
        /// todo: use read-only lists.
        private readonly List<List<RelationshipAttribute>> _includedChains;

        public IncludeService(IResourceGraph resourceGraph, ICurrentRequest currentRequest) : base(resourceGraph, currentRequest)
        {
            _includedChains = new List<List<RelationshipAttribute>>();
        }

        /// <inheritdoc/>
        public List<List<RelationshipAttribute>> Get()
        {
            return _includedChains.Select(chain => chain.ToList()).ToList();
        }

        /// <inheritdoc/>
        public virtual void Parse(KeyValuePair<string, StringValues> queryParameter)
        {
            var value = (string)queryParameter.Value;
            if (string.IsNullOrWhiteSpace(value))
                throw new JsonApiException(HttpStatusCode.BadRequest, "Include parameter must not be empty if provided");

            var chains = value.Split(QueryConstants.COMMA).ToList();
            foreach (var chain in chains)
                ParseChain(chain);
        }

        private void ParseChain(string chain)
        {
            var parsedChain = new List<RelationshipAttribute>();
            var chainParts = chain.Split(QueryConstants.DOT);
            var resourceContext = _requestResource;
            foreach (var relationshipName in chainParts)
            {
                var relationship = resourceContext.Relationships.SingleOrDefault(r => r.PublicRelationshipName == relationshipName);
                if (relationship == null)
                    throw InvalidRelationshipError(resourceContext, relationshipName);

                if (relationship.CanInclude == false)
                    throw CannotIncludeError(resourceContext, relationshipName);

                parsedChain.Add(relationship);
                resourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
            }
            _includedChains.Add(parsedChain);
        }

        private JsonApiException CannotIncludeError(ResourceContext resourceContext, string requestedRelationship)
        {
           return new JsonApiException(HttpStatusCode.BadRequest, $"Including the relationship {requestedRelationship} on {resourceContext.ResourceName} is not allowed");
        }

        private JsonApiException InvalidRelationshipError(ResourceContext resourceContext, string requestedRelationship)
        {
           return new JsonApiException(HttpStatusCode.BadRequest, $"Invalid relationship {requestedRelationship} on {resourceContext.ResourceName}",
                $"{resourceContext.ResourceName} does not have a relationship named {requestedRelationship}");
        }
    }
}
