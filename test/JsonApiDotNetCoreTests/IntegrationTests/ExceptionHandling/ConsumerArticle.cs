using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling")]
    public sealed class ConsumerArticle : Identifiable<int>
    {
        [Attr]
        public string Code { get; set; } = null!;
    }
}
