using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;
using Microsoft.EntityFrameworkCore;

namespace JsonApiDotNetCoreTests.IntegrationTests.CompoundAttributes;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Owned]
public sealed class Contact
{
    [Attr]
    public string DisplayName { get; set; } = null!;

    [Attr(IsCompound = true)]
    public Address LivingAddress { get; set; } = null!;

    [Attr(IsCompound = true)]
    public IList<Address>? PreviousLivingAddresses { get; set; }

    [Attr(IsCompound = true)]
    public PhoneNumber? PrimaryPhoneNumber { get; set; }

    [Attr(IsCompound = true)]
    public IList<PhoneNumber> SecondaryPhoneNumbers { get; set; } = [];

    [Attr]
    public string[] EmailAddresses { get; set; } = [];

    [Attr]
    public string[]? Websites { get; set; }
}
