using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds necessary components such as routing to your application
        /// </summary>
        /// <param name="app"></param>
        /// <param name="useMvc"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app, bool useMvc = true)
        {
            DisableDetailedErrorsIfProduction(app);
            LogResourceGraphValidations(app);
            app.UseMiddleware<CurrentRequestMiddleware>();
            app.UseRouting();
            if (useMvc)
            {
                app.UseMvc();
            }
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var inverseRelationshipResolver = scope.ServiceProvider.GetService<IInverseRelationships>();
                inverseRelationshipResolver?.Resolve();
            }
            return app;
        }

        private static void DisableDetailedErrorsIfProduction(IApplicationBuilder app)
        {
            var webHostEnvironment = (IWebHostEnvironment) app.ApplicationServices.GetService(typeof(IWebHostEnvironment));
            if (webHostEnvironment.EnvironmentName == "Production")
            {
                JsonApiOptions.DisableErrorStackTraces = true;
                JsonApiOptions.DisableErrorSource = true;
            }
        }

        private static void LogResourceGraphValidations(IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetService(typeof(ILogger<ResourceGraphBuilder>)) as ILogger;
            var resourceGraph = app.ApplicationServices.GetService(typeof(IResourceGraph)) as ResourceGraph;

            if (logger != null && resourceGraph != null)
            {
                resourceGraph.ValidationResults.ForEach((v) =>
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
