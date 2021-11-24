using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion")]
    public sealed class Department : Identifiable<int>, ISoftDeletable
    {
        [Attr]
        public string Name { get; set; } = null!;

        public DateTimeOffset? SoftDeletedAt { get; set; }

        [HasOne]
        public Company? Company { get; set; }
    }
}
