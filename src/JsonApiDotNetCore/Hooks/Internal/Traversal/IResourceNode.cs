using System.Collections;
using RightType = System.Type;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    /// <summary>
    /// This is the interface that nodes need to inherit from
    /// </summary>
    internal interface IResourceNode
    {
        /// <summary>
        /// Each node represents the resources of a given type throughout a particular layer.
        /// </summary>
        RightType ResourceType { get; }
        /// <summary>
        /// The unique set of resources in this node. Note that these are all of the same type.
        /// </summary>
        IEnumerable UniqueResources { get; }
        /// <summary>
        /// Relationships to the next layer
        /// </summary>
        /// <value>The relationships to next layer.</value>
        RelationshipProxy[] RelationshipsToNextLayer { get; }
        /// <summary>
        /// Relationships to the previous layer
        /// </summary>
        IRelationshipsFromPreviousLayer RelationshipsFromPreviousLayer { get; }

        /// <summary>
        /// A helper method to assign relationships to the previous layer after firing hooks.
        /// Or, in case of the root node, to update the original source enumerable.
        /// </summary>
        void Reassign(IEnumerable source = null);
        /// <summary>
        /// A helper method to internally update the unique set of resources as a result of 
        /// a filter action in a hook.
        /// </summary>
        /// <param name="updated">Updated.</param>
        void UpdateUnique(IEnumerable updated);
    }
}
