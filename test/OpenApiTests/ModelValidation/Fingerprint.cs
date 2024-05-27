using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ModelValidation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ModelValidation", GenerateControllerEndpoints = JsonApiEndpoints.Post | JsonApiEndpoints.Patch)]
public sealed class Fingerprint : Identifiable<Guid>
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = default!;

    [Attr]
    [StringLength(18, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z]+$", ErrorMessage = "Only letters are allowed.")]
    public string? UserName { get; set; }

    [Attr]
    [CreditCard]
    public string? CreditCard { get; set; }

    [Attr]
    [EmailAddress]
    public string? Email { get; set; }

    [Attr]
    [Phone]
    public string? Phone { get; set; }

    [Attr]
    [Range(0, 123)]
    public int? Age { get; set; }

    [Attr]
    public Uri? ProfilePicture { get; set; }

    [Attr]
    [Url]
    public string? BackgroundPicture { get; set; }

    [Attr]
    [Range(typeof(TimeSpan), "01:00", "05:00")]
    public TimeSpan? NextRevalidation { get; set; }

    [Attr]
    public DateTime? ValidatedAt { get; set; }

    [Attr]
    public DateOnly? ValidatedDateAt { get; set; }

    [Attr]
    public TimeOnly? ValidatedTimeAt { get; set; }
}
