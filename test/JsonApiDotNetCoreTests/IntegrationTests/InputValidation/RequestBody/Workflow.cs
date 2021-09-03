using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.InputValidation.RequestBody
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Workflow : Identifiable<Guid>
    {
        [Attr]
        public WorkflowStage Stage { get; set; }
    }
}
