using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Execution
{
    /// <summary>
    /// A helper class that provides insights in which relationships have been updated for which resources.
    /// </summary>
    [PublicAPI]
    public interface IRelationshipGetters<TLeftResource>
        where TLeftResource : class, IIdentifiable
    {
        /// <summary>
        /// Gets a dictionary of all resources that have an affected relationship to type <typeparamref name="TLeftResource" />
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TLeftResource>> GetByRelationship<TRightResource>()
            where TRightResource : class, IIdentifiable;

        /// <summary>
        /// Gets a dictionary of all resources that have an affected relationship to type <paramref name="resourceType" />
        /// </summary>
        Dictionary<RelationshipAttribute, HashSet<TLeftResource>> GetByRelationship(Type resourceType);

        /// <summary>
        /// Gets a collection of all the resources for the property within <paramref name="navigationAction" /> has been affected by the request
        /// </summary>
        HashSet<TLeftResource> GetAffected(Expression<Func<TLeftResource, object>> navigationAction);
    }
}
