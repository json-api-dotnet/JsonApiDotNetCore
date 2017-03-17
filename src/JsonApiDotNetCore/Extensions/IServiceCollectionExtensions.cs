using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddJsonApi<TContext>(this IServiceCollection services) 
            where TContext : DbContext
        {
            _addInternals<TContext>(services, new JsonApiOptions());
        }

        public static void AddJsonApi<TContext>(this IServiceCollection services, Action<JsonApiOptions> options) 
            where TContext : DbContext
        {
            var config = new JsonApiOptions();
            options(config);
            _addInternals<TContext>(services, config);
        }

        private static void _addInternals<TContext>(IServiceCollection services, JsonApiOptions jsonApiOptions)
            where TContext : DbContext
        {
            services.AddJsonApiInternals<TContext>(jsonApiOptions);
            services.AddMvc()
                .AddMvcOptions(opt => {
                    opt.Filters.Add(typeof(JsonApiExceptionFilter));
                    opt.SerializeAsJsonApi(jsonApiOptions);
                });
        }

        public static void AddJsonApiInternals<TContext>(this IServiceCollection services, JsonApiOptions jsonApiOptions) 
            where TContext : DbContext
        {
            var contextGraphBuilder = new ContextGraphBuilder<TContext>();
            var contextGraph = contextGraphBuilder.Build();

            services.AddScoped(typeof(DbContext), typeof(TContext));
            services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));

            services.AddSingleton<JsonApiOptions>(jsonApiOptions);
            services.AddSingleton<IContextGraph>(contextGraph);
            services.AddScoped<IJsonApiContext,JsonApiContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddScoped<JsonApiRouteHandler>();

            services.AddScoped<IMetaBuilder, MetaBuilder>();
            services.AddScoped<IDocumentBuilder, DocumentBuilder>();
            services.AddScoped<IJsonApiSerializer, JsonApiSerializer>();
            services.AddScoped<IJsonApiWriter, JsonApiWriter>();
        }

        public static void SerializeAsJsonApi(this MvcOptions options, JsonApiOptions jsonApiOptions)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());
            
            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());

            options.Conventions.Insert(0, new DasherizedRoutingConvention(jsonApiOptions.Namespace));
        }
    }
}
