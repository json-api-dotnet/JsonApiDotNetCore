using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCore.Extensions
{
  public static class IApplicationBuilderExtensions
  {
    public static IApplicationBuilder UseJsonApi(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<JsonApiMiddleware>();
    }
  }
}
