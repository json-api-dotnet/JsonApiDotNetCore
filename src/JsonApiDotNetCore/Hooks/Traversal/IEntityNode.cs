using System.Collections;
using RightType = System.Type;

namespace JsonApiDotNetCore.Hooks
{
    /// <summary>
    /// This is the interface that nodes need to inherit from
    /// </summary>
    internal interface INode
    {
        /// <summary>
        /// Each node representes the entities of a given type throughout a particular layer.
        /// </summary>
        RightType ResourceType { get; }
        /// <summary>
        /// The unique set of entities in this node. Note that these are all of the same type.
        /// </summary>
        IEnumerable UniqueEntities { get; }
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
        /// A helper method to internally update the unique set of entities as a result of 
        /// a filter action in a hook.
        /// </summary>
        /// <param name="updated">Updated.</param>
        void UpdateUnique(IEnumerable updated);
    }
}
