using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceDefinitions.Reading")]
public sealed class Constellation : Identifiable<int>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    [Required]
    public Season? VisibleDuring { get; set; }

    [HasMany]
    public ISet<Star> Stars { get; set; } = new HashSet<Star>();
}
