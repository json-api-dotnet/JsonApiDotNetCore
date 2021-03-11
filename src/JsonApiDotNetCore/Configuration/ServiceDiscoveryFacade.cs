using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using JsonApiDotNetCore.Errors;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JsonApiDotNetCore.Configuration
{
    /// <summary>
    /// Scans for types like resources, services, repositories and resource definitions in an assembly and registers them to the IoC container.
    /// </summary>
    [PublicAPI]
    public class ServiceDiscoveryFacade
    {
        internal static readonly HashSet<Type> ServiceInterfaces = new HashSet<Type>
        {
            typeof(IResourceService<>),
            typeof(IResourceService<,>),
            typeof(IResourceCommandService<>),
            typeof(IResourceCommandService<,>),
            typeof(IResourceQueryService<>),
            typeof(IResourceQueryService<,>),
            typeof(IGetAllService<>),
            typeof(IGetAllService<,>),
            typeof(IGetByIdService<>),
            typeof(IGetByIdService<,>),
            typeof(IGetSecondaryService<>),
            typeof(IGetSecondaryService<,>),
            typeof(IGetRelationshipService<>),
            typeof(IGetRelationshipService<,>),
            typeof(ICreateService<>),
            typeof(ICreateService<,>),
            typeof(IAddToRelationshipService<>),
            typeof(IAddToRelationshipService<,>),
            typeof(IUpdateService<>),
            typeof(IUpdateService<,>),
            typeof(ISetRelationshipService<>),
            typeof(ISetRelationshipService<,>),
            typeof(IDeleteService<>),
            typeof(IDeleteService<,>),
            typeof(IRemoveFromRelationshipService<>),
            typeof(IRemoveFromRelationshipService<,>)
        };

        internal static readonly HashSet<Type> RepositoryInterfaces = new HashSet<Type>
        {
            typeof(IResourceRepository<>),
            typeof(IResourceRepository<,>),
            typeof(IResourceWriteRepository<>),
            typeof(IResourceWriteRepository<,>),
            typeof(IResourceReadRepository<>),
            typeof(IResourceReadRepository<,>)
        };

        internal static readonly HashSet<Type> ResourceDefinitionInterfaces = new HashSet<Type>
        {
            typeof(IResourceDefinition<>),
            typeof(IResourceDefinition<,>)
        };

        private readonly ILogger<ServiceDiscoveryFacade> _logger;
        private readonly IServiceCollection _services;
        private readonly ResourceGraphBuilder _resourceGraphBuilder;
        private readonly IJsonApiOptions _options;
        private readonly ResourceDescriptorAssemblyCache _assemblyCache = new ResourceDescriptorAssemblyCache();
        private readonly TypeLocator _typeLocator = new TypeLocator();

        public ServiceDiscoveryFacade(IServiceCollection services, ResourceGraphBuilder resourceGraphBuilder, IJsonApiOptions options,
            ILoggerFactory loggerFactory)
        {
            ArgumentGuard.NotNull(services, nameof(services));
            ArgumentGuard.NotNull(resourceGraphBuilder, nameof(resourceGraphBuilder));
            ArgumentGuard.NotNull(loggerFactory, nameof(loggerFactory));
            ArgumentGuard.NotNull(options, nameof(options));

            _logger = loggerFactory.CreateLogger<ServiceDiscoveryFacade>();
            _services = services;
            _resourceGraphBuilder = resourceGraphBuilder;
            _options = options;
        }

        /// <summary>
        /// Mark the calling assembly for scanning of resources and injectables.
        /// </summary>
        public ServiceDiscoveryFacade AddCurrentAssembly()
        {
            return AddAssembly(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Mark the specified assembly for scanning of resources and injectables.
        /// </summary>
        public ServiceDiscoveryFacade AddAssembly(Assembly assembly)
        {
            ArgumentGuard.NotNull(assembly, nameof(assembly));

            _assemblyCache.RegisterAssembly(assembly);
            _logger.LogDebug($"Registering assembly '{assembly.FullName}' for discovery of resources and injectables.");

            return this;
        }

        internal void DiscoverResources()
        {
            foreach (ResourceDescriptor resourceDescriptor in _assemblyCache.GetResourceDescriptorsPerAssembly().SelectMany(tuple => tuple.resourceDescriptors))
            {
                AddResource(resourceDescriptor);
            }
        }

        internal void DiscoverInjectables()
        {
            foreach ((Assembly assembly, IReadOnlyCollection<ResourceDescriptor> resourceDescriptors) in _assemblyCache.GetResourceDescriptorsPerAssembly())
            {
                AddDbContextResolvers(assembly);
                AddInjectables(resourceDescriptors, assembly);
            }
        }

        private void AddInjectables(IReadOnlyCollection<ResourceDescriptor> resourceDescriptors, Assembly assembly)
        {
            foreach (ResourceDescriptor resourceDescriptor in resourceDescriptors)
            {
                AddServices(assembly, resourceDescriptor);
                AddRepositories(assembly, resourceDescriptor);
                AddResourceDefinitions(assembly, resourceDescriptor);

                if (_options.EnableResourceHooks)
                {
                    AddResourceHookDefinitions(assembly, resourceDescriptor);
                }
            }
        }

        private void AddDbContextResolvers(Assembly assembly)
        {
            IEnumerable<Type> dbContextTypes = _typeLocator.GetDerivedTypes(assembly, typeof(DbContext));

            foreach (Type dbContextType in dbContextTypes)
            {
                Type resolverType = typeof(DbContextResolver<>).MakeGenericType(dbContextType);
                _services.AddScoped(typeof(IDbContextResolver), resolverType);
            }
        }

        private void AddResource(ResourceDescriptor resourceDescriptor)
        {
            _resourceGraphBuilder.Add(resourceDescriptor.ResourceType, resourceDescriptor.IdType);
        }

        private void AddResourceHookDefinitions(Assembly assembly, ResourceDescriptor identifiable)
        {
            try
            {
                Type resourceDefinition = _typeLocator.GetDerivedGenericTypes(assembly, typeof(ResourceHooksDefinition<>), identifiable.ResourceType)
                    .SingleOrDefault();

                if (resourceDefinition != null)
                {
                    _services.AddScoped(typeof(ResourceHooksDefinition<>).MakeGenericType(identifiable.ResourceType), resourceDefinition);
                }
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidConfigurationException($"Cannot define multiple ResourceHooksDefinition<> implementations for '{identifiable.ResourceType}'",
                    exception);
            }
        }

        private void AddServices(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            foreach (Type serviceInterface in ServiceInterfaces)
            {
                RegisterImplementations(assembly, serviceInterface, resourceDescriptor);
            }
        }

        private void AddRepositories(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            foreach (Type repositoryInterface in RepositoryInterfaces)
            {
                RegisterImplementations(assembly, repositoryInterface, resourceDescriptor);
            }
        }

        private void AddResourceDefinitions(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            foreach (Type resourceDefinitionInterface in ResourceDefinitionInterfaces)
            {
                RegisterImplementations(assembly, resourceDefinitionInterface, resourceDescriptor);
            }
        }

        private void RegisterImplementations(Assembly assembly, Type interfaceType, ResourceDescriptor resourceDescriptor)
        {
            Type[] genericArguments = interfaceType.GetTypeInfo().GenericTypeParameters.Length == 2
                ? ArrayFactory.Create(resourceDescriptor.ResourceType, resourceDescriptor.IdType)
                : ArrayFactory.Create(resourceDescriptor.ResourceType);

            (Type implementation, Type registrationInterface)? result =
                _typeLocator.GetGenericInterfaceImplementation(assembly, interfaceType, genericArguments);

            if (result != null)
            {
                (Type implementation, Type registrationInterface) = result.Value;
                _services.AddScoped(registrationInterface, implementation);
            }
        }
    }
}
