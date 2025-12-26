using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.QueryStrings;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.QueryStrings")]
public sealed class Comment : Identifiable<long>
{
    [Attr]
    public required string Text { get; set; }

    [Attr]
    public DateTime CreatedAt { get; set; }

    [Attr]
    public int NumStars { get; set; }

    [HasOne]
    public WebAccount? Author { get; set; }

    [HasOne]
    public required BlogPost Parent { get; set; }
}
