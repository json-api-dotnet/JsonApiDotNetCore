using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ListingAddress
{
    [Attr]
    public string? Street { get; set; }

    [Attr]
    [Required]
    public string? City { get; set; }

    [Attr]
    public string? CountryCode { get; set; }
}
