using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.ExceptionHandling")]
    public sealed class ThrowingArticle : Identifiable<int>
    {
        [Attr]
        [NotMapped]
        public string Status => throw new InvalidOperationException("Article status could not be determined.");
    }
}
