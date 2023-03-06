using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.OptimisticConcurrency")]
public sealed class FriendlyUrl : PostgresVersionedIdentifiable<long>
{
    [Attr]
    public string Uri { get; set; } = null!;

    [HasOne]
    public WebPage? Page { get; set; }
}
