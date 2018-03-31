using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app, bool useMvc = true)
        {
            var environment = (IHostingEnvironment)app.ApplicationServices.GetService(typeof(IHostingEnvironment));

            if(environment.IsProduction())
            {
                JsonApiOptions.DisableErrorStackTraces = true;
                JsonApiOptions.DisableErrorSource = true;
            }

            app.UseMiddleware<RequestMiddleware>();

            if (useMvc)
                app.UseMvc();

            return app;
        }
    }
}
