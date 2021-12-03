using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace JsonApiDotNetCoreTests.IntegrationTests.Archiving
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource(ControllerNamespace = "JsonApiDotNetCoreTests.IntegrationTests.Archiving")]
    public sealed class TelevisionNetwork : Identifiable<int>
    {
        [Attr]
        public string Name { get; set; } = null!;

        [HasMany]
        public ISet<TelevisionStation> Stations { get; set; } = new HashSet<TelevisionStation>();
    }
}
