using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCore.Routing
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app)
        {
            app.UseMvc();
            
            return app;
        }
    }
}
