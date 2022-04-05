using System.Text;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Serialization.Response;

/// <summary>
/// Represents a dependency tree of resource objects. It provides the values for 'data' and 'included' in the response body. The tree is built by
/// recursively walking the resource relationships from the inclusion chains. Note that a subsequent chain may add additional relationships to a resource
/// object that was produced by an earlier chain. Afterwards, this tree is used to fill relationship objects in the resource objects (depending on sparse
/// fieldsets) and to emit all entries in relationship declaration order.
/// </summary>
internal sealed class ResourceObjectTreeNode : IEquatable<ResourceObjectTreeNode>
{
    // Placeholder root node for the tree, which is never emitted itself.
    private static readonly ResourceType RootType = new("(root)", typeof(object), typeof(object));
    private static readonly IIdentifiable RootResource = new EmptyResource();

    // Direct children from root. These are emitted in 'data'.
    private List<ResourceObjectTreeNode>? _directChildren;

    // Related resource objects per relationship. These are emitted in 'included'.
    private Dictionary<RelationshipAttribute, HashSet<ResourceObjectTreeNode>>? _childrenByRelationship;

    private bool IsTreeRoot => RootType.Equals(ResourceType);

    // The resource this node was built for. We only store it for the LinkBuilder.
    public IIdentifiable Resource { get; }

    // The resource type. We use its relationships to maintain order.
    public ResourceType ResourceType { get; }

    // The produced resource object from Resource. For each resource, at most one ResourceObject and one tree node must exist.
    public ResourceObject ResourceObject { get; }

    public ResourceObjectTreeNode(IIdentifiable resource, ResourceType resourceType, ResourceObject resourceObject)
    {
        ArgumentGuard.NotNull(resource, nameof(resource));
        ArgumentGuard.NotNull(resourceType, nameof(resourceType));
        ArgumentGuard.NotNull(resourceObject, nameof(resourceObject));

        Resource = resource;
        ResourceType = resourceType;
        ResourceObject = resourceObject;
    }

    public static ResourceObjectTreeNode CreateRoot()
    {
        return new ResourceObjectTreeNode(RootResource, RootType, new ResourceObject());
    }

    public void AttachDirectChild(ResourceObjectTreeNode treeNode)
    {
        ArgumentGuard.NotNull(treeNode, nameof(treeNode));

        _directChildren ??= new List<ResourceObjectTreeNode>();
        _directChildren.Add(treeNode);
    }

    public void EnsureHasRelationship(RelationshipAttribute relationship)
    {
        ArgumentGuard.NotNull(relationship, nameof(relationship));

        _childrenByRelationship ??= new Dictionary<RelationshipAttribute, HashSet<ResourceObjectTreeNode>>();

        if (!_childrenByRelationship.ContainsKey(relationship))
        {
            _childrenByRelationship[relationship] = new HashSet<ResourceObjectTreeNode>();
        }
    }

    public void AttachRelationshipChild(RelationshipAttribute relationship, ResourceObjectTreeNode rightNode)
    {
        ArgumentGuard.NotNull(relationship, nameof(relationship));
        ArgumentGuard.NotNull(rightNode, nameof(rightNode));

        if (_childrenByRelationship == null)
        {
            throw new InvalidOperationException("Call EnsureHasRelationship() first.");
        }

        HashSet<ResourceObjectTreeNode> rightNodes = _childrenByRelationship[relationship];
        rightNodes.Add(rightNode);
    }

    /// <summary>
    /// Recursively walks the tree and returns the set of unique nodes. Uses relationship declaration order.
    /// </summary>
    public IReadOnlySet<ResourceObjectTreeNode> GetUniqueNodes()
    {
        AssertIsTreeRoot();

        var visited = new HashSet<ResourceObjectTreeNode>();

        VisitSubtree(this, visited);

        return visited;
    }

    private static void VisitSubtree(ResourceObjectTreeNode treeNode, ISet<ResourceObjectTreeNode> visited)
    {
        if (visited.Contains(treeNode))
        {
            return;
        }

        if (!treeNode.IsTreeRoot)
        {
            visited.Add(treeNode);
        }

        VisitDirectChildrenInSubtree(treeNode, visited);
        VisitRelationshipChildrenInSubtree(treeNode, visited);
    }

    private static void VisitDirectChildrenInSubtree(ResourceObjectTreeNode treeNode, ISet<ResourceObjectTreeNode> visited)
    {
        if (treeNode._directChildren != null)
        {
            foreach (ResourceObjectTreeNode child in treeNode._directChildren)
            {
                VisitSubtree(child, visited);
            }
        }
    }

    private static void VisitRelationshipChildrenInSubtree(ResourceObjectTreeNode treeNode, ISet<ResourceObjectTreeNode> visited)
    {
        if (treeNode._childrenByRelationship != null)
        {
            foreach (RelationshipAttribute relationship in treeNode.ResourceType.Relationships)
            {
                if (treeNode._childrenByRelationship.TryGetValue(relationship, out HashSet<ResourceObjectTreeNode>? rightNodes))
                {
                    VisitRelationshipChildInSubtree(rightNodes, visited);
                }
            }
        }
    }

    private static void VisitRelationshipChildInSubtree(HashSet<ResourceObjectTreeNode> rightNodes, ISet<ResourceObjectTreeNode> visited)
    {
        foreach (ResourceObjectTreeNode rightNode in rightNodes)
        {
            VisitSubtree(rightNode, visited);
        }
    }

    public IReadOnlySet<ResourceObjectTreeNode>? GetRightNodesInRelationship(RelationshipAttribute relationship)
    {
        return _childrenByRelationship != null && _childrenByRelationship.TryGetValue(relationship, out HashSet<ResourceObjectTreeNode>? rightNodes)
            ? rightNodes
            : null;
    }

    /// <summary>
    /// Provides the value for 'data' in the response body. Uses relationship declaration order.
    /// </summary>
    public IReadOnlyList<ResourceObject> GetResponseData()
    {
        AssertIsTreeRoot();

        return GetDirectChildren().Select(child => child.ResourceObject).ToArray();
    }

    /// <summary>
    /// Provides the value for 'included' in the response body. Uses relationship declaration order.
    /// </summary>
#pragma warning disable AV1130 // Return type in method signature should be an interface to an unchangeable collection
    public IList<ResourceObject> GetResponseIncluded()
#pragma warning restore AV1130 // Return type in method signature should be an interface to an unchangeable collection
    {
        AssertIsTreeRoot();

        var visited = new HashSet<ResourceObjectTreeNode>();

        foreach (ResourceObjectTreeNode child in GetDirectChildren())
        {
            VisitRelationshipChildrenInSubtree(child, visited);
        }

        return visited.Select(node => node.ResourceObject).ToArray();
    }

    private IList<ResourceObjectTreeNode> GetDirectChildren()
    {
        return _directChildren == null ? Array.Empty<ResourceObjectTreeNode>() : _directChildren;
    }

    private void AssertIsTreeRoot()
    {
        if (!IsTreeRoot)
        {
            throw new InvalidOperationException("Internal error: this method should only be called from the root of the tree.");
        }
    }

    public bool Equals(ResourceObjectTreeNode? other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ResourceObjectComparer.Instance.Equals(ResourceObject, other.ResourceObject);
    }

    public override bool Equals(object? other)
    {
        return Equals(other as ResourceObjectTreeNode);
    }

    public override int GetHashCode()
    {
        return ResourceObject.GetHashCode();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(IsTreeRoot ? ResourceType.PublicName : $"{ResourceObject.Type}:{ResourceObject.Id}");

        if (_directChildren != null)
        {
            builder.Append($", children: {_directChildren.Count}");
        }
        else if (_childrenByRelationship != null)
        {
            builder.Append($", children: {string.Join(',', _childrenByRelationship.Select(pair => $"{pair.Key.PublicName} ({pair.Value.Count})"))}");
        }

        return builder.ToString();
    }

    private sealed class EmptyResource : IIdentifiable
    {
        public string? StringId { get; set; }
        public string? LocalId { get; set; }
    }

    private sealed class ResourceObjectComparer : IEqualityComparer<ResourceObject>
    {
        public static readonly ResourceObjectComparer Instance = new();

        private ResourceObjectComparer()
        {
        }

        public bool Equals(ResourceObject? left, ResourceObject? right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null || left.GetType() != right.GetType())
            {
                return false;
            }

            return left.Type == right.Type && left.Id == right.Id && left.Lid == right.Lid;
        }

        public int GetHashCode(ResourceObject obj)
        {
            return HashCode.Combine(obj.Type, obj.Id, obj.Lid);
        }
    }
}
