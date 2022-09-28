using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace AnnotationTests.Models;

[PublicAPI]
[NoResource]
[ResourceLinks(TopLevelLinks = LinkTypes.None, ResourceLinks = LinkTypes.None, RelationshipLinks = LinkTypes.None)]
public sealed class HiddenNode : Identifiable<Guid>
{
    [EagerLoad]
    public HiddenNode? Parent { get; set; }
}
