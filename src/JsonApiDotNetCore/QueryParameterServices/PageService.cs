using System;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Internal.Query;
using JsonApiDotNetCore.Managers.Contracts;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Query
{
    /// <inheritdoc/>
    public class PageService : QueryParameterService, IPageService
    {
        private readonly IJsonApiOptions _options;
        public PageService(IJsonApiOptions options, IResourceGraph resourceGraph, ICurrentRequest currentRequest) : base(resourceGraph, currentRequest)
        {
            _options = options;
            DefaultPageSize = _options.DefaultPageSize;
        }

        /// <summary>
        /// constructor used for unit testing
        /// </summary>
        internal PageService(IJsonApiOptions options)
        {
            _options = options;
            DefaultPageSize = _options.DefaultPageSize;
        }

        /// <inheritdoc/>
        public int PageSize
        {
            get
            {
                if (RequestedPageSize.HasValue)
                {
                    return RequestedPageSize.Value;
                }
                return DefaultPageSize;
            }
        }

        /// <inheritdoc/>
        public int DefaultPageSize { get; set; }

        /// <inheritdoc/>
        public int? RequestedPageSize { get; set; }

        /// <inheritdoc/>
        public int CurrentPage { get; set; } = 1;

        /// <inheritdoc/>
        public bool Backwards { get; set; }

        /// <inheritdoc/>
        public int TotalPages => (TotalRecords == null || PageSize == 0) ? -1 : (int)Math.Ceiling(decimal.Divide(TotalRecords.Value, PageSize));

        /// <inheritdoc/>
        public bool CanPaginate => TotalPages > 1;

        /// <inheritdoc/>
        public int? TotalRecords { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled(DisableQueryAttribute disableQueryAttribute)
        {
            return !disableQueryAttribute.ContainsParameter(StandardQueryStringParameters.Page);
        }

        /// <inheritdoc/>
        public bool CanParse(string parameterName)
        {
            return parameterName == "page[size]" || parameterName == "page[number]";
        }

        /// <inheritdoc/>
        public virtual void Parse(string parameterName, StringValues parameterValue)
        {
            EnsureNoNestedResourceRoute(parameterName);
            // expected input = page[size]=<integer>
            //                  page[number]=<integer greater than zero> 
            var propertyName = parameterName.Split(QueryConstants.OPEN_BRACKET, QueryConstants.CLOSE_BRACKET)[1];

            const string SIZE = "size";
            const string NUMBER = "number";

            if (propertyName == SIZE)
            {
                if (!int.TryParse(parameterValue, out var size))
                {
                    ThrowBadPagingRequest(parameterName, parameterValue, "value could not be parsed as an integer");
                }
                else if (size < 1)
                {
                    ThrowBadPagingRequest(parameterName, parameterValue, "value needs to be greater than zero");
                }
                else if (size > _options.MaximumPageSize)
                {
                    ThrowBadPagingRequest(parameterName, parameterValue, $"page size cannot be higher than {_options.MaximumPageSize}.");
                }
                else
                {
                    RequestedPageSize = size;
                }
            }
            else if (propertyName == NUMBER)
            { 
                if (!int.TryParse(parameterValue, out var number))
                {
                    ThrowBadPagingRequest(parameterName, parameterValue, "value could not be parsed as an integer");
                }
                else if (number == 0)
                {
                    ThrowBadPagingRequest(parameterName, parameterValue, "page index is not zero-based");
                }
                else if (number > _options.MaximumPageNumber)
                {
                    ThrowBadPagingRequest(parameterName, parameterValue, $"page index cannot be higher than {_options.MaximumPageNumber}.");
                }
                else
                {
                    Backwards = (number < 0);
                    CurrentPage = Math.Abs(number);
                }
            }
        }

        private void ThrowBadPagingRequest(string parameterName, StringValues parameterValue, string message)
        {
            throw new JsonApiException(HttpStatusCode.BadRequest, $"Invalid page query parameter '{parameterName}={parameterValue}': {message}");
        }
    }
}
