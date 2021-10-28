using System.Collections.Generic;
using JetBrains.Annotations;
using JsonApiDotNetCore.Resources;
using JsonApiDotNetCore.Resources.Annotations;

namespace UnitTests.TestModels
{
    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    public sealed class TestResourceWithList : Identifiable<int>
    {
        [Attr]
        public List<ComplexType> ComplexFields { get; set; }
    }
}
