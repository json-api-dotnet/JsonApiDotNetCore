using System.Collections.Generic;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    internal interface ITraversalHelper
    {
        /// <summary>
        /// Crates the next layer
        /// </summary>
        NodeLayer CreateNextLayer(IResourceNode node);
        /// <summary>
        /// Creates the next layer based on the nodes provided
        /// </summary>
        NodeLayer CreateNextLayer(IEnumerable<IResourceNode> nodes);
        /// <summary>
        /// Creates a root node for breadth-first-traversal (BFS). Note that typically, in
        /// JADNC, the root layer will be homogeneous. Also, because it is the first layer,
        /// there can be no relationships to previous layers, only to next layers.
        /// </summary>
        /// <returns>The root node.</returns>
        /// <param name="rootResources">Root resources.</param>
        /// <typeparam name="TResource">The 1st type parameter.</typeparam>
        RootNode<TResource> CreateRootNode<TResource>(IEnumerable<TResource> rootResources) where TResource : class, IIdentifiable;
    }
}
