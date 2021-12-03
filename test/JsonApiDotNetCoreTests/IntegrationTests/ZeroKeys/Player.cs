using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ZeroKeys")]
    public sealed class Player : Identifiable<string>
    {
        [Attr]
        public string EmailAddress { get; set; } = null!;

        [HasOne]
        public Game? ActiveGame { get; set; }

        [HasMany]
        public ICollection<Game> RecentlyPlayed { get; set; } = new List<Game>();
    }
}
