using System;

namespace JsonApiDotNetCore.Controllers
{
    public class DisableQueryAttribute : Attribute
    {
        /// <summary>
        /// Disabled one of the native query parameters for a controller.
        /// </summary>
        /// <param name="queryParams"></param>
        public DisableQueryAttribute(QueryParams queryParams)
        {
            QueryParams = queryParams.ToString("G").ToLower();
        }

        /// <summary>
        /// It is allowed to use strings to indicate which query parameters
        /// should be disabled, because the user may have defined a custom
        /// query parameter that is not included in the  <see cref="QueryParams"/> enum.
        /// </summary>
        /// <param name="customQueryParams"></param>
        public DisableQueryAttribute(string customQueryParams)
        {
            QueryParams = customQueryParams.ToLower();
        }

        public string QueryParams { get; }
    }
}