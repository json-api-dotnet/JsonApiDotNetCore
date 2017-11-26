using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Services {
    public interface IQueryParser {
        QuerySet Parse(IQueryCollection query);
    }

    public class QueryParser : IQueryParser {
        private readonly IControllerContext _controllerContext;
        private readonly JsonApiOptions _options;

        private const char OPEN_BRACKET = '[';
        private const char CLOSE_BRACKET = ']';
        public QueryParser(
            IControllerContext controllerContext,
            JsonApiOptions options) {
            _controllerContext = controllerContext;
            _options = options;
        }

        public virtual QuerySet Parse(IQueryCollection query) {
            var querySet = new QuerySet();
            var disabledQueries = _controllerContext.GetControllerAttribute<DisableQueryAttribute>() ? .QueryParams ?? QueryParams.None;

            foreach (var pair in query) {
                if (pair.Key.StartsWith("filter")) {
                    if (disabledQueries.HasFlag(QueryParams.Filter) == false)
                        querySet.Filters.AddRange(ParseFilterQuery(pair.Key, pair.Value));
                    continue;
                }

                if (pair.Key.StartsWith("sort")) {
                    if (disabledQueries.HasFlag(QueryParams.Sort) == false)
                        querySet.SortParameters = ParseSortParameters(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith("include")) {
                    if (disabledQueries.HasFlag(QueryParams.Include) == false)
                        querySet.IncludedRelationships = ParseIncludedRelationships(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith("page")) {
                    if (disabledQueries.HasFlag(QueryParams.Page) == false)
                        querySet.PageQuery = ParsePageQuery(querySet.PageQuery, pair.Key, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith("fields")) {
                    if (disabledQueries.HasFlag(QueryParams.Fields) == false)
                        querySet.Fields = ParseFieldsQuery(pair.Key, pair.Value);
                    continue;
                }

                if (_options.AllowCustomQueryParameters == false)
                    throw new JsonApiException(400, $"{pair} is not a valid query.");
            }

            return querySet;
        }

        protected virtual List<FilterQuery> ParseFilterQuery(string key, string value) {
            // expected input = filter[id]=1
            // expected input = filter[id]=eq:1
            var queries = new List<FilterQuery>();

            var propertyName = key.Split(OPEN_BRACKET, CLOSE_BRACKET) [1];

            var values = value.Split(',');
            foreach (var val in values) {
                (var operation,
                    var filterValue) = ParseFilterOperation(val);
                queries.Add(new FilterQuery(propertyName, filterValue, operation));
            }

            return queries;
        }

        protected virtual(string operation, string value) ParseFilterOperation(string value) {
            if (value.Length < 3)
                return (string.Empty, value);

            var operation = value.Split(':');

            if (operation.Length == 1)
                return (string.Empty, value);

            // remove prefix from value
            if (Enum.TryParse(operation[0], out FilterOperations op) == false)
                return (string.Empty, value);

            var prefix = operation[0];
            value = string.Join(":", operation.Skip(1));

            return (prefix, value);
        }

        protected virtual PageQuery ParsePageQuery(PageQuery pageQuery, string key, string value) {
            // expected input = page[size]=10
            //                  page[number]=1
            pageQuery = pageQuery ?? new PageQuery();

            var propertyName = key.Split('[', ']') [1];

            if (propertyName == "size")
                pageQuery.PageSize = Convert.ToInt32(value);
            else if (propertyName == "number")
                pageQuery.PageOffset = Convert.ToInt32(value);

            return pageQuery;
        }

        // sort=id,name
        // sort=-id
        protected virtual List<SortQuery> ParseSortParameters(string value) {
            const char SORT_DELIMITER = ',';
            const char DESCENDING_SORT_OPERATOR = '-';

            var sortParameters = new List<SortQuery>();
            var sortSegments = value.Split(SORT_DELIMITER);
            foreach (var sortSegment in sortSegments) {

                var propertyName = sortSegment;
                var direction = SortDirection.Ascending;

                if (sortSegment[0] == DESCENDING_SORT_OPERATOR) {
                    direction = SortDirection.Descending;
                    propertyName = propertyName.Substring(1);
                }

                var attribute = GetAttribute(propertyName);

                sortParameters.Add(new SortQuery(direction, attribute));
            };

            return sortParameters;
        }

        protected virtual List<string> ParseIncludedRelationships(string value) {
            if (value.Contains("."))
                throw new JsonApiException(400, "Deeply nested relationships are not supported");

            return value
                .Split(',')
                .ToList();
        }

        protected virtual List<string> ParseFieldsQuery(string key, string value) {
            // expected: fields[TYPE]=prop1,prop2
            var typeName = key.Split('[', ']') [1];

            var includedFields = new List<string> { "Id" };

            if (typeName != _controllerContext.RequestEntity.EntityName)
                return includedFields;

            var fields = value.Split(',');
            foreach (var field in fields) {
                var internalAttrName = _controllerContext.RequestEntity
                    .Attributes
                    .SingleOrDefault(attr => attr.PublicAttributeName == field)
                    .InternalAttributeName;
                includedFields.Add(internalAttrName);
            }

            return includedFields;
        }

        protected virtual AttrAttribute GetAttribute(string propertyName) => _controllerContext
            .RequestEntity
            .Attributes
            .FirstOrDefault(attr =>
                string.Equals(attr.PublicAttributeName, propertyName, StringComparison.OrdinalIgnoreCase)
            );
    }
}
