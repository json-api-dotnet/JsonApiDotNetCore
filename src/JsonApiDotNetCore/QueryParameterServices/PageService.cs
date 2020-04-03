using System;
using System.Collections.Generic;
using System.Net;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Exceptions;
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

            if (propertyName == "size")
            {
                RequestedPageSize = ParsePageSize(parameterValue, _options.MaximumPageSize);
            }
            else if (propertyName == "number")
            {
                var number = ParsePageNumber(parameterValue, _options.MaximumPageNumber);

                // TODO: It doesn't seem right that a negative paging value reverses the sort order.
                // A better way would be to allow ?sort=- to indicate reversing results.
                // Then a negative paging value, like -5, could mean: "5 pages back from the last page"

                Backwards = number < 0;
                CurrentPage = Backwards ? -number : number;
            }
        }

        private int ParsePageSize(string parameterValue, int? maxValue)
        {
            bool success = int.TryParse(parameterValue, out int number);
            if (success && number >= 1)
            {
                if (maxValue == null || number <= maxValue)
                {
                    return number;
                }
            }

            var message = maxValue == null
                ? $"Value '{parameterValue}' is invalid, because it must be a whole number that is greater than zero."
                : $"Value '{parameterValue}' is invalid, because it must be a whole number that is greater than zero and not higher than {maxValue}.";

            throw new InvalidQueryStringParameterException("page[size]",
                "The specified value is not in the range of valid values.", message);
        }

        private int ParsePageNumber(string parameterValue, int? maxValue)
        {
            bool success = int.TryParse(parameterValue, out int number);
            if (success && number != 0)
            {
                if (maxValue == null || (number >= 0 ? number <= maxValue : number >= -maxValue))
                {
                    return number;
                }
            }

            var message = maxValue == null
                ? $"Value '{parameterValue}' is invalid, because it must be a whole number that is non-zero."
                : $"Value '{parameterValue}' is invalid, because it must be a whole number that is non-zero and not higher than {maxValue} or lower than -{maxValue}.";

            throw new InvalidQueryStringParameterException("page[number]",
                "The specified value is not in the range of valid values.", message);
        }
    }
}
