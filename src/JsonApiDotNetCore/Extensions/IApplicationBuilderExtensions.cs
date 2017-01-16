using System;
using JsonApiDotNetCore.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Routing
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            // Verify if AddMvc was done before calling UseMvc
            // We use the MvcMarkerService to make sure if all the services were added.
            if (app.ApplicationServices.GetService(typeof(MvcMarkerService)) == null)
                throw new InvalidOperationException();

            var middlewarePipelineBuilder = app.ApplicationServices.GetRequiredService<MiddlewareFilterBuilder>();
            middlewarePipelineBuilder.ApplicationBuilder = app.New();

            var routes = new RouteBuilder(app)
            {
                DefaultHandler = app.ApplicationServices.GetRequiredService<JsonApiRouteHandler>(),
            };

            routes.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(app.ApplicationServices));

            return app.UseRouter(routes.Build());
        }
    }
}
