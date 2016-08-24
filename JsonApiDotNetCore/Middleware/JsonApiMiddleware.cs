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

        var wasHandled = _jsonApiService.HandleJsonApiRoute(context, _serviceProvider);

        if(!wasHandled) {
          _logger.LogInformation("Request not handled by JsonApiService. Middleware pipeline continued.");
          await _next.Invoke(context);
        }
        else {
          _logger.LogInformation("Request handled by JsonApiService. Middleware pipeline terminated.");
        }
      }
  }
}
