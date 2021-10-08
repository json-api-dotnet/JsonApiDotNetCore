using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.UnitTests.Serialization.Response.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Food : Identifiable<int>
    {
        [Attr]
        public string Dish { get; set; }
    }
}
