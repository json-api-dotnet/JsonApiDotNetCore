using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState")]
public sealed class SystemDirectory : Identifiable<int>
{
    [RegularExpression("^[0-9]+$")]
    public override int Id { get; set; }

    [Attr(PublicName = "directoryName")]
    [RegularExpression(@"^[\w\s]+$")]
    public required string Name { get; set; }

    [Attr]
    [Required]
    public bool? IsCaseSensitive { get; set; }

    [HasMany]
    public ICollection<SystemDirectory> Subdirectories { get; set; } = new List<SystemDirectory>();

    [HasMany]
    public ICollection<SystemFile> Files { get; set; } = new List<SystemFile>();

    [HasOne]
    public SystemDirectory? Self { get; set; }

    [HasOne]
    public SystemDirectory? AlsoSelf { get; set; }

    [HasOne]
    public SystemDirectory? Parent { get; set; }
}
