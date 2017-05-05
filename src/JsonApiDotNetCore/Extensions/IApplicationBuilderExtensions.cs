using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app, bool useMvc = true)
        {
            app.UseMiddleware<RequestMiddleware>();

            if (useMvc)
                app.UseMvc();

            return app;
        }
    }
}
