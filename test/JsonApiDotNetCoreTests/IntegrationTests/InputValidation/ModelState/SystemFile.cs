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

    [Attr]
    [Range(typeof(DateOnly), "2000-01-01", "2050-01-01", ParseLimitsInInvariantCulture = true)]
    public DateOnly CreatedOn { get; set; }

    [Attr]
    [Range(typeof(TimeOnly), "09:00:00", "17:30:00", ParseLimitsInInvariantCulture = true)]
    public TimeOnly CreatedAt { get; set; }
}
