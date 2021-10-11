#nullable disable

using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Department : Identifiable<int>, ISoftDeletable
    {
        [Attr]
        public string Name { get; set; }

        public DateTimeOffset? SoftDeletedAt { get; set; }

        [HasOne]
        public Company Company { get; set; }
    }
}
