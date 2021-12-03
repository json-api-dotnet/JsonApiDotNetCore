using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
    public sealed class Comment : Identifiable<int>
    {
        [Attr]
        public string Text { get; set; } = null!;

        [Attr]
        public DateTime CreatedAt { get; set; }

        [HasOne]
        public WebAccount? Author { get; set; }

        [HasOne]
        public BlogPost Parent { get; set; } = null!;
    }
}
