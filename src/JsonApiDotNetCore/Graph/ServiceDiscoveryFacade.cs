using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Models.Fluent;
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
            typeof(IGetRelationshipService<>),
            typeof(IGetRelationshipService<,>),
            typeof(IGetRelationshipsService<>),
            typeof(IGetRelationshipsService<,>),
            typeof(IUpdateService<>),
            typeof(IUpdateService<,>),
            typeof(IDeleteService<>),
            typeof(IDeleteService<,>)
        };

        private static readonly HashSet<Type> RepositoryInterfaces = new HashSet<Type> {
            typeof(IResourceRepository<>),
            typeof(IResourceRepository<,>),
            typeof(IResourceWriteRepository<>),
            typeof(IResourceWriteRepository<,>),
            typeof(IResourceReadRepository<>),
            typeof(IResourceReadRepository<,>)
        };

        private readonly IServiceCollection _services;
        private readonly IResourceGraphBuilder _resourceGraphBuilder;
        private readonly IResourceMappingService _resourceMappingService;
        private readonly IdentifiableTypeCache _typeCache = new IdentifiableTypeCache();

        public ServiceDiscoveryFacade(IServiceCollection services, IResourceGraphBuilder resourceGraphBuilder, IResourceMappingService resourceMappingService)
        {
            _services = services;
            _resourceGraphBuilder = resourceGraphBuilder;
            _resourceMappingService = resourceMappingService;
        }

        public ServiceDiscoveryFacade AddResourceMapping<TResource>(ResourceMapping<TResource> resourceMapping) where TResource : class
        {
            _resourceMappingService.RegisterResourceMapping(resourceMapping);

            return this;
        }

        /// <summary>
        /// Adds resource, service and repository implementations to the container.
        /// </summary>
        public ServiceDiscoveryFacade AddCurrentAssembly() => AddAssembly(Assembly.GetCallingAssembly());

        /// <summary>
        /// Adds resource, service and repository implementations defined in the specified assembly to the container.
        /// </summary>
        public ServiceDiscoveryFacade AddAssembly(Assembly assembly)
        {
            RegisterResourceMappings(assembly);

            AddDbContextResolvers(assembly);

            var resourceDescriptors = _typeCache.GetIdentifiableTypes(assembly);
            foreach (var resourceDescriptor in resourceDescriptors)
            {
                AddResource(assembly, resourceDescriptor);
                AddServices(assembly, resourceDescriptor);
                AddRepositories(assembly, resourceDescriptor);
            }
            return this;
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
            RegisterResourceDefinition(assembly, resourceDescriptor);

            _resourceGraphBuilder.AddResource(resourceDescriptor.ResourceType, resourceDescriptor.IdType);
        }

        private void RegisterResourceDefinition(Assembly assembly, ResourceDescriptor identifiable)
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
            foreach (var serviceInterface in RepositoryInterfaces)
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
            var service = TypeLocator.GetGenericInterfaceImplementation(assembly, interfaceType, genericArguments);

            if (service.implementation != null)
            {
                _services.AddScoped(service.registrationInterface, service.implementation);
            }
        }

        private void RegisterResourceMappings(Assembly assembly)
        {            
            _resourceMappingService.RegisterResourceMappings(assembly);
        }        
    }
}
