using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Serialization.Building;
using JsonApiDotNetCore.Serialization.Client.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures JsonApiDotNetCore by registering resources manually.
        /// </summary>
        public static IServiceCollection AddJsonApi(this IServiceCollection services,
            Action<JsonApiOptions> options = null,
            Action<ServiceDiscoveryFacade> discovery = null,
            Action<ResourceGraphBuilder> resources = null,
            IMvcCoreBuilder mvcBuilder = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            SetupApplicationBuilder(services, options, discovery, resources, mvcBuilder, null);

            return services;
        }

        /// <summary>
        /// Configures JsonApiDotNetCore by registering resources from an Entity Framework Core model.
        /// </summary>
        public static IServiceCollection AddJsonApi<TDbContext>(this IServiceCollection services,
            Action<JsonApiOptions> options = null,
            Action<ServiceDiscoveryFacade> discovery = null,
            Action<ResourceGraphBuilder> resources = null,
            IMvcCoreBuilder mvcBuilder = null)
            where TDbContext : DbContext
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            SetupApplicationBuilder(services, options, discovery, resources, mvcBuilder, typeof(TDbContext));

            return services;
        }

        private static void SetupApplicationBuilder(IServiceCollection services, Action<JsonApiOptions> configureOptions,
            Action<ServiceDiscoveryFacade> configureAutoDiscovery,
            Action<ResourceGraphBuilder> configureResourceGraph, IMvcCoreBuilder mvcBuilder, Type dbContextType)
        {
            using var applicationBuilder = new JsonApiApplicationBuilder(services, mvcBuilder ?? services.AddMvcCore());

            applicationBuilder.ConfigureJsonApiOptions(configureOptions);
            applicationBuilder.ConfigureAutoDiscovery(configureAutoDiscovery);
            applicationBuilder.AddResourceGraph(dbContextType, configureResourceGraph);
            applicationBuilder.ConfigureMvc();
            applicationBuilder.DiscoverInjectables();
            applicationBuilder.ConfigureServiceContainer(dbContextType);
        }
        
        /// <summary>
        /// Enables client serializers for sending requests and receiving responses
        /// in json:api format. Internally only used for testing.
        /// Will be extended in the future to be part of a JsonApiClientDotNetCore package.
        /// </summary>
        public static IServiceCollection AddClientSerialization(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<IResponseDeserializer, ResponseDeserializer>();
            services.AddScoped<IRequestSerializer>(sp =>
            {
                var graph = sp.GetRequiredService<IResourceGraph>();
                return new RequestSerializer(graph, new ResourceObjectBuilder(graph, new ResourceObjectBuilderSettings()));
            });
            return services;
        }

        /// <summary>
        /// Adds all required registrations for the service to the container.
        /// </summary>
        /// <exception cref="InvalidConfigurationException"/>
        public static IServiceCollection AddResourceService<TService>(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var typeImplementsAnExpectedInterface = false;

            var serviceImplementationType = typeof(TService);

            // it is _possible_ that a single concrete type could be used for multiple resources...
            var resourceDescriptors = GetResourceTypesFromServiceImplementation(serviceImplementationType);

            foreach (var resourceDescriptor in resourceDescriptors)
            {
                foreach (var openGenericType in ServiceDiscoveryFacade.ServiceInterfaces)
                {
                    // A shorthand interface is one where the ID type is omitted
                    // e.g. IResourceService<TResource> is the shorthand for IResourceService<TResource, TId>
                    var isShorthandInterface = openGenericType.GetTypeInfo().GenericTypeParameters.Length == 1;
                    if (isShorthandInterface && resourceDescriptor.IdType != typeof(int))
                        continue; // we can't create a shorthand for ID types other than int

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

            if (!typeImplementsAnExpectedInterface)
                throw new InvalidConfigurationException($"{serviceImplementationType} does not implement any of the expected JsonApiDotNetCore interfaces.");

            return services;
        }

        private static HashSet<ResourceDescriptor> GetResourceTypesFromServiceImplementation(Type type)
        {
            var resourceDescriptors = new HashSet<ResourceDescriptor>();
            var interfaces = type.GetInterfaces();
            foreach (var i in interfaces)
            {
                if (i.IsGenericType)
                {
                    var firstGenericArgument = i.GenericTypeArguments.FirstOrDefault();
                    if (TypeLocator.TryGetResourceDescriptor(firstGenericArgument, out var resourceDescriptor))
                    {
                        resourceDescriptors.Add(resourceDescriptor);
                    }
                }
            }
            return resourceDescriptors;
        }
    }
}
