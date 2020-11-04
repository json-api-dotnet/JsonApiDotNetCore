using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Repositories;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Services
{
    // TODO: Reconsider responsibilities (IQueryLayerComposer?)
    /// <inheritdoc/>
    // TODO: Refactor this type (it is a helper method).
    public class GetResourcesByIds : IGetResourcesByIds
    {
        private readonly IResourceGraph _resourceGraph;
        private readonly IResourceRepositoryAccessor _resourceRepositoryAccessor;

        public GetResourcesByIds(IResourceGraph resourceGraph, IResourceRepositoryAccessor resourceRepositoryAccessor)
        {
            _resourceGraph = resourceGraph ?? throw new ArgumentNullException(nameof(resourceGraph));
            _resourceRepositoryAccessor = resourceRepositoryAccessor ?? throw new ArgumentNullException(nameof(resourceRepositoryAccessor));
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<IIdentifiable>> Get(Type resourceType, ISet<object> typedIds)
        {
            if (resourceType == null) throw new ArgumentNullException(nameof(resourceType));
            if (typedIds == null ) throw new ArgumentNullException(nameof(typedIds));

            if (typedIds.Any())
            {
                var resourceContext = _resourceGraph.GetResourceContext(resourceType);

                var primaryIdProjection = CreatePrimaryIdProjection(resourceContext);
                
                var idValues = typedIds.Select(id => id.ToString()).ToArray();
                var idsFilter = CreateFilterByIds(idValues, resourceContext);

                var queryLayer = new QueryLayer(resourceContext)
                {
                    Projection = primaryIdProjection,
                    Filter = idsFilter
                };

                return await _resourceRepositoryAccessor.GetAsync(resourceType, queryLayer);
            }

            return Array.Empty<IIdentifiable>();
        }

        private Dictionary<ResourceFieldAttribute, QueryLayer> CreatePrimaryIdProjection(ResourceContext resourceContext)
        {
            var idAttribute = resourceContext.Attributes.Single(a => a.Property.Name == nameof(Identifiable.Id));
            var primaryIdProjection = new Dictionary<ResourceFieldAttribute, QueryLayer> {{idAttribute, null}};
            return primaryIdProjection;
        }

        private FilterExpression CreateFilterByIds(ICollection<string> ids, ResourceContext resourceContext)
        {
            var idAttribute = resourceContext.Attributes.Single(attr => attr.Property.Name == nameof(Identifiable.Id));
            var idChain = new ResourceFieldChainExpression(idAttribute);

            if (ids.Count == 1)
            {
                var constant = new LiteralConstantExpression(ids.Single());
                return new ComparisonExpression(ComparisonOperator.Equals, idChain, constant);
            }

            var constants = ids.Select(id => new LiteralConstantExpression(id)).ToList();
            return new EqualsAnyOfExpression(idChain, constants);
        }
    }
}
