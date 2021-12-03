using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.SoftDeletion")]
    public sealed class Company : Identifiable<int>, ISoftDeletable
    {
        [Attr]
        public string Name { get; set; } = null!;

        public DateTimeOffset? SoftDeletedAt { get; set; }

        [HasMany]
        public ICollection<Department> Departments { get; set; } = new List<Department>();
    }
}
