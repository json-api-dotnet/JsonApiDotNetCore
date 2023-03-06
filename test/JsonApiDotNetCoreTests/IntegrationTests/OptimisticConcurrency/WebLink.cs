using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency")]
public sealed class WebLink : PostgresVersionedIdentifiable<long>
{
    [Attr]
    public string? Text { get; set; }

    [Attr]
    public string Url { get; set; } = null!;

    [Attr]
    public bool OpensInNewTab { get; set; }

    [HasMany]
    public IList<PageFooter> UsedIn { get; set; } = new List<PageFooter>();
}
