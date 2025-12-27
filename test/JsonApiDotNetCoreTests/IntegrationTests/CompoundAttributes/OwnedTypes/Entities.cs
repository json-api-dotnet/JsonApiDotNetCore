using System.ComponentModel.DataAnnotations;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.OwnedTypes;

// TODO: Move to separate files.

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.OwnedTypes")]
public sealed class AddressBook : Identifiable<long>
{
    [Attr(IsCompound = true)]
    public ContactRoot? EmergencyContact { get; set; }

    [Attr(IsCompound = true)]
    public required List<ContactRoot>? Favorites { get; set; } = [];

    [Attr]
    public List<string?>? SyncUrls { get; set; }

    [HasMany]
    public ISet<Contact> Contacts { get; set; } = new HashSet<Contact>();
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes.OwnedTypes")]
public sealed class Contact : Identifiable<long>
{
    [Attr(IsCompound = true)]
    public required ContactRoot Content { get; set; }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class ContactName
{
    [Attr]
    public string? FirstName { get; set; }

    [Attr]
    public required string LastName { get; set; }

    [Attr]
    public required string DisplayName { get; set; }

    public override string ToString()
    {
        return DisplayName;
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class ContactCompany
{
    [Attr]
    public required string Name { get; set; }

    [Attr]
    public string? JobTitle { get; set; }

    [Attr]
    public string? Department { get; set; }

    public override string ToString()
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(JobTitle))
        {
            builder.Append(JobTitle);
        }

        if (!string.IsNullOrEmpty(Department))
        {
            if (builder.Length > 0)
            {
                builder.Append(", ");
            }

            builder.Append(Department);
        }

        if (builder.Length > 0)
        {
            builder.Append(" at ");
        }

        builder.Append(Name);

        return builder.ToString();
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class ContactPhoneNumber
{
    [Attr]
    public string? CountryPrefix { get; set; }

    [Attr]
    [MinLength(5)]
    public required sbyte[] Digits { get; set; } = [];

    [Attr]
    public string? Label { get; set; }

    public override string ToString()
    {
        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(CountryPrefix))
        {
            builder.Append(CountryPrefix);
            builder.Append(' ');
        }

        builder.Append(string.Join(string.Empty, Digits));

        if (!string.IsNullOrEmpty(Label))
        {
            builder.Append($" ({Label})");
        }

        return builder.ToString();
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class ContactAddress
{
    [Attr]
    public required string IsoCountryCode { get; set; }

    [Attr]
    public required string Line1 { get; set; }

    [Attr]
    public string? Line2 { get; set; }

    [Attr]
    public string? PostalCode { get; set; }

    [Attr]
    public string? City { get; set; }

    [Attr]
    public string? Label { get; set; }

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append(Line1);

        if (!string.IsNullOrEmpty(Line2))
        {
            builder.Append(' ');
            builder.Append(Line2);
        }

        builder.Append(", ");

        if (!string.IsNullOrEmpty(City))
        {
            builder.Append(City);
            builder.Append(", ");
        }

        builder.Append(IsoCountryCode);

        if (!string.IsNullOrEmpty(Label))
        {
            builder.Append($" ({Label})");
        }

        return builder.ToString();
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
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

    public override string ToString()
    {
        return Year != null ? $"{Year:D4}-{Month:D2}-{Day:D2}" : $"{Month:D2}-{Day:D2}";
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class LabeledValue<T>
{
    [Attr]
    public required T Value { get; set; }

    [Attr]
    public string? Label { get; set; }

    public static LabeledValue<T> Create(T value, string? label = null)
    {
        return new LabeledValue<T>
        {
            Value = value,
            Label = label
        };
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append(Value);

        if (!string.IsNullOrEmpty(Label))
        {
            builder.Append($" ({Label})");
        }

        return builder.ToString();
    }
}

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class ContactRoot
{
    [Attr(IsCompound = true)]
    public required ContactName Name { get; set; }

    [Attr(IsCompound = true)]
    public ContactCompany? Company { get; set; }

    [Attr(IsCompound = true)]
    public required List<LabeledValue<string>> EmailAddresses { get; set; } = [];

    [Attr(IsCompound = true)]
    public ContactPhoneNumber? EmergencyPhoneNumber { get; set; }

    [Attr(IsCompound = true)]
    public List<ContactPhoneNumber>? PhoneNumbers { get; set; }

    [Attr(IsCompound = true)]
    public List<ContactAddress>? Addresses { get; set; }

    [Attr(IsCompound = true)]
    public ContactDate? BirthDate { get; set; }

    [Attr(IsCompound = true)]
    public List<LabeledValue<Uri>>? Websites { get; set; }

    [Attr(IsCompound = true)]
    public List<LabeledValue<string>>? Relations { get; set; }

    [Attr]
    public string? Remarks { get; set; }

    public override string ToString()
    {
        return Name.ToString();
    }
}
