using JsonApiDotNetCore.Internal;
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
            services.AddSingleton(contextGraph);
        }
    }
}
