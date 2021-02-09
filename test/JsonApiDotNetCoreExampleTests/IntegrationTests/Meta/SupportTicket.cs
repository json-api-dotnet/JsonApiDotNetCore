using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    public sealed class SupportTicket : Identifiable
    {
        [Attr]
        public string Description { get; set; }
    }
}
