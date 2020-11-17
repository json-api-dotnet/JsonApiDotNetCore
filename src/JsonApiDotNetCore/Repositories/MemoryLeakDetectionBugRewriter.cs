using System;
using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Repositories
{
    /// <summary>
    /// Removes projections from a <see cref="QueryLayer"/> when its resource type uses injected parameters,
    /// as a workaround for EF Core bug https://github.com/dotnet/efcore/issues/20502, which exists in versions below v5.
    /// </summary>
    /// <remarks>
    /// Note that by using this workaround, nested filtering, paging and sorting all remain broken in EF Core 3.1 when using injected parameters in resources.
    /// But at least it enables simple top-level queries to succeed without an exception.
    /// </remarks>
    public sealed class MemoryLeakDetectionBugRewriter
    {
        public QueryLayer Rewrite(QueryLayer queryLayer)
        {
            if (queryLayer == null) throw new ArgumentNullException(nameof(queryLayer));

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

        private IDictionary<ResourceFieldAttribute, QueryLayer> RewriteProjection(IDictionary<ResourceFieldAttribute, QueryLayer> projection, ResourceContext resourceContext)
        {
            if (projection == null || projection.Count == 0)
            {
                return projection;
            }

            var newProjection = new Dictionary<ResourceFieldAttribute, QueryLayer>();
            foreach (var (field, layer) in projection)
            {
                var newLayer = RewriteLayer(layer);
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
