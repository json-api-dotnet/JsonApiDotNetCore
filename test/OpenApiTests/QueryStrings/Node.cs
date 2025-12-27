using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.QueryStrings")]
public sealed class Node : Identifiable<long>
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public string? Comment { get; set; }

    [HasMany]
    public IList<NameValuePair> Values { get; set; } = new List<NameValuePair>();

    [HasOne]
    public Node? Parent { get; set; }

    [HasMany]
    public ISet<Node> Children { get; set; } = new HashSet<Node>();
}
