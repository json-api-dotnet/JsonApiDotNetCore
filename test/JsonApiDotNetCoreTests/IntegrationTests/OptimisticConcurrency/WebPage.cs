using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency")]
public sealed class WebPage : PostgresVersionedIdentifiable<long>
{
    [Attr]
    public string Title { get; set; } = null!;

    [HasOne]
    public FriendlyUrl Url { get; set; } = null!;

    [HasMany]
    public IList<TextBlock> Content { get; set; } = new List<TextBlock>();

    [HasOne]
    public PageFooter? Footer { get; set; }
}
