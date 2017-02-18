using Microsoft.AspNetCore.Builder;

namespace JsonApiDotNetCore.Routing
{
    public static class IApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseJsonApi(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var contentType = context.Request.ContentType;
                if (contentType != null)
                {
                    var contentTypeArr = contentType.Split(';');
                    if (contentTypeArr[0] == "application/vnd.api+json" && contentTypeArr.Length == 2)
                    {
                        context.Response.StatusCode = 415;
                        context.Response.Body.Flush();
                        return;
                    }
                }

                await next.Invoke();
            });

            app.UseMvc();

            return app;
        }
    }
}
