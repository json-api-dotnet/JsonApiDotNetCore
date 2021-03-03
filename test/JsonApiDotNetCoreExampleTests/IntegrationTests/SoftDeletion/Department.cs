using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.SoftDeletion
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Department : Identifiable, ISoftDeletable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public bool IsSoftDeleted { get; set; }

        [HasOne]
        public Company Company { get; set; }
    }
}
