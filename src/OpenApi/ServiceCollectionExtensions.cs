using System;
using JsonApiDotNetCore;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenApi
{
    public static class ServiceCollectionExtensions
    {
        public static void AddOpenApi(this IServiceCollection services, IMvcCoreBuilder builder, Action<SwaggerGenOptions> setupSwaggerGenAction)
        {
            ArgumentGuard.NotNull(services, nameof(services));
            ArgumentGuard.NotNull(builder, nameof(builder));

            builder.AddApiExplorer();

            builder.AddMvcOptions(options => options.Conventions.Add(new OpenApiEndpointConvention()));

            services.AddSwaggerGen(setupSwaggerGenAction);
        }
    }
}
