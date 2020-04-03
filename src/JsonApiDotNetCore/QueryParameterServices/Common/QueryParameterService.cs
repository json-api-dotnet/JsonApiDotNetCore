using System.Linq;
using System.Net;
using JsonApiDotNetCore.Exceptions;
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

        protected QueryParameterService() { }

        protected QueryParameterService(IResourceGraph resourceGraph, ICurrentRequest currentRequest)
        {
            _mainRequestResource = currentRequest.GetRequestResource();
            _resourceGraph = resourceGraph;
            _requestResource = currentRequest.RequestRelationship != null
                ? resourceGraph.GetResourceContext(currentRequest.RequestRelationship.RightType)
                : _mainRequestResource;
        }

        /// <summary>
        /// Helper method for parsing query parameters into attributes
        /// </summary>
        protected AttrAttribute GetAttribute(string queryParameterName, string target, RelationshipAttribute relationship = null)
        {
            var attribute = relationship != null
                ? _resourceGraph.GetAttributes(relationship.RightType).FirstOrDefault(a => a.Is(target))
                : _requestResource.Attributes.FirstOrDefault(attr => attr.Is(target));

            if (attribute == null)
            {
                throw new InvalidQueryStringParameterException(queryParameterName,
                    "The attribute requested in query string does not exist.",
                    $"The attribute '{target}' does not exist on resource '{_requestResource.ResourceName}'.");
            }

            return attribute;
        }

        /// <summary>
        /// Helper method for parsing query parameters into relationships attributes
        /// </summary>
        protected RelationshipAttribute GetRelationship(string queryParameterName, string propertyName)
        {
            if (propertyName == null) return null;
            var relationship = _requestResource.Relationships.FirstOrDefault(r => r.Is(propertyName));
            if (relationship == null)
            {
                throw new InvalidQueryStringParameterException(queryParameterName,
                    "The relationship requested in query string does not exist.",
                    $"The relationship '{propertyName}' does not exist on resource '{_requestResource.ResourceName}'.");
            }

            return relationship;
        }

        /// <summary>
        /// Throw an exception if query parameters are requested that are unsupported on nested resource routes.
        /// </summary>
        protected void EnsureNoNestedResourceRoute(string parameterName)
        {
            if (_requestResource != _mainRequestResource)
            {
                throw new JsonApiException(HttpStatusCode.BadRequest, $"Query parameter {parameterName} is currently not supported on nested resource endpoints (i.e. of the form '/article/1/author?{parameterName}=...'");
            }
        }
    }
}
