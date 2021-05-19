using System;
using JetBrains.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public interface ISoftDeletable
    {
        DateTimeOffset? SoftDeletedAt { get; set; }
    }
}
