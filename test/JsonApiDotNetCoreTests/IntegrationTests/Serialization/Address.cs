using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Address
{
    public string Street { get; set; } = null!;
    public string? ZipCode { get; set; }
    public string City { get; set; } = null!;
    public string Country { get; set; } = null!;
}
