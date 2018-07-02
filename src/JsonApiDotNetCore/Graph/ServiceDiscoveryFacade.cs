using JsonApiDotNetCore.Builders;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Internal;
using JsonApiDotNetCore.Models;
using JsonApiDotNetCore.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace JsonApiDotNetCore.Graph
{
    public class ServiceDiscoveryFacade
    {
        private readonly IServiceCollection _services;
        private readonly IContextGraphBuilder _graphBuilder;

        public ServiceDiscoveryFacade(IServiceCollection services, IContextGraphBuilder graphBuilder)
        {
            _services = services;
            _graphBuilder = graphBuilder;
        }

        /// <summary>
        /// Add resources, services and repository implementations to the container.
        /// </summary>
        /// <param name="resourceNameFormatter">The type name formatter used to get the string representation of resource names.</param>
        public ServiceDiscoveryFacade AddCurrentAssemblyServices(IResourceNameFormatter resourceNameFormatter = null)
            => AddAssemblyServices(Assembly.GetCallingAssembly(), resourceNameFormatter);

        /// <summary>
        /// Add resources, services and repository implementations to the container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        /// <param name="resourceNameFormatter">The type name formatter used to get the string representation of resource names.</param>
        public ServiceDiscoveryFacade AddAssemblyServices(Assembly assembly, IResourceNameFormatter resourceNameFormatter = null)
        {
            AddDbContextResolvers(assembly);
            AddAssemblyResources(assembly, resourceNameFormatter);
            AddAssemblyServices(assembly);
            AddAssemblyRepositories(assembly);

            return this;
        }

        private void AddDbContextResolvers(Assembly assembly)
        {
            var dbContextTypes = TypeLocator.GetDerivedTypes(assembly, typeof(DbContext));
            foreach(var dbContextType in dbContextTypes)
            {
                var resolverType = typeof(DbContextResolver<>).MakeGenericType(dbContextType);
                _services.AddScoped(typeof(IDbContextResolver), resolverType);
            }
        }

        /// <summary>
        /// Adds resources to the graph and registers <see cref="ResourceDefinition{T}"/> types on the container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        /// <param name="resourceNameFormatter">The type name formatter used to get the string representation of resource names.</param>
        public ServiceDiscoveryFacade AddAssemblyResources(Assembly assembly, IResourceNameFormatter resourceNameFormatter = null)
        {
            var identifiables = TypeLocator.GetIdentifableTypes(assembly);
            foreach (var identifiable in identifiables)
            {
                RegisterResourceDefinition(assembly, identifiable);
                AddResourceToGraph(identifiable, resourceNameFormatter);
            }

            return this;
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
                // TODO: need a better way to communicate failure since this is unlikely to occur during a web request
                throw new JsonApiException(500,
                    $"Cannot define multiple ResourceDefinition<> implementations for '{identifiable.ResourceType}'", e);
            }            
        }

        private void AddResourceToGraph(ResourceDescriptor identifiable, IResourceNameFormatter resourceNameFormatter = null)
        {
            var resourceName = FormatResourceName(identifiable.ResourceType, resourceNameFormatter);
            _graphBuilder.AddResource(identifiable.ResourceType, identifiable.IdType, resourceName);
        }

        private string FormatResourceName(Type resourceType, IResourceNameFormatter resourceNameFormatter)
        {
            resourceNameFormatter = resourceNameFormatter ?? new DefaultResourceNameFormatter();
            return resourceNameFormatter.FormatResourceName(resourceType);
        }

        /// <summary>
        /// Add <see cref="IResourceService{T, TId}"/> implementations to container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        public ServiceDiscoveryFacade AddAssemblyServices(Assembly assembly)
        {
            RegisterServiceImplementations(assembly, typeof(IResourceService<,>));
            RegisterServiceImplementations(assembly, typeof(ICreateService<,>));
            RegisterServiceImplementations(assembly, typeof(IGetAllService<,>));
            RegisterServiceImplementations(assembly, typeof(IGetByIdService<,>));
            RegisterServiceImplementations(assembly, typeof(IGetRelationshipService<,>));
            RegisterServiceImplementations(assembly, typeof(IUpdateService<,>));
            RegisterServiceImplementations(assembly, typeof(IDeleteService<,>));

            return this;
        }

        /// <summary>
        /// Add <see cref="IEntityRepository{T, TId}"/> implementations to container.
        /// </summary>
        /// <param name="assembly">The assembly to search for resources in.</param>
        public ServiceDiscoveryFacade AddAssemblyRepositories(Assembly assembly)
            => RegisterServiceImplementations(assembly, typeof(IEntityRepository<,>));

        private ServiceDiscoveryFacade RegisterServiceImplementations(Assembly assembly, Type interfaceType)
        {
            var identifiables = TypeLocator.GetIdentifableTypes(assembly);
            foreach (var identifiable in identifiables)
            {
                var service = TypeLocator.GetGenericInterfaceImplementation(assembly, interfaceType, identifiable.ResourceType, identifiable.IdType);
                if (service.implementation != null)
                    _services.AddScoped(service.registrationInterface, service.implementation);
            }

            return this;
        }
    }
}
