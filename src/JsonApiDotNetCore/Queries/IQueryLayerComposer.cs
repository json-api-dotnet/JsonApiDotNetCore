using System.Collections.Generic;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Takes scoped expressions from <see cref="IQueryConstraintProvider"/>s and transforms them.
    /// </summary>
    public interface IQueryLayerComposer
    {
        /// <summary>
        /// Builds a top-level filter from constraints, used to determine total resource count.
        /// </summary>
        FilterExpression GetTopFilter();

        /// <summary>
        /// Collects constraints and builds a <see cref="QueryLayer"/> out of them, used to retrieve the actual resources.
        /// </summary>
        QueryLayer Compose(ResourceContext requestResource);

        /// <summary>
        /// Wraps a layer for a secondary endpoint into a primary layer, rewriting top-level includes.
        /// </summary>
        QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceContext primaryResourceContext,
            TId primaryId, RelationshipAttribute secondaryRelationship);

        /// <summary>
        /// Gets the secondary projection for a relationship endpoint.
        /// </summary>
        IDictionary<ResourceFieldAttribute, QueryLayer> GetSecondaryProjectionForRelationshipEndpoint(
            ResourceContext secondaryResourceContext);

        /// <summary>
        /// Builds a query that filters on the specified IDs and selects them.
        /// </summary>
        QueryLayer ComposeForSecondaryResourceIds(ISet<object> typedIds, ResourceContext resourceContext);
    }
}
