using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace NoDbConsoleQueryExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Track : Identifiable<long>
{
    [Attr]
    public string FileName { get; set; } = null!;

    [Attr]
    public string? DisplayName { get; set; }

    [Attr]
    public int LengthInSeconds { get; set; }

    [HasOne]
    public Genre Genre { get; set; } = null!;

    [HasOne]
    public Artist Artist { get; set; } = null!;
}
