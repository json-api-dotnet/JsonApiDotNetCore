using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.ResourceHooks.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class Passport : Identifiable
    {
        [HasOne]
        public Person Person { get; set; }
    }
}
