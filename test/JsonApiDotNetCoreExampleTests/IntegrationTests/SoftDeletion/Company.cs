using System.Collections.Generic;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    public sealed class Company : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsSoftDeleted { get; set; }

        [HasMany]
        public ICollection<Department> Departments { get; set; }
    }
}
