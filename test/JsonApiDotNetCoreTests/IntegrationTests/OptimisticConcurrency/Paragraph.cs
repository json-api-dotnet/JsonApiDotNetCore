using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency")]
public sealed class Paragraph : PostgresVersionedIdentifiable<long>
{
    [Attr]
    public string? Heading { get; set; }

    [Attr]
    public string Text { get; set; } = null!;

    [HasOne]
    public WebImage? TopImage { get; set; }

    [HasMany]
    public IList<TextBlock> UsedIn { get; set; } = new List<TextBlock>();
}
