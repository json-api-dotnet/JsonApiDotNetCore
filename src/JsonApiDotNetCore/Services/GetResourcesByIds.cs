using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Services
{
    // TODO: Reconsider responsibilities (IQueryLayerComposer?)
    /// <inheritdoc/>
    // TODO: Refactor this type (it is a helper method).
    public class GetResourcesByIds : IGetResourcesByIds
    {
        private readonly IResourceContextProvider _resourceContextProvider;
        private readonly IResourceRepositoryAccessor _resourceRepositoryAccessor;
        private readonly IQueryLayerComposer _queryLayerComposer;

        public GetResourcesByIds(IResourceContextProvider resourceContextProvider, IResourceRepositoryAccessor resourceRepositoryAccessor, IQueryLayerComposer queryLayerComposer)
        {
            _resourceContextProvider = resourceContextProvider ?? throw new ArgumentNullException(nameof(resourceContextProvider));
            _resourceRepositoryAccessor = resourceRepositoryAccessor ?? throw new ArgumentNullException(nameof(resourceRepositoryAccessor));
            _queryLayerComposer = queryLayerComposer ?? throw new ArgumentNullException(nameof(queryLayerComposer));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<IIdentifiable>> Get(Type resourceType, ISet<object> typedIds)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (typedIds == null ) throw new ArgumentNullException(nameof(typedIds));

            var resourceContext = _resourceContextProvider.GetResourceContext(resourceType);
            var queryLayer = _queryLayerComposer.ComposeForSecondaryResourceIds(typedIds, resourceContext);

            return await _resourceRepositoryAccessor.GetAsync(resourceType, queryLayer);
        }
    }
}
