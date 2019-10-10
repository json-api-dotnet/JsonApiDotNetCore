using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Query;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{

    public interface IQueryParser
    {
        QuerySet Parse(IQueryCollection query);
    }

    public class QueryParser : IQueryParser
    {
        private readonly IIncludeService _includeService;
        private readonly ISparseFieldsService _fieldQuery;
        private readonly IPageQueryService _pageQuery;
        private readonly ICurrentRequest _currentRequest;
        private readonly IContextEntityProvider _provider;
        private readonly IJsonApiOptions _options;
        private readonly IServiceProvider _sp;
        private ContextEntity _primaryResource;

        public QueryParser(IIncludeService includeService,
            ISparseFieldsService fieldQuery,
            ICurrentRequest currentRequest,
            IContextEntityProvider provider,
            IPageQueryService pageQuery,
            IJsonApiOptions options)
        {
            _includeService = includeService;
            _fieldQuery = fieldQuery;
            _currentRequest = currentRequest;
            _pageQuery = pageQuery;
            _provider = provider;
            _options = options;
        }

        public virtual QuerySet Parse(IQueryCollection query)
        {
            var type = typeof(IQueryParameterService);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsInterface && t.Inherits(type))
                .Select(t => (IQueryParameterService)_sp.GetService(t));


            _primaryResource = _currentRequest.GetRequestResource();
            var querySet = new QuerySet();
            var disabledQueries = _currentRequest.DisabledQueryParams;
            foreach (var pair in query)
            {
                if (pair.Key.StartsWith(QueryConstants.FILTER, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Filters) == false)
                        querySet.Filters.AddRange(ParseFilterQuery(pair.Key, pair.Value));
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.SORT, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Sort) == false)
                        querySet.SortParameters = ParseSortParameters(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(_includeService.Name, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Include) == false)
                        _includeService.Parse(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.PAGE, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Page) == false)
                        querySet.PageQuery = ParsePageQuery(querySet.PageQuery, pair.Key, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.FIELDS, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Fields) == false)
                        querySet.Fields = ParseFieldsQuery(pair.Key, pair.Value);
                    continue;
                }

                if (_options.AllowCustomQueryParameters == false)
                    throw new JsonApiException(400, $"{pair} is not a valid query.");
            }

            return querySet;
        }



        protected virtual List<FilterQuery> ParseFilterQuery(string key, string value)
        {
            // expected input = filter[id]=1
            // expected input = filter[id]=eq:1
            var queries = new List<FilterQuery>();
            var propertyName = key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];

            // InArray case
            string op = GetFilterOperation(value);
            if (string.Equals(op, FilterOperations.@in.ToString(), StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, FilterOperations.nin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                (var operation, var filterValue) = ParseFilterOperation(value);
                queries.Add(new FilterQuery(propertyName, filterValue, op));
            }
            else
            {
                var values = value.Split(QueryConstants.COMMA);
                foreach (var val in values)
                {
                    (var operation, var filterValue) = ParseFilterOperation(val);
                    queries.Add(new FilterQuery(propertyName, filterValue, operation));
                }
            }

            return queries;
        }

        protected virtual (string operation, string value) ParseFilterOperation(string value)
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

        protected virtual PageQuery ParsePageQuery(PageQuery pageQuery, string key, string value)
        {
            // expected input = page[size]=10
            //                  page[number]=1
            pageQuery = pageQuery ?? new PageQuery();

            var propertyName = key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];

            const string SIZE = "size";
            const string NUMBER = "number";

            if (propertyName == SIZE)
            {
                pageQuery.PageSize = int.TryParse(value, out var pageSize) ?
                pageSize :
                throw new JsonApiException(400, $"Invalid page size '{value}'");
            }

            else if (propertyName == NUMBER)
                pageQuery.PageOffset = int.TryParse(value, out var pageOffset) ?
                pageOffset :
                throw new JsonApiException(400, $"Invalid page size '{value}'");

            return pageQuery;
        }

        // sort=id,name
        // sort=-id
        protected virtual List<SortQuery> ParseSortParameters(string value)
        {
            var sortParameters = new List<SortQuery>();

            const char DESCENDING_SORT_OPERATOR = '-';
            var sortSegments = value.Split(QueryConstants.COMMA);
            if (sortSegments.Where(s => s == string.Empty).Count() > 0)
            {
                throw new JsonApiException(400, "The sort URI segment contained a null value.");
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

                sortParameters.Add(new SortQuery(direction, propertyName));
            };

            return sortParameters;
        }


        protected virtual List<string> ParseFieldsQuery(string key, string value)
        {
            // expected: fields[TYPE]=prop1,prop2
            var typeName = key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];
            var includedFields = new List<string> { nameof(Identifiable.Id) };

            var relationship = _primaryResource.Relationships.SingleOrDefault(a => a.Is(typeName));
            if (relationship == default && string.Equals(typeName, _primaryResource.EntityName, StringComparison.OrdinalIgnoreCase) == false)
                return includedFields;

            var fields = value.Split(QueryConstants.COMMA);
            foreach (var field in fields)
            {
                if (relationship != default)
                {
                    var relationProperty = _provider.GetContextEntity(relationship.DependentType);
                    var attr = relationProperty.Attributes.SingleOrDefault(a => a.Is(field));
                    if (attr == null)
                        throw new JsonApiException(400, $"'{relationship.DependentType.Name}' does not contain '{field}'.");

                    _fieldQuery.Register(attr, relationship);
                    // e.g. "Owner.Name"
                    includedFields.Add(relationship.InternalRelationshipName + "." + attr.InternalAttributeName);

                }
                else
                {
                    var attr = _primaryResource.Attributes.SingleOrDefault(a => a.Is(field));
                    if (attr == null)
                        throw new JsonApiException(400, $"'{_primaryResource.EntityName}' does not contain '{field}'.");

                    _fieldQuery.Register(attr, relationship);
                    // e.g. "Name"
                    includedFields.Add(attr.InternalAttributeName);
                }
            }

            return includedFields;
        }

        protected virtual AttrAttribute GetAttribute(string propertyName)
        {
            try
            {
                return _primaryResource
                    .Attributes
                    .Single(attr => attr.Is(propertyName));
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(400, $"Attribute '{propertyName}' does not exist on resource '{_primaryResource.EntityName}'", e);
            }
        }

        private string GetFilterOperation(string value)
        {
            var values = value.Split(QueryConstants.COLON);

            if (values.Length == 1)
                return string.Empty;

            var operation = values[0];
            // remove prefix from value
            if (Enum.TryParse(operation, out FilterOperations op) == false)
                return string.Empty;

            return operation;
        }
    }
}
