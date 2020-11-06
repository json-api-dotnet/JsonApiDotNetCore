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
        /// Builds a filter to match on the specified IDs.
        /// </summary>
        FilterExpression GetFilterOnResourceIds<TId>(ICollection<TId> ids, ResourceContext resourceContext);

        /// <summary>
        /// Builds a join table filter, which matches on the specified IDs.
        /// </summary>
        FilterExpression GetJoinTableFilter<TLeftId, TRightId>(TLeftId leftId, ICollection<TRightId> rightIds, HasManyThroughAttribute relationship);

        /// <summary>
        /// Collects constraints and builds a <see cref="QueryLayer"/> out of them, used to retrieve the actual resources.
        /// </summary>
        QueryLayer ComposeFromConstraints(ResourceContext requestResource);

        /// <summary>
        /// Collects constraints and builds a <see cref="QueryLayer"/> out of them, used to retrieve one resource.
        /// </summary>
        QueryLayer ComposeForGetById<TId>(TId id, ResourceContext resourceContext, TopFieldSelection fieldSelection);

        /// <summary>
        /// Collects constraints and builds the secondary layer for a relationship endpoint.
        /// </summary>
        QueryLayer ComposeSecondaryLayerForRelationship(ResourceContext secondaryResourceContext);

        /// <summary>
        /// Wraps a layer for a secondary endpoint into a primary layer, rewriting top-level includes.
        /// </summary>
        QueryLayer WrapLayerForSecondaryEndpoint<TId>(QueryLayer secondaryLayer, ResourceContext primaryResourceContext,
            TId primaryId, RelationshipAttribute secondaryRelationship);

        /// <summary>
        /// Builds a query that retrieves the primary resource, including all of its attributes and all targeted relationships, during a create/update/delete request.
        /// </summary>
        QueryLayer ComposeForUpdate<TId>(TId id, ResourceContext primaryResource);
    }
}
