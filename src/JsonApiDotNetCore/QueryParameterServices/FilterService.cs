using System;
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
    public class FilterService : QueryParameterService, IFilterService
    {
        private readonly List<FilterQueryContext> _filters;
        private readonly IResourceDefinition _requestResourceDefinition;

        public FilterService(IResourceDefinitionProvider resourceDefinitionProvider, IResourceGraph resourceGraph, ICurrentRequest currentRequest) : base(resourceGraph, currentRequest)
        {
            _requestResourceDefinition = resourceDefinitionProvider.Get(_requestResource.ResourceType);
            _filters = new List<FilterQueryContext>();
        }

        /// <inheritdoc/>
        public List<FilterQueryContext> Get()
        {
            return _filters;
        }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Filter);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            return parameterName.StartsWith("filter[") && parameterName.EndsWith("]");
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            EnsureNoNestedResourceRoute(parameterName);
            var queries = GetFilterQueries(parameterName, parameterValue);
            _filters.AddRange(queries.Select(x => GetQueryContexts(x, parameterName)));
        }

        private FilterQueryContext GetQueryContexts(FilterQuery query, string parameterName)
        {
            var queryContext = new FilterQueryContext(query);
            var customQuery = _requestResourceDefinition?.GetCustomQueryFilter(query.Target);
            if (customQuery != null)
            {
                queryContext.IsCustom = true;
                queryContext.CustomQuery = customQuery;
                return queryContext;
            }

            queryContext.Relationship = GetRelationship(parameterName, query.Relationship);
            var attribute = GetAttribute(parameterName, query.Attribute, queryContext.Relationship);

            if (!attribute.Capabilities.HasFlag(AttrCapabilities.AllowFilter))
            {
                throw new InvalidQueryStringParameterException(parameterName, "Filtering on the requested attribute is not allowed.",
                    $"Filtering on attribute '{attribute.PublicAttributeName}' is not allowed.");
            }

            queryContext.Attribute = attribute;

            return queryContext;
        }

        /// todo: this could be simplified a bunch 
        private List<FilterQuery> GetFilterQueries(string parameterName, StringValues parameterValue)
        {
            // expected input = filter[id]=1
            // expected input = filter[id]=eq:1
            var propertyName = parameterName.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];
            var queries = new List<FilterQuery>();
            // InArray case
            string op = GetFilterOperation(parameterValue);
            if (op == FilterOperation.@in.ToString() || op == FilterOperation.nin.ToString())
            {
                var (_, filterValue) = ParseFilterOperation(parameterValue);
                queries.Add(new FilterQuery(propertyName, filterValue, op));
            }
            else
            {
                var values = ((string)parameterValue).Split(QueryConstants.COMMA);
                foreach (var val in values)
                {
                    var (operation, filterValue) = ParseFilterOperation(val);
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
            if (Enum.TryParse(operation, out FilterOperation _) == false)
                return string.Empty;

            return operation;
        }
    }
}
