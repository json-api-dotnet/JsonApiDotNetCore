using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public class ResourceAccessor : IResourceAccessor
    {
        private static readonly Type _openResourceReadRepositoryType = typeof(IResourceReadRepository<,>);
        private static readonly MethodInfo _accessorMethod;
        
        static ResourceAccessor()
        {
            _accessorMethod = typeof(ResourceAccessor).GetMethod(nameof(GetById), BindingFlags.NonPublic | BindingFlags.Static);
        }
        
        private static async Task<IEnumerable<IIdentifiable>> GetById<TResource, TId>(
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
        
        private readonly IServiceProvider _serviceProvider;
        private readonly IResourceContextProvider _provider;
        private readonly Dictionary<Type, (MethodInfo, object)> _parameterizedMethodRepositoryCache = new Dictionary<Type, (MethodInfo, object)>();
        
        public ResourceAccessor(IServiceProvider serviceProvider, IResourceContextProvider provider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _provider = provider ?? throw new ArgumentException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IIdentifiable>> GetResourcesByIdAsync(Type resourceType, IEnumerable<string> ids)
        {
            var resourceContext = _provider.GetResourceContext(resourceType);
            var (parameterizedMethod, repository) = GetParameterizedMethodAndRepository(resourceType, resourceContext);
            
            var resources = await parameterizedMethod.InvokeAsync(null, ids, repository, resourceContext);
            
            return (IEnumerable<IIdentifiable>)resources;
        }

        private (MethodInfo, object) GetParameterizedMethodAndRepository(Type resourceType, ResourceContext resourceContext)
        {
            if (!_parameterizedMethodRepositoryCache.TryGetValue(resourceType, out var accessorPair))
            {
                var parameterizedMethod = _accessorMethod.MakeGenericMethod(resourceType, resourceContext.IdentityType);
                var repositoryType = _openResourceReadRepositoryType.MakeGenericType(resourceType, resourceContext.IdentityType); 
                var repository = _serviceProvider.GetRequiredService(repositoryType);
                
                accessorPair = (parameterizedMethod, repository);
                _parameterizedMethodRepositoryCache.Add(resourceType, accessorPair);
            }

            return accessorPair;
        }
    }
}
