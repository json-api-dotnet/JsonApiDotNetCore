using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Configuration;

[PublicAPI]
public static class ServiceCollectionExtensions
{
    private static readonly TypeLocator TypeLocator = new();

    /// <summary>
    /// Configures JsonApiDotNetCore by registering resources manually.
    /// </summary>
#pragma warning disable AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    public static IServiceCollection AddJsonApi(this IServiceCollection services, Action<JsonApiOptions>? options = null,
        Action<ServiceDiscoveryFacade>? discovery = null, Action<ResourceGraphBuilder>? resources = null, IMvcCoreBuilder? mvcBuilder = null,
        ICollection<Type>? dbContextTypes = null)
#pragma warning restore AV1553 // Do not use optional parameters with default value null for strings, collections or tasks
    {
        ArgumentGuard.NotNull(services);

        SetupApplicationBuilder(services, options, discovery, resources, mvcBuilder, dbContextTypes ?? Array.Empty<Type>());

        return services;
    }

    /// <summary>
    /// Configures JsonApiDotNetCore by registering resources from an Entity Framework Core model.
    /// </summary>
    public static IServiceCollection AddJsonApi<TDbContext>(this IServiceCollection services, Action<JsonApiOptions>? options = null,
        Action<ServiceDiscoveryFacade>? discovery = null, Action<ResourceGraphBuilder>? resources = null, IMvcCoreBuilder? mvcBuilder = null)
        where TDbContext : DbContext
    {
        return AddJsonApi(services, options, discovery, resources, mvcBuilder, typeof(TDbContext).AsArray());
    }

    private static void SetupApplicationBuilder(IServiceCollection services, Action<JsonApiOptions>? configureOptions,
        Action<ServiceDiscoveryFacade>? configureAutoDiscovery, Action<ResourceGraphBuilder>? configureResources, IMvcCoreBuilder? mvcBuilder,
        ICollection<Type> dbContextTypes)
    {
        using var applicationBuilder = new JsonApiApplicationBuilder(services, mvcBuilder ?? services.AddMvcCore());

        applicationBuilder.ConfigureJsonApiOptions(configureOptions);
        applicationBuilder.ConfigureAutoDiscovery(configureAutoDiscovery);
        applicationBuilder.ConfigureResourceGraph(dbContextTypes, configureResources);
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
        ArgumentGuard.NotNull(services);

        RegisterTypeForUnboundInterfaces(services, typeof(TService), ServiceDiscoveryFacade.ServiceUnboundInterfaces);

        return services;
    }

    /// <summary>
    /// Adds IoC container registrations for the various JsonApiDotNetCore resource repository interfaces, such as
    /// <see cref="IResourceReadRepository{TResource,TId}" /> and <see cref="IResourceWriteRepository{TResource, TId}" />.
    /// </summary>
    public static IServiceCollection AddResourceRepository<TRepository>(this IServiceCollection services)
    {
        ArgumentGuard.NotNull(services);

        RegisterTypeForUnboundInterfaces(services, typeof(TRepository), ServiceDiscoveryFacade.RepositoryUnboundInterfaces);

        return services;
    }

    /// <summary>
    /// Adds IoC container registrations for the various JsonApiDotNetCore resource definition interfaces, such as
    /// <see cref="IResourceDefinition{TResource,TId}" />.
    /// </summary>
    public static IServiceCollection AddResourceDefinition<TResourceDefinition>(this IServiceCollection services)
    {
        ArgumentGuard.NotNull(services);

        RegisterTypeForUnboundInterfaces(services, typeof(TResourceDefinition), ServiceDiscoveryFacade.ResourceDefinitionUnboundInterfaces);

        return services;
    }

    private static void RegisterTypeForUnboundInterfaces(IServiceCollection serviceCollection, Type implementationType, IEnumerable<Type> unboundInterfaces)
    {
        bool seenCompatibleInterface = false;
        ResourceDescriptor? resourceDescriptor = ResolveResourceTypeFromServiceImplementation(implementationType);

        if (resourceDescriptor != null)
        {
            foreach (Type unboundInterface in unboundInterfaces)
            {
                Type closedInterface = unboundInterface.MakeGenericType(resourceDescriptor.ResourceClrType, resourceDescriptor.IdClrType);

                if (closedInterface.IsAssignableFrom(implementationType))
                {
                    serviceCollection.AddScoped(closedInterface, implementationType);
                    seenCompatibleInterface = true;
                }
            }
        }

        if (!seenCompatibleInterface)
        {
            throw new InvalidConfigurationException($"Type '{implementationType}' does not implement any of the expected JsonApiDotNetCore interfaces.");
        }
    }

    private static ResourceDescriptor? ResolveResourceTypeFromServiceImplementation(Type? serviceType)
    {
        if (serviceType != null)
        {
            foreach (Type @interface in serviceType.GetInterfaces())
            {
                Type? firstTypeArgument = @interface.IsGenericType ? @interface.GenericTypeArguments.First() : null;
                ResourceDescriptor? resourceDescriptor = TypeLocator.ResolveResourceDescriptor(firstTypeArgument);

                if (resourceDescriptor != null)
                {
                    return resourceDescriptor;
                }
            }
        }

        return null;
    }
}
