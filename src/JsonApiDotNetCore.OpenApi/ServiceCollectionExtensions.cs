using System;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the OpenAPI integration to JsonApiDotNetCore by configuring Swashbuckle.
        /// </summary>
        public static void AddOpenApi(this IServiceCollection services, IMvcCoreBuilder mvcBuilder, Action<SwaggerGenOptions> setupSwaggerGenAction = null)
        {
            ArgumentGuard.NotNull(services, nameof(services));
            ArgumentGuard.NotNull(mvcBuilder, nameof(mvcBuilder));

            mvcBuilder.AddApiExplorer();

            mvcBuilder.AddMvcOptions(options => options.Conventions.Add(new OpenApiEndpointConvention()));

            services.AddSwaggerGen(setupSwaggerGenAction);
        }
    }
}
