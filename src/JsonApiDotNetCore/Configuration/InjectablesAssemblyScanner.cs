using System.Reflection;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JsonApiDotNetCore.Configuration;

/// <summary>
/// Scans assemblies for injectables (types that implement <see cref="IResourceService{TResource,TId}" />,
/// <see cref="IResourceRepository{TResource,TId}" /> or <see cref="IResourceDefinition{TResource,TId}" />) and registers them in the IoC container.
/// </summary>
internal sealed class InjectablesAssemblyScanner
{
    internal static readonly HashSet<Type> ServiceUnboundInterfaces =
    [
        typeof(IResourceService<,>),
        typeof(IResourceCommandService<,>),
        typeof(IResourceQueryService<,>),
        typeof(IGetAllService<,>),
        typeof(IGetByIdService<,>),
        typeof(IGetSecondaryService<,>),
        typeof(IGetRelationshipService<,>),
        typeof(ICreateService<,>),
        typeof(IAddToRelationshipService<,>),
        typeof(IUpdateService<,>),
        typeof(ISetRelationshipService<,>),
        typeof(IDeleteService<,>),
        typeof(IRemoveFromRelationshipService<,>)
    ];

    internal static readonly HashSet<Type> RepositoryUnboundInterfaces =
    [
        typeof(IResourceRepository<,>),
        typeof(IResourceWriteRepository<,>),
        typeof(IResourceReadRepository<,>)
    ];

    internal static readonly HashSet<Type> ResourceDefinitionUnboundInterfaces = [typeof(IResourceDefinition<,>)];

    private readonly ResourceDescriptorAssemblyCache _assemblyCache;
    private readonly IServiceCollection _services;
    private readonly TypeLocator _typeLocator = new();

    public InjectablesAssemblyScanner(ResourceDescriptorAssemblyCache assemblyCache, IServiceCollection services)
    {
        ArgumentGuard.NotNull(assemblyCache);
        ArgumentGuard.NotNull(services);

        _assemblyCache = assemblyCache;
        _services = services;
    }

    public void DiscoverInjectables()
    {
        IReadOnlyCollection<ResourceDescriptor> descriptors = _assemblyCache.GetResourceDescriptors();
        IReadOnlyCollection<Assembly> assemblies = _assemblyCache.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            AddDbContextResolvers(assembly);
            AddInjectables(descriptors, assembly);
        }
    }

    private void AddDbContextResolvers(Assembly assembly)
    {
        IEnumerable<Type> dbContextTypes = _typeLocator.GetDerivedTypes(assembly, typeof(DbContext));

        foreach (Type dbContextType in dbContextTypes)
        {
            Type dbContextResolverClosedType = typeof(DbContextResolver<>).MakeGenericType(dbContextType);
            _services.TryAddScoped(typeof(IDbContextResolver), dbContextResolverClosedType);
        }
    }

    private void AddInjectables(IEnumerable<ResourceDescriptor> resourceDescriptors, Assembly assembly)
    {
        foreach (ResourceDescriptor resourceDescriptor in resourceDescriptors)
        {
            AddServices(assembly, resourceDescriptor);
            AddRepositories(assembly, resourceDescriptor);
            AddResourceDefinitions(assembly, resourceDescriptor);
        }
    }

    private void AddServices(Assembly assembly, ResourceDescriptor resourceDescriptor)
    {
        foreach (Type serviceUnboundInterface in ServiceUnboundInterfaces)
        {
            RegisterImplementations(assembly, serviceUnboundInterface, resourceDescriptor);
        }
    }

    private void AddRepositories(Assembly assembly, ResourceDescriptor resourceDescriptor)
    {
        foreach (Type repositoryUnboundInterface in RepositoryUnboundInterfaces)
        {
            RegisterImplementations(assembly, repositoryUnboundInterface, resourceDescriptor);
        }
    }

    private void AddResourceDefinitions(Assembly assembly, ResourceDescriptor resourceDescriptor)
    {
        foreach (Type resourceDefinitionUnboundInterface in ResourceDefinitionUnboundInterfaces)
        {
            RegisterImplementations(assembly, resourceDefinitionUnboundInterface, resourceDescriptor);
        }
    }

    private void RegisterImplementations(Assembly assembly, Type interfaceType, ResourceDescriptor resourceDescriptor)
    {
        Type[] typeArguments =
        [
            resourceDescriptor.ResourceClrType,
            resourceDescriptor.IdClrType
        ];

        (Type implementationType, Type serviceInterface)? result = _typeLocator.GetContainerRegistrationFromAssembly(assembly, interfaceType, typeArguments);

        if (result != null)
        {
            (Type implementationType, Type serviceInterface) = result.Value;
            _services.TryAddScoped(serviceInterface, implementationType);
        }
    }
}
