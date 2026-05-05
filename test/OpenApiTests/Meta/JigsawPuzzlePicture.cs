using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Meta")]
public sealed class JigsawPuzzlePicture : Identifiable<long>
{
    [Attr]
    public string ImageUrl { get; set; } = null!;
}
