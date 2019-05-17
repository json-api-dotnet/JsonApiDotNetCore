using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Hooks;
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

        public static IServiceCollection AddJsonApi<TContext>(
            this IServiceCollection services,
            Action<JsonApiOptions> options,
            IMvcCoreBuilder mvcBuilder) where TContext : DbContext
        {
            var config = new JsonApiOptions();
            options(config);
            config.BuildResourceGraph(builder => builder.AddDbContext<TContext>());

            mvcBuilder.AddMvcOptions(opt => AddMvcOptions(opt, config));

            AddJsonApiInternals<TContext>(services, config);
            return services;
        }

        public static IServiceCollection AddJsonApi(
            this IServiceCollection services,
            Action<JsonApiOptions> configureOptions,
            IMvcCoreBuilder mvcBuilder,
            Action<ServiceDiscoveryFacade> autoDiscover = null)
        {
            var config = new JsonApiOptions();
            configureOptions(config);

            if(autoDiscover != null)
            {
                var facade = new ServiceDiscoveryFacade(services, config.ResourceGraphBuilder);
                autoDiscover(facade);
            }

            mvcBuilder.AddMvcOptions(opt => AddMvcOptions(opt, config));

            AddJsonApiInternals(services, config);
            return services;
        }

        private static void AddMvcOptions(MvcOptions options, JsonApiOptions config)
        {
            options.Filters.Add(typeof(JsonApiExceptionFilter));
            options.Filters.Add(typeof(TypeMatchFilter));
            options.SerializeAsJsonApi(config);
        }

        public static void AddJsonApiInternals<TContext>(
            this IServiceCollection services,
            JsonApiOptions jsonApiOptions) where TContext : DbContext
        {
            if (jsonApiOptions.ResourceGraph == null)
                jsonApiOptions.BuildResourceGraph<TContext>(null);

            services.AddScoped<IDbContextResolver, DbContextResolver<TContext>>();

            AddJsonApiInternals(services, jsonApiOptions);
        }

        public static void AddJsonApiInternals(
            this IServiceCollection services,
            JsonApiOptions jsonApiOptions)
        {
            if (jsonApiOptions.ResourceGraph == null)
                jsonApiOptions.ResourceGraph = jsonApiOptions.ResourceGraphBuilder.Build();

            if (jsonApiOptions.ResourceGraph.UsesDbContext == false)
            {
                services.AddScoped<DbContext>();
                services.AddSingleton(new DbContextOptionsBuilder().Options);
            }

            if (jsonApiOptions.EnableOperations)
                AddOperationServices(services);

            services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            services.AddScoped(typeof(IGuidEntityRepository<>), typeof(DefaultGuidEntityRepository<>));
            services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));

            services.AddScoped(typeof(ICreateService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(ICreateService<,>), typeof(EntityResourceService<,>));

            services.AddScoped(typeof(IGetAllService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IGetAllService<,>), typeof(EntityResourceService<,>));

            services.AddScoped(typeof(IGetByIdService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IGetByIdService<,>), typeof(EntityResourceService<,>));

            services.AddScoped(typeof(IGetRelationshipService<,>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IGetRelationshipService<,>), typeof(EntityResourceService<,>));

            services.AddScoped(typeof(IUpdateService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IUpdateService<,>), typeof(EntityResourceService<,>));

            services.AddScoped(typeof(IDeleteService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IDeleteService<,>), typeof(EntityResourceService<,>));

            services.AddScoped(typeof(IResourceService<>), typeof(EntityResourceService<>));
            services.AddScoped(typeof(IResourceService<,>), typeof(EntityResourceService<,>));

            services.AddSingleton(jsonApiOptions);
            services.AddSingleton(jsonApiOptions.ResourceGraph);
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

            if (jsonApiOptions.EnableResourceHooks)
            {
                services.AddSingleton(typeof(IHooksDiscovery<>), typeof(HooksDiscovery<>));
                services.AddScoped(typeof(IResourceHookContainer<>), typeof(ResourceDefinition<>));
                services.AddTransient(typeof(IResourceHookExecutor), typeof(ResourceHookExecutor));
                services.AddTransient<IHookExecutorHelper, HookExecutorHelper>();
            }

            services.AddScoped<IInverseRelationships, InverseRelationships>();
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

            services.AddScoped<IOperationProcessorResolver, OperationProcessorResolver>();
        }

        public static void SerializeAsJsonApi(this MvcOptions options, JsonApiOptions jsonApiOptions)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());

            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());

            options.Conventions.Insert(0, new DasherizedRoutingConvention(jsonApiOptions.Namespace));
        }

        /// <summary>
        /// Adds all required registrations for the service to the container
        /// </summary>
        /// <exception cref="JsonApiSetupException"/>
        public static IServiceCollection AddResourceService<T>(this IServiceCollection services) 
        {
            var typeImplementsAnExpectedInterface = false;

            var serviceImplementationType = typeof(T);

            // it is _possible_ that a single concrete type could be used for multiple resources...
            var resourceDescriptors = GetResourceTypesFromServiceImplementation(serviceImplementationType);

            foreach(var resourceDescriptor in resourceDescriptors)
            {
                foreach(var openGenericType in ServiceDiscoveryFacade.ServiceInterfaces)
                {
                    // A shorthand interface is one where the id type is ommitted
                    // e.g. IResourceService<T> is the shorthand for IResourceService<T, TId>
                    var isShorthandInterface = (openGenericType.GetTypeInfo().GenericTypeParameters.Length == 1);
                    if(isShorthandInterface && resourceDescriptor.IdType != typeof(int))
                        continue; // we can't create a shorthand for id types other than int

                    var concreteGenericType = isShorthandInterface
                        ? openGenericType.MakeGenericType(resourceDescriptor.ResourceType)
                        : openGenericType.MakeGenericType(resourceDescriptor.ResourceType, resourceDescriptor.IdType);

                    if(concreteGenericType.IsAssignableFrom(serviceImplementationType)) {
                        services.AddScoped(concreteGenericType, serviceImplementationType);
                        typeImplementsAnExpectedInterface = true;
                    }
                }
            }

            if(typeImplementsAnExpectedInterface == false)
                throw new JsonApiSetupException($"{serviceImplementationType} does not implement any of the expected JsonApiDotNetCore interfaces.");

            return services;
        }

        private static HashSet<ResourceDescriptor> GetResourceTypesFromServiceImplementation(Type type)
        {
            var resourceDecriptors = new HashSet<ResourceDescriptor>();
            var interfaces = type.GetInterfaces();
            foreach(var i in interfaces)
            {
                if(i.IsGenericType)
                {
                    var firstGenericArgument = i.GenericTypeArguments.FirstOrDefault();
                    if(TypeLocator.TryGetResourceDescriptor(firstGenericArgument, out var resourceDescriptor) == true)
                    {
                        resourceDecriptors.Add(resourceDescriptor);
                    }
                }
            }

            return resourceDecriptors;
        }
    }
}
