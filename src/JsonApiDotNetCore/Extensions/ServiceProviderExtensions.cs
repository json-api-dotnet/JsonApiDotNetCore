using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void AddJsonApi<T>(this IServiceCollection services) where T : DbContext
        {
            var contextGraphBuilder = new ContextGraphBuilder<T>();
            var contextGraph = contextGraphBuilder.Build();

            var jsonApiContext = new JsonApiContext();
            jsonApiContext.ContextGraph = contextGraph;

            services.AddSingleton<IJsonApiContext>(jsonApiContext);
        }
    }
}
