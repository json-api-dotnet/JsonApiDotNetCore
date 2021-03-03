using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Middleware
{
    [PublicAPI]
    public static class HttpContextExtensions
    {
        private const string IsJsonApiRequestKey = "JsonApiDotNetCore_IsJsonApiRequest";

        /// <summary>
        /// Indicates whether the currently executing HTTP request is being handled by JsonApiDotNetCore.
        /// </summary>
        public static bool IsJsonApiRequest(this HttpContext httpContext)
        {
            ArgumentGuard.NotNull(httpContext, nameof(httpContext));

            string value = httpContext.Items[IsJsonApiRequestKey] as string;
            return value == bool.TrueString;
        }

        internal static void RegisterJsonApiRequest(this HttpContext httpContext)
        {
            ArgumentGuard.NotNull(httpContext, nameof(httpContext));

            httpContext.Items[IsJsonApiRequestKey] = bool.TrueString;
        }
    }
}
