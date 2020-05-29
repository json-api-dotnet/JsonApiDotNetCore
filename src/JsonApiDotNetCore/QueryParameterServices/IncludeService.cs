using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models.Annotation;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    public class IncludeService : QueryParameterService, IIncludeService
    {
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
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Include);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            return parameterName == "include";
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            var value = (string)parameterValue;
            var chains = value.Split(QueryConstants.COMMA).ToList();
            foreach (var chain in chains)
                ParseChain(chain, parameterName);
        }

        private void ParseChain(string chain, string parameterName)
        {
            var parsedChain = new List<RelationshipAttribute>();
            var chainParts = chain.Split(QueryConstants.DOT);
            var resourceContext = _requestResource;
            foreach (var relationshipName in chainParts)
            {
                var relationship = resourceContext.Relationships.SingleOrDefault(r => r.PublicName == relationshipName);
                if (relationship == null)
                {
                    throw new InvalidQueryStringParameterException(parameterName, "The requested relationship to include does not exist.",
                        $"The relationship '{relationshipName}' on '{resourceContext.ResourceName}' does not exist.");
                }

                if (!relationship.CanInclude)
                {
                    throw new InvalidQueryStringParameterException(parameterName, "Including the requested relationship is not allowed.",
                        $"Including the relationship '{relationshipName}' on '{resourceContext.ResourceName}' is not allowed.");
                }

                parsedChain.Add(relationship);
                resourceContext = _resourceGraph.GetResourceContext(relationship.RightType);
            }
            _includedChains.Add(parsedChain);
        }
    }
}
