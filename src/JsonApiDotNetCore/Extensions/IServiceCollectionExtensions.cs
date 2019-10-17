using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Formatters;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Internal.Generics;
using JsonApiDotNetCore.Managers;
using JsonApiDotNetCore.Managers.Contracts;
using JsonApiDotNetCore.Middleware;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Hooks;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Query;
using JsonApiDotNetCore.Serialization.Server.Builders;
using JsonApiDotNetCore.Serialization.Server;
using JsonApiDotNetCore.Serialization.Client;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        static private readonly Action<JsonApiOptions> _noopConfig = opt => { };
        public static IServiceCollection AddJsonApi<TContext>(this IServiceCollection services,
                                                              IMvcCoreBuilder mvcBuilder = null)
            where TContext : DbContext
        {
            return AddJsonApi<TContext>(services, _noopConfig, mvcBuilder);
        }

        /// <summary>
        /// Enabling JsonApiDotNetCore using the EF Core DbContext to build the ResourceGraph.
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="configureAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonApi<TContext>(this IServiceCollection services,
                                                              Action<JsonApiOptions> configureOptions,
                                                              IMvcCoreBuilder mvcBuilder = null)
            where TContext : DbContext
        {
            var options = new JsonApiOptions();
            // add basic Mvc functionality
            mvcBuilder = mvcBuilder ?? services.AddMvcCore();
            // configures JsonApiOptions;
            configureOptions(options);
            // ResourceGraphBuilder should not be exposed on JsonApiOptions.
            // Instead, ResourceGraphBuilder should consume JsonApiOptions
            // build the resource graph using ef core DbContext
            options.BuildResourceGraph(builder => builder.AddDbContext<TContext>());
            ConfigureMvc(services, mvcBuilder, options);
            // register services
            AddJsonApiInternals<TContext>(services, options);
            return services;
        }


        /// <summary>
        /// Enabling JsonApiDotNetCore using manual declaration to build the ResourceGraph.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonApi(this IServiceCollection services,
                                                    Action<JsonApiOptions> configureOptions,
                                                    IMvcCoreBuilder mvcBuilder = null)
        {
            var options = new JsonApiOptions();
            // add basic Mvc functionality
            mvcBuilder = mvcBuilder ?? services.AddMvcCore();
            // configures JsonApiOptions;
            configureOptions(options);
            ConfigureMvc(services, mvcBuilder, options);
            // register services
            AddJsonApiInternals(services, options);
            return services;
        }

        /// <summary>
        /// Enabling JsonApiDotNetCore using the EF Core DbContext to build the ResourceGraph.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configureOptions"></param>
        /// <param name="autoDiscover"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonApi(this IServiceCollection services,
                                                    Action<JsonApiOptions> configureOptions,
                                                    Action<ServiceDiscoveryFacade> autoDiscover,
                                                    IMvcCoreBuilder mvcBuilder = null)
        {
            var options = new JsonApiOptions();
            // add basic Mvc functionality
            mvcBuilder = mvcBuilder ?? services.AddMvcCore();
            // configures JsonApiOptions;
            configureOptions(options);
            // build the resource graph using auto discovery.
            var facade = new ServiceDiscoveryFacade(services, options.ResourceGraphBuilder);
            autoDiscover(facade);
            ConfigureMvc(services, mvcBuilder, options);
            // register services
            AddJsonApiInternals(services, options);
            return services;
        }

        private static void ConfigureMvc(IServiceCollection services, IMvcCoreBuilder mvcBuilder, JsonApiOptions options)
        {
            // add JsonApi filters and serializers
            mvcBuilder.AddMvcOptions(opt => AddMvcOptions(opt, options));
            // register services that allow user to override behaviour that is configured on startup, like routing conventions
            AddStartupConfigurationServices(services, options);
            var intermediateProvider = services.BuildServiceProvider();
            mvcBuilder.AddMvcOptions(opt => opt.Conventions.Insert(0, intermediateProvider.GetRequiredService<IJsonApiRoutingConvention>()));
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

        /// <summary>
        /// Adds services to the container that need to be retrieved with an intermediate provider during Startup.
        /// </summary>
        private static void AddStartupConfigurationServices(this IServiceCollection services, JsonApiOptions jsonApiOptions)
        {
            services.AddSingleton<IJsonApiOptions>(jsonApiOptions);
            services.TryAddSingleton<IResourceNameFormatter>(new KebabCaseFormatter());
            services.TryAddSingleton<IJsonApiRoutingConvention, DefaultRoutingConvention>();
        }

        public static void AddJsonApiInternals(
            this IServiceCollection services,
            JsonApiOptions jsonApiOptions)
        {
            var graph = jsonApiOptions.ResourceGraph ?? jsonApiOptions.ResourceGraphBuilder.Build();

            if (graph.UsesDbContext == false)
            {
                services.AddScoped<DbContext>();
                services.AddSingleton(new DbContextOptionsBuilder().Options);
            }

            services.AddScoped(typeof(IEntityRepository<>), typeof(DefaultEntityRepository<>));
            services.AddScoped(typeof(IEntityRepository<,>), typeof(DefaultEntityRepository<,>));

            services.AddScoped(typeof(IEntityReadRepository<,>), typeof(DefaultEntityRepository<,>));
            services.AddScoped(typeof(IEntityWriteRepository<,>), typeof(DefaultEntityRepository<,>));

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

            services.AddSingleton<ILinksConfiguration>(jsonApiOptions);
            services.AddSingleton(graph);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IContextEntityProvider>(graph);
            services.AddScoped<ICurrentRequest, CurrentRequest>();
            services.AddScoped<IScopedServiceProvider, RequestScopedServiceProvider>(); 
            services.AddScoped<IJsonApiWriter, JsonApiWriter>();
            services.AddScoped<IJsonApiReader, JsonApiReader>();
            services.AddScoped<IGenericProcessorFactory, GenericProcessorFactory>();
            services.AddScoped(typeof(GenericProcessor<>));
            services.AddScoped<IQueryParameterDiscovery, QueryParameterDiscovery>();
            services.AddScoped<ITargetedFields, TargetedFields>();
            services.AddScoped<IFieldsExplorer, FieldsExplorer>();
            services.AddScoped<IResourceDefinitionProvider, ResourceDefinitionProvider>();
            services.AddScoped<IFieldsToSerialize, FieldsToSerialize>();
            services.AddScoped<IQueryParameterActionFilter, QueryParameterActionFilter>();

            AddServerSerialization(services);
            AddQueryParameterServices(services);
            if (jsonApiOptions.EnableResourceHooks)
                AddResourceHooks(services);

            services.AddScoped<IInverseRelationships, InverseRelationships>();
        }

        private static void AddQueryParameterServices(IServiceCollection services)
        {
            services.AddScoped<IIncludeService, IncludeService>();
            services.AddScoped<IFilterService, FilterService>();
            services.AddScoped<ISortService, SortService>();
            services.AddScoped<ISparseFieldsService, SparseFieldsService>();
            services.AddScoped<IPageService, PageService>();
            services.AddScoped<IOmitDefaultService, OmitDefaultService>();
            services.AddScoped<IOmitNullService, OmitNullService>();

            services.AddScoped<IQueryParameterService>(sp => sp.GetService<IIncludeService>());
            services.AddScoped<IQueryParameterService>(sp => sp.GetService<IFilterService>());
            services.AddScoped<IQueryParameterService>(sp => sp.GetService<ISortService>());
            services.AddScoped<IQueryParameterService>(sp => sp.GetService<ISparseFieldsService>());
            services.AddScoped<IQueryParameterService>(sp => sp.GetService<IPageService>());
            services.AddScoped<IQueryParameterService>(sp => sp.GetService<IOmitDefaultService>());
            services.AddScoped<IQueryParameterService>(sp => sp.GetService<IOmitNullService>());
        }


        private static void AddResourceHooks(IServiceCollection services)
        {
            services.AddSingleton(typeof(IHooksDiscovery<>), typeof(HooksDiscovery<>));
            services.AddScoped(typeof(IResourceHookContainer<>), typeof(ResourceDefinition<>));
            services.AddTransient(typeof(IResourceHookExecutor), typeof(ResourceHookExecutor));
            services.AddTransient<IHookExecutorHelper, HookExecutorHelper>();
            services.AddTransient<ITraversalHelper, TraversalHelper>();
        }

        private static void AddServerSerialization(IServiceCollection services)
        {
            services.AddScoped<IIncludedResourceObjectBuilder, IncludedResourceObjectBuilder>();
            services.AddScoped<IJsonApiDeserializer, RequestDeserializer>();
            services.AddScoped<IResourceObjectBuilderSettingsProvider, ResourceObjectBuilderSettingsProvider>();
            services.AddScoped<IJsonApiSerializerFactory, ResponseSerializerFactory>();
            services.AddScoped<ILinkBuilder, LinkBuilder>();
            services.AddScoped(typeof(IMetaBuilder<>), typeof(MetaBuilder<>));
            services.AddScoped(typeof(ResponseSerializer<>));
            services.AddScoped(sp => sp.GetRequiredService<IJsonApiSerializerFactory>().GetSerializer());
            services.AddScoped<IResourceObjectBuilder, ResponseResourceObjectBuilder>();
        }

        /// <summary>
        /// Enables client serializers for sending requests and receiving responses
        /// in json:api format. Internally only used for testing.
        /// Will be extended in the future to be part of a JsonApiClientDotNetCore package.
        /// </summary>
        public static void AddClientSerialization(this IServiceCollection services)
        {
            services.AddScoped<IResponseDeserializer, ResponseDeserializer>();

            services.AddScoped<IRequestSerializer>(sp =>
            {
                var resourceObjectBuilder = new ResourceObjectBuilder(sp.GetService<IResourceGraph>(), sp.GetService<IContextEntityProvider>(), sp.GetService<IResourceObjectBuilderSettingsProvider>().Get());
                return new RequestSerializer(sp.GetService<IFieldsExplorer>(), sp.GetService<IResourceGraph>(), resourceObjectBuilder);
            });

        }

        public static void SerializeAsJsonApi(this MvcOptions options, JsonApiOptions jsonApiOptions)
        {
            options.InputFormatters.Insert(0, new JsonApiInputFormatter());
            options.OutputFormatters.Insert(0, new JsonApiOutputFormatter());
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

            foreach (var resourceDescriptor in resourceDescriptors)
            {
                foreach (var openGenericType in ServiceDiscoveryFacade.ServiceInterfaces)
                {
                    // A shorthand interface is one where the id type is ommitted
                    // e.g. IResourceService<T> is the shorthand for IResourceService<T, TId>
                    var isShorthandInterface = (openGenericType.GetTypeInfo().GenericTypeParameters.Length == 1);
                    if (isShorthandInterface && resourceDescriptor.IdType != typeof(int))
                        continue; // we can't create a shorthand for id types other than int

                    var concreteGenericType = isShorthandInterface
                        ? openGenericType.MakeGenericType(resourceDescriptor.ResourceType)
                        : openGenericType.MakeGenericType(resourceDescriptor.ResourceType, resourceDescriptor.IdType);

                    if (concreteGenericType.IsAssignableFrom(serviceImplementationType))
                    {
                        services.AddScoped(concreteGenericType, serviceImplementationType);
                        typeImplementsAnExpectedInterface = true;
                    }
                }
            }

            if (typeImplementsAnExpectedInterface == false)
                throw new JsonApiSetupException($"{serviceImplementationType} does not implement any of the expected JsonApiDotNetCore interfaces.");

            return services;
        }

        private static HashSet<ResourceDescriptor> GetResourceTypesFromServiceImplementation(Type type)
        {
            var resourceDecriptors = new HashSet<ResourceDescriptor>();
            var interfaces = type.GetInterfaces();
            foreach (var i in interfaces)
            {
                if (i.IsGenericType)
                {
                    var firstGenericArgument = i.GenericTypeArguments.FirstOrDefault();
                    if (TypeLocator.TryGetResourceDescriptor(firstGenericArgument, out var resourceDescriptor) == true)
                    {
                        resourceDecriptors.Add(resourceDescriptor);
                    }
                }
            }
            return resourceDecriptors;
        }
    }
}
