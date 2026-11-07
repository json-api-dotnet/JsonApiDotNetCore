using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public sealed class CompanyAddress
{
    [Attr]
    public string? Street { get; set; }

    [Attr]
    public string? City { get; set; }
}
