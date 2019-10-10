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
        void Parse(IQueryCollection query);
    }

    public class QueryParser : IQueryParser
    {
        private readonly IncludeService _includeService;
        private readonly SparseFieldsService _sparseFieldsService;
        private readonly FilterService _filterService;
        private readonly SortService _sortService;
        private readonly OmitDefaultService _omitDefaultService;
        private readonly OmitNullService _omitNull;
        private readonly PageService _pageService;

        private readonly ICurrentRequest _currentRequest;
        private readonly IContextEntityProvider _provider;
        private readonly IJsonApiOptions _options;
        private readonly IServiceProvider _sp;
        private ContextEntity _primaryResource;

        public QueryParser(
            ICurrentRequest currentRequest,
            IContextEntityProvider provider,
            IJsonApiOptions options)
        {
            _currentRequest = currentRequest;
            _provider = provider;
            _options = options;
        }

        public virtual void Parse(IQueryCollection query)
        {

            _primaryResource = _currentRequest.GetRequestResource();
            var disabledQueries = _currentRequest.DisabledQueryParams;

            foreach (var pair in query)
            {
                if (pair.Key.StartsWith(QueryConstants.FILTER, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Filters) == false)
                        _filterService.Parse(pair.Key, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.SORT, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Sort) == false)
                        //querySet.SortParameters = ParseSortParameters(pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(_includeService.Name, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Include) == false)
                        _includeService.Parse(null, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.PAGE, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Page) == false)
                        //querySet.PageQuery = ParsePageQuery(querySet.PageQuery, pair.Key, pair.Value);
                    continue;
                }

                if (pair.Key.StartsWith(QueryConstants.FIELDS, StringComparison.Ordinal))
                {
                    if (disabledQueries.HasFlag(QueryParams.Fields) == false)
                        _sparseFieldsService.Parse(pair.Key, pair.Value);
                    continue;
                }

                if (_options.AllowCustomQueryParameters == false)
                    throw new JsonApiException(400, $"{pair} is not a valid query.");
            }
        }

        private void GetQueryParameterServices()
        {
            var type = typeof(IQueryParameterService);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsInterface && t.Inherits(type))
                .Select(t => (IQueryParameterService)_sp.GetService(t));
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


    }
}
