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

    [Attr]
    public List<string>? SyncUrls { get; set; }

    [HasMany]
    public ISet<Contact> Contacts { get; set; } = new HashSet<Contact>();
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes")]
public sealed class Contact : Identifiable<long>
{
    [Attr(IsCompound = true)]
    public required ContactRoot Content { get; set; }

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
    public required ContactName Name { get; set; }

    [Attr(IsCompound = true)]
    public ContactCompany? Company { get; set; }

    [Attr(IsCompound = true)]
    public List<ContactPhoneNumber> PhoneNumbers { get; set; } = [];

    [Attr(IsCompound = true)]
    public List<ContactAddress> Addresses { get; set; } = [];

    [Attr(IsCompound = true)]
    public ContactDate? BirthDate { get; set; }

    [Attr(IsCompound = true)]
    public List<LabeledValue<string>> EmailAddresses { get; set; } = [];

    [Attr(IsCompound = true)]
    public List<LabeledValue<string>> Relations { get; set; } = [];
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactName
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public required string LastName { get; set; }

    [Attr]
    public required string DisplayName { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactCompany
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public string? Department { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactAddress
{
    [Attr]
    public required string IsoCountryCode { get; set; }

    [Attr]
    public required string City { get; set; }

    [Attr]
    public string? Line1 { get; set; }

    [Attr]
    public string? PostalCode { get; set; }

    [Attr]
    public string? Label { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactPhoneNumber
{
    [Attr]
    public string? CountryPrefix { get; set; }

    [Attr]
    public required string Number { get; set; }

    [Attr]
    public string? Label { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ContactDate
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
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class LabeledValue<T>
{
    [Attr]
    public required T Value { get; set; }

    [Attr]
    public string? Label { get; set; }
}
