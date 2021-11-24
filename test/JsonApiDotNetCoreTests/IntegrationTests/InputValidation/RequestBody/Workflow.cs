using System;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.InputValidation.RequestBody")]
    public sealed class Workflow : Identifiable<Guid>
    {
        [Attr]
        public WorkflowStage Stage { get; set; }
    }
}
