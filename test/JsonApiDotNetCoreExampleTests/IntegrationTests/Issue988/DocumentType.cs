using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.Issue988
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class DocumentType : Identifiable<Guid>
    {
        [Attr]
        public string Description { get; set; }
    }
}
