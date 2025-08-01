using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        AssertAspNetCoreOpenApiIsNotRegistered(builder.ApplicationServices);

        using (IServiceScope scope = builder.ApplicationServices.CreateScope())
        {
            var inverseNavigationResolver = scope.ServiceProvider.GetRequiredService<IInverseNavigationResolver>();
            inverseNavigationResolver.Resolve();
        }

        builder.UseMiddleware<JsonApiMiddleware>();
    }

    private static void AssertAspNetCoreOpenApiIsNotRegistered(IServiceProvider serviceProvider)
    {
        Type? optionsType = TryLoadOptionsType();

        if (optionsType != null)
        {
            Type configureType = typeof(IConfigureOptions<>).MakeGenericType(optionsType);
            object? configureInstance = serviceProvider.GetService(configureType);

            if (configureInstance != null)
            {
                throw new InvalidConfigurationException("JsonApiDotNetCore is incompatible with ASP.NET OpenAPI. " +
                    "Remove 'services.AddOpenApi()', or replace it by calling 'services.AddOpenApiForJsonApi()' after 'services.AddJsonApi()' " +
                    "from the JsonApiDotNetCore.OpenApi.Swashbuckle NuGet package.");
            }
        }
    }

    private static Type? TryLoadOptionsType()
    {
        try
        {
            return Type.GetType("Microsoft.AspNetCore.OpenApi.OpenApiOptions, Microsoft.AspNetCore.OpenApi");
        }
        catch (FileLoadException)
        {
            return null;
        }
    }
}
