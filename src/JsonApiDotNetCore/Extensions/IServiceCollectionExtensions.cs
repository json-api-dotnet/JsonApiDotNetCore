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
            var mvcBuilder = services.AddMvc();
            AddJsonApi<TContext>(services, (opt) => {}, mvcBuilder);
        }

        public static void AddJsonApi<TContext>(this IServiceCollection services, Action<JsonApiOptions> options) 
            where TContext : DbContext
        {
            var mvcBuilder = services.AddMvc();
            AddJsonApi<TContext>(services, options, mvcBuilder);
        }

         public static void AddJsonApi<TContext>(this IServiceCollection services, 
            Action<JsonApiOptions> options,
            IMvcBuilder mvcBuilder) where TContext : DbContext
        {
            var config = new JsonApiOptions();
            
            options(config);

            if(config.ContextGraph == null)
                config.BuildContextGraph<TContext>(null);

            services.AddScoped(typeof(DbContext), typeof(TContext));

            mvcBuilder
                .AddMvcOptions(opt => {
                    opt.Filters.Add(typeof(JsonApiExceptionFilter));
                    opt.SerializeAsJsonApi(config);
                });

            AddJsonApiInternals(services, config);
        }

        public static void AddJsonApi(this IServiceCollection services, 
            Action<JsonApiOptions> options,
            IMvcBuilder mvcBuilder)
        {
            var config = new JsonApiOptions();
            
            options(config);

            AddJsonApiInternals(services, config);
        }

        public static void AddJsonApiInternals(
            this IServiceCollection services, 
            JsonApiOptions jsonApiOptions)
        {
            services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));
            services.AddScoped(typeof(IResourceService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IResourceService<,>), typeof(EntityResourceService<,>));
            services.AddSingleton<JsonApiOptions>(jsonApiOptions);
            services.AddSingleton<IContextGraph>(jsonApiOptions.ContextGraph);
            services.AddScoped<IJsonApiContext,JsonApiContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<JsonApiRouteHandler>();
            services.AddScoped<IMetaBuilder, MetaBuilder>();
            services.AddScoped<IDocumentBuilder, DocumentBuilder>();
            services.AddScoped<IJsonApiSerializer, JsonApiSerializer>();
            services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            services.AddScoped<IJsonApiDeSerializer, JsonApiDeSerializer>();
            services.AddScoped<IJsonApiReader, JsonApiReader>();
            services.AddScoped<IGenericProcessorFactory, GenericProcessorFactory>();
            services.AddScoped(typeof(GenericProcessor<>));
        }

        public static void SerializeAsJsonApi(this MvcOptions options, JsonApiOptions jsonApiOptions)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());
            
            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());

            options.Conventions.Insert(0, new DasherizedRoutingConvention(jsonApiOptions.Namespace));
        }
    }
}
