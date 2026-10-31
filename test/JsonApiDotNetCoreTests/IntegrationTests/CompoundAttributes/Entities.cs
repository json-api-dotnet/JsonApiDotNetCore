using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes")]
public sealed class AddressBook : Identifiable<long>
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr(IsCompound = true)]
    public ContactRoot? EmergencyContact { get; set; }

    [Attr(IsCompound = true)]
    public List<ContactRoot> Favorites { get; set; } = [];

    // Scalar collection of strings for text-matching filter tests (startsWith, contains, endsWith)
    // The collection-level MaxLength rule applies to the number of elements.
    [Attr]
    [MaxLength(5)]
    public List<string>? Categories { get; set; }

    // Scalar collection of Guid value types for count, has, and sort=count tests
    // Restricted capability: AllowSort blocked
    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowSort)]
    public List<Guid>? SyncDeviceIds { get; set; }

    [HasMany]
    public ISet<Contact> Contacts { get; set; } = new HashSet<Contact>();
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes")]
public sealed class Contact : Identifiable<long>
{
    [Attr(IsCompound = true)]
    public ContactRoot Content { get; set; } = null!;

    [Attr]
    public string? Remarks { get; set; }

    [HasOne]
    public AddressBook? AddressBook { get; set; }

    [HasMany]
    public ISet<ContactNote> Notes { get; set; } = new HashSet<ContactNote>();
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes")]
public sealed class ContactNote : Identifiable<long>
{
    [Attr]
    public string Text { get; set; } = null!;

    [Attr]
    [Required]
    public int? Priority { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactRoot
{
    [Attr(IsCompound = true)]
    public ContactName Name { get; set; } = null!;

    [Attr(IsCompound = true)]
    public ContactCompany? Company { get; set; }

    [Attr(IsCompound = true)]
    public List<ContactPhoneNumber> PhoneNumbers { get; set; } = [];

    [Attr(IsCompound = true)]
    public List<ContactAddress> Addresses { get; set; } = [];

    [Attr(IsCompound = true)]
    public ContactDate? BirthDate { get; set; }

    // Restricted capability: AllowView blocked (never serialized to clients)
    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
    public string? SecurityPin { get; set; }

    // Scalar collection with nullable elements (int?) and collection-level nullability
    [Attr]
    public List<int?>? Ratings { get; set; }

    [Attr(IsCompound = true)]
    public List<LabeledValue<string>> EmailAddresses { get; set; } = [];

    // Nullable, so that null and an empty collection are both legal on the same type
    [Attr(IsCompound = true)]
    public List<LabeledValue<string>>? Relations { get; set; }

    // Generic compound type with DateOnly value type
    [Attr(IsCompound = true)]
    public List<LabeledValue<DateOnly>> ImportantDates { get; set; } = [];

    // Scalar collections of enum, decimal and DateTime elements
    [Attr]
    public List<ContactMethod> PreferredContactMethods { get; set; } = [];

    [Attr]
    public List<decimal> CreditLimits { get; set; } = [];

    [Attr]
    public List<DateTime> Anniversaries { get; set; } = [];

    // Restricted capability: AllowFilter blocked
    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowFilter)]
    public string? InternalNotes { get; set; }

    // Intermediate compound with AllowFilter blocked, which blocks all nested paths
    [Attr(IsCompound = true, Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowFilter)]
    public ContactAudit? Audit { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactAudit
{
    [Attr]
    public string? CreatedBy { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactName
{
    [Attr]
    [MaxLength(20)]
    public string? FirstName { get; set; }

    [Attr]
    public string? MiddleName { get; set; }

    [Attr]
    public string LastName { get; set; } = null!;

    // Calculated read-only property (get-only; no setter)
    [Attr]
    public string DisplayName =>
        string.Join(' ', new[]
        {
            FirstName,
            MiddleName,
            LastName
        }.Where(s => !string.IsNullOrEmpty(s)));

    // Restricted capability: AllowCreate blocked (POST returns 422, PATCH allowed)
    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowCreate)]
    public string? NickName { get; set; }

    // Restricted capability: AllowChange blocked (POST allowed, PATCH returns 422)
    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowChange)]
    public string? MaidenName { get; set; }

    // Restricted capability: AllowView blocked (never returned in responses)
    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
    public string? SecretAlias { get; set; }

    // Restricted capability: AllowSort blocked
    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowSort)]
    public string? PhoneticName { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactCompany
{
    [Attr]
    public string Name { get; set; } = null!;

    [Attr]
    public string? Department { get; set; }

    // Scalar collection of TimeSpan value types inside a JSON-mapped complex type
    [Attr]
    public List<TimeSpan> BusinessHours { get; set; } = [];

    // Scalar collection of strings, next to another scalar collection
    [Attr]
    public List<string> Departments { get; set; } = [];

    // Private setter
    [Attr]
    public string? RegistrationNumber { get; private set; }

    public void SetRegistrationNumber(string? value)
    {
        RegistrationNumber = value;
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactAddress
{
    // Init-only setter
    [Attr]
    public string IsoCountryCode { get; init; } = null!;

    [Attr]
    public string City { get; set; } = null!;

    [Attr]
    public string? Line1 { get; set; }

    // Custom public name with an uppercase letter and a hyphen
    [Attr(PublicName = "Address-Line2")]
    public string? Line2 { get; set; }

    // Custom public name "zipCode"; JsonPropertyName is neither needed nor used
    [Attr(PublicName = "zipCode")]
    public string? PostalCode { get; set; }

    [Attr]
    public string? Label { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactPhoneNumber
{
    [Attr]
    public string? CountryPrefix { get; set; }

    // [Required] attribute on collection element to test write-side ModelState validation
    [Attr]
    [Required]
    public string Number { get; set; } = null!;

    [Attr]
    public string? Label { get; set; }

    [Attr]
    public PhoneKind? Kind { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactDate : IValidatableObject
{
    [Attr]
    [Range(1800, 2500)]
    public ushort? Year { get; set; }

    [Attr]
    [Required]
    [Range(1, 12)]
    public byte? Month { get; set; }

    [Attr]
    [Required]
    [Range(1, 31)]
    public byte? Day { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Month is >= 1 and <= 12 && Day is >= 1 && Day > DateTime.DaysInMonth(Year ?? 2000, Month.Value))
        {
            yield return new ValidationResult("The day does not exist in the specified month.", [nameof(Day)]);
        }
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LabeledValue<T>
{
    [Attr]
    public T Value { get; set; } = default!;

    [Attr]
    public string? Label { get; set; }
}

public enum ContactMethod
{
    Phone,
    Email,
    Letter
}

public enum PhoneKind
{
    Mobile,
    Landline,
    Fax
}
