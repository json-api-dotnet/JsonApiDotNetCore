using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Meta
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class SupportTicket : Identifiable
    {
        [Attr]
        public string Description { get; set; }
    }
}
