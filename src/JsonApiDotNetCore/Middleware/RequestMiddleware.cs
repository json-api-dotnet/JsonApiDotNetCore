using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Extensions;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Middleware
{
    public class RequestMiddleware
    {
        private readonly RequestDelegate _next;
        
        public RequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (IsValid(context))
                await _next(context);
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
            if (context.Request.Headers.TryGetValue(Constants.AcceptHeader, out StringValues acceptHeaders) == false)
                return true;

            foreach (var acceptHeader in acceptHeaders)
            {
                if (ContainsMediaTypeParameters(acceptHeader) == false)
                    continue;

                FlushResponse(context, 406);
                return false;
            }
            return true;
        }

        private static bool ContainsMediaTypeParameters(string mediaType)
        {
            const char delimeter = ';';
            var subSpans = mediaType.SpanSplit(delimeter);
            if (subSpans.Count == 0) return false;
            return subSpans.Count == 2 && subSpans[0].ToString() == Constants.ContentType;
        }

        private static void FlushResponse(HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Body.Flush();
        }
    }
}
