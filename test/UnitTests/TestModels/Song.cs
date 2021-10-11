#nullable disable

using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Song : Identifiable<int>
    {
        [Attr]
        public string Title { get; set; }
    }
}
