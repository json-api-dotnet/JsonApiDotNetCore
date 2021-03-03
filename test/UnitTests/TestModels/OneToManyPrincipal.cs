using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class OneToManyPrincipal : IdentifiableWithAttribute
    {
        [HasMany]
        public ISet<OneToManyDependent> Dependents { get; set; }
    }
}
