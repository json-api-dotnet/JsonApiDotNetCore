using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Graph;
using JsonApiDotNetCore.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Serialization.Client;
using JsonApiDotNetCore.Serialization;
using JsonApiDotNetCore.Internal.Contracts;
using JsonApiDotNetCore.Serialization.Server;
using JsonApiDotNetCore.Services;

namespace JsonApiDotNetCore.Extensions
{
    // ReSharper disable once InconsistentNaming
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Enabling JsonApiDotNetCore using the EF Core DbContext to build the ResourceGraph.
        /// </summary>
        /// <typeparam name="TContext"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonApi<TEfCoreDbContext>(this IServiceCollection services,
                                                              Action<JsonApiOptions> options = null,
                                                              Action<IResourceGraphBuilder> resources = null,
                                                              IMvcCoreBuilder mvcBuilder = null)
            where TEfCoreDbContext : DbContext
        {
            var application = new JsonApiApplicationBuilder(services, mvcBuilder ?? services.AddMvcCore());
            if (options != null)
                application.ConfigureJsonApiOptions(options);
            application.ConfigureMvc();
            application.ConfigureResources<TEfCoreDbContext>(resources);
            application.ConfigureServices();
            return services;
        }

        /// <summary>
        /// Enabling JsonApiDotNetCore using manual declaration to build the ResourceGraph.
        /// </summary>z
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        public static IServiceCollection AddJsonApi(this IServiceCollection services,
                                                    Action<JsonApiOptions> options = null,
                                                    Action<IServiceDiscoveryFacade> discovery = null,
                                                    Action<IResourceGraphBuilder> resources = null,
                                                    IMvcCoreBuilder mvcBuilder = null)
        {
            var application = new JsonApiApplicationBuilder(services, mvcBuilder ?? services.AddMvcCore());
            if (options != null)
                application.ConfigureJsonApiOptions(options);
            application.ConfigureMvc();
            if (discovery != null)
                application.AutoDiscover(discovery);
            if (resources != null)
                application.ConfigureResources(resources);
            application.ConfigureServices();
            return services;
        }


        /// <summary>
        /// Enables client serializers for sending requests and receiving responses
        /// in json:api format. Internally only used for testing.
        /// Will be extended in the future to be part of a JsonApiClientDotNetCore package.
        /// </summary>
        public static IServiceCollection AddClientSerialization(this IServiceCollection services)
        {
            services.AddScoped<IResponseDeserializer, ResponseDeserializer>();

            services.AddScoped<IRequestSerializer>(sp =>
            {
                var resourceObjectBuilder = new ResourceObjectBuilder(sp.GetService<IResourceGraphExplorer>(), sp.GetService<IResourceObjectBuilderSettingsProvider>().Get());
                return new RequestSerializer(sp.GetService<IResourceGraphExplorer>(), resourceObjectBuilder);
            });
           return services;
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
