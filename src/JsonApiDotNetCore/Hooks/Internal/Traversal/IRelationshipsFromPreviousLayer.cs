using System.Collections;
using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    /// <summary>
    /// A helper class for mapping relationships between a current and previous layer
    /// </summary>
    internal interface IRelationshipsFromPreviousLayer
    {
        /// <summary>
        /// Grouped by relationship to the previous layer, gets all the resources of the current layer
        /// </summary>
        /// <returns>
        /// The right side resources.
        /// </returns>
        IDictionary<RelationshipAttribute, IEnumerable> GetRightResources();

        /// <summary>
        /// Grouped by relationship to the previous layer, gets all the resources of the previous layer
        /// </summary>
        /// <returns>
        /// The right side resources.
        /// </returns>
        IDictionary<RelationshipAttribute, IEnumerable> GetLeftResources();
    }
}
