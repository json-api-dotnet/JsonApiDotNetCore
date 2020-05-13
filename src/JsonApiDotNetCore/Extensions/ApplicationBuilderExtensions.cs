using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Validates the resource graph and optionally registers the JsonApiDotNetCore middleware.
        /// </summary>
        /// <remarks>
        /// The <paramref name="skipRegisterMiddleware"/> can be used to skip any middleware registration, in which case the developer
        /// is responsible for registering required middleware.
        /// </remarks>
        /// <param name="app"></param>
        /// <param name="skipRegisterMiddleware">Indicates to not register any middleware. This enables callers to take full control of middleware registration order.</param>
        /// <param name="useAuthentication">Indicates if 'app.UseAuthentication()' should be called. Ignored when <paramref name="skipRegisterMiddleware"/> is set to true.</param>
        /// <param name="useAuthorization">Indicates if 'app.UseAuthorization()' should be called. Ignored when <paramref name="skipRegisterMiddleware"/> is set to true.</param>
        /// <example>
        /// The next example illustrates how to manually register middleware.
        /// <code>
        /// app.UseJsonApi(skipRegisterMiddleware: true);
        /// app.UseRouting();
        /// app.UseMiddleware{JsonApiMiddleware}();
        /// app.UseEndpoints(endpoints => endpoints.MapControllers());
        /// </code>
        /// </example>
        public static void UseJsonApi(this IApplicationBuilder app, bool skipRegisterMiddleware = false, bool useAuthentication = false, bool useAuthorization = false)
        {
            if (!skipRegisterMiddleware) 
            {
                // An endpoint is selected and set on the HttpContext if a match is found
                app.UseRouting();

                if (useAuthentication)
                {
                    app.UseAuthentication();
                }

                if (useAuthorization)
                {
                    app.UseAuthorization();
                }

                // middleware to run after routing occurs.
                app.UseMiddleware<JsonApiMiddleware>();

                // Executes the endpoints that was selected by routing.
                app.UseEndpoints(endpoints => endpoints.MapControllers());
            }
        }
    }
}
