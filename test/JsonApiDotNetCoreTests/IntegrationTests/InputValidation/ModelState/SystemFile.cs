using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.InputValidation.ModelState")]
public sealed class SystemFile : Identifiable<int>
{
    [Attr]
    [MinLength(1)]
    public string FileName { get; set; } = null!;

    [Attr]
    [Required]
    public FileAttributes? Attributes { get; set; }

    [Attr]
    [Range(typeof(long), "1", "9223372036854775807")]
    public long SizeInBytes { get; set; }
}
