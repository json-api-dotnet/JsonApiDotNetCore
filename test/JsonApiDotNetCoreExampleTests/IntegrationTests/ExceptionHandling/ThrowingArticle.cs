using System;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.ExceptionHandling
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class ThrowingArticle : Identifiable
    {
        [Attr]
        [NotMapped]
        public string Status => throw new InvalidOperationException("Article status could not be determined.");
    }
}
