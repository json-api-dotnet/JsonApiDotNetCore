using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
    public sealed class WebAccount : Identifiable<int>
    {
        [Attr]
        public string UserName { get; set; } = null!;

        [Attr(Capabilities = ~AttrCapabilities.AllowView)]
        public string Password { get; set; } = null!;

        [Attr]
        public string DisplayName { get; set; } = null!;

        [Attr(Capabilities = AttrCapabilities.All & ~(AttrCapabilities.AllowFilter | AttrCapabilities.AllowSort))]
        public DateTime? DateOfBirth { get; set; }

        [Attr]
        public string EmailAddress { get; set; } = null!;

        [HasMany]
        public IList<BlogPost> Posts { get; set; } = new List<BlogPost>();

        [HasOne]
        public AccountPreferences? Preferences { get; set; }

        [HasMany]
        public IList<LoginAttempt> LoginAttempts { get; set; } = new List<LoginAttempt>();
    }
}
