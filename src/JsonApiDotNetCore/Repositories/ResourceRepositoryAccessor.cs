using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using Microsoft.Extensions.DependencyInjection;

namespace JsonApiDotNetCore.Repositories
{
    /// <inheritdoc />
    public class ResourceRepositoryAccessor : IResourceRepositoryAccessor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IResourceContextProvider _resourceContextProvider;

        public ResourceRepositoryAccessor(IServiceProvider serviceProvider, IResourceContextProvider resourceContextProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IIdentifiable>> GetAsync(Type resourceType, QueryLayer layer)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            dynamic repository = GetReadRepository(resourceType);
            return (IReadOnlyCollection<IIdentifiable>) await repository.GetAsync(layer);
        }

        protected object GetReadRepository(Type resourceType)
        {
            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);

            if (resourceContext.IdentityType == typeof(int))
            {
                var intRepositoryType = typeof(IResourceReadRepository<>).MakeGenericType(resourceContext.ResourceType);
                var intRepository = _serviceProvider.GetService(intRepositoryType);

                if (intRepository != null)
                {
                    return intRepository;
                }
            }

            var resourceDefinitionType = typeof(IResourceReadRepository<,>).MakeGenericType(resourceContext.ResourceType, resourceContext.IdentityType);
            return _serviceProvider.GetRequiredService(resourceDefinitionType);
        }
    }
}
