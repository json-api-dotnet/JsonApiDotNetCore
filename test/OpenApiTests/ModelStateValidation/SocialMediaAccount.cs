using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace OpenApiTests.ModelStateValidation;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "OpenApiTests.ModelStateValidation", GenerateControllerEndpoints = JsonApiEndpoints.Post | JsonApiEndpoints.Patch)]
public sealed class SocialMediaAccount : Identifiable<Guid>
{
    [Attr]
    public Guid? AlternativeId { get; set; }

    [Attr]
#if NET8_0_OR_GREATER
    [Length(2, 20)]
#endif
    public string? FirstName { get; set; }

    [Attr]
    [Compare(nameof(FirstName))]
    public string? GivenName { get; set; }

    [Attr]
    [Required(AllowEmptyStrings = true)]
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
#if NET8_0_OR_GREATER
    [Base64String]
#endif
    public string? Password { get; set; }

    [Attr]
    [Phone]
    public string? Phone { get; set; }

    [Attr]
    [Range(0.1, 122.9, ConvertValueInInvariantCulture = true, ParseLimitsInInvariantCulture = true)]
    public double? Age { get; set; }

    [Attr]
    public Uri? ProfilePicture { get; set; }

    [Attr]
    [Url]
    public string? BackgroundPicture { get; set; }

    [Attr]
#if NET8_0_OR_GREATER
    [Length(1, 10)]
#endif
    public List<string>? Tags { get; set; }

    [Attr]
#if NET8_0_OR_GREATER
    [AllowedValues(null, "NL", "FR")]
#endif
    public string? CountryCode { get; set; }

    [Attr]
#if NET8_0_OR_GREATER
    [DeniedValues("pluto")]
#endif
    public string? Planet { get; set; }

    [Attr]
    [Range(typeof(TimeSpan), "01:00", "05:00")]
    public TimeSpan? NextRevalidation { get; set; }

    [Attr]
    public DateTime? ValidatedAt { get; set; }

    [Attr]
    public DateOnly? ValidatedAtDate { get; set; }

    [Attr]
    public TimeOnly? ValidatedAtTime { get; set; }
}
