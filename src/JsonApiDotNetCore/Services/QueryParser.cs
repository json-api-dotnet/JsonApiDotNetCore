using System;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Models;
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
            var arrOpVal = ParseFilterOperationAndValue(value);
            if (arrOpVal.operation == FilterOperations.@in || arrOpVal.operation == FilterOperations.nin)
                queries.Add(new FilterQuery(propertyName, arrOpVal.value, arrOpVal.operation));
            else
            {
                var values = value.Split(QueryConstants.COMMA);
                foreach (var val in values)
                {
                    var opVal = ParseFilterOperationAndValue(val);
                    queries.Add(new FilterQuery(propertyName, opVal.value, opVal.operation));
                }
            }

            return queries;
        }

        [Obsolete("Use " + nameof(ParseFilterOperationAndValue) + " method instead.")]
        protected virtual (string operation, string value) ParseFilterOperation(string value)
        {
            if (value.Length < 3)
                return (string.Empty, value);

            var operation = GetFilterOperationOld(value);
            var values = value.Split(QueryConstants.COLON);

            if (string.IsNullOrEmpty(operation))
                return (string.Empty, value);

            value = string.Join(QueryConstants.COLON_STR, values.Skip(1));

            return (operation, value);
        }

        /// <summary>
        /// Parse filter operation enum and value by string value.
        /// Input string can contain:
        /// a) property value only, then FilterOperations.eq and value is returned
        /// b) filter prefix and value e.g. "prefix:value", then FilterOperations.prefix and value is returned
        /// In case of prefix is provided and is not in FilterOperations enum,
        /// invalid filter prefix exception is thrown.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual (FilterOperations operation, string value) ParseFilterOperationAndValue(string input)
        {
            // value is empty
            if (input.Length == 0)
                return (FilterOperations.eq, input);

            // split value
            var values = input.Split(QueryConstants.COLON);
            // value only
            if (values.Length == 1)
                return (FilterOperations.eq, input);
            // prefix:value
            else if (values.Length == 2)
            {
                var (operation, succeeded) = GetFilterOperation(values[0]);
                if (succeeded == false)
                    throw new JsonApiException(400, $"Invalid filter prefix '{values[0]}'");

                return (operation, values[1]);
            }
            // some:colon:value OR prefix:some:colon:value (datetime)
            else
            {
                // succeeded = false means no prefix found => some value with colons(datetime)
                // succeeded = true means prefix provide + some value with colons(datetime)
                var (operation, succeeded) = GetFilterOperation(values[0]);
                var value = "";
                // datetime
                if (succeeded == false)
                    value = string.Join(QueryConstants.COLON_STR, values);
                else
                    value = string.Join(QueryConstants.COLON_STR, values.Skip(1));
                return (operation, value);
            }
        }

        /// <summary>
        /// Returns typed operation result and info about parsing success
        /// </summary>
        /// <param name="operation">String represented operation</param>
        /// <returns></returns>
        private static (FilterOperations operation, bool succeeded) GetFilterOperation(string operation)
        {
            var success = Enum.TryParse(operation, out FilterOperations opertion);
            return (opertion, success);
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
                throw new JsonApiException(400, $"Invalid page size '{value}'");

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

            const string ID = "Id";
            var includedFields = new List<string> { ID };

            // this will not support nested inclusions, it requires that the typeName is the current request type
            if (string.Equals(typeName, _controllerContext.RequestEntity.EntityName, StringComparison.OrdinalIgnoreCase) == false)
                return includedFields;

            var fields = value.Split(QueryConstants.COMMA);
            foreach (var field in fields)
            {
                var attr = GetAttribute(field);
                var internalAttrName = attr.InternalAttributeName;
                includedFields.Add(internalAttrName);
            }

            return includedFields;
        }

        private string GetFilterOperationOld(string value)
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

        protected virtual AttrAttribute GetAttribute(string attribute)
        {
            try
            {
                return _controllerContext
                    .RequestEntity
                    .Attributes
                    .Single(attr => attr.Is(attribute));
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiException(400, $"Attribute '{attribute}' does not exist on resource '{_controllerContext.RequestEntity.EntityName}'", e);
            }
        }
    }
}
