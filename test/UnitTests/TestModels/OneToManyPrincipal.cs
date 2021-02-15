using System.Collections.Generic;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    public sealed class OneToManyPrincipal : IdentifiableWithAttribute
    {
        [HasMany] public ISet<OneToManyDependent> Dependents { get; set; }
    }
}