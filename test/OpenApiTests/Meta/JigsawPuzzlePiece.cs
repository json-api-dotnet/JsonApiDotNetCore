using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Meta")]
public sealed class JigsawPuzzlePiece : Identifiable<long>
{
    [Attr]
    public string ImageUrl { get; set; } = null!;

    [HasOne]
    public JigsawPuzzle Puzzle { get; set; } = null!;
}
