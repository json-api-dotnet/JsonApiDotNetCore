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
#if !NET6_0
    [Length(2, 20)]
#endif
    public string? FirstName { get; set; }

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
#if !NET6_0
    [Base64String]
    [MinLength(MinPasswordCharsInBase64)]
    [MaxLength(MaxPasswordCharsInBase64)]
#endif
    public string? Password { get; set; }

    [Attr]
    [Phone]
    public string? Phone { get; set; }

    [Attr]
#if NET6_0
    [Range(0.1, 122.9)]
#else
    [Range(0.1, 122.9, MinimumIsExclusive = true, MaximumIsExclusive = true)]
#endif
    public double? Age { get; set; }

    [Attr]
    public Uri? ProfilePicture { get; set; }

    [Attr]
    [Url]
    public string? BackgroundPicture { get; set; }

    [Attr]
#if !NET6_0
    [Length(1, 10)]
#endif
    public List<string>? Tags { get; set; }

    [Attr]
#if !NET6_0
    [AllowedValues(null, "NL", "FR")]
#endif
    public string? CountryCode { get; set; }

    [Attr]
#if !NET6_0
    [DeniedValues("pluto")]
#endif
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
