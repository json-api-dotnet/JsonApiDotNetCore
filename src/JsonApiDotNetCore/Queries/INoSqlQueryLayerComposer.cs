using JetBrains.Annotations;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Queries.Expressions;

namespace JsonApiDotNetCore.Queries
{
    /// <summary>
    /// Takes scoped expressions from <see cref="IQueryConstraintProvider" />s and transforms them.
    /// Additionally provides specific transformations for NoSQL databases without support for joins.
    /// </summary>
    [PublicAPI]
    public interface INoSqlQueryLayerComposer
    {
        /// <summary>
        /// Builds a filter from constraints, used to determine total resource count on a primary
        /// collection endpoint.
        /// </summary>
        FilterExpression? GetPrimaryFilterFromConstraintsForNoSql(ResourceType primaryResourceType);

        /// <summary>
        /// Composes a <see cref="QueryLayer" /> and an <see cref="IncludeExpression" /> from the
        /// constraints specified by the request. Used for primary resources.
        /// </summary>
        (QueryLayer QueryLayer, IncludeExpression Include) ComposeFromConstraintsForNoSql(ResourceType requestResourceType);

        /// <summary>
        /// Composes a <see cref="QueryLayer" /> and an <see cref="IncludeExpression" /> from the
        /// constraints specified by the request. Used for primary resources.
        /// </summary>
        (QueryLayer QueryLayer, IncludeExpression Include) ComposeForGetByIdWithConstraintsForNoSql<TId>(
            TId id,
            ResourceType primaryResourceType,
            TopFieldSelection fieldSelection)
            where TId : notnull;


        /// <summary>
        /// Composes a <see cref="QueryLayer" /> with a filter expression in the form "equals(id,'{stringId}')".
        /// </summary>
        QueryLayer ComposeForGetByIdForNoSql<TId>(TId id, ResourceType primaryResourceType)
            where TId : notnull;

        /// <summary>
        /// Composes a <see cref="QueryLayer" /> from the constraints specified by the request
        /// and a filter expression in the form "equals({propertyName},'{propertyValue}')".
        /// Used for secondary or included resources.
        /// </summary>
        /// <param name="requestResourceType">The <see cref="ResourceType" /> of the secondary or included resource.</param>
        /// <param name="propertyName">The name of the property of the secondary or included resource used for filtering.</param>
        /// <param name="propertyValue">The value of the property of the secondary or included resource used for filtering.</param>
        /// <param name="isIncluded">
        /// <see langword="true" />, if the resource is included by the request (e.g., "{url}?include={relationshipName}");
        /// <see langword="false" />, if the resource is a secondary resource (e.g., "/{primary}/{id}/{relationshipName}").
        /// </param>
        /// <returns>A tuple with a <see cref="QueryLayer" /> and an <see cref="IncludeExpression" />.</returns>
        (QueryLayer QueryLayer, IncludeExpression Include) ComposeFromConstraintsForNoSql(
            ResourceType requestResourceType,
            string propertyName,
            string propertyValue,
            bool isIncluded);

        /// <summary>
        /// Builds a query that retrieves the primary resource, including all of its attributes
        /// and all targeted relationships, during a create/update/delete request.
        /// </summary>
        (QueryLayer QueryLayer, IncludeExpression Include) ComposeForUpdateForNoSql<TId>(TId id, ResourceType primaryResourceType)
            where TId : notnull;
    }
}
