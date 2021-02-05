using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.RestrictedControllers
{
    public sealed class Table : Identifiable
    {
        [Attr]
        public int LegCount { get; set; }
    }
}
