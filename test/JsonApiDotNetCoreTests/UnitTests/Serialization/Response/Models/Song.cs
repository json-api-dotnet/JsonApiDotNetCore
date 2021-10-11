#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Song : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; }
    }
}
