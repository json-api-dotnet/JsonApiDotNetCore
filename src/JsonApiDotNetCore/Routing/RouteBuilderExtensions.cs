using System;
using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCore.Routing
{
    public static class RouteBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            return app.UseMvc();
        }
    }
}
