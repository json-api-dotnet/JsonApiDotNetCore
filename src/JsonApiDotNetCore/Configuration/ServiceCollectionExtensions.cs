#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration
{
    [PublicAPI]
    public static class ServiceCollectionExtensions
    {
        private static readonly TypeLocator TypeLocator = new();

        /// <summary>
        /// Configures JsonApiDotNetCore by registering resources manually.
        /// </summary>
        public static IServiceCollection AddJsonApi(this IServiceCollection services, Action<JsonApiOptions> options = null,
            Action<ServiceDiscoveryFacade> discovery = null, Action<ResourceGraphBuilder> resources = null, IMvcCoreBuilder mvcBuilder = null,
            ICollection<Type> dbContextTypes = null)
        {
            ArgumentGuard.NotNull(services, nameof(services));

            SetupApplicationBuilder(services, options, discovery, resources, mvcBuilder, dbContextTypes ?? Array.Empty<Type>());

            return services;
        }

        /// <summary>
        /// Configures JsonApiDotNetCore by registering resources from an Entity Framework Core model.
        /// </summary>
        public static IServiceCollection AddJsonApi<TDbContext>(this IServiceCollection services, Action<JsonApiOptions> options = null,
            Action<ServiceDiscoveryFacade> discovery = null, Action<ResourceGraphBuilder> resources = null, IMvcCoreBuilder mvcBuilder = null)
            where TDbContext : DbContext
        {
            return AddJsonApi(services, options, discovery, resources, mvcBuilder, typeof(TDbContext).AsArray());
        }

        private static void SetupApplicationBuilder(IServiceCollection services, Action<JsonApiOptions> configureOptions,
            Action<ServiceDiscoveryFacade> configureAutoDiscovery, Action<ResourceGraphBuilder> configureResourceGraph, IMvcCoreBuilder mvcBuilder,
            ICollection<Type> dbContextTypes)
        {
            using var applicationBuilder = new JsonApiApplicationBuilder(services, mvcBuilder ?? services.AddMvcCore());

            applicationBuilder.ConfigureJsonApiOptions(configureOptions);
            applicationBuilder.ConfigureAutoDiscovery(configureAutoDiscovery);
            applicationBuilder.AddResourceGraph(dbContextTypes, configureResourceGraph);
            applicationBuilder.ConfigureMvc();
            applicationBuilder.DiscoverInjectables();
            applicationBuilder.ConfigureServiceContainer(dbContextTypes);
        }

        /// <summary>
        /// Adds IoC container registrations for the various JsonApiDotNetCore resource service interfaces, such as <see cref="IGetAllService{TResource,TId}" />,
        /// <see cref="ICreateService{TResource, TId}" /> and the various others.
        /// </summary>
        public static IServiceCollection AddResourceService<TService>(this IServiceCollection services)
        {
            ArgumentGuard.NotNull(services, nameof(services));

            RegisterForConstructedType(services, typeof(TService), ServiceDiscoveryFacade.ServiceInterfaces);

            return services;
        }

        /// <summary>
        /// Adds IoC container registrations for the various JsonApiDotNetCore resource repository interfaces, such as
        /// <see cref="IResourceReadRepository{TResource,TId}" /> and <see cref="IResourceWriteRepository{TResource, TId}" />.
        /// </summary>
        public static IServiceCollection AddResourceRepository<TRepository>(this IServiceCollection services)
        {
            ArgumentGuard.NotNull(services, nameof(services));

            RegisterForConstructedType(services, typeof(TRepository), ServiceDiscoveryFacade.RepositoryInterfaces);

            return services;
        }

        /// <summary>
        /// Adds IoC container registrations for the various JsonApiDotNetCore resource definition interfaces, such as
        /// <see cref="IResourceDefinition{TResource,TId}" />.
        /// </summary>
        public static IServiceCollection AddResourceDefinition<TResourceDefinition>(this IServiceCollection services)
        {
            ArgumentGuard.NotNull(services, nameof(services));

            RegisterForConstructedType(services, typeof(TResourceDefinition), ServiceDiscoveryFacade.ResourceDefinitionInterfaces);

            return services;
        }

        private static void RegisterForConstructedType(IServiceCollection services, Type implementationType, IEnumerable<Type> openGenericInterfaces)
        {
            bool seenCompatibleInterface = false;
            ResourceDescriptor resourceDescriptor = TryGetResourceTypeFromServiceImplementation(implementationType);

            if (resourceDescriptor != null)
            {
                foreach (Type openGenericInterface in openGenericInterfaces)
                {
                    Type constructedType = openGenericInterface.MakeGenericType(resourceDescriptor.ResourceClrType, resourceDescriptor.IdClrType);

                    if (constructedType.IsAssignableFrom(implementationType))
                    {
                        services.AddScoped(constructedType, implementationType);
                        seenCompatibleInterface = true;
                    }
                }
            }

            if (!seenCompatibleInterface)
            {
                throw new InvalidConfigurationException($"{implementationType} does not implement any of the expected JsonApiDotNetCore interfaces.");
            }
        }

        private static ResourceDescriptor TryGetResourceTypeFromServiceImplementation(Type serviceType)
        {
            if (serviceType != null)
            {
                foreach (Type @interface in serviceType.GetInterfaces())
                {
                    Type firstGenericArgument = @interface.IsGenericType ? @interface.GenericTypeArguments.First() : null;
                    ResourceDescriptor resourceDescriptor = TypeLocator.TryGetResourceDescriptor(firstGenericArgument);

                    if (resourceDescriptor != null)
                    {
                        return resourceDescriptor;
                    }
                }
            }

            return null;
        }
    }
}
