using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app, bool useMvc = true)
        {
            DisableDetailedErrorsIfProduction(app);
            LogResourceGraphValidations(app);

            app.UseEndpointRouting();

            app.UseMiddleware<CurrentRequestMiddleware>();

            if (useMvc)
                app.UseMvc();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var inverseRelationshipResolver = scope.ServiceProvider.GetService<IInverseRelationships>();
                inverseRelationshipResolver?.Resolve();
            }

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

        private static void LogResourceGraphValidations(IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetService(typeof(ILogger<ResourceGraphBuilder>)) as ILogger;
            var resourceGraph = app.ApplicationServices.GetService(typeof(IContextEntityProvider)) as ResourceGraph;

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
