using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency")]
public sealed class TextBlock : PostgresVersionedIdentifiable<long>
{
    [Attr]
    public int ColumnCount { get; set; }

    [HasMany]
    public IList<Paragraph> Paragraphs { get; set; } = new List<Paragraph>();

    [HasMany]
    public IList<WebPage> UsedAt { get; set; } = new List<WebPage>();
}
