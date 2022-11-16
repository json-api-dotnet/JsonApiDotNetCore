using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services
{
    public interface IQueryParser
    {
        QuerySet Parse(IQueryCollection query);
    }

    public class QueryParser : IQueryParser
    {
        private readonly IControllerContext _controllerContext;
        private readonly JsonApiOptions _options;

        public QueryParser(
            IControllerContext controllerContext,
            JsonApiOptions options)
        {
            _controllerContext = controllerContext;
            _options = options;
        }

        public virtual QuerySet Parse(IQueryCollection query)
        {
            var querySet = new QuerySet();
            var disabledQueries = _controllerContext.GetControllerAttribute<DisableQueryAttribute>()?.QueryParams ?? QueryParams.None;

            foreach (var pair in query)
            {
                if (pair.Key.StartsWith(QueryConstants.FILTER))
                {
                    if (disabledQueries.HasFlag(QueryParams.Filter) == false)
                        querySet.Filters.AddRange(ParseFilterQuery(pair.Key, pair.Value));
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.SORT))
                {
                    if (disabledQueries.HasFlag(QueryParams.Sort) == false)
                        querySet.SortParameters = ParseSortParameters(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.INCLUDE))
                {
                    if (disabledQueries.HasFlag(QueryParams.Include) == false)
                        querySet.IncludedRelationships = ParseIncludedRelationships(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.PAGE))
                {
                    if (disabledQueries.HasFlag(QueryParams.Page) == false)
                        querySet.PageQuery = ParsePageQuery(querySet.PageQuery, pair.Key, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.FIELDS))
                {
                    if (disabledQueries.HasFlag(QueryParams.Fields) == false)
                        querySet.Fields = ParseFieldsQuery(pair.Key, pair.Value);
                    continue;
                }

                if (_options.AllowCustomQueryParameters == false)
                    throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"{pair} is not a valid query."
                    });
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
                pageQuery.PageSize = int.TryParse(value, out var pageSize) ?
                pageSize :
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"Invalid page size '{value}'"
                    });

            else if (propertyName == NUMBER)
                pageQuery.PageOffset = int.TryParse(value, out var pageOffset) ?
                pageOffset :
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"Invalid page size '{value}'"
                    });

            return pageQuery;
        }

        // sort=id,name
        // sort=-id
        protected virtual List<SortQuery> ParseSortParameters(string value)
        {
            var sortParameters = new List<SortQuery>();

            const char DESCENDING_SORT_OPERATOR = '-';
            var sortSegments = value.Split(QueryConstants.COMMA);

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

        protected virtual List<string> ParseIncludedRelationships(string value)
        {
            return value
                .Split(QueryConstants.COMMA)
                .ToList();
        }

        protected virtual List<string> ParseFieldsQuery(string key, string value)
        {
            // expected: fields[TYPE]=prop1,prop2
            var typeName = key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];
            var includedFields = new List<string> { nameof(Identifiable<int>.Id) };

            var relationship = _controllerContext.RequestEntity.Relationships.SingleOrDefault(a => a.Is(typeName));
            if (relationship == default && string.Equals(typeName, _controllerContext.RequestEntity.EntityName, StringComparison.OrdinalIgnoreCase) == false)
                return includedFields;

            var fields = value.Split(QueryConstants.COMMA);
            foreach (var field in fields)
            {
                if (relationship != default)
                {
                    var relationProperty = _options.ResourceGraph.GetContextEntity(relationship.Type);
                    var attr = relationProperty.Attributes.SingleOrDefault(a => a.Is(field));
                    if(attr == null)
                        throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"'{relationship.Type.Name}' does not contain '{field}'."
                    });

                    // e.g. "Owner.Name"
                    includedFields.Add(relationship.InternalRelationshipName + "." + attr.InternalAttributeName);
                }
                else
                {
                    var attr = _controllerContext.RequestEntity.Attributes.SingleOrDefault(a => a.Is(field));
                    if (attr == null)
                        throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                    {
                        Title = $"'{_controllerContext.RequestEntity.EntityName}' does not contain '{field}'."
                    });

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
                return _controllerContext
                    .RequestEntity
                    .Attributes
                    .Single(attr => attr.Is(propertyName));
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(new Error(HttpStatusCode.BadRequest)
                {
                    Title = $"Attribute '{propertyName}' does not exist on resource '{_controllerContext.RequestEntity.EntityName}'",
                    Source = e
                });
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
