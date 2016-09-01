using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace JsonApiDotNetCore.Middleware
{
  public class JsonApiMiddleware
  {
      private readonly RequestDelegate _next;
      private readonly ILogger _logger;
      private readonly IRouter _router;
      private readonly IServiceProvider _serviceProvider;

      public JsonApiMiddleware(RequestDelegate next,
        ILogger<JsonApiMiddleware> logger,
        IRouter router,
        IServiceProvider serviceProvider)
      {
        _next = next;
        _logger = logger;
        _router = router;
        _serviceProvider = serviceProvider;
      }

      public async Task Invoke(HttpContext context)
      {
        _logger.LogInformation("Passing request to JsonApiService: " + context.Request.Path);

        if(IsJsonApiRequest(context)) {
          var routeWasHandled = _router.HandleJsonApiRoute(context, _serviceProvider);
          if(!routeWasHandled)
            RespondNotFound(context);
        }
        else
        {
          await _next.Invoke(context);

          RespondUnsupportedMediaType(context);
        }
      }

      private bool IsJsonApiRequest(HttpContext context)
      {
        StringValues acceptHeader;
        if(context.Request.Headers.TryGetValue("Accept", out acceptHeader) && acceptHeader == "application/vnd.api+json")
        {
          if(context.Request.ContentLength > 0) {
            if(context.Request.ContentType == "application/vnd.api+json") {
              return true;
            }
            _logger.LogInformation("Content-Type invalid for JsonAPI");
            return false;
          }
          return true;
        }

        _logger.LogInformation("Accept header invalid for JsonAPI");

        return false;
      }

      private void RespondUnsupportedMediaType(HttpContext context)
      {
        context.Response.StatusCode = 415;
        context.Response.Body.Flush();
      }

      private void RespondNotFound(HttpContext context)
      {
        context.Response.StatusCode = 404;
        context.Response.Body.Flush();
      }
  }
}
