using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.CustomRoutes
{
    public sealed class Civilian : Identifiable
    {
        [Attr]
        public string Name { get; set; }
    }
}
