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
        _logger.LogInformation("Handling request: " + context.Request.Path);

        _jsonApiService.HandleJsonApiRoute(context, _serviceProvider);

        await _next.Invoke(context);

        _logger.LogInformation("Finished handling request.");
      }
  }
}
