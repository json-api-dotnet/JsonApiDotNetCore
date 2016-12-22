using System;
using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCore.Routing
{
    public static class RouteBuilderExtensions
    {
        // public static void UseJsonApi(this IRouteBuilder routes)
        // {
        //     var defaultHandler = routes.ApplicationBuilder.ApplicationServices.GetRequiredService<MvcRouteHandler>();
        //     routes.MapRoute(
        //             name: "default",
        //             template: "{controller=Home}/{action=Index}/{id?}");
        // }

        // private static RequestDelegate RequestHandler()
        // {
        //     return (context) => {
        //         var model = context.GetRouteValue("model");
        //         return context.Response.WriteAsync($"model: {model}");
        //     };
        // }

        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMvc();
            // routes =>
            // { 
            //     routes.MapRoute(
            //         name: "relational",
            //         template: "{controller}/{id:int}");
            // }
        }
    }
}
