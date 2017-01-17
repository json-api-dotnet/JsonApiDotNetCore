using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void AddJsonApi<TContext>(this IServiceCollection services) 
            where TContext : DbContext
        {
            services.AddJsonApiInternals<TContext>();
            services.AddMvc()
                .AddMvcOptions(options => options.SerializeAsJsonApi());
        }

        public static void AddJsonApiInternals<TContext>(this IServiceCollection services) 
            where TContext : DbContext
        {
            var contextGraphBuilder = new ContextGraphBuilder<TContext>();
            var contextGraph = contextGraphBuilder.Build();

            services.AddScoped(typeof(DbContext), typeof(TContext));

            services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));

            services.AddSingleton<IContextGraph>(contextGraph);
            services.AddScoped<IJsonApiContext,JsonApiContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<JsonApiRouteHandler>();
        }

        public static void SerializeAsJsonApi(this MvcOptions options)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());
            
            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());
        }
    }
}
