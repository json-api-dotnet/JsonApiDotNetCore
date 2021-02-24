using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Company : Identifiable, ISoftDeletable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsSoftDeleted { get; set; }

        [HasMany]
        public ICollection<Department> Departments { get; set; }
    }
}
