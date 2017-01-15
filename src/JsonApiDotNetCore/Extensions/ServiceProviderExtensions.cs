using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void AddJsonApi<T>(this IServiceCollection services) where T : DbContext
        {
            services.AddJsonApiInternals<T>();
            services.AddMvc()
                .AddMvcOptions(options => options.SerializeAsJsonApi());
        }

        public static void AddJsonApiInternals<T>(this IServiceCollection services) where T : DbContext
        {
            var contextGraphBuilder = new ContextGraphBuilder<T>();
            var contextGraph = contextGraphBuilder.Build();

            services.AddSingleton<IContextGraph>(contextGraph);
            services.AddSingleton<IJsonApiContext, JsonApiContext>();
        }

        public static void SerializeAsJsonApi(this MvcOptions options)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());

            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());
        }
    }
}
