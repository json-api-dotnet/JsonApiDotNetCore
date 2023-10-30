using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.QueryStrings")]
public sealed class Node : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? Comment { get; set; }

    [HasOne]
    public Node? Parent { get; set; }

    [HasMany]
    public ISet<Node> Children { get; set; } = new HashSet<Node>();
}
