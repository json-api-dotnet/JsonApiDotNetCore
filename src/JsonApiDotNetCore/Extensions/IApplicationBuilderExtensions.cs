using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app, bool useMvc = true)
        {
            DisableDetailedErrorsIfProduction(app);
            LogContextGraphValidations(app);

            app.UseMiddleware<RequestMiddleware>();

            if (useMvc)
                app.UseMvc();

            return app;
        }

        private static void DisableDetailedErrorsIfProduction(IApplicationBuilder app)
        {
            var environment = (IHostingEnvironment)app.ApplicationServices.GetService(typeof(IHostingEnvironment));

            if (environment.IsProduction())
            {
                JsonApiOptions.DisableErrorStackTraces = true;
                JsonApiOptions.DisableErrorSource = true;
            }
        }

        private static void LogContextGraphValidations(IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetService(typeof(ILogger<ContextGraphBuilder>)) as ILogger;
            var contextGraph = app.ApplicationServices.GetService(typeof(IContextGraph)) as ContextGraph;

            if (logger != null && contextGraph != null)
            {
                contextGraph.ValidationResults.ForEach((v) =>
                    logger.Log(
                        v.LogLevel,
                        new EventId(),
                        v.Message,
                        exception: null,
                        formatter: (m, e) => m));
            }
        }
    }
}
