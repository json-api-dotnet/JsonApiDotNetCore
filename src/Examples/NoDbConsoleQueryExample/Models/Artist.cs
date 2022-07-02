using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace NoDbConsoleQueryExample.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Artist : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [HasMany]
    public ISet<Track> Tracks { get; set; } = new HashSet<Track>();
}
