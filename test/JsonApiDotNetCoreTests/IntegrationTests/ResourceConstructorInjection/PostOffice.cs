using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ResourceConstructorInjection")]
public sealed class PostOffice(InjectionDbContext injectionDbContext) : Identifiable<int>
{
    private readonly TimeProvider _timeProvider = injectionDbContext.TimeProvider;

    [Attr]
    public string Address { get; set; } = null!;

    [Attr(Capabilities = AttrCapabilities.AllowView)]
    [NotMapped]
    public bool IsOpen => IsWithinOperatingHours();

    [HasMany]
    public IList<GiftCertificate> GiftCertificates { get; set; } = new List<GiftCertificate>();

    private bool IsWithinOperatingHours()
    {
        DateTimeOffset utcNow = _timeProvider.GetUtcNow();
        return utcNow.DayOfWeek is >= DayOfWeek.Monday and <= DayOfWeek.Friday && utcNow.Hour is >= 9 and <= 17;
    }
}
