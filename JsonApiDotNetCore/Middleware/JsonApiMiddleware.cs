using System;
using System.Threading.Tasks;
using JsonApiDotNetCore.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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

        if(context.Request.ContentType == "application/vnd.api+json") {
          var routeWasHandled = _router.HandleJsonApiRoute(context, _serviceProvider);
          if(!routeWasHandled)
            RespondNotFound(context);
        }
        else
        {
          _logger.LogInformation("Content-Type invalid for JsonAPI");

          await _next.Invoke(context);

          RespondUnsupportedMediaType(context);
        }
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
