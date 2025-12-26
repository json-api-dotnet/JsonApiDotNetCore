using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
[Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion")]
public sealed class Department : Identifiable<long>, ISoftDeletable
{
    [Attr]
    public required string Name { get; set; }

    public DateTimeOffset? SoftDeletedAt { get; set; }

    [HasOne]
    public Company? Company { get; set; }
}
