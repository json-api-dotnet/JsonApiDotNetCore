using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;

namespace JsonApiDotNetCoreTests.IntegrationTests.NonJsonApiControllers
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class UnknownResource : Identifiable<int>
    {
        public string? Value { get; set; }
    }
}
