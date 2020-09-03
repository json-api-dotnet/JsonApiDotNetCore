using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public class ServiceDiscoveryFacade
    {
        internal static readonly HashSet<Type> ServiceInterfaces = new HashSet<Type> {
            typeof(IResourceService<>),
            typeof(IResourceService<,>),
            typeof(IResourceCommandService<>),
            typeof(IResourceCommandService<,>),
            typeof(IResourceQueryService<>),
            typeof(IResourceQueryService<,>),
            typeof(ICreateService<>),
            typeof(ICreateService<,>),
            typeof(IGetAllService<>),
            typeof(IGetAllService<,>),
            typeof(IGetByIdService<>),
            typeof(IGetByIdService<,>),
            typeof(IGetSecondaryService<>),
            typeof(IGetSecondaryService<,>),
            typeof(IGetRelationshipService<>),
            typeof(IGetRelationshipService<,>),
            typeof(IUpdateService<>),
            typeof(IUpdateService<,>),
            typeof(IDeleteService<>),
            typeof(IDeleteService<,>)
        };

        private static readonly HashSet<Type> _repositoryInterfaces = new HashSet<Type> {
            typeof(IResourceRepository<>),
            typeof(IResourceRepository<,>),
            typeof(IResourceWriteRepository<>),
            typeof(IResourceWriteRepository<,>),
            typeof(IResourceReadRepository<>),
            typeof(IResourceReadRepository<,>)
        };

        private readonly ILogger<ServiceDiscoveryFacade> _logger;
        private readonly IServiceCollection _services;
        private readonly ResourceGraphBuilder _resourceGraphBuilder;
        private readonly IdentifiableTypeCache _typeCache = new IdentifiableTypeCache();
        private readonly Dictionary<Assembly, IList<ResourceDescriptor>> _resourceDescriptorsPerAssemblyCache = new Dictionary<Assembly, IList<ResourceDescriptor>>();

        public ServiceDiscoveryFacade(IServiceCollection services, ResourceGraphBuilder resourceGraphBuilder, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }
            
            _logger = loggerFactory.CreateLogger<ServiceDiscoveryFacade>();
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _resourceGraphBuilder = resourceGraphBuilder ?? throw new ArgumentNullException(nameof(resourceGraphBuilder));
        }

        /// <summary>
        /// Mark the calling assembly for scanning of resources and injectables.
        /// </summary>
        public ServiceDiscoveryFacade AddCurrentAssembly() => AddAssembly(Assembly.GetCallingAssembly());

        /// <summary>
        /// Mark the specified assembly for scanning of resources and injectables.
        /// </summary>
        public ServiceDiscoveryFacade AddAssembly(Assembly assembly)
        {
            _resourceDescriptorsPerAssemblyCache.Add(assembly, null);
            _logger.LogDebug($"Registering assembly '{assembly.FullName}' for discovery of resources and injectables.");

            return this;
        }
        
        internal void DiscoverResources()
        {
            foreach (var (assembly, discoveredResourceDescriptors) in  _resourceDescriptorsPerAssemblyCache.ToArray())
            {
                var resourceDescriptors = GetResourceDescriptorsFromCache(discoveredResourceDescriptors, assembly);

                foreach (var descriptor in resourceDescriptors)
                {
                    AddResource(assembly, descriptor);
                }
            }
        }
        
        internal void DiscoverInjectables()
        {
            foreach (var (assembly, discoveredResourceDescriptors) in  _resourceDescriptorsPerAssemblyCache.ToArray())
            {
                AddDbContextResolvers(assembly);

                var resourceDescriptors = GetResourceDescriptorsFromCache(discoveredResourceDescriptors, assembly);

                foreach (var descriptor in resourceDescriptors)
                {
                    AddResourceDefinition(assembly, descriptor);
                    AddServices(assembly, descriptor);
                    AddRepositories(assembly, descriptor);
                }
            }
        }
        
        private void AddDbContextResolvers(Assembly assembly)
        {
            var dbContextTypes = TypeLocator.GetDerivedTypes(assembly, typeof(DbContext));
            foreach (var dbContextType in dbContextTypes)
            {
                var resolverType = typeof(DbContextResolver<>).MakeGenericType(dbContextType);
                _services.AddScoped(typeof(IDbContextResolver), resolverType);
            }
        }
        
        private void AddResource(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            _resourceGraphBuilder.Add(resourceDescriptor.ResourceType, resourceDescriptor.IdType);
        }

        private void AddResourceDefinition(Assembly assembly, ResourceDescriptor identifiable)
        {
            try
            {
                var resourceDefinition = TypeLocator.GetDerivedGenericTypes(assembly, typeof(ResourceDefinition<>), identifiable.ResourceType)
                    .SingleOrDefault();

                if (resourceDefinition != null)
                {
                    _services.AddScoped(typeof(ResourceDefinition<>).MakeGenericType(identifiable.ResourceType), resourceDefinition);
                }
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidConfigurationException($"Cannot define multiple ResourceDefinition<> implementations for '{identifiable.ResourceType}'", e);
            }
        }

        private void AddServices(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            foreach (var serviceInterface in ServiceInterfaces)
            {
                RegisterServiceImplementations(assembly, serviceInterface, resourceDescriptor);
            }
        }

        private void AddRepositories(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            foreach (var serviceInterface in _repositoryInterfaces)
            {
                RegisterServiceImplementations(assembly, serviceInterface, resourceDescriptor);
            }
        }

        private void RegisterServiceImplementations(Assembly assembly, Type interfaceType, ResourceDescriptor resourceDescriptor)
        {
            if (resourceDescriptor.IdType == typeof(Guid) && interfaceType.GetTypeInfo().GenericTypeParameters.Length == 1)
            {
                return;
            }
            var genericArguments = interfaceType.GetTypeInfo().GenericTypeParameters.Length == 2 ? new[] { resourceDescriptor.ResourceType, resourceDescriptor.IdType } : new[] { resourceDescriptor.ResourceType };
            var (implementation, registrationInterface) = TypeLocator.GetGenericInterfaceImplementation(assembly, interfaceType, genericArguments);

            if (implementation != null)
            {
                _services.AddScoped(registrationInterface, implementation);
            }
        }
        
        private IList<ResourceDescriptor> GetResourceDescriptorsFromCache(IList<ResourceDescriptor> discoveredResourceDescriptors, Assembly assembly)
        {
            IList<ResourceDescriptor> resourceDescriptors;
            if (discoveredResourceDescriptors == null)
            {
                resourceDescriptors = (IList<ResourceDescriptor>)_typeCache.GetIdentifiableTypes(assembly);
                _resourceDescriptorsPerAssemblyCache[assembly] = resourceDescriptors;
            }
            else
            {
                resourceDescriptors = discoveredResourceDescriptors;
            }

            return resourceDescriptors;
        }
    }
}
