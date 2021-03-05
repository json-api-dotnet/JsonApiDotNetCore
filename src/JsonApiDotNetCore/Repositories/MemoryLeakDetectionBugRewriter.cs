using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Removes projections from a <see cref="QueryLayer" /> when its resource type uses injected parameters, as a workaround for EF Core bug
    /// https://github.com/dotnet/efcore/issues/20502, which exists in versions below v5.
    /// </summary>
    /// <remarks>
    /// Note that by using this workaround, nested filtering, paging and sorting all remain broken in EF Core 3.1 when using injected parameters in
    /// resources. But at least it enables simple top-level queries to succeed without an exception.
    /// </remarks>
    [PublicAPI]
    public sealed class MemoryLeakDetectionBugRewriter
    {
        public QueryLayer Rewrite(QueryLayer queryLayer)
        {
            ArgumentGuard.NotNull(queryLayer, nameof(queryLayer));

            return RewriteLayer(queryLayer);
        }

        private QueryLayer RewriteLayer(QueryLayer queryLayer)
        {
            if (queryLayer != null)
            {
                queryLayer.Projection = RewriteProjection(queryLayer.Projection, queryLayer.ResourceContext);
            }

            return queryLayer;
        }

        private IDictionary<ResourceFieldAttribute, QueryLayer> RewriteProjection(IDictionary<ResourceFieldAttribute, QueryLayer> projection,
            ResourceContext resourceContext)
        {
            if (projection.IsNullOrEmpty())
            {
                return projection;
            }

            var newProjection = new Dictionary<ResourceFieldAttribute, QueryLayer>();

            foreach ((ResourceFieldAttribute field, QueryLayer layer) in projection)
            {
                QueryLayer newLayer = RewriteLayer(layer);
                newProjection.Add(field, newLayer);
            }

            if (!ResourceFactory.HasSingleConstructorWithoutParameters(resourceContext.ResourceType))
            {
                return null;
            }

            return newProjection;
        }
    }
}
