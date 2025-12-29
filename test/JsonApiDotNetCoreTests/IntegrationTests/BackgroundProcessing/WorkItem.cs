using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.BackgroundProcessing;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.BackgroundProcessing")]
public sealed class WorkItem : Identifiable<long>
{
    [Attr]
    public string Description { get; set; } = null!;

    [Attr]
    public string? Status { get; set; }
}