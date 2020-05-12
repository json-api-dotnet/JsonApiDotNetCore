using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class SortService : QueryParameterService, ISortService
    {
        private const char DESCENDING_SORT_OPERATOR = '-';
        private readonly IResourceDefinitionProvider _resourceDefinitionProvider;
        private List<SortQueryContext> _queries;

        public SortService(IResourceDefinitionProvider resourceDefinitionProvider,
                           IResourceGraph resourceGraph,
                           ICurrentRequest currentRequest)
            : base(resourceGraph, currentRequest)
        {
            _resourceDefinitionProvider = resourceDefinitionProvider;
            _queries = new List<SortQueryContext>();
        }

        /// <inheritdoc/>
        public List<SortQueryContext> Get()
        {
            if (!_queries.Any())
            {
                var requestResourceDefinition = _resourceDefinitionProvider.Get(_requestResource.ResourceType);
                if (requestResourceDefinition != null)
                    return requestResourceDefinition.DefaultSort()?.Select(d => BuildQueryContext(new SortQuery(d.Item1.PublicAttributeName, d.Item2))).ToList();
            }
            return _queries.ToList();
        }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Sort);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            return parameterName == "sort";
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            EnsureNoNestedResourceRoute(parameterName);
            var queries = BuildQueries(parameterValue, parameterName);

            _queries = queries.Select(BuildQueryContext).ToList();
        }

        private List<SortQuery> BuildQueries(string value, string parameterName)
        {
            var sortParameters = new List<SortQuery>();

            var sortSegments = value.Split(QueryConstants.COMMA);
            if (sortSegments.Any(s => s == string.Empty))
            {
                throw new InvalidQueryStringParameterException(parameterName, "The list of fields to sort on contains empty elements.", null);
            }

            foreach (var sortSegment in sortSegments)
            {
                var propertyName = sortSegment;
                var direction = SortDirection.Ascending;

                if (sortSegment[0] == DESCENDING_SORT_OPERATOR)
                {
                    direction = SortDirection.Descending;
                    propertyName = propertyName.Substring(1);
                }

                sortParameters.Add(new SortQuery(propertyName, direction));
            }

            return sortParameters;
        }

        private SortQueryContext BuildQueryContext(SortQuery query)
        {
            var relationship = GetRelationship("sort", query.Relationship);
            var attribute = GetAttribute("sort", query.Attribute, relationship);

            if (!attribute.Capabilities.HasFlag(AttrCapabilities.AllowSort))
            {
                throw new InvalidQueryStringParameterException("sort", "Sorting on the requested attribute is not allowed.",
                    $"Sorting on attribute '{attribute.PublicAttributeName}' is not allowed.");
            }

            return new SortQueryContext(query)
            {
                Attribute = attribute,
                Relationship = relationship
            };
        }
    }
}
