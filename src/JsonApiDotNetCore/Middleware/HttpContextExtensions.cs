using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Middleware
{
    public static class HttpContextExtensions
    {
        private const string _isJsonApiRequestKey = "JsonApiDotNetCore_IsJsonApiRequest";

        /// <summary>
        /// Indicates whether the currently executing HTTP request is being handled by JsonApiDotNetCore.
        /// </summary>
        public static bool IsJsonApiRequest(this HttpContext httpContext)
        {
            ArgumentGuard.NotNull(httpContext, nameof(httpContext));

            string value = httpContext.Items[_isJsonApiRequestKey] as string;
            return value == bool.TrueString;
        }

        internal static void RegisterJsonApiRequest(this HttpContext httpContext)
        {
            ArgumentGuard.NotNull(httpContext, nameof(httpContext));

            httpContext.Items[_isJsonApiRequestKey] = bool.TrueString;
        }
    }
}
