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
    public const int MinPasswordChars = 15;
    public const int MinPasswordCharsInBase64 = (int)(4.0 / 3 * MinPasswordChars);

    public const int MaxPasswordChars = 45;
    public const int MaxPasswordCharsInBase64 = (int)(4.0 / 3 * MaxPasswordChars);

    [Attr]
    public Guid? AlternativeId { get; set; }

    [Attr]
    [Length(2, 20)]
    public string? FirstName { get; set; }

    [Attr]
    [Required(AllowEmptyStrings = true)]
    public string LastName { get; set; } = null!;

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
    [Base64String]
    [MinLength(MinPasswordCharsInBase64)]
    [MaxLength(MaxPasswordCharsInBase64)]
    public string? Password { get; set; }

    [Attr]
    [Phone]
    public string? Phone { get; set; }

    [Attr]
    [Range(0.1, 122.9, MinimumIsExclusive = true, MaximumIsExclusive = true)]
    public double? Age { get; set; }

    [Attr]
    public Uri? ProfilePicture { get; set; }

    [Attr]
    [Url]
    public string? BackgroundPicture { get; set; }

    [Attr]
    [Length(1, 10)]
    public List<string>? Tags { get; set; }

    [Attr]
    [AllowedValues(null, "NL", "FR")]
    public string? CountryCode { get; set; }

    [Attr]
    [DeniedValues("pluto")]
    public string? Planet { get; set; }

    [Attr]
    [Range(typeof(TimeSpan), "01:00", "05:00", ConvertValueInInvariantCulture = true)]
    public TimeSpan? NextRevalidation { get; set; }

    [Attr]
    public DateTime? ValidatedAt { get; set; }

    [Attr]
    public DateOnly? ValidatedAtDate { get; set; }

    [Attr]
    public TimeOnly? ValidatedAtTime { get; set; }
}
