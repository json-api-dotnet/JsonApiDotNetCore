using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Routing
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                if (IsValid(context))
                    await next.Invoke();
            });

            app.UseMvc();

            return app;
        }

        private static bool IsValid(HttpContext context)
        {
            return IsValidContentTypeHeader(context) && IsValidAcceptHeader(context);
        }

        private static bool IsValidContentTypeHeader(HttpContext context)
        {
            var contentType = context.Request.ContentType;
            if (contentType != null && ContainsMediaTypeParameters(contentType))
            {
                FlushResponse(context, 415);
                return false;
            }
            return true;
        }

        private static bool IsValidAcceptHeader(HttpContext context)
        {
            var acceptHeaders = new StringValues();
            if (context.Request.Headers.TryGetValue("Accept", out acceptHeaders))
            {
                foreach (var acceptHeader in acceptHeaders)
                {
                    if (ContainsMediaTypeParameters(acceptHeader))
                    {
                        FlushResponse(context, 406);
                        return false;
                    }
                }
            }
            return true;
        }

        private static bool ContainsMediaTypeParameters(string mediaType)
        {
            var mediaTypeArr = mediaType.Split(';');
            return (mediaTypeArr[0] == "application/vnd.api+json" && mediaTypeArr.Length == 2);
        }

        private static void FlushResponse(HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Body.Flush();
        }
    }
}
