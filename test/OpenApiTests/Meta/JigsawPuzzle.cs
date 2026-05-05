using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.Meta;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.Meta")]
public sealed class JigsawPuzzle : Identifiable<long>
{
    [Attr]
    public string Title { get; set; } = null!;

    [HasOne]
    public JigsawPuzzlePicture FrontPicture { get; set; } = null!;

    [HasOne]
    public JigsawPuzzlePicture? BackPicture { get; set; }

    [HasMany]
    public ISet<JigsawPuzzlePiece> Pieces { get; set; } = new HashSet<JigsawPuzzlePiece>();
}
