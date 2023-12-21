using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;
using TestBuildingBlocks;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection")]
public sealed class GiftCertificate(InjectionDbContext injectionDbContext) : Identifiable<int>
{
    private readonly ISystemClock _systemClock = injectionDbContext.SystemClock;

    [Attr]
    public DateTimeOffset IssueDate { get; set; }

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    [NotMapped]
    public bool HasExpired => IssueDate.AddYears(1) < _systemClock.UtcNow;

    [HasOne]
    public PostOffice? Issuer { get; set; }
}
