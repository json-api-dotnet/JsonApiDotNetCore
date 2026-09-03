using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Registers the JsonApiDotNetCore middleware.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IApplicationBuilder" /> to add the middleware to.
    /// </param>
    /// <example>
    /// The code below is the minimal that is required for proper activation, which should be added to your Startup.Configure method.
    /// <code><![CDATA[
    /// app.UseRouting();
    /// app.UseJsonApi();
    /// app.UseEndpoints(endpoints => endpoints.MapControllers());
    /// ]]></code>
    /// </example>
    public static void UseJsonApi(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        using (IServiceScope scope = builder.ApplicationServices.CreateScope())
        {
            var inverseNavigationResolver = scope.ServiceProvider.GetRequiredService<IInverseNavigationResolver>();
            inverseNavigationResolver.Resolve();
        }

        builder.UseMiddleware<JsonApiMiddleware>();
    }
}
