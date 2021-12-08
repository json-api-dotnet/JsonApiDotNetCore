using JetBrains.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion;

[UsedImplicitly(ImplicitUseTargetFlags.Members)]
public interface ISoftDeletable
{
    DateTimeOffset? SoftDeletedAt { get; set; }
}
