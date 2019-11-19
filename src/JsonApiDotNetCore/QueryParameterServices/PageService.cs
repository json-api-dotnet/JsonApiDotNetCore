using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Query;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class PageService : QueryParameterService, IPageService
    {
        private readonly IJsonApiOptions _options;

        public PageService(IJsonApiOptions options)
        {
            _options = options;
            PageSize = _options.DefaultPageSize;
        }

        /// <inheritdoc/>
        public int? TotalRecords { get; set; }

        /// <inheritdoc/>
        public int PageSize { get; set; }

        /// <inheritdoc/>
        public int CurrentPage { get; set; } = 1;

        /// <inheritdoc/>
        public int TotalPages => (TotalRecords == null || PageSize == 0) ? -1 : (int)Math.Ceiling(decimal.Divide(TotalRecords.Value, PageSize));

        /// <inheritdoc/>
        public bool CanPaginate { get { return TotalPages > 1; } }

        /// <inheritdoc/>
        public virtual void Parse(KeyValuePair<string, StringValues> queryParameter)
        {
            // expected input = page[size]=<integer>
            //                  page[number]=<integer > 0>
            var propertyName = queryParameter.Key.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];

            const string SIZE = "size";
            const string NUMBER = "number";

            if (propertyName == SIZE)
            {
                if (!int.TryParse(queryParameter.Value, out var size))
                {
                    ThrowBadPagingRequest(queryParameter, "value could not be parsed as an integer");
                }
                else if (size < 1)
                {
                    ThrowBadPagingRequest(queryParameter, "value needs to be greater than zero");
                }
                else
                {
                    PageSize = size;
                }
            }
            else if (propertyName == NUMBER)
            { 
                if (!int.TryParse(queryParameter.Value, out var number))
                {
                    ThrowBadPagingRequest(queryParameter, "value could not be parsed as an integer");
                }
                else if (number == 0)
                {
                    ThrowBadPagingRequest(queryParameter, "page index is not zero-based");
                }
                else
                {
                    CurrentPage = number;
                }
            }
        }

        private void ThrowBadPagingRequest(KeyValuePair<string, StringValues> parameter, string message)
        {
            throw new JsonApiException(400, $"Invalid page query parameter '{parameter.Key}={parameter.Value}': {message}");
        }

    }
}
