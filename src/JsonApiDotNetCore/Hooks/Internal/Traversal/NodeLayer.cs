using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCore.Hooks.Internal.Traversal
{
    /// <summary>
    /// A helper class that represents all resources in the current layer that are being traversed for which hooks will be executed (see
    /// IResourceHookExecutor)
    /// </summary>
    internal sealed class NodeLayer : IEnumerable<IResourceNode>
    {
        private readonly List<IResourceNode> _collection;

        public NodeLayer(List<IResourceNode> nodes)
        {
            _collection = nodes;
        }

        public bool AnyResources()
        {
            return _collection.Any(node => node.UniqueResources.Cast<IIdentifiable>().Any());
        }

        public IEnumerator<IResourceNode> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
