using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Blog : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; } = null!;

        [HasOne]
        public Person Reviewer { get; set; } = null!;

        [HasOne]
        public Person Author { get; set; } = null!;
    }
}
