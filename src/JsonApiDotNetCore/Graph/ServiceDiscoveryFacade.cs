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
        internal static HashSet<Type> ServiceInterfaces = new HashSet<Type> {
            typeof(IResourceService<>),
            typeof(IResourceService<,>),
            typeof(IResourceCmdService<>),
            typeof(IResourceCmdService<,>),
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

        internal static HashSet<Type> RepositoryInterfaces = new HashSet<Type> {
            typeof(IResourceRepository<>),
            typeof(IResourceRepository<,>),
            typeof(IResourceWriteRepository<>),
            typeof(IResourceWriteRepository<,>),
            typeof(IResourceReadRepository<>),
            typeof(IResourceReadRepository<,>)
        };
        private readonly IServiceCollection _services;
        private readonly IResourceGraphBuilder _resourceGraphBuilder;
        private readonly List<ResourceDescriptor> _identifiables = new List<ResourceDescriptor>();

        public ServiceDiscoveryFacade(IServiceCollection services, IResourceGraphBuilder resourceGraphBuilder)
        {
            _services = services;
            _resourceGraphBuilder = resourceGraphBuilder;
        }

        /// <summary>
        /// Add resources, services and repository implementations to the container.
        /// </summary>
        public ServiceDiscoveryFacade AddCurrentAssembly() => AddAssembly(Assembly.GetCallingAssembly());

        /// <summary>
        /// Add resources, services and repository implementations to the container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        public ServiceDiscoveryFacade AddAssembly(Assembly assembly)
        {
            AddDbContextResolvers(assembly);

            var resourceDescriptors = TypeLocator.GetIdentifableTypes(assembly);
            foreach (var resourceDescriptor in resourceDescriptors)
            {
                AddResource(assembly, resourceDescriptor);
                AddServices(assembly, resourceDescriptor);
                AddRepositories(assembly, resourceDescriptor);
            }
            return this;
        }


        public IEnumerable<Type> FindDerivedTypes(Type baseType)
        {
            return baseType.Assembly.GetTypes().Where(t =>
            {
                if (t.BaseType != null)
                {
                    return baseType.IsSubclassOf(t);
                }
                return false;

            });
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

        /// <summary>
        /// Adds resources to the resourceGraph and registers <see cref="ResourceDefinition{T}"/> types on the container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        public ServiceDiscoveryFacade AddResources(Assembly assembly)
        {
            var identifiables = TypeLocator.GetIdentifableTypes(assembly);
            foreach (var identifiable in identifiables)
                AddResource(assembly, identifiable);

            return this;
        }

        private void AddResource(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            RegisterResourceDefinition(assembly, resourceDescriptor);
            AddResourceToGraph(resourceDescriptor);
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

        private void AddResourceToGraph(ResourceDescriptor identifiable)
        {
            _resourceGraphBuilder.AddResource(identifiable.ResourceType, identifiable.IdType);
        }

        /// <summary>
        /// Add <see cref="IResourceService{T, TId}"/> implementations to container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        public ServiceDiscoveryFacade AddServices(Assembly assembly)
        {
            var resourceDescriptors = TypeLocator.GetIdentifableTypes(assembly);
            foreach (var resourceDescriptor in resourceDescriptors)
            {
                AddServices(assembly, resourceDescriptor);
            }
            return this;
        }

        private void AddServices(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            foreach (var serviceInterface in ServiceInterfaces)
            {
                RegisterServiceImplementations(assembly, serviceInterface, resourceDescriptor);
            }
        }

        /// <summary>
        /// Add <see cref="IResourceRepository{T, TId}"/> implementations to container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        public ServiceDiscoveryFacade AddRepositories(Assembly assembly)
        {
            var resourceDescriptors = TypeLocator.GetIdentifableTypes(assembly);
            foreach (var resourceDescriptor in resourceDescriptors)
            {
                AddRepositories(assembly, resourceDescriptor);
            }

            return this;
        }

        private void AddRepositories(Assembly assembly, ResourceDescriptor resourceDescriptor)
        {
            foreach (var serviceInterface in RepositoryInterfaces)
            {
                RegisterServiceImplementations(assembly, serviceInterface, resourceDescriptor);
            }
        }
        public int i = 0;
        private void RegisterServiceImplementations(Assembly assembly, Type interfaceType, ResourceDescriptor resourceDescriptor)
        {
            if (resourceDescriptor.IdType == typeof(Guid) && interfaceType.GetTypeInfo().GenericTypeParameters.Length == 1)
            {
                return;
            }
            var genericArguments = interfaceType.GetTypeInfo().GenericTypeParameters.Length == 2 ? new[] { resourceDescriptor.ResourceType, resourceDescriptor.IdType } : new[] { resourceDescriptor.ResourceType };
            var service = TypeLocator.GetGenericInterfaceImplementation(assembly, interfaceType, genericArguments);
            //if(service.implementation?.Name == "CustomArticleService" && genericArguments[0].Name != "Article")
            //{
            //    service = TypeLocator.GetGenericInterfaceImplementation(assembly, interfaceType, genericArguments);
            //}
            if (service.implementation != null)
            {
                _services.AddScoped(service.registrationInterface, service.implementation);
            }
        }
    }
}
