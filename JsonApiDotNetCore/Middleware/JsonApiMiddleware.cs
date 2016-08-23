using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Middleware
{
  public class JsonApiMiddleware
  {
      private readonly RequestDelegate _next;
      private readonly ILogger _logger;

      public JsonApiMiddleware(RequestDelegate next, ILogger<JsonApiMiddleware> logger)
      {
          _next = next;
          _logger = logger;
      }

      public async Task Invoke(HttpContext context)
      {
          _logger.LogInformation("Handling request: " + context.Request.Path);
          await _next.Invoke(context);
          _logger.LogInformation("Finished handling request.");
      }
  }
}
