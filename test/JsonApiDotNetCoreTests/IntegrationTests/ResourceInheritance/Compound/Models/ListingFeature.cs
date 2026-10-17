using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceInheritance.Compound.Models;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class ListingFeature
{
    [Attr]
    public string? Name { get; set; }

    [Attr]
    public bool IsPremium { get; set; }
}
