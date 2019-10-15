using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    public class FilterService : QueryParameterService, IFilterService
    {

        private readonly List<FilterQueryContext> _filters;
        private IResourceDefinition _requestResourceDefinition;

        public FilterService(IResourceDefinitionProvider resourceDefinitionProvider, IContextEntityProvider contextEntityProvider, ICurrentRequest currentRequest) : base(contextEntityProvider, currentRequest)
        {
            _requestResourceDefinition = resourceDefinitionProvider.Get(_requestResource.EntityType);
            _filters = new List<FilterQueryContext>();
        }

        public List<FilterQueryContext> Get()
        {
            return _filters;
        }

        public virtual void Parse(KeyValuePair<string, StringValues> queryParameter)
        {
            var queries = GetFilterQueries(queryParameter);
            _filters.AddRange(queries.Select(GetQueryContexts));
        }

        private FilterQueryContext GetQueryContexts(FilterQuery query)
        {
            var queryContext = new FilterQueryContext(query);
            if (_requestResourceDefinition != null && _requestResourceDefinition.HasCustomQueryFilter(query.Target))
            {
                queryContext.IsCustom = true;
                return queryContext;
            }

            queryContext.Relationship = GetRelationship(query.Relationship);
            var attribute = GetAttribute(query.Attribute, queryContext.Relationship);

            if (attribute.IsFilterable == false)
                throw new JsonApiException(400, $"Filter is not allowed for attribute '{attribute.PublicAttributeName}'.");
            queryContext.Attribute = attribute;

            return queryContext;
        }

        /// todo: this could be simplified a bunch 
        private List<FilterQuery> GetFilterQueries(KeyValuePair<string, StringValues> queryParameter)
        {
            // expected input = filter[id]=1
            // expected input = filter[id]=eq:1
            var propertyName = queryParameter.Key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];
            var queries = new List<FilterQuery>();
            // InArray case
            string op = GetFilterOperation(queryParameter.Value);
            if (string.Equals(op, FilterOperation.@in.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, FilterOperation.nin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                (var _, var filterValue) = ParseFilterOperation(queryParameter.Value);
                queries.Add(new FilterQuery(propertyName, filterValue, op));
            }
            else
            {
                var values = ((string)queryParameter.Value).Split(QueryConstants.COMMA);
                foreach (var val in values)
                {
                    (var operation, var filterValue) = ParseFilterOperation(val);
                    queries.Add(new FilterQuery(propertyName, filterValue, operation));
                }
            }
            return queries;
        }

        /// todo: this could be simplified a bunch 
        private (string operation, string value) ParseFilterOperation(string value)
        {
            if (value.Length < 3)
                return (string.Empty, value);

            var operation = GetFilterOperation(value);
            var values = value.Split(QueryConstants.COLON);

            if (string.IsNullOrEmpty(operation))
                return (string.Empty, value);

            value = string.Join(QueryConstants.COLON_STR, values.Skip(1));

            return (operation, value);
        }

        /// todo: this could be simplified a bunch 
        private string GetFilterOperation(string value)
        {
            var values = value.Split(QueryConstants.COLON);

            if (values.Length == 1)
                return string.Empty;

            var operation = values[0];
            // remove prefix from value
            if (Enum.TryParse(operation, out FilterOperation op) == false)
                return string.Empty;

            return operation;
        }
    }
}
