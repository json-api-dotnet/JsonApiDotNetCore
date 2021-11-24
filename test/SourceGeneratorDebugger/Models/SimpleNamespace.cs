using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

// ReSharper disable CheckNamespace

namespace SourceGeneratorDebugger
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    [Resource]
    public sealed class SimpleNamespace : Identifiable<int>
    {
        [Attr]
        public string? Value { get; set; }
    }
}
