using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace AnnotationTests.Models;

[PublicAPI]
[Resource(PublicName = "tree-node", ControllerNamespace = "Models", GenerateControllerEndpoints = JsonApiEndpoints.Query)]
public sealed class TreeNode : Identifiable<long>
{
    [Attr(PublicName = "name", Capabilities = AttrCapabilities.AllowSort)]
    public string? DisplayName { get; set; }

    [HasOne(PublicName = "orders", CanInclude = true, Links = LinkTypes.All)]
    public TreeNode? Parent { get; set; }

    [HasMany(PublicName = "orders", CanInclude = true, Links = LinkTypes.All)]
    public ISet<TreeNode> Children { get; set; } = new HashSet<TreeNode>();
}
