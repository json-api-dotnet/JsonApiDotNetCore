using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    public class SortService : QueryParameterService, ISortService
    {
        const char DESCENDING_SORT_OPERATOR = '-';
        private readonly IResourceDefinitionProvider _resourceDefinitionProvider;
        private List<SortQueryContext> _queries;
        private bool _isProcessed;

        public SortService(IResourceDefinitionProvider resourceDefinitionProvider,
                           IContextEntityProvider contextEntityProvider,
                           ICurrentRequest currentRequest)
            : base(contextEntityProvider, currentRequest)
        {
            _resourceDefinitionProvider = resourceDefinitionProvider;
            _queries = new List<SortQueryContext>();
        }

        /// <inheritdoc/>
        public virtual void Parse(KeyValuePair<string, StringValues> queryParameter)
        {
            CheckIfProcessed(); // disallow multiple sort parameters.
            var queries = BuildQueries(queryParameter.Value);

            _queries = queries.Select(BuildQueryContext).ToList();
        }

        /// <inheritdoc/>
        public List<SortQueryContext> Get()
        {
            if (_queries == null)
            {
                var requestResourceDefinition = _resourceDefinitionProvider.Get(_requestResource.EntityType);
                if (requestResourceDefinition != null)
                    return requestResourceDefinition.DefaultSort()?.Select(d => BuildQueryContext(new SortQuery(d.Item1.PublicAttributeName, d.Item2))).ToList();
            }
            return _queries.ToList();
        }

        private List<SortQuery> BuildQueries(string value)
        {
            var sortParameters = new List<SortQuery>();

            var sortSegments = value.Split(QueryConstants.COMMA);
            if (sortSegments.Any(s => s == string.Empty))
                throw new JsonApiException(400, "The sort URI segment contained a null value.");

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
            var relationship = GetRelationship(query.Relationship);
            var attribute = GetAttribute(query.Attribute, relationship);

            if (attribute.IsSortable == false)
                throw new JsonApiException(400, $"Sort is not allowed for attribute '{attribute.PublicAttributeName}'.");

            return new SortQueryContext(query)
            {
                Attribute = attribute,
                Relationship = relationship
            };
        }

        private void CheckIfProcessed()
        {
            if (_isProcessed)
                throw new JsonApiException(400, "The sort query parameter occured in the URI more than once.");

            _isProcessed = true;
        }

    }
}
