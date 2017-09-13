using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Services.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        public static void AddJsonApi<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            var mvcBuilder = services.AddMvc();
            AddJsonApi<TContext>(services, (opt) => { }, mvcBuilder);
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

            config.BuildContextGraph(builder => builder.AddDbContext<TContext>());

            mvcBuilder
                .AddMvcOptions(opt =>
                {
                    opt.Filters.Add(typeof(JsonApiExceptionFilter));
                    opt.SerializeAsJsonApi(config);
                });

            AddJsonApiInternals<TContext>(services, config);
        }

        public static void AddJsonApi(this IServiceCollection services,
            Action<JsonApiOptions> options,
            IMvcBuilder mvcBuilder)
        {
            var config = new JsonApiOptions();

            options(config);

            mvcBuilder
                .AddMvcOptions(opt =>
                {
                    opt.Filters.Add(typeof(JsonApiExceptionFilter));
                    opt.SerializeAsJsonApi(config);
                });

            AddJsonApiInternals(services, config);
        }

        public static void AddJsonApiInternals<TContext>(
            this IServiceCollection services,
            JsonApiOptions jsonApiOptions) where TContext : DbContext
        {
            if (jsonApiOptions.ContextGraph == null)
                jsonApiOptions.BuildContextGraph<TContext>(null);

            services.AddScoped(typeof(DbContext), typeof(TContext));
            AddJsonApiInternals(services, jsonApiOptions);
        }

        public static void AddJsonApiInternals(
            this IServiceCollection services,
            JsonApiOptions jsonApiOptions)
        {
            if (!jsonApiOptions.ContextGraph.UsesDbContext)
            {
                services.AddScoped<DbContext>();
                services.AddSingleton<DbContextOptions>(new DbContextOptionsBuilder().Options);
            }

            if (jsonApiOptions.EnabledExtensions.Contains(JsonApiExtension.Operations))
                AddOperationServices(services);

            services.AddScoped<IDbContextResolver, DbContextResolver>();
            services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));
            services.AddScoped(typeof(IResourceService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IResourceService<,>), typeof(EntityResourceService<,>));
            services.AddSingleton<JsonApiOptions>(jsonApiOptions);
            services.AddSingleton<IContextGraph>(jsonApiOptions.ContextGraph);
            services.AddScoped<IJsonApiContext, JsonApiContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<JsonApiRouteHandler>();
            services.AddScoped<IMetaBuilder, MetaBuilder>();
            services.AddScoped<IDocumentBuilder, DocumentBuilder>();
            services.AddScoped<IJsonApiSerializer, JsonApiSerializer>();
            services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            services.AddScoped<IJsonApiDeSerializer, JsonApiDeSerializer>();
            services.AddScoped<IJsonApiReader, JsonApiReader>();
            services.AddScoped<IJsonApiOperationsReader, JsonApiOperationsReader>();
            services.AddScoped<IGenericProcessorFactory, GenericProcessorFactory>();
            services.AddScoped(typeof(GenericProcessor<>));
            services.AddScoped<IQueryAccessor, QueryAccessor>();
        }

        private static void AddOperationServices(IServiceCollection services)
        {
            services.AddScoped<IOperationsProcessor, OperationsProcessor>();
            services.AddSingleton<IOperationProcessorResolver, OperationProcessorResolver>();
            services.AddSingleton<IGenericProcessorFactory, GenericProcessorFactory>();
        }

        public static void SerializeAsJsonApi(this MvcOptions options, JsonApiOptions jsonApiOptions)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());

            if (jsonApiOptions.EnabledExtensions.Contains(JsonApiExtension.Operations))
                options.InputFormatters.Insert(0, new JsonApiOperationsInputFormatter());

            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());

            options.Conventions.Insert(0, new DasherizedRoutingConvention(jsonApiOptions.Namespace));
        }
    }
}
