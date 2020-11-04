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
        private readonly IResourceFactory _resourceFactory;

        public ResourceRepositoryAccessor(IServiceProvider serviceProvider, IResourceContextProvider resourceContextProvider, IResourceFactory resourceFactory)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentException(nameof(serviceProvider));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        }

        /// <inheritdoc />
        public async Task<IReadOnlyCollection<IIdentifiable>> GetAsync(Type resourceType, QueryLayer layer)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (layer == null) throw new ArgumentNullException(nameof(layer));

            dynamic runtimeResourceTypeParameter = _resourceFactory.CreateInstance(resourceType);
            dynamic runtimeIdTypeParameter = ((IIdentifiable)runtimeResourceTypeParameter).GetTypedId();

            dynamic repository = GetRepository(runtimeResourceTypeParameter, runtimeIdTypeParameter);

            return (IReadOnlyCollection<IIdentifiable>) await repository.GetAsync(layer);
        }

        private object GetRepository<TResource, TId>(TResource _, TId __) where TResource : class, IIdentifiable<TId>
        {
            return _serviceProvider.GetRequiredService<IResourceRepository<TResource,TId>>();
        }
    }
}
