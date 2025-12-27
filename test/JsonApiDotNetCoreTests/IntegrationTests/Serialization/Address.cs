using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Serialization;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class Address
{
    public required string Street { get; set; }
    public string? ZipCode { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
}
