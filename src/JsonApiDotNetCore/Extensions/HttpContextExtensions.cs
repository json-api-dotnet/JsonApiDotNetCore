using Microsoft.AspNetCore.Http;

namespace JsonApiDotNetCore.Extensions
{
    public static class HttpContextExtensions
    {
        public static bool IsJsonApiRequest(this HttpContext httpContext)
        {
            string value = httpContext.Items["IsJsonApiRequest"] as string;
            return value == bool.TrueString;
        }

        internal static void SetJsonApiRequest(this HttpContext httpContext)
        {
            httpContext.Items["IsJsonApiRequest"] = bool.TrueString;
        }
    }
}
