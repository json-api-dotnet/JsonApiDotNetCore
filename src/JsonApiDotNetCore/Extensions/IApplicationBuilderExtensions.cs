using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        /// <returns></returns>
        public static void UseJsonApi(this IApplicationBuilder app)
        {
            DisableDetailedErrorsIfProduction(app);
            LogResourceGraphValidations(app);
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var inverseRelationshipResolver = scope.ServiceProvider.GetService<IInverseRelationships>();
                inverseRelationshipResolver?.Resolve();
            }

            // An endpoint is selected and set on the HttpContext if a match is found
            app.UseRouting();

            // middleware to run after routing occurs.
            app.UseMiddleware<CurrentRequestMiddleware>();

            // Executes the endpoints that was selected by routing.
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
