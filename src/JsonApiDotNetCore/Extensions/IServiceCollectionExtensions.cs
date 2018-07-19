using System;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Services;
using JsonApiDotNetCore.Services.Operations;
using JsonApiDotNetCore.Services.Operations.Processors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonApi<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            var mvcBuilder = services.AddMvcCore();
            return AddJsonApi<TContext>(services, opt => { }, mvcBuilder);
        }

        public static IServiceCollection AddJsonApi<TContext>(this IServiceCollection services, Action<JsonApiOptions> options)
            where TContext : DbContext
        {
            var mvcBuilder = services.AddMvcCore();
            return AddJsonApi<TContext>(services, options, mvcBuilder);
        }

        public static IServiceCollection AddJsonApi<TContext>(this IServiceCollection services,
           Action<JsonApiOptions> options,
           IMvcCoreBuilder mvcBuilder) where TContext : DbContext
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
            return services;
        }

        public static IServiceCollection AddJsonApi(this IServiceCollection services,
            Action<JsonApiOptions> options,
            IMvcCoreBuilder mvcBuilder)
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
            return services;
        }

        public static void AddJsonApiInternals<TContext>(
            this IServiceCollection services,
            JsonApiOptions jsonApiOptions) where TContext : DbContext
        {
            if (jsonApiOptions.ContextGraph == null)
                jsonApiOptions.BuildContextGraph<TContext>(null);

            services.AddScoped<IDbContextResolver, DbContextResolver<TContext>>();

            AddJsonApiInternals(services, jsonApiOptions);
        }

        public static void AddJsonApiInternals(
            this IServiceCollection services,
            JsonApiOptions jsonApiOptions)
        {
            if (jsonApiOptions.ContextGraph.UsesDbContext == false)
            {
                services.AddScoped<DbContext>();
                services.AddSingleton(new DbContextOptionsBuilder().Options);
            }

            if (jsonApiOptions.EnableOperations)
                AddOperationServices(services);

            services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));

            services.AddScoped(typeof(ICreateService<>), typeof(DefaultResourceService<>));
            services.AddScoped(typeof(ICreateService<,>), typeof(DefaultResourceService<,>));

            services.AddScoped(typeof(IGetAllService<>), typeof(DefaultResourceService<>));
            services.AddScoped(typeof(IGetAllService<,>), typeof(DefaultResourceService<,>));

            services.AddScoped(typeof(IGetByIdService<>), typeof(DefaultResourceService<>));
            services.AddScoped(typeof(IGetByIdService<,>), typeof(DefaultResourceService<,>));

            services.AddScoped(typeof(IGetRelationshipService<,>), typeof(DefaultResourceService<>));
            services.AddScoped(typeof(IGetRelationshipService<,>), typeof(DefaultResourceService<,>));

            services.AddScoped(typeof(IUpdateService<>), typeof(DefaultResourceService<>));
            services.AddScoped(typeof(IUpdateService<,>), typeof(DefaultResourceService<,>));

            services.AddScoped(typeof(IDeleteService<>), typeof(DefaultResourceService<>));
            services.AddScoped(typeof(IDeleteService<,>), typeof(DefaultResourceService<,>));

            services.AddScoped(typeof(IResourceService<>), typeof(DefaultResourceService<>));
            services.AddScoped(typeof(IResourceService<,>), typeof(DefaultResourceService<,>));

            services.AddSingleton(jsonApiOptions);
            services.AddSingleton(jsonApiOptions.ContextGraph);
            services.AddScoped<IJsonApiContext, JsonApiContext>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IScopedServiceProvider, RequestScopedServiceProvider>();
            services.AddScoped<JsonApiRouteHandler>();
            services.AddScoped<IMetaBuilder, MetaBuilder>();
            services.AddScoped<IDocumentBuilder, DocumentBuilder>();
            services.AddScoped<IJsonApiSerializer, JsonApiSerializer>();
            services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            services.AddScoped<IJsonApiDeSerializer, JsonApiDeSerializer>();
            services.AddScoped<IJsonApiReader, JsonApiReader>();
            services.AddScoped<IGenericProcessorFactory, GenericProcessorFactory>();
            services.AddScoped(typeof(GenericProcessor<>));
            services.AddScoped<IQueryAccessor, QueryAccessor>();
            services.AddScoped<IQueryParser, QueryParser>();
            services.AddScoped<IControllerContext, Services.ControllerContext>();
            services.AddScoped<IDocumentBuilderOptionsProvider, DocumentBuilderOptionsProvider>();
        }

        private static void AddOperationServices(IServiceCollection services)
        {
            services.AddScoped<IOperationsProcessor, OperationsProcessor>();

            services.AddScoped(typeof(ICreateOpProcessor<>), typeof(CreateOpProcessor<>));
            services.AddScoped(typeof(ICreateOpProcessor<,>), typeof(CreateOpProcessor<,>));

            services.AddScoped(typeof(IGetOpProcessor<>), typeof(GetOpProcessor<>));
            services.AddScoped(typeof(IGetOpProcessor<,>), typeof(GetOpProcessor<,>));

            services.AddScoped(typeof(IRemoveOpProcessor<>), typeof(RemoveOpProcessor<>));
            services.AddScoped(typeof(IRemoveOpProcessor<,>), typeof(RemoveOpProcessor<,>));

            services.AddScoped(typeof(IUpdateOpProcessor<>), typeof(UpdateOpProcessor<>));
            services.AddScoped(typeof(IUpdateOpProcessor<,>), typeof(UpdateOpProcessor<,>));

            services.AddSingleton<IOperationProcessorResolver, OperationProcessorResolver>();
            services.AddSingleton<IGenericProcessorFactory, GenericProcessorFactory>();
        }

        public static void SerializeAsJsonApi(this MvcOptions options, JsonApiOptions jsonApiOptions)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());

            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());

            options.Conventions.Insert(0, new DasherizedRoutingConvention(jsonApiOptions.Namespace));
        }
    }
}
