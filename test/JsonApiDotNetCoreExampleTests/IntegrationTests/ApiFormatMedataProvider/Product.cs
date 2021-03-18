using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ApiFormatMedataProvider
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Product : Identifiable
    {
        [Attr]
        public string Name { get; set; }

        [Attr]
        public decimal Price { get; set; }
    }
}
