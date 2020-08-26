using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    public class ServiceDiscoveryFacade : IServiceDiscoveryFacade
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

        private readonly IServiceCollection _services;
        private readonly IResourceGraphBuilder _resourceGraphBuilder;
        private readonly IdentifiableTypeCache _typeCache = new IdentifiableTypeCache();
        private readonly Dictionary<Assembly, IEnumerable<ResourceDescriptor>> _discoverableAssemblies = new Dictionary<Assembly, IEnumerable<ResourceDescriptor>>();
        
        public ServiceDiscoveryFacade(IServiceCollection services, IResourceGraphBuilder resourceGraphBuilder)
        {
            _services = services;
            _resourceGraphBuilder = resourceGraphBuilder;
        }

        /// <inheritdoc/>
        public ServiceDiscoveryFacade AddCurrentAssembly() => AddAssembly(Assembly.GetCallingAssembly());

        /// <inheritdoc/>
        public ServiceDiscoveryFacade AddAssembly(Assembly assembly)
        {
            _discoverableAssemblies.Add(assembly, null);
            
            return this;
        }

        /// <inheritdoc/>
        void IServiceDiscoveryFacade.DiscoverResources()
        {
            
            foreach (var (assembly, discoveredResourceDescriptors) in  _discoverableAssemblies.ToArray())
            {
                var resourceDescriptors = GetOrSetResourceDescriptors(discoveredResourceDescriptors, assembly);

                foreach (var descriptor in resourceDescriptors)
                {
                    AddResource(assembly, descriptor);
                }
            }
        }
        
        /// <inheritdoc/>
        void IServiceDiscoveryFacade.DiscoverServices()
        {
            foreach (var (assembly, discoveredResourceDescriptors) in  _discoverableAssemblies.ToArray())
            {
                AddDbContextResolvers(assembly);

                var resourceDescriptors = GetOrSetResourceDescriptors(discoveredResourceDescriptors, assembly);

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
            _resourceGraphBuilder.AddResource(resourceDescriptor.ResourceType, resourceDescriptor.IdType);
        }

        private void AddResourceDefinition(Assembly assembly, ResourceDescriptor identifiable)
        {
            try
            {
                var resourceDefinition = TypeLocator.GetDerivedGenericTypes(assembly, typeof(ResourceDefinition<>), identifiable.ResourceType)
                    .SingleOrDefault();

                if (resourceDefinition != null)
                    _services.AddScoped(typeof(ResourceDefinition<>).MakeGenericType(identifiable.ResourceType), resourceDefinition);
            }
            catch (InvalidOperationException e)
            {
                throw new JsonApiSetupException($"Cannot define multiple ResourceDefinition<> implementations for '{identifiable.ResourceType}'", e);
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
        
        private IEnumerable<ResourceDescriptor> GetOrSetResourceDescriptors(IEnumerable<ResourceDescriptor> discoveredResourceDescriptors, Assembly assembly)
        {
            IEnumerable<ResourceDescriptor> resourceDescriptors;
            if (discoveredResourceDescriptors == null)
            {
                resourceDescriptors = _typeCache.GetIdentifiableTypes(assembly);
                _discoverableAssemblies[assembly] = resourceDescriptors;
            }
            else
            {
                resourceDescriptors = discoveredResourceDescriptors;
            }

            return resourceDescriptors;
        }
    }
}