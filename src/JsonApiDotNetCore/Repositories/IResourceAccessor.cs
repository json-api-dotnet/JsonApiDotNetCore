using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public interface IResourceAccessor
    {
        Task<IEnumerable<IIdentifiable>> GetResourcesByIdAsync(Type resourceType, IEnumerable<string> ids);
    }

    /// <inheritdoc />
    public class ResourceAccessor : IResourceAccessor
    {
        private static readonly Type _openResourceReadRepositoryType = typeof(IResourceReadRepository<,>);
        private static readonly MethodInfo _accessorMethod;
        private readonly IServiceProvider _serviceProvider;
        private readonly IResourceContextProvider _provider;

        static ResourceAccessor()
        {
            _accessorMethod = typeof(ResourceAccessor).GetMethod(nameof(Accessor), BindingFlags.NonPublic | BindingFlags.Static);
        }
    
        private static async Task<IEnumerable<IIdentifiable>> Accessor<TResource, TId>(
            IEnumerable<string> ids, 
            IResourceReadRepository<TResource, TId> repository,
            ResourceContext resourceContext)
            where TResource : class, IIdentifiable<TId>
        {
            var idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
            
            var queryLayer = new QueryLayer(resourceContext)
            {
                Filter = new EqualsAnyOfExpression(new ResourceFieldChainExpression(idAttribute),
                    ids.Select(id => new LiteralConstantExpression(id.ToString())).ToList())
            };

            return await repository.GetAsync(queryLayer);
        }

        public ResourceAccessor(IServiceProvider serviceProvider, IResourceContextProvider provider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _provider = provider ?? throw new ArgumentException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IIdentifiable>> GetResourcesByIdAsync(Type resourceType, IEnumerable<string> ids)
        {
            var resourceContext = _provider.GetResourceContext(resourceType);
            var repository = GetRepository(resourceType, resourceContext.IdentityType);
            
            var parameterizedAccessor = _accessorMethod.MakeGenericMethod(resourceType, resourceContext.IdentityType);
            var resources = (IEnumerable<IIdentifiable>) await parameterizedAccessor.InvokeAsync(null, new[] {ids, repository, resourceContext});

            var result = ids.Select(id => resources.FirstOrDefault(r => r.StringId == id) ).ToArray();

            return result;
        }

        private object GetRepository(Type resourceType, Type identifierType)
        {
            var repositoryType = _openResourceReadRepositoryType.MakeGenericType(resourceType, identifierType);
            var repository = _serviceProvider.GetRequiredService(repositoryType);

            return repository;
        }
    }
}
