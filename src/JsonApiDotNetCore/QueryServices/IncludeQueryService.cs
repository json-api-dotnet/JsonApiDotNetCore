using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.QueryServices.Contracts;

namespace JsonApiDotNetCore.QueryServices
{
    public class IncludeQueryService : IIncludeQueryService, IQueryParameter
    {
        private readonly List<List<RelationshipAttribute>> _includedChains;
        private readonly ICurrentRequest _currentRequest;
        private readonly IContextEntityProvider _provider;

        public IncludeQueryService(ICurrentRequest currentRequest,
                                   IContextEntityProvider provider)
        {
            _currentRequest = currentRequest;
            _provider = provider;
            _includedChains = new List<List<RelationshipAttribute>>();
        }


        /// <summary>
        /// For testing purposes.
        /// </summary>
        internal IncludeQueryService() : this(null, null) { }

        public string Name => QueryConstants.INCLUDE;

        /// <inheritdoc/>
        public List<List<RelationshipAttribute>> Get()
        {
            return _includedChains;
        }

        /// <inheritdoc/>
        public void Parse(string value)
        {
            var inclusions = value.Split(QueryConstants.COMMA).ToList();
            foreach (var chain in inclusions)
            {
                var parsedChain = new List<RelationshipAttribute>();
                var resourceContext = _currentRequest.GetRequestResource();
                var splittedPath = chain.Split(QueryConstants.DOT);
                foreach (var requestedRelationship in splittedPath)
                {
                    var relationship = resourceContext.Relationships.Single(r => r.PublicRelationshipName == requestedRelationship);
                    if (relationship == null)
                        throw new JsonApiException(400, $"Invalid relationship {requestedRelationship} on {resourceContext.EntityName}",
                            $"{resourceContext.EntityName} does not have a relationship named {requestedRelationship}");

                    if (relationship.CanInclude == false)
                        throw new JsonApiException(400, $"Including the relationship {requestedRelationship} on {resourceContext.EntityName} is not allowed");

                    parsedChain.Add(relationship);
                    resourceContext = _provider.GetContextEntity(relationship.PrincipalType);
                }
                _includedChains.Add(parsedChain);
            }
        }
    }
}
