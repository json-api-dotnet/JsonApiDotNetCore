using System.Linq;
using System.Text.RegularExpressions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;

namespace JsonApiDotNetCore.Query
{
    /// <summary>
    /// Base class for query parameters.
    /// </summary>
    public abstract class QueryParameterService
    {
        protected readonly IResourceGraph _resourceGraph;
        protected readonly ResourceContext _requestResource;
        private readonly ResourceContext _mainRequestResource;

        protected QueryParameterService(IResourceGraph resourceGraph, ICurrentRequest currentRequest)
        {
            _mainRequestResource = currentRequest.GetRequestResource();
            _resourceGraph = resourceGraph;
            _requestResource = currentRequest.RequestRelationship != null
                ? resourceGraph.GetResourceContext(currentRequest.RequestRelationship.RightType)
                : _mainRequestResource;
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
        public virtual string Name => GetParameterNameFromType();

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
            var attribute = relationship != null
                ? _resourceGraph.GetAttributes(relationship.RightType).FirstOrDefault(a => a.Is(target))
                : _requestResource.Attributes.FirstOrDefault(attr => attr.Is(target));

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
                throw new JsonApiException(400, $"{propertyName} is not a valid relationship on {_requestResource.ResourceName}.");

            return relationship;
        }

        /// <summary>
        /// Throw an exception if query parameters are requested that are unsupported on nested resource routes.
        /// </summary>
        protected void EnsureNoNestedResourceRoute()
        {
            if (_requestResource != _mainRequestResource)
            {
                throw new JsonApiException(400, $"Query parameter {Name} is currently not supported on nested resource endpoints (i.e. of the form '/article/1/author?{Name}=...'");
            }
        }
    }
}
