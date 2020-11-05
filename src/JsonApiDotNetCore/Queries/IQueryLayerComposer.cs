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
        FilterExpression GetTopFilterFromConstraints();

        /// <summary>
        /// Collects constraints and builds a <see cref="QueryLayer"/> out of them, used to retrieve the actual resources.
        /// </summary>
        QueryLayer ComposeFromConstraints(ResourceContext requestResource);

        /// <summary>
        /// Wraps a layer for a secondary endpoint into a primary layer, rewriting top-level includes.
        /// </summary>
        QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceContext primaryResourceContext,
            TId primaryId, RelationshipAttribute secondaryRelationship);

        /// <summary>
        /// Collects constraints and builds the secondary layer for a relationship endpoint.
        /// </summary>
        QueryLayer ComposeSecondaryLayerForRelationship(ResourceContext secondaryResourceContext);

        /// <summary>
        /// Builds a query that filters on the specified IDs and selects them.
        /// </summary>
        QueryLayer ComposeForFilterOnResourceIds(ISet<object> typedIds, ResourceContext resourceContext);

        /// <summary>
        /// Builds a query that retrieves the primary resource, including all of its attributes and all targeted relationships, during a create/update/delete request.
        /// </summary>
        QueryLayer ComposeForUpdate<TId>(TId id, ResourceContext primaryResource);
    }
}
