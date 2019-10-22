using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Base clas for query parameters.
    /// </summary>
    public abstract class QueryParameterService
    {
        protected readonly IResourceGraph _resourceGraph;
        protected readonly ContextEntity _requestResource;

        protected QueryParameterService(IResourceGraph resourceGraph, ICurrentRequest currentRequest)
        {
            _resourceGraph = resourceGraph;
            _requestResource = currentRequest.GetRequestResource();
        }

        protected QueryParameterService() { }

        /// <summary>
        /// Derives the name of the query parameter from the name of the implementing type.
        /// </summary>
        /// <example>
        /// The following query param service will match the query  displayed in URL
        /// `?include=some-relationship`
        /// <code>public class IncludeService : QueryParameterService  { /* ... */  } </code>
        /// </example>
        public virtual string Name { get { return GetParameterNameFromType(); } }

        /// <summary>
        /// Gets the query parameter name from the implementing class name. Trims "Service"
        /// from the name if present.
        /// </summary>
        private string GetParameterNameFromType() => new Regex("Service$").Replace(GetType().Name, string.Empty).ToLower();

        /// <summary>
        /// Helper method for parsing query parameters into attributes
        /// </summary>
        protected AttrAttribute GetAttribute(string target, RelationshipAttribute relationship = null)
        {
            AttrAttribute attribute;
            if (relationship != null)
                attribute = _resourceGraph.GetAttributes(relationship.DependentType).FirstOrDefault(a => a.Is(target));
            else
                attribute = _requestResource.Attributes.FirstOrDefault(attr => attr.Is(target));

            if (attribute == null)
                throw new JsonApiException(400, $"'{target}' is not a valid attribute.");

            return attribute;
        }

        /// <summary>
        /// Helper method for parsing query parameters into relationships attributes
        /// </summary>
        protected RelationshipAttribute GetRelationship(string propertyName)
        {
            if (propertyName == null) return null;
            var relationship = _requestResource.Relationships.FirstOrDefault(r => r.Is(propertyName));
            if (relationship == null)
                throw new JsonApiException(400, $"{propertyName} is not a valid relationship on {_requestResource.EntityName}.");

            return relationship;
        }
    }
}
