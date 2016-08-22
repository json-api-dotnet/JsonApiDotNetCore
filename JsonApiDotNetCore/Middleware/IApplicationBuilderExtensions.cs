using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCore.Middleware
{
  public static class IApplicationBuilderExtensions
  {
      public static IApplicationBuilder UseJsonApi(this IApplicationBuilder builder)
      {
          return builder.UseMiddleware<JsonApiMiddleware>();
      }
  }
}
