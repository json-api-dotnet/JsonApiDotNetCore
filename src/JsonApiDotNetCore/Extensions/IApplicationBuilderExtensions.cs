using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        /// <param name="skipRegisterMiddleware">Do not register any middleware</param>
        /// <param name="useAuthentication">Register Authentication middleware</param>
        /// <param name="useAuthorization">Register Authorization middleware</param>
        /// <returns></returns>
        public static void UseJsonApi(this IApplicationBuilder app, bool skipRegisterMiddleware = false, bool useAuthentication = false, bool useAuthorization = false)
        {
            LogResourceGraphValidations(app);
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var inverseRelationshipResolver = scope.ServiceProvider.GetService<IInverseRelationships>();
                inverseRelationshipResolver?.Resolve();
            }

            if (!skipRegisterMiddleware) {
                // An endpoint is selected and set on the HttpContext if a match is found
                app.UseRouting();

                if (useAuthentication)
                {
                    app.UseAuthentication();
                };

                if (useAuthorization)
                {
                    app.UseAuthorization();
                };

                // middleware to run after routing occurs.
                app.UseMiddleware<CurrentRequestMiddleware>();

                // Executes the endpoints that was selected by routing.
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }
        }

        /// <summary>
        /// Configures your application to return stack traces in error results.
        /// </summary>
        /// <param name="app"></param>
        public static void EnableDetailedErrors(this IApplicationBuilder app)
        {
            JsonApiOptions.DisableErrorStackTraces = false;
            JsonApiOptions.DisableErrorSource = false;
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
