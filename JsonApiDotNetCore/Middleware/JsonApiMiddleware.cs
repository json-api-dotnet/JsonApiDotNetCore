using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Middleware
{
  public class JsonApiMiddleware
  {
      private readonly RequestDelegate _next;
      private readonly ILogger _logger;
      private readonly JsonApiService _jsonApiService;
      private readonly IServiceProvider _serviceProvider;

      public JsonApiMiddleware(RequestDelegate next,
        ILogger<JsonApiMiddleware> logger,
        JsonApiService jsonApiService,
        IServiceProvider serviceProvider)
      {
        _next = next;
        _logger = logger;
        _jsonApiService = jsonApiService;
        _serviceProvider = serviceProvider;
      }

      public async Task Invoke(HttpContext context)
      {
        _logger.LogInformation("Passing request to JsonApiService: " + context.Request.Path);

        if(context.Request.ContentType == "application/vnd.api+json") {
          var wasHandled = _jsonApiService.HandleJsonApiRoute(context, _serviceProvider);
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
  }
}
