using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class WebAccount : Identifiable<long>
{
    [Attr]
    public required string UserName { get; set; }

    [Attr(Capabilities = AttrCapabilities.All & ~AttrCapabilities.AllowView)]
    public required string Password { get; set; }

    [Attr]
    public required string DisplayName { get; set; }

    [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
    public DateTime? DateOfBirth { get; set; }

    [Attr]
    public required string EmailAddress { get; set; }

    [HasOne(Capabilities = HasOneCapabilities.All & ~HasOneCapabilities.AllowView)]
    public Human? Person { get; set; }

    [HasMany]
    public IList<BlogPost> Posts { get; set; } = new List<BlogPost>();

    [HasOne]
    public AccountPreferences? Preferences { get; set; }

    [HasMany]
    public IList<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
}
