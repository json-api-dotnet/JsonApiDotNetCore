using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    public sealed class ConsumerArticle : Identifiable
    {
        [Attr]
        public string Code { get; set; }
    }
}
